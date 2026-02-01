#!/bin/bash
# ============================================
# 应用打包脚本
# 用途：构建并打包应用，用于部署交付
# ============================================

set -e

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

print_info() { echo -e "${GREEN}[INFO]${NC} $1"; }
print_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
print_error() { echo -e "${RED}[ERROR]${NC} $1"; }
print_step() { echo -e "${CYAN}[STEP]${NC} $1"; }

# 获取脚本所在目录的父目录（项目根目录）
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"

# 版本
VERSION_FILE="$ROOT_DIR/VERSION"
VERSION="latest"
if [ -f "$VERSION_FILE" ]; then
    VERSION=$(cat "$VERSION_FILE" | tr -d '\n')
    [ -z "$VERSION" ] && VERSION="latest"
fi

# 输出目录
OUTPUT_DIR="$SCRIPT_DIR/dist"
PUBLISH_API_DIR="$ROOT_DIR/publish/api"
PUBLISH_WEB_DIR="$ROOT_DIR/publish/web"

print_step "========================================"
print_step "   应用打包脚本"
print_step "========================================"
echo ""

# 清理并创建输出目录
print_step "准备输出目录"
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR/apps"
mkdir -p "$OUTPUT_DIR/publish/api"
mkdir -p "$OUTPUT_DIR/publish/web"

# 1. 复制 apps 配置
print_step "复制应用配置文件"
cp -r "$SCRIPT_DIR/docker-compose.yml" "$OUTPUT_DIR/apps/"
cp -r "$SCRIPT_DIR/.env.example" "$OUTPUT_DIR/apps/"
cp -r "$SCRIPT_DIR/README.md" "$OUTPUT_DIR/apps/"
cp -r "$SCRIPT_DIR/deploy.sh" "$OUTPUT_DIR/apps/"

# 2. 检查并复制 API 发布产物
print_step "检查 API 发布产物"
if [ -d "$PUBLISH_API_DIR" ] && [ "$(ls -A $PUBLISH_API_DIR 2>/dev/null)" ]; then
    cp -r "$PUBLISH_API_DIR"/* "$OUTPUT_DIR/publish/api/"
    print_info "API 发布产物已复制"
else
    print_warn "未找到 API 发布产物，请先运行: scripts/build-api.sh"
    print_warn "或手动发布到: $PUBLISH_API_DIR"
fi

# 3. 检查并复制 Web 发布产物
print_step "检查 Web 发布产物"
if [ -d "$PUBLISH_WEB_DIR" ] && [ "$(ls -A $PUBLISH_WEB_DIR 2>/dev/null)" ]; then
    cp -r "$PUBLISH_WEB_DIR"/* "$OUTPUT_DIR/publish/web/"
    print_info "Web 发布产物已复制"
else
    print_warn "未找到 Web 发布产物，请先运行: scripts/build-web.sh"
    print_warn "或手动构建 web 并复制 dist 到: $PUBLISH_WEB_DIR"
fi

# 4. 复制启动脚本
print_step "生成启动脚本"
cat > "$OUTPUT_DIR/start.sh" << 'SCRIPT_EOF'
#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
APPS_DIR="$SCRIPT_DIR/apps"

# 检查环境文件
if [ ! -f "$APPS_DIR/.env" ]; then
    echo "[ERROR] 未找到环境变量文件: $APPS_DIR/.env"
    echo "[INFO] 请先复制 .env.example 为 .env 并配置"
    exit 1
fi

cd "$APPS_DIR"
echo "[STEP] 启动应用服务..."
./deploy.sh
SCRIPT_EOF

chmod +x "$OUTPUT_DIR/start.sh"

# 5. 生成 README
cat > "$OUTPUT_DIR/README.md" << 'README_EOF'
# 应用部署包

该目录为应用服务的部署包，用于交付部署。

## 目录结构

```
dist/
├── apps/               # 应用配置文件
│   ├── docker-compose.yml
│   ├── .env.example    # 环境变量模板
│   ├── deploy.sh       # 部署脚本
│   └── README.md
├── publish/
│   ├── api/            # 后端 API 发布产物
│   └── web/            # 前端 Web 发布产物
├── start.sh            # 一键启动脚本
└── README.md           # 本文件
```

## 部署步骤

### 1. 配置环境变量

```bash
cd apps
cp .env.example .env
# 编辑 .env 文件，配置 INFRA_HOST 等参数
vi .env
```

### 2. 配置 API 连接

将 API 配置文件放到 `apps/deploy/api/Configurations/`：
- `ConnectionStrings.json` - 数据库、Redis 连接
- `AI.json` - AI 服务地址（Qdrant/TEI/vLLM）

### 3. 构建或加载 Docker 镜像

确保服务器上有 `lm-api` 和 `lm-web` 镜像。

如果没有，可以从镜像仓库拉取或本地构建。

### 4. 启动服务

方式一：使用一键启动脚本
```bash
./start.sh
```

方式二：手动启动
```bash
cd apps
./deploy.sh
```

## 注意事项

- 该部署包不包含基础设施服务（MySQL/Redis/Qdrant/TEI/vLLM）
- 需要在基础环境服务器部署完成后再部署本应用
- 确保 Docker 和 Docker Compose 已安装
README_EOF

# 6. 生成版本信息
echo "$VERSION" > "$OUTPUT_DIR/VERSION"

# 完成
echo ""
print_step "========================================"
print_info "打包完成！"
print_step "========================================"
echo ""
print_info "输出目录: $OUTPUT_DIR"
echo ""
print_info "后续步骤:"
echo "  1. 将 dist 目录打包: tar czf app-package.tar.gz -C dist ."
echo "  2. 上传到目标服务器"
echo "  3. 在服务器上解压并运行: tar xzf app-package.tar.gz && cd dist && ./start.sh"
echo ""
