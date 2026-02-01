# 应用部署说明

目录用途
- 本目录仅用于应用服务（API/Web/Nginx）
- 与基础环境分离部署在独立服务器

启动
- 复制并修改 .env.apps
- 在本目录执行：
  docker compose --env-file .env.apps up -d

配置要点
- 使用基础环境内网 IP 更新 API 配置：
  - api/Configurations/ConnectionStrings.json（数据库/Redis）
  - api/Configurations/AI.json（Qdrant/TEI/vLLM）
- 上述配置通过卷挂载到 /app/Configurations
