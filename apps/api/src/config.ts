export interface ApiConfig {
  host: string;
  port: number;
  nodeEnv: string;
  sessionSecret: string;
  devAuthEnabled: boolean;
  corsOrigin: string;
}

export function readConfig(env: NodeJS.ProcessEnv = process.env): ApiConfig {
  const nodeEnv = env.NODE_ENV ?? "development";
  const sessionSecret = env.PEOPLESYNCD_SESSION_SECRET ?? "genesis-development-secret-change-me";
  if (nodeEnv === "production" && !env.PEOPLESYNCD_SESSION_SECRET) {
    throw new Error("PEOPLESYNCD_SESSION_SECRET is required in production");
  }
  return {
    host: env.PEOPLESYNCD_API_HOST ?? "127.0.0.1",
    port: Number(env.PEOPLESYNCD_API_PORT ?? 8080),
    nodeEnv,
    sessionSecret,
    devAuthEnabled: nodeEnv !== "production" && env.PEOPLESYNCD_DEV_AUTH !== "false",
    corsOrigin: env.PEOPLESYNCD_CORS_ORIGIN ?? "http://localhost:5173"
  };
}
