# 前端 Docker 部署指南

## 目录结构

```
lm/
├── scripts/
│   ├── build-web.sh          # 前端构建脚本
│   └── deploy-web.sh         # 前端部署脚本
├── web/
│   ├── Dockerfile.build      # 前端 Dockerfile
│   ├── .dockerignore         # Docker 构建忽略文件
│   └── conf/
│       └── nginx.docker.conf # Nginx 配置
├── docker-compose.web.yml    # 前端服务编排
└── publish/web/              # 构建输出目录（自动生成）
```

## 快速开始

### 一键部署（推荐）

```bash
./scripts/deploy-web.sh full
```

这将自动完成：
1. 构建前端项目
2. 构建 Docker 镜像
3. 启动前端服务

### 分步部署

#### 1. 构建前端

```bash
# 本地构建（需要 Node.js 和 pnpm）
./scripts/build-web.sh

# 或使用 Docker 构建（无需本地环境）
./scripts/build-web.sh --docker
```

#### 2. 构建 Docker 镜像

```bash
docker build -t lm-web:latest -f web/Dockerfile.build .
```

#### 3. 启动服务

```bash
# 使用 docker-compose（推荐）
docker-compose -f docker-compose.web.yml up -d

# 或使用 docker run
docker run -d \
  --name lm-web \
  --network lm-network \
  -p 80:80 \
  --restart unless-stopped \
  lm-web:latest
```

## 常用命令

### 部署脚本命令

```bash
# 完整部署
./scripts/deploy-web.sh full

# 仅构建（前端 + 镜像）
./scripts/deploy-web.sh build

# 仅启动服务
./scripts/deploy-web.sh start

# 停止服务
./scripts/deploy-web.sh stop

# 重启服务
./scripts/deploy-web.sh restart

# 查看日志
./scripts/deploy-web.sh logs

# 查看状态
./scripts/deploy-web.sh status
```

### 构建脚本命令

```bash
# 本地构建（默认）
./scripts/build-web.sh

# 显式指定本地构建
./scripts/build-web.sh --local

# Docker 构建
./scripts/build-web.sh --docker

# 查看帮助
./scripts/build-web.sh --help
```

### Docker Compose 命令

```bash
# 启动服务
docker-compose -f docker-compose.web.yml up -d

# 停止服务
docker-compose -f docker-compose.web.yml down

# 查看日志
docker-compose -f docker-compose.web.yml logs -f web

# 查看状态
docker-compose -f docker-compose.web.yml ps

# 重启服务
docker-compose -f docker-compose.web.yml restart web
```

### Docker 命令

```bash
# 查看容器
docker ps | grep lm-web

# 查看日志
docker logs -f lm-web

# 进入容器
docker exec -it lm-web sh

# 重启容器
docker restart lm-web
```

## 配置说明

### 环境变量

在 `docker-compose.web.yml` 或启动命令中配置：

```bash
# 前端端口（默认 80）
WEB_PORT=80
```

### Nginx 配置

Nginx 配置文件位于：`web/conf/nginx.docker.conf`

主要配置项：
- 静态资源服务
- Gzip 压缩
- 缓存策略
- API 代理（可选）
- 安全头部

### API 代理配置

如需在容器内代理 API 请求，取消注释 `nginx.docker.conf` 中的以下配置：

```nginx
location /api/ {
    proxy_pass http://lm-api:9530/api/;
    # ... 其他配置
}
```

## 技术栈

- **构建工具**: Vite 4.4.9
- **框架**: Vue 3.3.4
- **包管理器**: pnpm 8.x
- **Web 服务器**: Nginx (stable-alpine)
- **Node.js**: 20-alpine

## Dockerfile 说明

### 多阶段构建

```dockerfile
阶段1: deps       - 安装依赖
阶段2: builder    - 构建应用
阶段3: production - 生产镜像
```

### 优化点

- ✅ 使用 pnpm 缓存加速依赖安装
- ✅ 多阶段构建减小镜像体积
- ✅ Alpine 基础镜像
- ✅ 非 root 用户运行
- ✅ 健康检查
- ✅ 国内 npm 镜像加速

## 日志查看

### 容器日志

```bash
# Docker 日志
docker logs -f lm-web

# Compose 日志
docker-compose -f docker-compose.web.yml logs -f web
```

### Nginx 访问日志

```bash
# 进入容器查看
docker exec -it lm-web sh
cat /var/log/nginx/access.log
cat /var/log/nginx/error.log
```

## 故障排查

### 服务启动失败

1. 查看容器状态：
```bash
docker ps -a | grep lm-web
```

2. 查看日志：
```bash
docker logs lm-web
```

3. 检查网络：
```bash
docker network ls
docker network inspect lm-network
```

### 端口冲突

检查端口占用：
```bash
sudo lsof -i :80
```

修改端口：
```bash
# 在 docker-compose.web.yml 中修改
WEB_PORT=8080
```

### 构建失败

1. 清理缓存：
```bash
pnpm clean:cache
```

2. 重新安装依赖：
```bash
pnpm reinstall
```

3. 使用 Docker 构建：
```bash
./scripts/build-web.sh --docker
```

## 性能优化

### 构建优化

- 使用多阶段构建
- 依赖层缓存
- 并行构建
- 国内镜像加速

### 运行时优化

- Nginx Gzip 压缩
- 静态资源缓存
- CDN 加速（配置 `VITE_CDN=true`）
- 图片压缩

### 资源限制

在 `docker-compose.web.yml` 中配置：

```yaml
deploy:
  resources:
    limits:
      cpus: '0.5'
      memory: 512M
```

## 更新部署

### 更新前端代码

```bash
# 拉取最新代码
git pull

# 重新构建并部署
./scripts/deploy-web.sh full
```

### 仅重启服务

```bash
docker-compose -f docker-compose.web.yml restart web
```

## 监控和健康检查

### 健康检查端点

```bash
curl http://localhost/health
```

### 查看资源使用

```bash
docker stats lm-web
```

## 生产环境建议

1. **HTTPS 配置**：使用 Nginx 反向代理并配置 SSL
2. **CDN 加速**：配置 `VITE_CDN=true` 使用 CDN
3. **日志管理**：配置日志轮转和集中收集
4. **监控告警**：集成 Prometheus + Grafana
5. **资源限制**：配置合理的 CPU 和内存限制
6. **备份策略**：定期备份配置文件

## 清理

### 停止并删除容器

```bash
docker-compose -f docker-compose.web.yml down
```

### 删除镜像

```bash
docker rmi lm-web:latest
```

### 删除构建文件

```bash
rm -rf publish/web
```

## 与后端集成

完整系统部署：

```bash
# 1. 启动基础设施
docker-compose -f docker-compose.infrastructure.yml up -d

# 2. 启动后端 API
./scripts/deploy-api.sh start

# 3. 启动前端
./scripts/deploy-web.sh start
```

访问：
- 前端：http://localhost
- 后端 API：http://localhost:9530
