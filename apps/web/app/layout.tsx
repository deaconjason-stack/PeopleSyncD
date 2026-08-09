import type { Metadata } from "next";
import "./globals.css";
import "./page.css";

export const metadata: Metadata = {
  title: "PeopleSyncD Enterprise Platform",
  description: "The operating platform for people, organizations, knowledge, and enterprise intelligence.",
};

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="en">
      <body>
        <header className="psd-app-header">
          <a href="/" className="psd-brand" aria-label="PeopleSyncD home">PeopleSyncD</a>
          <nav aria-label="Primary navigation" className="psd-nav">
            <a href="/people">People</a>
            <a href="/login">Sign in</a>
          </nav>
        </header>
        {children}
      </body>
    </html>
  );
}
