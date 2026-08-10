export type StorageMode = "memory" | "postgres";
export type DatabaseRoleMode = "assume" | "direct";

export interface ApiConfig {
  host: string;
  port: number;
  nodeEnv: string;
  sessionSecret: string;
  mfaEncryptionKey: string;
  devAuthEnabled: boolean;
  corsOrigin: string;
  storageMode: StorageMode;
  databaseRoleMode: DatabaseRoleMode;
  databaseUrl?: string;
}

export function readConfig(env: NodeJS.ProcessEnv = process.env): ApiConfig {
  const nodeEnv = env.NODE_ENV ?? "development";
  const sessionSecret = env.PEOPLESYNCD_SESSION_SECRET ?? "genesis-development-secret-change-me";
  if (nodeEnv === "production" && !env.PEOPLESYNCD_SESSION_SECRET) {
    throw new Error("PEOPLESYNCD_SESSION_SECRET is required in production");
  }
  if (sessionSecret.length < 32) {
    throw new Error("PEOPLESYNCD_SESSION_SECRET must contain at least 32 characters");
  }

  const mfaEncryptionKey = env.PEOPLESYNCD_MFA_ENCRYPTION_KEY ?? sessionSecret;
  if (nodeEnv === "production" && !env.PEOPLESYNCD_MFA_ENCRYPTION_KEY) {
    throw new Error("PEOPLESYNCD_MFA_ENCRYPTION_KEY is required in production");
  }
  if (mfaEncryptionKey.length < 32) {
    throw new Error("PEOPLESYNCD_MFA_ENCRYPTION_KEY must contain at least 32 characters");
  }

  const runtimeDatabaseUrl = env.PEOPLESYNCD_RUNTIME_DATABASE_URL;
  const databaseUrl = runtimeDatabaseUrl ?? env.PEOPLESYNCD_DATABASE_URL ?? env.DATABASE_URL;
  const requestedStorage = env.PEOPLESYNCD_STORAGE ?? (databaseUrl ? "postgres" : "memory");
  if (requestedStorage !== "memory" && requestedStorage !== "postgres") {
    throw new Error("PEOPLESYNCD_STORAGE must be memory or postgres");
  }
  if (requestedStorage === "postgres" && !databaseUrl) {
    throw new Error("A PeopleSyncD PostgreSQL database URL is required for PostgreSQL storage");
  }
  if (nodeEnv === "production" && requestedStorage !== "postgres") {
    throw new Error("PostgreSQL storage is required in production");
  }
  if (nodeEnv === "production" && !runtimeDatabaseUrl) {
    throw new Error("PEOPLESYNCD_RUNTIME_DATABASE_URL is required in production");
  }

  const requestedRoleMode = env.PEOPLESYNCD_DATABASE_ROLE_MODE ?? (runtimeDatabaseUrl ? "direct" : "assume");
  if (requestedRoleMode !== "assume" && requestedRoleMode !== "direct") {
    throw new Error("PEOPLESYNCD_DATABASE_ROLE_MODE must be assume or direct");
  }
  if (nodeEnv === "production" && requestedRoleMode !== "direct") {
    throw new Error("Direct runtime database identity is required in production");
  }

  return {
    host: env.PEOPLESYNCD_API_HOST ?? "127.0.0.1",
    port: Number(env.PEOPLESYNCD_API_PORT ?? 8080),
    nodeEnv,
    sessionSecret,
    mfaEncryptionKey,
    devAuthEnabled: nodeEnv !== "production" && env.PEOPLESYNCD_DEV_AUTH !== "false",
    corsOrigin: env.PEOPLESYNCD_CORS_ORIGIN ?? "http://localhost:5173",
    storageMode: requestedStorage,
    databaseRoleMode: requestedRoleMode,
    databaseUrl
  };
}
