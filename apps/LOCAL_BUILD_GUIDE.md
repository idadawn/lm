# 本地快速构建指南

## 🚀 概述

通过本地构建模式，可以直接从 `web/dist` 目录构建 Docker 镜像，跳过打包和解压步骤，大幅提升构建速度。

## 📋 前提条件

1. 确保前端已构建完成：
   ```bash
   cd /home/dawn/project/lm/web
   pnpm build
   ```

2. 确保 Docker 已安装并运行

## 🎯 使用方法

### 1. 独立本地构建脚本

```bash
# 进入项目根目录
cd /home/dawn/project/lm/apps

# 基础本地构建
./deploy/build-web-local.sh

# 带版本号构建
./deploy/build-web-local.sh -v 1.2.3

# 构建并自动重启容器
./deploy/build-web-local.sh -r

# 查看帮助
./deploy/build-web-local.sh -h
```

### 2. 统一构建脚本（推荐）

```bash
# 构建所有服务（前端使用本地模式）
./build-all.sh --local

# 只构建前端（本地模式）
./build-all.sh -w --local

# 构建并重启服务
./build-all.sh -w --local -r

# 指定版本构建
./build-all.sh -w --local -v 1.2.3 -r
```

## ⚡ 性能对比

| 构建方式 | 步骤 | 耗时 |
|---------|------|------|
| 标准构建 | build → pack → extract → docker build | ~2-3分钟 |
| 本地构建 | build → copy → docker build | ~30秒 |

## 🔧 构建流程

### 本地构建流程：
1. ✅ 检查 `web/dist` 目录是否存在
2. ✅ 复制 dist 到 `deploy/web/dist`
3. ✅ 更新配置文件
4. ✅ 构建 Docker 镜像
5. ✅ 清理临时文件（可选）

### 标准构建流程：
1. ✅ 检查 `web/dist` 目录是否存在
2. ⚠️ 打包成 `dist.zip`
3. ⚠️ 解压到 `deploy/web/dist`
4. ✅ 更新配置文件
5. ✅ 构建 Docker 镜像
6. ✅ 清理临时文件

## 📝 注意事项

1. **本地模式**适合开发环境，快速迭代
2. **标准模式**适合生产环境，便于分发部署包
3. 两种模式生成的镜像是完全相同的
4. 本地模式要求 `web/dist` 必须存在且包含有效的构建产物

## 🐛 常见问题

### Q: 构建失败提示 "dist 目录不存在"
A: 请先在前端目录执行 `pnpm build`

### Q: 如何查看构建的镜像？
A: 使用 `docker images | grep lm-web`

### Q: 如何运行构建的镜像？
A: ```bash
   docker run -d -p 8080:80 --name lm-web lm-web:latest
   ```

### Q: 如何进入容器调试？
A: ```bash
   docker exec -it lm-web sh
   ```

## 🚀 快速开始

```bash
# 1. 构建前端
cd /home/dawn/project/lm/web
pnpm build

# 2. 本地快速构建
cd /home/dawn/project/lm/apps
./build-all.sh -w --local -r

# 3. 查看结果
docker ps | grep lm-web
```

享受飞快的构建速度吧！🎉