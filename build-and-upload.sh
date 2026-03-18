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

# 1. 编译 API (发布模式，只包含修改过的文件)
if check_tool "dotnet"; then
    log_info "编译 API 服务..."
    API_PROJECT="$PROJECT_ROOT/api/src/application/Poxiao.API.Entry/Poxiao.API.Entry.csproj"
    dotnet publish "$API_PROJECT" \
        -c Release \
        -r win-x64 \
        --no-restore \
        -o "$BUILD_DIR/api"
fi

# 2. 编译 Worker
if check_tool "dotnet"; then
    log_info "编译 Worker 服务..."
    WORKER_PROJECT="$PROJECT_ROOT/api/src/application/Poxiao.Lab.CalcWorker/Poxiao.Lab.CalcWorker.csproj"
    dotnet publish "$WORKER_PROJECT" \
        -c Release \
        -r win-x64 \
        --no-restore \
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

# 4.1 打包后端 (只打包根目录的 dll + exe + json + config 等运行时文件)
mkdir -p "$BUILD_DIR/api-minimal"
cp "$BUILD_DIR/api/"*.dll "$BUILD_DIR/api-minimal/" 2>/dev/null || true
cp "$BUILD_DIR/api/"*.exe "$BUILD_DIR/api-minimal/" 2>/dev/null || true
cp "$BUILD_DIR/api/"*.json "$BUILD_DIR/api-minimal/" 2>/dev/null || true
cp "$BUILD_DIR/api/"*.config "$BUILD_DIR/api-minimal/" 2>/dev/null || true
cp "$BUILD_DIR/api/"*.xml "$BUILD_DIR/api-minimal/" 2>/dev/null || true
# 保留Configurations目录
cp -r "$BUILD_DIR/api/Configurations" "$BUILD_DIR/api-minimal/" 2>/dev/null || true

mkdir -p "$BUILD_DIR/worker-minimal"
cp "$BUILD_DIR/worker/"*.dll "$BUILD_DIR/worker-minimal/" 2>/dev/null || true
cp "$BUILD_DIR/worker/"*.exe "$BUILD_DIR/worker-minimal/" 2>/dev/null || true
cp "$BUILD_DIR/worker/"*.json "$BUILD_DIR/worker-minimal/" 2>/dev/null || true
cp "$BUILD_DIR/worker/"*.config "$BUILD_DIR/worker-minimal/" 2>/dev/null || true
cp "$BUILD_DIR/worker/"*.xml "$BUILD_DIR/worker-minimal/" 2>/dev/null || true
cp -r "$BUILD_DIR/worker/Configurations" "$BUILD_DIR/worker-minimal/" 2>/dev/null || true

# 打包后端 (api + worker 最小集)
tar -czvf "lm-backend-$VERSION.tar.gz" api-minimal worker-minimal

# 4.2 打包前端 (web)
tar -czvf "lm-web-$VERSION.tar.gz" web

# 5. 上传到 OSS
log_info "上传到 OSS..."

# 上传后端
$OSSUTIL cp -f "$BUILD_DIR/lm-backend-$VERSION.tar.gz" "$OSS_BUCKET/$OSS_PATH/"
BACKEND_URL="https://dawnlee.oss-rg-china-mainland.aliyuncs.com/lm/releases/lm-backend-$VERSION.tar.gz"

# 上传前端
$OSSUTIL cp -f "$BUILD_DIR/lm-web-$VERSION.tar.gz" "$OSS_BUCKET/$OSS_PATH/"
WEB_URL="https://dawnlee.oss-rg-china-mainland.aliyuncs.com/lm/releases/lm-web-$VERSION.tar.gz"

# 6. 更新版本列表
log_info "更新版本列表..."

# 下载现有版本列表
VERSIONS_FILE="/tmp/versions-$VERSION.json"
$OSSUTIL cp "$OSS_BUCKET/$OSS_PATH/versions.json" "$VERSIONS_FILE" 2>/dev/null || echo '{"versions":[]}' > "$VERSIONS_FILE"

# 使用 Node.js 更新 JSON（如果没有则安装）
if command -v node &> /dev/null; then
    node -e "
    const fs = require('fs');
    const data = JSON.parse(fs.readFileSync('$VERSIONS_FILE', 'utf8'));
    const newVersion = {
        version: '$VERSION',
        backend: '$BACKEND_URL',
        web: '$WEB_URL',
        date: new Date().toISOString(),
        latest: true
    };
    // 先清除其他版本的 latest 标记
    data.versions.forEach(v => v.latest = false);
    // 插入到开头（最新在前）
    data.versions.unshift(newVersion);
    // 只保留最近 10 个版本
    data.versions = data.versions.slice(0, 10);
    // 设置 latest 快捷引用
    data.latest = newVersion;
    fs.writeFileSync('$VERSIONS_FILE', JSON.stringify(data, null, 2));
    "
    
    # 上传版本列表
    $OSSUTIL cp -f "$VERSIONS_FILE" "$OSS_BUCKET/$OSS_PATH/versions.json"
    VERSIONS_URL="https://dawnlee.oss-rg-china-mainland.aliyuncs.com/lm/releases/versions.json"
fi

# 清理
log_info "清理临时文件..."
rm -rf "$BUILD_DIR"

# 输出结果
log_info "打包完成!"
echo ""
echo "========== 部署信息 =========="
echo "版本: $VERSION"
echo ""
echo "后端包 (API + Worker - 增量DLL):"
echo "  URL: $BACKEND_URL"
echo "  部署: 解压后复制 dll/json/exe 到 D:\\Lab\\api\\ 和 D:\\Lab\\worker\\"
echo ""
echo "前端包 (Web):"
echo "  URL: $WEB_URL"
echo "  部署到: D:\\Lab\\nginx\\html\\"
echo ""
echo "版本列表: $VERSIONS_URL"
echo "================================"
