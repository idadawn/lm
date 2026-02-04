#!/bin/bash
# ============================================
# 后端 API 镜像构建脚本
# 用途：构建 lm-api Docker 镜像
# ============================================

set -e

# ============================================
# 配置区域
# ============================================
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
API_DIR="/home/dawn/project/lm/api"
DEPLOY_API_DIR="${PROJECT_ROOT}/deploy/api"
DOCKERFILE="${API_DIR}/src/Dockerfile"

# 镜像配置
IMAGE_NAME="${IMAGE_NAME:-lm-api}"
VERSION_FILE="${PROJECT_ROOT}/VERSION"
APP_VERSION=$(cat "$VERSION_FILE" 2>/dev/null | tr -d '[:space:]')

# 如果 VERSION 文件不存在或为空，使用默认版本
if [ -z "$APP_VERSION" ]; then
    APP_VERSION="latest"
fi

# ============================================
# 颜色输出
# ============================================
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

log_info()  { echo -e "${GREEN}[INFO]${NC} $1"; }
log_warn()  { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }
log_step()  { echo -e "${BLUE}[STEP]${NC} $1"; }
log_debug() { echo -e "${CYAN}[DEBUG]${NC} $1"; }

# ============================================
# 显示帮助
# ============================================
show_help() {
    cat << EOF
用法: $(basename "$0") [选项]

选项:
    -v, --version VER    指定镜像版本号 (默认: ${APP_VERSION})
    -t, --tag TAG        指定镜像标签 (默认: 与版本号相同)
    -r, --reload         构建完成后重新加载容器
    --no-cache           构建时不使用缓存
    -h, --help           显示此帮助信息

示例:
    $(basename "$0")                    # 构建镜像
    $(basename "$0") -r                 # 构建并重新加载容器
    $(basename "$0") -v 1.2.3 -r        # 指定版本并重新加载

EOF
}

# ============================================
# 解析参数
# ============================================
parse_args() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            -v|--version)
                APP_VERSION="$2"
                shift 2
                ;;
            -t|--tag)
                IMAGE_TAG="$2"
                shift 2
                ;;
            -r|--reload)
                RELOAD_CONTAINER=true
                shift
                ;;
            --no-cache)
                NO_CACHE="--no-cache"
                shift
                ;;
            -h|--help)
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

    # 如果未指定标签，使用版本号作为标签
    IMAGE_TAG="${IMAGE_TAG:-$APP_VERSION}"
    
    # 确保版本号和标签不为空
    if [ -z "$APP_VERSION" ]; then
        APP_VERSION="latest"
    fi
    if [ -z "$IMAGE_TAG" ]; then
        IMAGE_TAG="latest"
    fi
}

# ============================================
# 环境检查
# ============================================
check_env() {
    log_step "检查环境..."

    # 检查 Docker
    if ! command -v docker &> /dev/null; then
        log_error "Docker 未安装，请先安装 Docker"
        exit 1
    fi

    # 检查 Docker 守护进程
    if ! docker info &> /dev/null; then
        log_error "Docker 守护进程未运行，请启动 Docker"
        exit 1
    fi

    log_info "Docker: $(docker --version | awk '{print $3}' | tr -d ',')"

    # 检查 Dockerfile 是否存在
    if [ ! -f "$DOCKERFILE" ]; then
        log_error "Dockerfile 不存在: $DOCKERFILE"
        exit 1
    fi

    # 检查 API 目录
    if [ ! -d "$API_DIR" ]; then
        log_error "API 目录不存在: $API_DIR"
        exit 1
    fi

    log_info "环境检查通过"
}

# ============================================
# 构建 Docker 镜像
# ============================================
build_image() {
    log_step "构建 Docker 镜像..."
    
    cd "$API_DIR"

    log_info "镜像名称: ${IMAGE_NAME}:${IMAGE_TAG}"
    log_info "Dockerfile: $DOCKERFILE"
    log_info "构建上下文: $API_DIR"

    # 构建镜像（Dockerfile 中的 COPY 路径是相对于构建上下文的）
    if ! docker build \
        -f "$DOCKERFILE" \
        --progress=plain \
        --network=host \
        -t "${IMAGE_NAME}:${IMAGE_TAG}" \
        -t "${IMAGE_NAME}:latest" \
        $NO_CACHE \
        "$API_DIR"; then
        log_error "Docker 镜像构建失败"
        exit 1
    fi

    log_info "Docker 镜像构建成功"
}

# ============================================
# 重新加载容器
# ============================================
reload_container() {
    if [ "$RELOAD_CONTAINER" != true ]; then
        return 0
    fi

    log_step "重新加载容器..."

    # 检查 docker-compose 是否存在
    local COMPOSE_CMD=""
    if command -v docker-compose &> /dev/null; then
        COMPOSE_CMD="docker-compose"
    elif docker compose version &> /dev/null 2>&1; then
        COMPOSE_CMD="docker compose"
    else
        log_warn "未找到 docker-compose 命令"
        return 0
    fi

    # 使用 docker-compose 重新加载
    cd "$PROJECT_ROOT"
    
    # 检查 docker-compose.yml 是否存在
    if [ ! -f "docker-compose.yml" ]; then
        log_warn "未找到 docker-compose.yml，无法自动重新加载"
        return 0
    fi

    log_info "使用 ${COMPOSE_CMD} 重新加载 api 服务..."
    
    # 重新加载 api 服务
    if ${COMPOSE_CMD} up -d --no-deps api 2>/dev/null; then
        log_info "lm-api 服务已重新加载"
        
        # 等待 api 健康
        log_info "等待服务健康检查..."
        sleep 5
        
        # 检查 api 健康状态
        local api_health=$(docker inspect --format='{{.State.Health.Status}}' lm-api 2>/dev/null || echo "unknown")
        if [ "$api_health" = "healthy" ]; then
            log_info "lm-api 健康状态: healthy"
        else
            log_warn "lm-api 健康状态: $api_health"
        fi
        
        log_info "服务重新加载完成"
    else
        log_error "服务重新加载失败"
        log_info "可以手动执行: ${COMPOSE_CMD} up -d"
    fi
}

# ============================================
# 显示构建结果
# ============================================
show_result() {
    log_step "构建结果"
    
    echo ""
    echo "========================================"
    echo "镜像名称: ${IMAGE_NAME}"
    echo "镜像标签: ${IMAGE_TAG}, latest"
    echo "========================================"
    echo ""

    # 显示镜像信息
    docker images "$IMAGE_NAME" --format "table {{.Repository}}:{{.Tag}}\t{{.Size}}\t{{.CreatedAt}}" | head -5

    echo ""
    echo "使用方式:"
    echo "  docker run -d -p 9530:9530 --name lm-api ${IMAGE_NAME}:${IMAGE_TAG}"
    echo ""
    log_info "后端镜像构建完成!"
    echo "========================================"
}

# ============================================
# 主流程
# ============================================
main() {
    parse_args "$@"

    echo ""
    echo "========================================"
    echo "  后端 API 镜像构建脚本"
    echo "  版本: ${APP_VERSION}"
    echo "========================================"
    echo ""

    check_env
    build_image
    reload_container
    show_result
}

# 执行主流程
main "$@"
