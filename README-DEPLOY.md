# 部署使用说明（基础环境与应用分离）

适用场景
- 基础环境与应用服务部署在不同服务器
- 内网通信，AI 服务必须部署
- 前后端同机（同一应用服务器）

目录结构
- 基础环境：`infra/`
- 应用服务：`apps/`

一、基础环境服务器（infra）
1. 进入目录
   - `cd infra`
2. 复制并修改环境变量
   - `copy .env.infra .env.infra.local`（或直接编辑 .env.infra）
   - 重点：数据库/Redis/AI 端口、模型路径
3. 启动基础设施
   - `docker compose --env-file .env.infra up -d`

包含服务
- MySQL、Redis、Qdrant、TEI、vLLM

二、应用服务器（apps）
1. 进入目录
   - `cd apps`
2. 复制并修改环境变量
   - `copy .env.apps .env.apps.local`（或直接编辑 .env.apps）
   - 重点：`INFRA_HOST`（基础环境服务器内网 IP）
3. 启动应用服务
   - `docker compose --env-file .env.apps up -d`

包含服务
- API、Web、Nginx

三、API 配置（应用服务器）
以下配置通过卷挂载到容器：
- `deploy/api/Configurations/ConnectionStrings.json`
- `deploy/api/Configurations/AI.json`

请将上述配置中的基础环境地址修改为内网地址：
- 数据库、Redis 使用 `INFRA_HOST` 对应的内网 IP
- AI（Qdrant/TEI/vLLM）同样使用内网 IP

四、常见问题
- 内网 IP 每台机器不同：只需修改该机器的 `apps/.env.apps` 与 API 配置地址
- 不启用 RabbitMQ：当前 compose 未包含 RabbitMQ，如需请在 `infra/docker-compose.yml` 增加
- GPU：vLLM 需要 GPU；如无 GPU，请调整为 CPU 或部署到具备 GPU 的基础环境服务器
