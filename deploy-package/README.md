# 部署包说明（Linux 应用服务器）

该目录由 `scripts/publish-all.ps1` 生成，用于交付部署。
不包含基础设施（MySQL/Redis/Qdrant/TEI/vLLM）。

包含内容
- `apps/`（应用 compose 与 env 模板）
- `publish/api/`（后端发布产物）
- `publish/web/`（前端 dist 产物）
- `start-apps.sh`（Linux 一键启动前后端服务）

使用方式（Linux）
1) 编辑 `apps/.env.apps`（INFRA_HOST、端口）
2) 将 API 配置放到 `deploy/api/Configurations/`
3) 运行：`bash start-apps.sh`

说明
- 需要 Docker 与 Docker Compose（compose v2）
- 基础设施在另一台服务器部署
