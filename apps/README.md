# 应用部署目录

此目录包含应用服务的部署配置，适用于**应用服务器**（前后端同机部署）。

## 目录结构

```
apps/
├── build-all.ps1       # Windows 统一构建脚本
├── docker-compose.yml  # Docker Compose 配置
├── .env.example        # 环境变量模板
├── README.md           # 本文件
└── deploy/             # 部署数据目录（运行后生成）
    ├── api/            # API 数据
    ├── nginx/          # Nginx 配置
    └── web/            # Web 静态文件
```

## 快速开始

### 1. 环境准备

```powershell
# 复制环境变量模板
copy .env.example .env

# 编辑 .env，配置基础环境地址
notepad .env
```

### 2. 构建和部署

#### Windows 本机构建

```powershell
# 进入 apps 目录
cd apps

# 构建所有（后端 + 前端 + Docker 镜像）
.\build-all.ps1

# 只构建后端
.\build-all.ps1 -SkipWeb

# 只构建前端
.\build-all.ps1 -SkipApi

# 构建并推送到镜像仓库
.\build-all.ps1 -Push -Registry "registry.cn-hangzhou.aliyuncs.com/your-namespace"
```

#### 启动服务

```powershell
# 启动所有服务
docker-compose up -d

# 查看日志
docker-compose logs -f

# 停止服务
docker-compose down
```

### 3. 访问应用

- 应用入口: http://localhost:8923
- API 地址: http://localhost:9530

## 环境变量说明

| 变量名 | 默认值 | 说明 |
|--------|--------|------|
| `DEPLOY_DIR` | ./deploy | 部署数据目录 |
| `API_PORT` | 9530 | API 服务端口 |
| `NGINX_PORT` | 8923 | Nginx 对外端口 |
| `CONTAINER_PREFIX` | lm | 容器名称前缀 |
| `INFRA_HOST` | - | 基础环境服务器 IP |
| `INFRA_MYSQL_PORT` | 3307 | MySQL 端口 |
| `INFRA_REDIS_PORT` | 6380 | Redis 端口 |
| `INFRA_QDRANT_PORT` | 6333 | Qdrant 端口 |

## 前置条件

**基础环境必须已启动**（在另一台服务器或本机）：
- MySQL 8.0
- Redis 7.0+
- Qdrant (向量数据库，用于 AI 功能)

## 常用命令

```powershell
# 重新构建并启动
docker-compose up -d --build

# 查看 API 日志
docker-compose logs -f api

# 重启 API 服务
docker-compose restart api

# 进入 API 容器
docker-compose exec api bash

# 清理所有数据（危险！）
docker-compose down -v
```

## 网络说明

- `lm-apps-network`: 应用服务内部网络
- `lm-infra-network`: 外部网络（连接基础环境），需要预先创建

创建基础环境网络：
```bash
docker network create lm-infra-network
```
