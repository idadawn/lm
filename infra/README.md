# 基础环境部署说明

目录用途
- 本目录仅用于基础设施服务（MySQL/Redis/Qdrant/TEI/vLLM）
- 与应用服务分离部署在独立服务器

启动
- 复制并修改 .env.infra
- 在本目录执行：
  docker compose --env-file .env.infra up -d

注意
- 仅内网访问，不对公网暴露端口
- 如需增加 RabbitMQ，请在此 compose 中追加服务
