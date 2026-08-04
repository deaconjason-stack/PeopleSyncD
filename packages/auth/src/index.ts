import {
  createCipheriv,
  createDecipheriv,
  createHash,
  createHmac,
  randomBytes,
  timingSafeEqual
} from "node:crypto";
import type { SessionClaims } from "@peoplesyncd/shared";

const BASE32_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

function encode(value: string): string {
  return Buffer.from(value, "utf8").toString("base64url");
}

function decode(value: string): string {
  return Buffer.from(value, "base64url").toString("utf8");
}

function signature(payload: string, secret: string): string {
  return createHmac("sha256", secret).update(payload).digest("base64url");
}

function derivedKey(keyMaterial: string, purpose: string): Buffer {
  if (keyMaterial.length < 32) throw new Error("Credential key material must contain at least 32 characters");
  return createHash("sha256").update(`peoplesyncd:${purpose}:v1:${keyMaterial}`, "utf8").digest();
}

function base32Encode(value: Buffer): string {
  let bits = 0;
  let accumulator = 0;
  let output = "";
  for (const byte of value) {
    accumulator = (accumulator << 8) | byte;
    bits += 8;
    while (bits >= 5) {
      bits -= 5;
      output += BASE32_ALPHABET[(accumulator >>> bits) & 31] ?? "";
    }
  }
  if (bits > 0) output += BASE32_ALPHABET[(accumulator << (5 - bits)) & 31] ?? "";
  return output;
}

function base32Decode(value: string): Buffer {
  const normalized = value.toUpperCase().replace(/=+$/u, "").replace(/\s+/gu, "");
  let bits = 0;
  let accumulator = 0;
  const output: number[] = [];
  for (const character of normalized) {
    const index = BASE32_ALPHABET.indexOf(character);
    if (index < 0) throw new Error("Invalid base32 secret");
    accumulator = (accumulator << 5) | index;
    bits += 5;
    if (bits >= 8) {
      bits -= 8;
      output.push((accumulator >>> bits) & 0xff);
    }
  }
  return Buffer.from(output);
}

export function createSessionToken(claims: SessionClaims, secret: string): string {
  if (secret.length < 16) throw new Error("Session secret must be at least 16 characters");
  if (!claims.sessionId) throw new Error("Session identifier is required");
  const payload = encode(JSON.stringify(claims));
  return `${payload}.${signature(payload, secret)}`;
}

export function verifySessionToken(token: string, secret: string, now = Math.floor(Date.now() / 1000)): SessionClaims {
  const [payload, supplied] = token.split(".");
  if (!payload || !supplied) throw new Error("Malformed session token");
  const expected = signature(payload, secret);
  const suppliedBuffer = Buffer.from(supplied);
  const expectedBuffer = Buffer.from(expected);
  if (suppliedBuffer.length !== expectedBuffer.length || !timingSafeEqual(suppliedBuffer, expectedBuffer)) {
    throw new Error("Invalid session signature");
  }
  const claims = JSON.parse(decode(payload)) as SessionClaims;
  if (
    !claims.sessionId ||
    !claims.subject ||
    !Array.isArray(claims.organizationIds) ||
    !Array.isArray(claims.permissions) ||
    !Array.isArray(claims.authenticationMethods)
  ) {
    throw new Error("Invalid session claims");
  }
  if (claims.expiresAt <= now) throw new Error("Session expired");
  return claims;
}

export function bearerToken(header: string | undefined): string {
  if (!header?.startsWith("Bearer ")) throw new Error("Missing bearer token");
  const token = header.slice("Bearer ".length).trim();
  if (!token) throw new Error("Missing bearer token");
  return token;
}

export function createTotpSecret(bytes = 20): string {
  if (!Number.isInteger(bytes) || bytes < 16 || bytes > 64) throw new Error("TOTP secret size must be between 16 and 64 bytes");
  return base32Encode(randomBytes(bytes));
}

export function totpCode(secret: string, atMilliseconds = Date.now(), periodSeconds = 30, digits = 6): string {
  if (!Number.isInteger(periodSeconds) || periodSeconds < 15) throw new Error("TOTP period is invalid");
  if (!Number.isInteger(digits) || digits < 6 || digits > 8) throw new Error("TOTP digits are invalid");
  const counter = Math.floor(atMilliseconds / 1000 / periodSeconds);
  const counterBuffer = Buffer.alloc(8);
  counterBuffer.writeBigUInt64BE(BigInt(counter));
  const digest = createHmac("sha1", base32Decode(secret)).update(counterBuffer).digest();
  const offset = (digest[digest.length - 1] ?? 0) & 0x0f;
  const binary =
    (((digest[offset] ?? 0) & 0x7f) << 24) |
    (((digest[offset + 1] ?? 0) & 0xff) << 16) |
    (((digest[offset + 2] ?? 0) & 0xff) << 8) |
    ((digest[offset + 3] ?? 0) & 0xff);
  return String(binary % 10 ** digits).padStart(digits, "0");
}

export function verifyTotpCode(
  secret: string,
  suppliedCode: string,
  atMilliseconds = Date.now(),
  window = 1,
  periodSeconds = 30,
  digits = 6
): boolean {
  if (!new RegExp(`^\\d{${digits}}$`, "u").test(suppliedCode)) return false;
  const supplied = Buffer.from(suppliedCode, "utf8");
  for (let offset = -window; offset <= window; offset += 1) {
    const candidate = Buffer.from(totpCode(secret, atMilliseconds + offset * periodSeconds * 1000, periodSeconds, digits), "utf8");
    if (candidate.length === supplied.length && timingSafeEqual(candidate, supplied)) return true;
  }
  return false;
}

export function totpProvisioningUri(input: { secret: string; accountName: string; issuer: string }): string {
  if (!input.accountName.trim() || !input.issuer.trim()) throw new Error("TOTP account and issuer are required");
  const label = `${input.issuer}:${input.accountName}`;
  const query = new URLSearchParams({
    secret: input.secret,
    issuer: input.issuer,
    algorithm: "SHA1",
    digits: "6",
    period: "30"
  });
  return `otpauth://totp/${encodeURIComponent(label)}?${query.toString()}`;
}

export function encryptCredential(plaintext: string, keyMaterial: string): string {
  const iv = randomBytes(12);
  const cipher = createCipheriv("aes-256-gcm", derivedKey(keyMaterial, "mfa-encryption"), iv);
  const encrypted = Buffer.concat([cipher.update(plaintext, "utf8"), cipher.final()]);
  const tag = cipher.getAuthTag();
  return `v1.${iv.toString("base64url")}.${tag.toString("base64url")}.${encrypted.toString("base64url")}`;
}

export function decryptCredential(ciphertext: string, keyMaterial: string): string {
  const [version, ivEncoded, tagEncoded, valueEncoded] = ciphertext.split(".");
  if (version !== "v1" || !ivEncoded || !tagEncoded || !valueEncoded) throw new Error("Encrypted credential format is invalid");
  const decipher = createDecipheriv(
    "aes-256-gcm",
    derivedKey(keyMaterial, "mfa-encryption"),
    Buffer.from(ivEncoded, "base64url")
  );
  decipher.setAuthTag(Buffer.from(tagEncoded, "base64url"));
  return Buffer.concat([decipher.update(Buffer.from(valueEncoded, "base64url")), decipher.final()]).toString("utf8");
}

export function createRecoveryCodes(count = 8): string[] {
  if (!Number.isInteger(count) || count < 4 || count > 20) throw new Error("Recovery code count is invalid");
  return Array.from({ length: count }, () => {
    const value = randomBytes(6).toString("hex").toUpperCase();
    return `${value.slice(0, 6)}-${value.slice(6)}`;
  });
}

export function hashRecoveryCode(code: string, keyMaterial: string): string {
  const normalized = code.trim().toUpperCase();
  if (!normalized) throw new Error("Recovery code is required");
  return createHmac("sha256", derivedKey(keyMaterial, "mfa-recovery"))
    .update(normalized, "utf8")
    .digest("base64url");
}
