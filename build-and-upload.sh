#!/bin/bash
# LM 打包脚本 - 编译 Windows 版本并上传到 OSS
# 用法: ./build-and-upload.sh [版本号]

set -e

# 配置
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$SCRIPT_DIR"
VERSION=${1:-"$(date +%Y%m%d-%H%M%S)"}
OSS_BUCKET="oss://dawnlee"
OSS_PATH="lm/releases"

# 颜色
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m'

log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# 检查工具
check_tool() {
    if ! command -v $1 &> /dev/null; then
        log_error "$1 command not found."
        return 1
    fi
    return 0
}

# 检查 ossutil
if [ ! -f "/tmp/ossutil" ]; then
    log_error "ossutil not found."
    exit 1
fi

OSSUTIL="/tmp/ossutil"

log_info "开始打包版本: $VERSION"
log_info "目标: Windows x64"

# 创建临时打包目录
BUILD_DIR="/tmp/lm-build-$VERSION"
rm -rf "$BUILD_DIR"
mkdir -p "$BUILD_DIR"

# 1. 编译 API
if check_tool "dotnet"; then
    log_info "编译 API 服务..."
    API_PROJECT="$PROJECT_ROOT/api/src/application/Poxiao.API.Entry/Poxiao.API.Entry.csproj"
    dotnet publish "$API_PROJECT" \
        -c Release \
        -r win-x64 \
        --self-contained false \
        -o "$BUILD_DIR/api"
fi

# 2. 编译 Worker
if check_tool "dotnet"; then
    log_info "编译 Worker 服务..."
    WORKER_PROJECT="$PROJECT_ROOT/api/src/application/Poxiao.Lab.CalcWorker/Poxiao.Lab.CalcWorker.csproj"
    dotnet publish "$WORKER_PROJECT" \
        -c Release \
        -r win-x64 \
        --self-contained false \
        -o "$BUILD_DIR/worker"
fi

# 3. 构建前端 Web
if check_tool "pnpm"; then
    log_info "构建前端 Web..."
    cd "$PROJECT_ROOT/web"
    pnpm build
    cd "$PROJECT_ROOT"
    mkdir -p "$BUILD_DIR/web"
    cp -r "$PROJECT_ROOT/web/dist/"* "$BUILD_DIR/web/"
fi

# 4. 打包
log_info "打包..."
cd "$BUILD_DIR"

# 4.1 打包后端 (api + worker)
tar -czvf "lm-backend-$VERSION.tar.gz" api worker

# 4.2 打包前端 (web)
tar -czvf "lm-web-$VERSION.tar.gz" web

# 5. 上传到 OSS
log_info "上传到 OSS..."

# 上传后端
$OSSUTIL cp "$BUILD_DIR/lm-backend-$VERSION.tar.gz" "$OSS_BUCKET/$OSS_PATH/"
BACKEND_URL="https://dawnlee.oss-rg-china-mainland.aliyuncs.com/lm/releases/lm-backend-$VERSION.tar.gz"

# 上传前端
$OSSUTIL cp "$BUILD_DIR/lm-web-$VERSION.tar.gz" "$OSS_BUCKET/$OSS_PATH/"
WEB_URL="https://dawnlee.oss-rg-china-mainland.aliyuncs.com/lm/releases/lm-web-$VERSION.tar.gz"

# 清理
log_info "清理临时文件..."
rm -rf "$BUILD_DIR"

# 输出结果
log_info "打包完成!"
echo ""
echo "========== 部署信息 =========="
echo "版本: $VERSION"
echo ""
echo "后端包 (API + Worker):"
echo "  URL: $BACKEND_URL"
echo "  部署到: D:\Lab\api\, D:\Lab\worker\"
echo ""
echo "前端包 (Web):"
echo "  URL: $WEB_URL"
echo "  部署到: D:\Lab\nginx\html\"
echo "================================"
