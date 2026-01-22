# 实验室数据分析系统 - Docker 部署指南

## 快速开始

### 一键部署（推荐）

```bash
# 1. 构建所有镜像
./scripts/build.sh

# 2. 启动所有服务
docker-compose up -d
```

### 仅启动基础设施

```bash
docker-compose up -d mysql redis qdrant
```

### 仅启动应用服务

```bash
docker-compose up -d api web
```

---

## 目录结构

```
lm/
├── scripts/
│   ├── build.sh              # 一键构建脚本（前后端）
│   ├── build-api.sh          # 后端构建脚本
│   └── build-web.sh          # 前端构建脚本
├── docker-compose.yml        # 统一的服务编排
├── .env                      # 环境变量配置
├── api/
│   └── Dockerfile.build      # 后端 Dockerfile
└── web/
    └── Dockerfile.build      # 前端 Dockerfile
```

---

## 构建脚本

### build.sh - 一键构建所有镜像

```bash
# 构建所有镜像（默认）
./scripts/build.sh

# 仅构建 API 镜像
./scripts/build.sh --api

# 仅构建 Web 镜像
./scripts/build.sh --web

# 无缓存构建
./scripts/build.sh --no-cache
```

### build-api.sh - 构建后端镜像

```bash
# 快速构建（增量编译，默认）
./scripts/build-api.sh

# 完整构建（包括依赖恢复）
./scripts/build-api.sh --full

# 清理缓存
./scripts/build-api.sh --clean-cache
```

### build-web.sh - 构建前端镜像

```bash
# 本地构建（需要 Node.js + pnpm）
./scripts/build-web.sh

# Docker 构建（无需本地环境）
./scripts/build-web.sh --docker
```

---

## Docker Compose 命令

### 启动服务

```bash
# 启动所有服务
docker-compose up -d

# 启动特定服务
docker-compose up -d mysql redis

# 查看服务状态
docker-compose ps
```

### 停止服务

```bash
# 停止所有服务
docker-compose down

# 停止特定服务
docker-compose stop api web

# 停止并删除数据卷（谨慎）
docker-compose down -v
```

### 查看日志

```bash
# 查看所有日志
docker-compose logs -f

# 查看特定服务日志
docker-compose logs -f api
docker-compose logs -f web
docker-compose logs -f mysql

# 查看最近 100 行
docker-compose logs --tail=100 api
```

### 重启服务

```bash
# 重启所有服务
docker-compose restart

# 重启特定服务
docker-compose restart api
docker-compose restart web
```

---

## 服务说明

### 基础设施服务

| 服务 | 容器名 | 端口 | 说明 |
|------|--------|------|------|
| MySQL | lm-mysql | 3307 | 数据库 |
| Redis | lm-redis | 6380 | 缓存 |
| Qdrant | lm-qdrant | 6333, 6334 | 向量数据库 |
| TEI | lm-tei | 8081 | 文本嵌入（AI） |
| vLLM | lm-vllm | 8082 | 大模型推理（AI） |

### 应用服务

| 服务 | 容器名 | 端口 | 说明 |
|------|--------|------|------|
| API | lm-api | 9530 | 后端 API |
| Web | lm-web | 80 | 前端界面 |

---

## 配置说明

### 环境变量 (.env)

```bash
# 部署数据目录
DEPLOY_DIR=./deploy

# MySQL 配置
MYSQL_VERSION=8.0
MYSQL_PORT=3307
MYSQL_DATABASE=lumei
MYSQL_USER=lumei
MYSQL_PASSWORD=your_password

# Redis 配置
REDIS_VERSION=8.0-alpine
REDIS_PORT=6380
REDIS_PASSWORD=your_password

# API 配置
API_PORT=9530

# Web 配置
WEB_PORT=80

# Qdrant 配置
QDRANT_VERSION=v1.12.1
QDRANT_HTTP_PORT=6333

# 网络配置
NETWORK_NAME=lm-network
CONTAINER_PREFIX=lm
```

---

## 常见问题

### 端口冲突

如果默认端口被占用，修改 `.env` 文件：

```bash
MYSQL_PORT=3308
REDIS_PORT=6381
API_PORT=9531
WEB_PORT=8080
```

### 数据持久化

数据存储在 `./deploy` 目录：

```
./deploy/
├── mysql/data/         # MySQL 数据
├── redis/data/         # Redis 数据
├── qdrant/storage/     # Qdrant 向量存储
├── api/logs/           # API 日志
└── api/uploads/        # 上传文件
```

### AI 服务启动

AI 服务（TEI、vLLM）需要 GPU，使用 profile 启动：

```bash
docker-compose --profile ai up -d tei vllm
```

---

## 完整部署流程

### 首次部署

```bash
# 1. 复制并配置环境变量
cp .env .env.local
vi .env.local

# 2. 构建镜像
./scripts/build.sh

# 3. 启动基础设施
docker-compose up -d mysql redis qdrant

# 4. 等待数据库就绪（约 30 秒）
docker-compose logs -f mysql

# 5. 启动应用服务
docker-compose up -d api web

# 6. 查看服务状态
docker-compose ps
```

### 更新部署

```bash
# 1. 拉取最新代码
git pull

# 2. 重新构建镜像
./scripts/build.sh --no-cache

# 3. 重启服务
docker-compose up -d api web
```

---

## 访问地址

- 前端: http://localhost
- 后端 API: http://localhost:9530
- MySQL: localhost:3307
- Redis: localhost:6380
- Qdrant Dashboard: http://localhost:6333/dashboard

---

## 健康检查

```bash
# 检查 API 健康状态
curl http://localhost:9530/health

# 检查 Web 健康状态
curl http://localhost/health

# 查看容器健康状态
docker-compose ps
```

---

## 监控和调试

### 查看资源使用

```bash
docker stats
```

### 进入容器

```bash
# 进入 API 容器
docker exec -it lm-api bash

# 进入 Web 容器
docker exec -it lm-web sh

# 进入 MySQL 容器
docker exec -it lm-mysql mysql -u lumei -p
```

### 查看日志

```bash
# API 日志
docker-compose logs -f api

# Web 日志（Nginx 访问日志）
docker exec lm-web cat /var/log/nginx/access.log

# MySQL 日志
docker-compose logs -f mysql
```

---

## 清理

### 停止并删除容器

```bash
docker-compose down
```

### 删除数据（谨慎）

```bash
# 删除容器和数据卷
docker-compose down -v

# 删除部署目录
rm -rf ./deploy
```

### 删除镜像

```bash
docker rmi lm-api:latest lm-web:latest
```

---

## 生产环境建议

1. **使用外部数据库**：生产环境建议使用云数据库
2. **配置 SSL**：使用 Nginx 反向代理并配置 HTTPS
3. **资源限制**：在 docker-compose.yml 中配置 CPU 和内存限制
4. **日志管理**：配置日志轮转和集中日志收集
5. **监控告警**：集成 Prometheus + Grafana
6. **备份策略**：定期备份数据库和重要数据
7. **安全加固**：
   - 修改默认密码
   - 限制容器权限
   - 使用 secrets 管理敏感信息
8. **高可用部署**：使用 Docker Swarm 或 Kubernetes

---

## 技术栈

### 后端
- .NET 10.0
- SqlSugar ORM
- MySQL 8.0
- Redis 8.0
- Qdrant 向量数据库

### 前端
- Vue 3.3.4
- Vite 4.4.9
- Ant Design Vue 3.2.20
- Nginx

### AI 服务
- Qdrant（向量数据库）
- TEI（文本嵌入）
- vLLM（大模型推理）
