#!/bin/bash
# LM 打包脚本 - 编译 Windows 版本并上传到 OSS
# 用法: ./build-and-upload.ps1 [版本号]

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

# 检查 dotnet
if ! command -v dotnet &> /dev/null; then
    log_error "dotnet command not found."
    exit 1
fi

# 检查 ossutil
if ! command -v ossutil &> /dev/null && [ ! -f "/tmp/ossutil" ]; then
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
log_info "编译 API 服务..."
API_PROJECT="$PROJECT_ROOT/api/src/application/Poxiao.API.Entry/Poxiao.API.Entry.csproj"
dotnet publish "$API_PROJECT" \
    -c Release \
    -r win-x64 \
    --self-contained false \
    -o "$BUILD_DIR/api"

# 2. 编译 Worker
log_info "编译 Worker 服务..."
WORKER_PROJECT="$PROJECT_ROOT/api/src/application/Poxiao.Lab.CalcWorker/Poxiao.Lab.CalcWorker.csproj"
dotnet publish "$WORKER_PROJECT" \
    -c Release \
    -r win-x64 \
    --self-contained false \
    -o "$BUILD_DIR/worker"

# 3. 打包
log_info "打包..."
cd "$BUILD_DIR"
tar -czvf "$BUILD_DIR/lm-$VERSION.tar.gz" api worker

# 4. 上传到 OSS
log_info "上传到 OSS..."
$OSSUTIL cp "$BUILD_DIR/lm-$VERSION.tar.gz" "$OSS_BUCKET/$OSS_PATH/"

# 清理
log_info "清理临时文件..."
rm -rf "$BUILD_DIR"

# 输出结果
OSS_URL="https://dawnlee.oss-rg-china-mainland.aliyuncs.com/lm/releases/lm-$VERSION.tar.gz"
log_info "打包完成!"
log_info "OSS 地址: $OSS_URL"
log_info "Windows 下载命令: curl -O $OSS_URL"

echo ""
echo "========== 部署信息 =========="
echo "版本: $VERSION"
echo "OSS 地址: $OSS_URL"
echo "================================"
