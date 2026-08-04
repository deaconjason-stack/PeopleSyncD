import { createHmac, timingSafeEqual } from "node:crypto";
import type { SessionClaims } from "@peoplesyncd/shared";

function encode(value: string): string {
  return Buffer.from(value, "utf8").toString("base64url");
}

function decode(value: string): string {
  return Buffer.from(value, "base64url").toString("utf8");
}

function signature(payload: string, secret: string): string {
  return createHmac("sha256", secret).update(payload).digest("base64url");
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
