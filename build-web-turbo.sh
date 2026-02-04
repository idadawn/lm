#!/bin/bash
# ============================================
# Turbo Build Web Script - 极速模式
# Usage: ./build-web-turbo.sh
# 针对大内存机器优化 - 使用所有CPU核心和内存
# ============================================

set -e

# 配置
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$SCRIPT_DIR"
WEB_PROJECT_PATH="${PROJECT_ROOT}/web"
OUTPUT_DIR="${PROJECT_ROOT}/apps/dist"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

# 获取CPU核心数
CPU_CORES=$(nproc)
log_info "CPU Cores available: $CPU_CORES"

# 内存配置 - 使用280GB，留20GB给系统
NODE_MEMORY="280000"

# Node.js和Vite优化
export NODE_OPTIONS="--max-old-space-size=$NODE_MEMORY"
export VITE_TSC=false             # 跳过类型检查
export VITE_SOURCE_MAP=false      # 禁用source map
export VITE_USE_PWA=false         # 禁用PWA
export VITE_LEGACY=false          # 禁用旧版浏览器支持
export VITE_DROP_CONSOLE=false    # 保留console，加速构建
export VITE_USE_IMAGEMIN=false    # 跳过图片压缩
export VITE_BUILD_REPORT=false    # 跳过构建报告

# 系统优化
export UV_THREADPOOL_SIZE=$CPU_CORES
export JOBS=$CPU_CORES

# Turbo模式 - 激进的并行化
export VITE_BUILD_PARALLEL=true

log_info "Starting TURBO build with $CPU_CORES cores and 280GB RAM..."
log_info "Web project: $WEB_PROJECT_PATH"
log_info "Output dir: $OUTPUT_DIR"

# 预优化
log_info "Pre-warming file system and caches..."
cd "$WEB_PROJECT_PATH"

# 并行安装依赖（如果存在lock文件）
if [ -f "pnpm-lock.yaml" ]; then
    log_info "Installing dependencies with turbo mode..."
    pnpm install --frozen-lockfile --prefer-offline --reporter=silent --store-dir /tmp/pnpm-store || true
else
    log_info "Installing dependencies..."
    pnpm install --reporter=silent
fi

# 清理构建缓存但不清理依赖
log_info "Cleaning build cache..."
rm -rf dist/ node_modules/.vite node_modules/.cache .turbo 2>/dev/null || true

# 并行构建 - 使用所有CPU核心
log_info "☄️ TURBO BUILD STARTING ☄️"
log_info "Building with $CPU_CORES parallel workers and 280GB Memory..."

# 计时开始
START_TIME=$(date +%s)

# 执行极速构建
pnpm build:fast

# 记录结果
END_TIME=$(date +%s)
DURATION=$((END_TIME - START_TIME))

# 检查构建结果
if [ -d "dist" ]; then
    log_info "✅ TURBO BUILD COMPLETED in ${DURATION}s!"

    # 复制构建结果
    log_info "Copying artifacts..."
    rm -rf "$OUTPUT_DIR"/* 2>/dev/null || true
    cp -r dist/* "$OUTPUT_DIR/"

    log_info "📊 Build statistics:"
    echo "  - Duration: ${DURATION} seconds"
    echo "  - Memory used: ${NODE_MEMORY}MB"
    echo "  - CPU cores: $CPU_CORES"
    echo "  - Output size:"
    du -sh "$OUTPUT_DIR" 2>/dev/null || echo "  Size calculation failed"

    echo "  - Files built:"
    find "$OUTPUT_DIR" -type f | wc -l
else
    log_error "❌ TURBO BUILD FAILED!"
    exit 1
fi

log_info "🚀 TURBO build completed successfully!"