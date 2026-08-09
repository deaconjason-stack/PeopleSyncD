import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "PeopleSyncD Enterprise Platform",
  description: "The operating platform for people, organizations, knowledge, and enterprise intelligence.",
};

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  );
}
