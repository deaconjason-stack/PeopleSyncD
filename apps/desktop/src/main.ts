import { app, BrowserWindow, shell } from "electron";
import path from "node:path";

const DEVELOPMENT_URL = process.env.PEOPLESYNCD_WEB_URL;

function trustedNavigation(url: string): boolean {
  return url.startsWith("file://") || url.startsWith("http://127.0.0.1:5173") || url.startsWith("http://localhost:5173");
}

async function createWindow(): Promise<void> {
  const window = new BrowserWindow({
    width: 1440,
    height: 940,
    minWidth: 1024,
    minHeight: 720,
    backgroundColor: "#07101e",
    show: false,
    webPreferences: {
      preload: path.join(__dirname, "preload.js"),
      contextIsolation: true,
      sandbox: true,
      nodeIntegration: false,
      webSecurity: true
    }
  });

  window.webContents.setWindowOpenHandler(({ url }) => {
    if (url.startsWith("https://")) void shell.openExternal(url);
    return { action: "deny" };
  });

  window.webContents.on("will-navigate", (event, url) => {
    if (!trustedNavigation(url)) event.preventDefault();
  });

  window.once("ready-to-show", () => window.show());

  if (DEVELOPMENT_URL) {
    await window.loadURL(DEVELOPMENT_URL);
    window.webContents.openDevTools({ mode: "detach" });
  } else {
    await window.loadFile(path.join(process.resourcesPath, "web", "index.html"));
  }
}

app.whenReady().then(async () => {
  await createWindow();
  app.on("activate", async () => {
    if (BrowserWindow.getAllWindows().length === 0) await createWindow();
  });
});

app.on("window-all-closed", () => {
  if (process.platform !== "darwin") app.quit();
});
