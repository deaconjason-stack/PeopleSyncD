function decodeBase64Url(value: string): ArrayBuffer {
  const padding = '='.repeat((4 - (value.length % 4)) % 4);
  const base64 = `${value.replace(/-/g, '+').replace(/_/g, '/')}${padding}`;
  const binary = window.atob(base64);
  const bytes = Uint8Array.from(binary, (character) => character.charCodeAt(0));
  return bytes.buffer;
}

function encodeBase64Url(value: ArrayBuffer): string {
  const bytes = new Uint8Array(value);
  let binary = '';
  for (const byte of bytes) binary += String.fromCharCode(byte);
  return window.btoa(binary).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/g, '');
}

interface CreationOptionsJson extends Omit<PublicKeyCredentialCreationOptions, 'challenge' | 'user' | 'excludeCredentials'> {
  challenge: string;
  user: Omit<PublicKeyCredentialUserEntity, 'id'> & { id: string };
  excludeCredentials?: Array<Omit<PublicKeyCredentialDescriptor, 'id'> & { id: string }>;
}

interface RequestOptionsJson extends Omit<PublicKeyCredentialRequestOptions, 'challenge' | 'allowCredentials'> {
  challenge: string;
  allowCredentials?: Array<Omit<PublicKeyCredentialDescriptor, 'id'> & { id: string }>;
}

function creationOptions(json: string): PublicKeyCredentialCreationOptions {
  const parsed = JSON.parse(json) as CreationOptionsJson;
  return {
    ...parsed,
    challenge: decodeBase64Url(parsed.challenge),
    user: { ...parsed.user, id: decodeBase64Url(parsed.user.id) },
    excludeCredentials: parsed.excludeCredentials?.map((credential) => ({
      ...credential,
      id: decodeBase64Url(credential.id),
    })),
  };
}

function requestOptions(json: string): PublicKeyCredentialRequestOptions {
  const parsed = JSON.parse(json) as RequestOptionsJson;
  return {
    ...parsed,
    challenge: decodeBase64Url(parsed.challenge),
    allowCredentials: parsed.allowCredentials?.map((credential) => ({
      ...credential,
      id: decodeBase64Url(credential.id),
    })),
  };
}

export async function createPasskeyCredential(optionsJson: string): Promise<string> {
  if (!window.PublicKeyCredential || !navigator.credentials) {
    throw new Error('This browser does not support WebAuthn passkeys.');
  }

  const credential = await navigator.credentials.create({ publicKey: creationOptions(optionsJson) });
  if (!(credential instanceof PublicKeyCredential)
      || !(credential.response instanceof AuthenticatorAttestationResponse)) {
    throw new Error('Passkey registration did not return a WebAuthn credential.');
  }

  const response = credential.response;
  return JSON.stringify({
    id: credential.id,
    rawId: encodeBase64Url(credential.rawId),
    type: credential.type,
    response: {
      attestationObject: encodeBase64Url(response.attestationObject),
      clientDataJSON: encodeBase64Url(response.clientDataJSON),
      transports: typeof response.getTransports === 'function' ? response.getTransports() : [],
    },
    clientExtensionResults: credential.getClientExtensionResults(),
  });
}

export async function createPasskeyAssertion(optionsJson: string): Promise<string> {
  if (!window.PublicKeyCredential || !navigator.credentials) {
    throw new Error('This browser does not support WebAuthn passkeys.');
  }

  const credential = await navigator.credentials.get({ publicKey: requestOptions(optionsJson) });
  if (!(credential instanceof PublicKeyCredential)
      || !(credential.response instanceof AuthenticatorAssertionResponse)) {
    throw new Error('Passkey authentication did not return a WebAuthn assertion.');
  }

  const response = credential.response;
  return JSON.stringify({
    id: credential.id,
    rawId: encodeBase64Url(credential.rawId),
    type: credential.type,
    response: {
      authenticatorData: encodeBase64Url(response.authenticatorData),
      signature: encodeBase64Url(response.signature),
      clientDataJSON: encodeBase64Url(response.clientDataJSON),
      userHandle: response.userHandle ? encodeBase64Url(response.userHandle) : null,
    },
    clientExtensionResults: credential.getClientExtensionResults(),
  });
}
