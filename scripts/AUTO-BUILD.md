# 自动构建使用指南

## 🎯 两种自动构建方案

### 方案 1：Git Hook 本地自动构建（推荐开发阶段）

每次提交代码时，如果 `VERSION` 文件有变更，自动触发后台构建。

#### 启用自动构建

```bash
./scripts/auto-build-setup.sh enable
```

#### 禁用自动构建

```bash
./scripts/auto-build-setup.sh disable
```

#### 查看状态

```bash
./scripts/auto-build-setup.sh status
```

#### 工作流程

```bash
# 1. 启用自动构建
./scripts/auto-build-setup.sh enable

# 2. 修改代码
vim api/...

# 3. 增加版本号
./scripts/version.sh bump patch

# 4. 提交代码（会自动触发构建）
git add .
git commit -m "feat: 新功能"

# 5. 查看构建日志
tail -f .build.log

# 6. 构建完成后启动服务
docker-compose up -d
```

---

### 方案 2：GitHub Actions CI/CD（推荐生产环境）

推送到 GitHub 后，在云端自动构建并推送镜像。

#### 配置 GitHub Secrets

在 GitHub 仓库设置中添加以下 Secrets：

1. 进入 `Settings` → `Secrets and variables` → `Actions`
2. 添加以下 secrets：
   - `DOCKER_USERNAME` - Docker Hub 用户名
   - `DOCKER_PASSWORD` - Docker Hub 密码或访问令牌

#### 工作流程

```bash
# 1. 修改代码
vim api/...

# 2. 增加版本号
./scripts/version.sh bump patch

# 3. 提交并推送
git add .
git commit -m "feat: 新功能"
git push

# 4. GitHub Actions 自动构建
# 查看: https://github.com/idadawn/lm/actions

# 5. 拉取最新镜像（从 Docker Hub）
docker pull your-username/lm-api:1.0.1
docker pull your-username/lm-web:1.0.1

# 6. 启动服务
docker-compose up -d
```

#### Tag 发布

```bash
# 创建 tag 触发正式发布
./scripts/version.sh set 1.0.0
git add .
git commit -m "release: v1.0.0"

git tag -a v1.0.0 -m "Release version 1.0.0"
git push --tags

# GitHub Actions 会自动构建并推送带版本标签的镜像
```

---

## 🔧 配置对比

| 特性 | Git Hook（本地） | GitHub Actions（云端） |
|------|-----------------|---------------------|
| 构建位置 | 本地机器 | GitHub 服务器 |
| 触发时机 | 提交代码 | 推送到 main/tag |
| 镜像推送 | 手动 | 自动到 Docker Hub |
| 适用场景 | 开发测试 | 生产部署 |
| 配置难度 | 简单 | 需要 GitHub Secrets |

---

## 📋 开发阶段推荐配置

```bash
# 1. 启用本地自动构建
./scripts/auto-build-setup.sh enable

# 2. 配置 GitHub Actions（可选，用于团队协作）
# 在 GitHub 设置中添加 DOCKER_USERNAME 和 DOCKER_PASSWORD

# 3. 日常开发流程
./scripts/version.sh bump patch
git add .
git commit -m "feat: 新功能"
# 本地自动构建...

# 4. 测试
docker-compose up -d

# 5. 如果有问题，回滚
docker-compose down
APP_VERSION=1.0.0 docker-compose up -d
```

---

## 🚀 生产环境推荐配置

```bash
# 1. 禁用本地自动构建
./scripts/auto-build-setup.sh disable

# 2. 配置 GitHub Actions
# 添加 GitHub Secrets

# 3. 发布流程
./scripts/version.sh bump minor
git add .
git commit -m "release: v1.1.0"
git tag -a v1.1.0 -m "Release 1.1.0"
git push
git push --tags

# 4. 等待 GitHub Actions 构建完成

# 5. 在生产服务器拉取镜像
docker pull your-username/lm-api:1.1.0
docker pull your-username/lm-web:1.1.0

# 6. 启动服务
export APP_VERSION=1.1.0
docker-compose up -d
```

---

## 🛠️ 故障排查

### Git Hook 不工作

```bash
# 检查 hook 文件权限
ls -la .git/hooks/post-commit

# 如果没有执行权限
chmod +x .git/hooks/post-commit

# 检查是否启用
./scripts/auto-build-setup.sh status
```

### GitHub Actions 失败

1. 查看构建日志：`https://github.com/idadawn/lm/actions`
2. 检查 GitHub Secrets 是否正确配置
3. 检查 Dockerfile 是否有语法错误

### 本地构建失败

```bash
# 查看构建日志
tail -f .build.log

# 手动构建
./scripts/build.sh
```

---

## 📚 最佳实践

1. **开发阶段**：使用 Git Hook 本地自动构建
2. **测试阶段**：使用 GitHub Actions PR 构建
3. **生产发布**：使用 GitHub Actions Tag 构建
4. **版本管理**：每次提交都增加版本号
5. **回滚机制**：保留旧版本镜像，便于快速回滚
