import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  reactStrictMode: true,
  // 输出为 standalone 模式，便于 Docker 部署
  output: "standalone",
  // 配置外部包（Canvas 图表库需要）
  transpilePackages: ["@ant-design/charts"],
  // 环境变量
  env: {
    AGENT_API_URL: process.env.AGENT_API_URL || "http://localhost:8000",
  },
  // 图片域名配置（如有需要）
  images: {
    remotePatterns: [],
  },
};

export default nextConfig;
