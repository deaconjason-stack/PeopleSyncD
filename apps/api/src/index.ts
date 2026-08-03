import { readConfig } from "./config";
import { buildServer } from "./server";

async function main(): Promise<void> {
  const config = readConfig();
  const app = buildServer(config);
  await app.listen({ host: config.host, port: config.port });
  console.log(`PeopleSyncD API listening on http://${config.host}:${config.port}`);
}

main().catch((error: unknown) => {
  console.error(error);
  process.exit(1);
});
