import { contextBridge } from "electron";

contextBridge.exposeInMainWorld("peopleSyncDDesktop", Object.freeze({
  platform: process.platform,
  version: process.versions.electron,
  releaseChannel: "internal-alpha",
  signed: false
}));
