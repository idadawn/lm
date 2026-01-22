#!/bin/bash
# ============================================
# 实验室数据分析系统 - Docker 镜像构建脚本
# ============================================

set -e

# ============================================
# 配置区域
# ============================================
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DOCKERFILE="${PROJECT_ROOT}/api/Dockerfile.build"
IMAGE_NAME="lm-api"

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
# 检查 Docker 是否安装
# ============================================
check_docker() {
    log_step "检查 Docker..."

    if ! command -v docker &> /dev/null; then
        log_error "Docker 未安装，请先安装 Docker"
        exit 1
    fi

    local docker_version=$(docker --version 2>&1)
    log_info "Docker 已安装: $docker_version"
}

# ============================================
# 读取版本号
# ============================================
get_version() {
    VERSION_FILE="${PROJECT_ROOT}/VERSION"
    if [ -f "$VERSION_FILE" ]; then
        APP_VERSION=$(cat "$VERSION_FILE" | tr -d '[:space:]')
    else
        APP_VERSION="latest"
        log_warn "VERSION 文件不存在，使用版本: $APP_VERSION"
    fi
}

# ============================================
# 检查发布目录是否存在
# ============================================
check_publish_dir() {
    log_step "检查发布目录..."

    PUBLISH_DIR="${PROJECT_ROOT}/publish/api"

    if [ ! -d "$PUBLISH_DIR" ]; then
        log_error "发布目录不存在: $PUBLISH_DIR"
        log_error "请先运行 ./scripts/build-api.sh 进行发布"
        exit 1
    fi

    if [ ! -f "$PUBLISH_DIR/Poxiao.API.Entry.dll" ]; then
        log_error "发布文件不完整: $PUBLISH_DIR/Poxiao.API.Entry.dll"
        log_error "请先运行 ./scripts/build-api.sh 进行发布"
        exit 1
    fi

    log_info "发布目录检查通过: $PUBLISH_DIR"
}

# ============================================
# 构建 Docker 镜像
# ============================================
build_image() {
    log_step "构建 Docker 镜像..."
    cd "$PROJECT_ROOT"

    log_info "镜像名称: $IMAGE_NAME"
    log_info "版本标签: $APP_VERSION"
    log_info "Dockerfile: $DOCKERFILE"

    # 构建带版本标签的镜像
    docker build -t ${IMAGE_NAME}:${APP_VERSION} -f api/Dockerfile.build .

    # 同时打 latest 标签
    docker tag ${IMAGE_NAME}:${APP_VERSION} ${IMAGE_NAME}:latest

    log_info "Docker 镜像构建完成!"
}

# ============================================
# 显示镜像信息
# ============================================
show_image_info() {
    log_step "镜像信息..."
    echo ""
    docker images | grep "$IMAGE_NAME" | head -5
    echo ""
    log_info "镜像标签:"
    log_info "  ${IMAGE_NAME}:${APP_VERSION}"
    log_info "  ${IMAGE_NAME}:latest"
    echo ""
}

# ============================================
# 显示帮助信息
# ============================================
show_help() {
    echo "用法: $0 [选项]"
    echo ""
    echo "选项:"
    echo "  --no-latest      不打 latest 标签"
    echo "  --help, -h       显示此帮助信息"
    echo ""
    echo "示例:"
    echo "  $0              # 构建镜像（推荐）"
    echo "  $0 --no-latest  # 构建镜像但不打 latest 标签"
    echo ""
}

# ============================================
# 主流程
# ============================================
main() {
    local TAG_LATEST=true

    # 解析命令行参数
    while [[ $# -gt 0 ]]; do
        case $1 in
            --no-latest)
                TAG_LATEST=false
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
    echo "Docker 镜像构建脚本"
    echo "=========================================="
    echo ""

    check_docker
    get_version
    check_publish_dir
    build_image

    if [ "$TAG_LATEST" = false ]; then
        # 移除 latest 标签
        docker rmi ${IMAGE_NAME}:latest 2>/dev/null || true
    fi

    show_image_info

    echo "=========================================="
    log_info "镜像构建完成!"
    echo "=========================================="
}

main "$@"
