import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "NLQ-Agent - 工业质量数据智能查询",
  description: "基于智能体的工业质量数据自然语言问数系统",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="zh-CN" suppressHydrationWarning>
      <body className="antialiased">{children}</body>
    </html>
  );
}
