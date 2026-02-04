# 检测室数据分析系统 - 应用部署

实验室信息管理系统（LIMS），集成 AI 功能，支持自然语言查询（NL to SQL）和向量搜索。

## 📦 系统要求

- Docker 20.10+
- Docker Compose 2.0+
- Linux/Windows/MacOS

## 🚀 快速开始

### 1. 环境准备

```bash
# 克隆代码库
git clone <repository-url>
cd lm/apps

# 复制环境变量模板
cp .env.example .env

# 编辑配置（必须设置 INFRA_HOST）
vim .env
```

### 2. 一键部署

**注意**：首次部署前，确保前端已构建并打包：

```bash
# 如果需要构建前端（在 web 目录执行）
cd /home/dawn/project/lm/web && pnpm build && cd -
./deploy/pack-dist.sh      # 打包前端
```

正式部署：

```bash
# 构建并部署全部服务
./deploy/build-all.sh && docker compose up -d

# 或者分步执行
./deploy/build-all.sh      # 构建镜像
docker compose up -d        # 启动服务
```

### 3. 验证部署

```bash
# 查看服务状态
docker compose ps

# 查看日志
docker compose logs -f

# 访问应用
# 应用入口: http://localhost:8923
# API 地址: http://localhost:9530
```

## 📋 部署选项

### 构建选项

```bash
# 只构建后端
./deploy/build-all.sh -a

# 只构建前端（需确保 dist.zip 已存在）
./deploy/build-all.sh -w

# 指定版本
./deploy/build-all.sh -v 1.2.3

# 构建并自动重启容器
./deploy/build-all.sh -r
```

### 服务管理

```bash
# 停止服务
docker compose down

# 重启服务
docker compose restart

# 查看 API 日志
docker compose logs -f api

# 进入容器调试
docker compose exec api bash
```

## 🔧 配置说明

### 环境变量 (.env)

| 变量名 | 默认值 | 说明 |
|--------|--------|------|
| `INFRA_HOST` | - | **必填** 基础环境服务器 IP |
| `DEPLOY_DIR` | ./deploy | 部署数据目录 |
| `API_PORT` | 9530 | API 服务端口 |
| `NGINX_PORT` | 8923 | 应用访问端口 |
| `CONTAINER_PREFIX` | lm | 容器名称前缀 |

### 基础环境依赖

部署前请确保基础环境已就绪：

- **MySQL 8.0+** (端口: `${INFRA_MYSQL_PORT}`)
- **Redis 7.0+** (端口: `${INFRA_REDIS_PORT}`)
- **Qdrant** (端口: `${INFRA_QDRANT_PORT}`) - AI 向量数据库
- **TEI** (端口: `${INFRA_TEI_PORT}`) - 文本嵌入服务 *(可选)*
- **vLLM** (端口: `${INFRA_VLLM_PORT}`) - LLM 推理服务 *(可选)*

### 网络配置

```bash
# 创建基础环境网络（如不存在）
docker network create lm-infra-network
```

## 📁 目录结构

```
apps/
├── docker-compose.yml          # Docker 服务配置
├── .env.example               # 环境变量模板
├── deploy/                    # 部署数据目录
│   ├── api/                  # API 相关数据
│   │   ├── Configurations/   # 配置文件
│   │   ├── logs/            # 日志文件
│   │   ├── uploads/         # 上传文件
│   │   ├── wwwroot/         # 静态资源
│   │   ├── resources/       # 业务资源文件
│   │   └── lib/             # 依赖库
│   └── nginx/               # Nginx 配置
└── deploy/                   # 构建脚本目录
    ├── build-all.sh         # 统一构建脚本
    ├── build-api.sh         # API 构建脚本
    └── build-web.sh         # Web 构建脚本
```

## 🛠️ 开发环境

### 本地开发

```bash
# 启动基础设施
docker compose --profile infra up -d

# API 开发（需要 .NET 10.0）
cd api/src/application/Poxiao.API.Entry
dotnet watch run --launch-profile dev

# Web 开发（需要 Node.js 16+ & pnpm）
cd web
pnpm install
pnpm dev
```

### 生产环境构建

前端需要先构建再打包：

```bash
# 1. 构建前端（在项目 web 目录执行）
cd /home/dawn/project/lm/web
pnpm install
pnpm build

# 2. 打包 dist
./deploy/pack-dist.sh
```

### Mock 服务器

仅用于前端独立开发：

```bash
cd web/mock
npm install
npm run mock
```

## 🆘 常见问题

### 服务启动失败

1. 检查基础环境是否可达
   ```bash
   ping $INFRA_HOST
   telnet $INFRA_HOST $INFRA_MYSQL_PORT
   ```

2. 查看容器日志
   ```bash
   docker compose logs api
   docker compose logs nginx
   ```

3. 验证镜像构建
   ```bash
   docker images | grep lm
   ```

### 数据迁移

资源和库文件已统一在 `deploy/api/` 目录下，迁移时直接复制整个目录即可。

## 📞 支持

如需帮助，请查看：
- [项目文档](../README.md)
- [构建脚本帮助] `./deploy/build-all.sh -h`
- [Docker 日志] `docker compose logs -f`