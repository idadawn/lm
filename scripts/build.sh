#!/bin/bash
# ============================================
# 实验室数据分析系统 - 一键构建脚本
# 构建前后端 Docker 镜像
# ============================================

set -e

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
    echo "  --api, -a         仅构建 API 镜像"
    echo "  --web, -w         仅构建 Web 镜像"
    echo "  --all, -A         构建所有镜像（默认）"
    echo "  --no-cache        构建时不使用缓存"
    echo "  --help, -h        显示此帮助信息"
    echo ""
    echo "示例:"
    echo "  $0                # 构建所有镜像"
    echo "  $0 --api          # 仅构建 API"
    echo "  $0 --web          # 仅构建 Web"
    echo "  $0 --no-cache     # 无缓存构建"
    echo ""
}

# ============================================
# 构建 API 镜像
# ============================================
build_api() {
    log_step "构建 API Docker 镜像..."
    ./scripts/build-api.sh "$@"
}

# ============================================
# 构建 Web 镜像
# ============================================
build_web() {
    log_step "构建 Web Docker 镜像..."
    ./scripts/build-web.sh "$@"
}

# ============================================
# 构建所有镜像
# ============================================
build_all() {
    log_step "构建所有 Docker 镜像..."
    echo ""

    build_api "$@"
    echo ""

    build_web "$@"
    echo ""

    log_step "构建完成!"
    echo ""
    echo "镜像列表:"
    docker images | grep -E "REPOSITORY|lm-api|lm-web" || true
}

# ============================================
# 主流程
# ============================================
main() {
    local BUILD_TARGET="all"
    local BUILD_ARGS=""

    # 解析命令行参数
    while [[ $# -gt 0 ]]; do
        case $1 in
            --api|-a)
                BUILD_TARGET="api"
                shift
                ;;
            --web|-w)
                BUILD_TARGET="web"
                shift
                ;;
            --all|-A)
                BUILD_TARGET="all"
                shift
                ;;
            --no-cache)
                BUILD_ARGS="$BUILD_ARGS --no-cache"
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
    echo "实验室数据分析系统 - 一键构建脚本"
    echo "=========================================="
    echo ""

    case "$BUILD_TARGET" in
        api)
            build_api $BUILD_ARGS
            ;;
        web)
            build_web $BUILD_ARGS
            ;;
        all)
            build_all $BUILD_ARGS
            ;;
    esac

    echo ""
    echo "=========================================="
    log_info "构建完成！"
    echo ""
    log_info "使用以下命令启动服务:"
    echo "  docker-compose up -d"
    echo ""
    log_info "或仅启动基础设施:"
    echo "  docker-compose up -d mysql redis qdrant"
    echo "=========================================="
}

main "$@"
