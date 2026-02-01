# 应用部署说明

## 目录用途

本目录仅用于应用服务（API/Web/Nginx），与基础环境分离部署在独立服务器。

## 快速开始

### 开发环境部署

```bash
# 1. 配置环境变量
cp .env.example .env
# 编辑 .env 文件，配置 INFRA_HOST 等参数

# 2. 启动服务
./deploy.sh
```

### 生产环境打包

```bash
# 1. 构建前后端（在项目根目录）
# API
cd .. && ./scripts/build-api.sh

# Web
cd .. && ./scripts/build-web.sh

# 2. 打包应用（在 apps 目录）
cd apps
./package.sh

# 3. 打包为 tar（在 apps 目录）
tar czf app-package-$(cat ../VERSION).tar.gz -C dist .
```

## 配置要点

### 1. 环境变量 (.env)

| 配置项 | 说明 | 默认值 |
|--------|------|--------|
| `INFRA_HOST` | 基础环境服务器内网 IP | 10.0.0.5 |
| `API_PORT` | API 服务端口 | 9530 |
| `NGINX_PORT` | Nginx 访问端口 | 8923 |

### 2. API 配置文件

将以下文件放到 `deploy/api/Configurations/`：

- **ConnectionStrings.json** - 数据库、Redis 连接
- **AI.json** - AI 服务地址（Qdrant/TEI/vLLM）

上述配置通过卷挂载到容器的 `/app/Configurations` 目录。

## 文件说明

| 文件 | 说明 |
|------|------|
| `docker-compose.yml` | Docker Compose 配置 |
| `.env.example` | 环境变量模板 |
| `deploy.sh` | 部署脚本（带检查） |
| `package.sh` | 打包脚本（生成部署包） |
| `README.md` | 本文件 |

## 常用命令

```bash
# 启动服务
docker compose up -d

# 查看日志
docker compose logs -f api

# 停止服务
docker compose down

# 重启服务
docker compose restart

# 查看服务状态
docker compose ps
```

## 访问地址

- 前端: `http://<服务器IP>:8923`
- API: `http://<服务器IP>:9530`
