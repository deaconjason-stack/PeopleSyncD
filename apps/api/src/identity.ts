import type { ApiConfig } from "./config";
import type { IdentityStore } from "./identity-contract";
import { InMemoryIdentityStore } from "./identity-memory";
import { PostgresIdentityStore } from "./identity-postgres";

export type { CreateSessionInput, IdentityStore } from "./identity-contract";
export { InMemoryIdentityStore } from "./identity-memory";
export { PostgresIdentityStore } from "./identity-postgres";

export function createIdentityStore(
  config: Pick<ApiConfig, "storageMode" | "databaseUrl" | "mfaEncryptionKey" | "databaseRoleMode">
): IdentityStore {
  if (config.storageMode === "postgres") {
    if (!config.databaseUrl) throw new Error("PostgreSQL identity storage requires a database URL");
    return new PostgresIdentityStore(
      config.databaseUrl,
      config.mfaEncryptionKey,
      config.databaseRoleMode === "assume"
    );
  }
  return new InMemoryIdentityStore(config.mfaEncryptionKey);
}
