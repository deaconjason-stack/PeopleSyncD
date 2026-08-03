export type StorageMode = "memory" | "postgres";

export interface ApiConfig {
  host: string;
  port: number;
  nodeEnv: string;
  sessionSecret: string;
  devAuthEnabled: boolean;
  corsOrigin: string;
  storageMode: StorageMode;
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

  const databaseUrl = env.PEOPLESYNCD_DATABASE_URL ?? env.DATABASE_URL;
  const requestedStorage = env.PEOPLESYNCD_STORAGE ?? (databaseUrl ? "postgres" : "memory");
  if (requestedStorage !== "memory" && requestedStorage !== "postgres") {
    throw new Error("PEOPLESYNCD_STORAGE must be memory or postgres");
  }
  if (requestedStorage === "postgres" && !databaseUrl) {
    throw new Error("PEOPLESYNCD_DATABASE_URL is required for PostgreSQL storage");
  }
  if (nodeEnv === "production" && requestedStorage !== "postgres") {
    throw new Error("PostgreSQL storage is required in production");
  }

  return {
    host: env.PEOPLESYNCD_API_HOST ?? "127.0.0.1",
    port: Number(env.PEOPLESYNCD_API_PORT ?? 8080),
    nodeEnv,
    sessionSecret,
    devAuthEnabled: nodeEnv !== "production" && env.PEOPLESYNCD_DEV_AUTH !== "false",
    corsOrigin: env.PEOPLESYNCD_CORS_ORIGIN ?? "http://localhost:5173",
    storageMode: requestedStorage,
    databaseUrl
  };
}
