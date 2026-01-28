#!/bin/bash
# ============================================
# 实验室数据分析系统 - 前端构建脚本
# 使用方式：
#   1. 本地构建（需要 Node.js 和 pnpm）
#   2. Docker 构建（无需本地环境）
# ============================================

set -e

# ============================================
# 配置区域
# ============================================
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
WEB_DIR="${PROJECT_ROOT}/web"
PUBLISH_DIR="${PROJECT_ROOT}/publish/web"
BUILD_MODE="local"  # local 或 docker

# 读取版本号
VERSION_FILE="${PROJECT_ROOT}/VERSION"
if [ -f "$VERSION_FILE" ]; then
    APP_VERSION=$(cat "$VERSION_FILE" | tr -d '[:space:]')
else
    APP_VERSION="latest"
    log_warn "VERSION 文件不存在，使用版本: $APP_VERSION"
fi

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

log_step() {
    echo -e "${BLUE}[STEP]${NC} $1"
}

# ============================================
# 显示帮助信息
# ============================================
show_help() {
    echo "用法: $0 [选项]"
    echo ""
    echo "选项:"
    echo "  --local, -l       本地构建（需要 Node.js 和 pnpm，默认）"
    echo "  --docker, -d      Docker 构建（无需本地环境）"
    echo "  --help, -h        显示此帮助信息"
    echo ""
    echo "示例:"
    echo "  $0               # 本地构建"
    echo "  $0 --docker      # Docker 构建"
    echo ""
}

# ============================================
# 检查 Node.js
# ============================================
check_nodejs() {
    log_step "检查 Node.js..."

    if ! command -v node &> /dev/null; then
        log_error "Node.js 未安装"
        log_info "请安装 Node.js 18+ 或使用 --docker 选项"
        exit 1
    fi

    local node_version=$(node -v | cut -d'v' -f2 | cut -d'.' -f1)
    if [ "$node_version" -lt 18 ]; then
        log_error "Node.js 版本过低: $(node -v)"
        log_info "需要 Node.js 18+ 或使用 --docker 选项"
        exit 1
    fi

    log_info "Node.js 版本: $(node -v)"
}

# ============================================
# 检查 pnpm
# ============================================
check_pnpm() {
    log_step "检查 pnpm..."

    if command -v pnpm &> /dev/null; then
        local pnpm_version=$(pnpm -v)
        log_info "pnpm 已安装: $pnpm_version"
        return 0
    fi

    log_error "pnpm 未安装，请先安装 pnpm"
    log_info "安装命令: npm install -g pnpm"
    exit 1
}

# ============================================
# 配置 npm 镜像（国内加速）
# ============================================
setup_pnpm_registry() {
    log_step "配置 pnpm 镜像..."
    pnpm config set registry https://registry.npmmirror.com
    log_info "已设置 pnpm 镜像: https://registry.npmmirror.com"
}

# ============================================
# 本地构建
# ============================================
build_local() {
    log_step "本地构建前端..."

    cd "$WEB_DIR"

    # 安装依赖
    log_info "安装依赖..."
    pnpm install

    # 构建生产版本
    log_info "开始构建..."
    pnpm run build

    # 复制到发布目录
    log_info "复制构建产物..."
    mkdir -p "$PUBLISH_DIR"
    rm -rf "${PUBLISH_DIR:?}"/*
    cp -r "$WEB_DIR/dist/"* "$PUBLISH_DIR/"

    log_info "构建完成!"
}

# ============================================
# Docker 构建 (已简化，改为直接拷贝 dist)
# ============================================
build_docker() {
    log_step "Docker 构建前端 (使用本地 dist)..."
    build_local
}

# ============================================
# 清理
# ============================================
clean() {
    log_step "清理旧构建文件..."
    if [ -d "$PUBLISH_DIR" ]; then
        rm -rf "$PUBLISH_DIR"
        log_info "已清理: $PUBLISH_DIR"
    fi
    mkdir -p "$PUBLISH_DIR"
}

# ============================================
# 构建 Docker 镜像
# ============================================
build_docker_image() {
    log_step "构建 Docker 镜像..."

    if ! command -v docker &> /dev/null; then
        log_warn "Docker 未安装，跳过镜像构建"
        return 0
    fi

    cd "$PROJECT_ROOT"

    # 构建带版本标签的镜像
    docker build --progress=plain --network=host -t lm-web:${APP_VERSION} -f web/Dockerfile web/

    # 同时打 latest 标签
    docker tag lm-web:${APP_VERSION} lm-web:latest

    log_info "Docker 镜像构建完成: lm-web:${APP_VERSION}"
}

# ============================================
# 显示构建信息
# ============================================
show_info() {
    log_step "构建信息..."
    echo ""
    echo "发布目录: $PUBLISH_DIR"
    echo "大小: $(du -sh "$PUBLISH_DIR" | cut -f1)"
    echo ""
}

# ============================================
# 主流程
# ============================================
main() {
    # 解析命令行参数
    while [[ $# -gt 0 ]]; do
        case $1 in
            --local|-l)
                BUILD_MODE="local"
                shift
                ;;
            --docker|-d)
                BUILD_MODE="docker"
                shift
                ;;
            --help|-h)
                show_help
                exit 0
                ;;
            *)
                log_error "未知选项: $1"
                show_help
                exit 1
                ;;
        esac
    done

    echo "=========================================="
    echo "前端构建脚本 (模式: $BUILD_MODE)"
    echo "=========================================="
    echo ""

    clean

    if [ "$BUILD_MODE" = "local" ] || [ "$BUILD_MODE" = "docker" ]; then
        check_nodejs
        check_pnpm
        setup_pnpm_registry
        build_local
    fi

    build_docker_image
    show_info

    echo "=========================================="
    log_info "构建完成!"
    echo "=========================================="
}

main "$@"
