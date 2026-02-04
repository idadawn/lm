#!/bin/bash
# ============================================
# 前端打包脚本
# 用途：将 web/dist 打包成 dist.zip
# ============================================

set -e

# 配置
PROJECT_ROOT="/home/dawn/project/lm"
DIST_DIR="${PROJECT_ROOT}/web/dist"
PUBLISH_DIR="${PROJECT_ROOT}/publish"
DIST_ZIP="${PUBLISH_DIR}/dist.zip"

# 颜色输出
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m'

log_info()  { echo -e "${GREEN}[INFO]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# 检查 dist 目录
if [ ! -d "$DIST_DIR" ]; then
    log_error "dist 目录不存在: $DIST_DIR"
    log_info "请先运行: cd /home/dawn/project/lm/web && pnpm build"
    exit 1
fi

# 创建 publish 目录
mkdir -p "$PUBLISH_DIR"

# 删除旧的 dist.zip
if [ -f "$DIST_ZIP" ]; then
    rm -f "$DIST_ZIP"
    log_info "已删除旧的 dist.zip"
fi

# 打包
log_info "正在打包 dist..."
cd "$DIST_DIR"
zip -r "$DIST_ZIP" . -q

# 显示结果
SIZE=$(du -sh "$DIST_ZIP" 2>/dev/null | cut -f1)
log_info "打包完成: $DIST_ZIP ($SIZE)"
