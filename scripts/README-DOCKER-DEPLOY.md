# API Docker 部署指南

## 目录结构

```
lm/
├── scripts/
│   ├── build-api.sh           # 本地构建脚本（发布到 publish/api，支持快速/完整构建）
│   └── deploy-api.sh          # Docker 一键部署脚本
├── api/
│   ├── Dockerfile.build       # API Dockerfile
│   ├── resources/             # 资源文件（必需）
│   └── src/application/Poxiao.API.Entry/
│       └── Configurations/    # 配置文件
├── docker-compose.infrastructure.yml  # 基础设施服务
├── docker-compose.app.yml             # API 服务
├── .dockerignore             # Docker 构建忽略文件
└── .env                      # 环境变量配置
```

## 快速开始

### 一键部署（推荐）

```bash
./scripts/deploy-api.sh full
```

这将自动完成：
1. 检查依赖（.NET SDK、Docker）
2. 构建 API 并发布到 `publish/api`
3. 构建 Docker 镜像
4. 启动基础设施服务（MySQL、Redis、Qdrant 等）
5. 启动 API 服务
6. 显示服务状态

### 分步部署

#### 1. 构建 API

```bash
# 快速构建（推荐，日常开发）- 增量编译，跳过依赖恢复
./scripts/build-api.sh

# 或
./scripts/build-api.sh --fast

# 完整构建（首次构建或依赖变化时）- 包含依赖恢复
./scripts/build-api.sh --full

# 清理构建缓存
./scripts/build-api.sh --clean-cache
```

此脚本会：
- 编译发布 API 到 `publish/api/`
- 复制 `resources` 目录
- 复制 `Configurations` 目录
- 支持增量编译和 NuGet 包缓存

#### 2. 构建 Docker 镜像

```bash
docker build -t lm-api:latest -f api/Dockerfile.build .
```

#### 3. 启动服务

```bash
# 启动基础设施
docker-compose -f docker-compose.infrastructure.yml up -d

# 启动 API
docker-compose -f docker-compose.app.yml up -d
```

## 常用命令

### 部署脚本命令

```bash
# 完整部署
./scripts/deploy-api.sh full

# 仅构建（API + 镜像）
./scripts/deploy-api.sh build

# 仅启动服务
./scripts/deploy-api.sh start

# 停止所有服务
./scripts/deploy-api.sh stop

# 重启 API 服务
./scripts/deploy-api.sh restart

# 查看日志
./scripts/deploy-api.sh logs

# 查看状态
./scripts/deploy-api.sh status
```

### Docker Compose 命令

```bash
# 基础设施服务
docker-compose -f docker-compose.infrastructure.yml up -d
docker-compose -f docker-compose.infrastructure.yml down
docker-compose -f docker-compose.infrastructure.yml ps

# API 服务
docker-compose -f docker-compose.app.yml up -d
docker-compose -f docker-compose.app.yml down
docker-compose -f docker-compose.app.yml ps
docker-compose -f docker-compose.app.yml logs -f api
docker-compose -f docker-compose.app.yml restart api
```

### Docker 命令

```bash
# 查看容器
docker ps

# 查看日志
docker logs -f lm-api

# 进入容器
docker exec -it lm-api bash

# 重启容器
docker restart lm-api
```

## 配置说明

### 环境变量 (.env)

```bash
# 部署数据目录
DEPLOY_DIR=./deploy

# 容器前缀（用于容器命名）
CONTAINER_PREFIX=lm

# 网络名称
NETWORK_NAME=lm-network

# API 端口
API_PORT=9530

# MySQL 配置
MYSQL_PORT=3306
MYSQL_DATABASE=poxiao_lab
MYSQL_USER=poxiao
MYSQL_PASSWORD=poxiao123

# Redis 配置
REDIS_PORT=6379
REDIS_PASSWORD=redis123456

# Qdrant 配置
QDRANT_HTTP_PORT=6333

# AI 服务配置
TEI_PORT=8081
VLLM_PORT=8082
```

### 生产环境配置

配置文件位于：`api/src/application/Poxiao.API.Entry/Configurations/`

#### ConnectionStrings.production.json

数据库连接配置，使用 Docker 服务名：

```json
{
  "ConnectionStrings": {
    "ConnectionConfigs": [
      {
        "Host": "lm-mysql",
        "Port": "3306",
        "UserName": "poxiao",
        "Password": "poxiao123"
      }
    ]
  }
}
```

#### Cache.production.json

Redis 缓存配置：

```json
{
  "Redis": {
    "Connection": "lm-redis:6379,password=redis123456"
  }
}
```

#### AI.json

AI 服务配置：

```json
{
  "AI": {
    "Chat": {
      "Endpoint": "http://lm-vllm:8082/v1"
    },
    "Embedding": {
      "Endpoint": "http://lm-tei:80"
    }
  },
  "VectorDB": {
    "Qdrant": {
      "Endpoint": "http://lm-qdrant:6333"
    }
  }
}
```

#### App.production.json

应用配置，注意 `SystemPath` 必须指向 `/app/resources`：

```json
{
  "Poxiao_App": {
    "SystemPath": "/app/resources",
    ...
  }
}
```

## 容器说明

### 基础设施服务

| 容器名 | 服务 | 端口 |
|--------|------|------|
| lm-mysql | MySQL | 3306 |
| lm-redis | Redis | 6379 |
| lm-qdrant | Qdrant | 6333, 6334 |
| lm-tei | TEI 推理 | 8081 |
| lm-vllm | vLLM 推理 | 8082 |

### API 服务

| 容器名 | 服务 | 端口 |
|--------|------|------|
| lm-api | API | 9530 |

## 数据持久化

数据存储在 `./deploy` 目录：

```
./deploy/
├── mysql_data/        # MySQL 数据
├── redis_data/        # Redis 数据
├── qdrant_storage/    # Qdrant 向量存储
├── api/
│   ├── logs/          # API 日志
│   └── uploads/       # 上传文件
└── mysql_init/        # MySQL 初始化脚本
```

## 日志查看

### API 日志

```bash
# Docker 日志
docker logs -f lm-api

# Compose 日志
docker-compose -f docker-compose.app.yml logs -f api

# 应用日志（挂载到宿主机）
tail -f ./deploy/api/logs/*.log
```

### 基础设施日志

```bash
# MySQL
docker logs -f lm-mysql

# Redis
docker logs -f lm-redis

# Qdrant
docker logs -f lm-qdrant
```

## 故障排查

### 服务启动失败

1. 查看容器状态：
```bash
docker ps -a
```

2. 查看日志：
```bash
docker logs lm-api
```

3. 检查网络：
```bash
docker network ls
docker network inspect lm-network
```

### 数据库连接失败

确认 MySQL 容器运行正常：
```bash
docker logs lm-mysql
docker exec -it lm-mysql mysql -u poxiao -p
```

### Redis 连接失败

确认 Redis 容器运行：
```bash
docker logs lm-redis
docker exec -it lm-redis redis-cli -a redis123456 ping
```

### AI 服务连接失败

确认 AI 服务运行：
```bash
docker logs lm-vllm
docker logs lm-tei
docker logs lm-qdrant
```

## 更新部署

### 更新 API 代码

```bash
# 拉取最新代码
git pull

# 重新构建并部署
./scripts/deploy-api.sh full
```

### 仅重启 API

```bash
docker-compose -f docker-compose.app.yml restart api
```

## 清理

### 停止并删除容器

```bash
# 停止 API
docker-compose -f docker-compose.app.yml down

# 停止基础设施
docker-compose -f docker-compose.infrastructure.yml down

# 停止所有
docker-compose -f docker-compose.app.yml down
docker-compose -f docker-compose.infrastructure.yml down
```

### 删除数据（谨慎）

```bash
# 删除容器和数据
rm -rf ./deploy
```

### 删除 Docker 镜像

```bash
docker rmi lm-api:latest
docker image prune
```

## 健康检查

API 服务包含健康检查：

```bash
# 检查健康状态
docker inspect lm-api | grep -A 10 Health

# 手动检查
curl http://localhost:9530/health
```

## 性能监控

```bash
# 查看资源使用
docker stats

# 查看特定容器
docker stats lm-api lm-mysql lm-redis
```

## 生产环境建议

1. **使用外部数据库**：生产环境建议使用外部 MySQL/Redis
2. **配置 SSL**：使用 Nginx 反向代理并配置 HTTPS
3. **日志管理**：配置日志轮转和集中日志收集
4. **监控告警**：集成 Prometheus + Grafana
5. **备份策略**：定期备份数据库和重要数据
6. **资源限制**：在 docker-compose 中配置资源限制
