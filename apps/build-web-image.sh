#!/bin/bash
# ============================================
# 前端镜像构建脚本
# 用途：从 dist.zip 构建前端 Docker 镜像
# ============================================

set -e

# ============================================
# 配置区域
# ============================================
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$SCRIPT_DIR"
WEB_DEPLOY_DIR="${PROJECT_ROOT}/deploy/web"

# 默认 dist.zip 路径（自动查找）
DEFAULT_DIST_ZIP="/home/dawn/project/lm/publish/dist.zip"

# 镜像配置
IMAGE_NAME="${IMAGE_NAME:-lm-web}"
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
    -f, --file PATH      指定 dist.zip 文件路径 (默认: /home/dawn/project/lm/publish/dist.zip)
    -v, --version VER    指定镜像版本号 (默认: ${APP_VERSION})
    -t, --tag TAG        指定镜像标签 (默认: 与版本号相同)
    -r, --reload         构建完成后重新加载容器
    --skip-cleanup       构建后不清理临时文件
    -h, --help           显示此帮助信息

示例:
    $(basename "$0")                    # 使用默认路径构建
    $(basename "$0") -r                 # 构建并重新加载容器
    $(basename "$0") -v 1.2.3 -r        # 指定版本并重新加载
    $(basename "$0") -t mytag           # 指定镜像标签
    $(basename "$0") --skip-cleanup     # 构建后保留临时文件

EOF
}

# ============================================
# 解析参数
# ============================================
parse_args() {
    # 设置默认 dist.zip 路径
    DIST_ZIP="$DEFAULT_DIST_ZIP"
    
    while [[ $# -gt 0 ]]; do
        case $1 in
            -f|--file)
                DIST_ZIP="$2"
                shift 2
                ;;
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
            --skip-cleanup)
                SKIP_CLEANUP=true
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

    # 检查 dist.zip 是否存在
    if [ ! -f "$DIST_ZIP" ]; then
        log_error "构建产物不存在: $DIST_ZIP"
        log_info "请确保 dist.zip 已放置在正确位置"
        exit 1
    fi

    log_info "构建产物: $DIST_ZIP"
    log_info "文件大小: $(du -sh "$DIST_ZIP" 2>/dev/null | cut -f1)"

    # 检查 Dockerfile 是否存在
    if [ ! -f "${WEB_DEPLOY_DIR}/Dockerfile" ]; then
        log_error "Dockerfile 不存在: ${WEB_DEPLOY_DIR}/Dockerfile"
        exit 1
    fi

    log_info "环境检查通过"
}

# ============================================
# 清理旧的构建文件
# ============================================
clean() {
    log_step "清理旧的构建文件..."

    # 清理旧的 dist 目录
    if [ -d "${WEB_DEPLOY_DIR}/dist" ]; then
        rm -rf "${WEB_DEPLOY_DIR}/dist"
        log_info "已清理旧的 dist 目录"
    fi
}

# ============================================
# 解压构建产物
# ============================================
extract_dist() {
    log_step "解压构建产物..."

    mkdir -p "${WEB_DEPLOY_DIR}/dist"

    # 检查 unzip 命令
    if command -v unzip &> /dev/null; then
        unzip -q "$DIST_ZIP" -d "${WEB_DEPLOY_DIR}/dist"
    else
        log_error "unzip 命令未安装"
        log_info "请安装 unzip: apt-get install unzip 或 yum install unzip"
        exit 1
    fi

    # 检查解压结果
    if [ ! -f "${WEB_DEPLOY_DIR}/dist/index.html" ]; then
        # 可能是 dist.zip 里包含了一层 dist 目录
        if [ -d "${WEB_DEPLOY_DIR}/dist/dist" ]; then
            mv "${WEB_DEPLOY_DIR}/dist/dist"/* "${WEB_DEPLOY_DIR}/dist/"
            rmdir "${WEB_DEPLOY_DIR}/dist/dist" 2>/dev/null || true
        else
            log_warn "未找到 index.html，请检查 dist.zip 结构"
        fi
    fi

    # 复制配置文件（覆盖原有的空配置）
    if [ -f "${WEB_DEPLOY_DIR}/config.js" ]; then
        cp "${WEB_DEPLOY_DIR}/config.js" "${WEB_DEPLOY_DIR}/dist/config.js"
        log_info "已更新 API 配置文件"
    fi

    log_info "解压完成"
}

# ============================================
# 构建 Docker 镜像
# ============================================
build_image() {
    log_step "构建 Docker 镜像..."
    cd "$WEB_DEPLOY_DIR"

    log_info "镜像名称: ${IMAGE_NAME}:${IMAGE_TAG}"
    log_info "构建路径: $WEB_DEPLOY_DIR"

    # 构建镜像
    if ! docker build --progress=plain -t "${IMAGE_NAME}:${IMAGE_TAG}" .; then
        log_error "Docker 镜像构建失败"
        exit 1
    fi

    # 打 latest 标签
    docker tag "${IMAGE_NAME}:${IMAGE_TAG}" "${IMAGE_NAME}:latest"

    log_info "Docker 镜像构建成功"
}

# ============================================
# 清理临时文件
# ============================================
cleanup() {
    if [ "$SKIP_CLEANUP" = true ]; then
        log_step "跳过清理 (--skip-cleanup)"
        log_info "临时文件保留在: ${WEB_DEPLOY_DIR}/dist"
        return 0
    fi

    log_step "清理临时文件..."

    if [ -d "${WEB_DEPLOY_DIR}/dist" ]; then
        rm -rf "${WEB_DEPLOY_DIR}/dist"
        log_info "已清理临时 dist 目录"
    fi
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
        log_warn "未找到 docker-compose 命令，尝试直接重启容器..."
        
        # 直接使用 docker 重启容器
        local web_container=$(docker ps -q --filter "name=lm-web" 2>/dev/null || true)
        if [ -n "$web_container" ]; then
            log_info "重启 lm-web 容器..."
            docker stop lm-web 2>/dev/null || true
            docker rm lm-web 2>/dev/null || true
            
            # 使用新镜像启动
            docker run -d \
                --name lm-web \
                --network lm-apps-network \
                --restart unless-stopped \
                --expose 80 \
                --health-cmd "curl -f http://localhost/health || exit 1" \
                --health-interval 30s \
                --health-timeout 10s \
                --health-retries 3 \
                --health-start-period 10s \
                "${IMAGE_NAME}:${IMAGE_TAG}" 2>/dev/null || {
                log_warn "容器启动失败，可能需要手动启动或使用 docker-compose"
            }
        else
            log_info "未发现运行中的 lm-web 容器，跳过重启"
        fi
        return 0
    fi

    # 使用 docker-compose 重新加载
    cd "$PROJECT_ROOT"
    
    # 检查 docker-compose.yml 是否存在
    if [ ! -f "docker-compose.yml" ]; then
        log_warn "未找到 docker-compose.yml，无法自动重新加载"
        return 0
    fi

    log_info "使用 ${COMPOSE_CMD} 重新加载服务..."
    
    # 更新镜像标签（如果使用 latest 标签需要拉取新镜像）
    export IMAGE_TAG
    
    # 重新加载 web 服务
    if ${COMPOSE_CMD} up -d --no-deps web 2>/dev/null; then
        log_info "lm-web 服务已重新加载"
        
        # 等待 web 健康
        log_info "等待服务健康检查..."
        sleep 3
        
        # 检查 web 健康状态
        local web_health=$(docker inspect --format='{{.State.Health.Status}}' lm-web 2>/dev/null || echo "unknown")
        if [ "$web_health" = "healthy" ]; then
            log_info "lm-web 健康状态: healthy"
        else
            log_warn "lm-web 健康状态: $web_health"
        fi
        
        # 重新加载 nginx（因为 nginx 依赖 web，可能需要重启以更新连接）
        log_info "重新加载 nginx..."
        ${COMPOSE_CMD} exec nginx nginx -s reload 2>/dev/null || {
            log_warn "nginx 重新加载配置失败，尝试重启..."
            ${COMPOSE_CMD} restart nginx 2>/dev/null || true
        }
        
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
    echo "  docker run -d -p 8080:80 --name lm-web ${IMAGE_NAME}:${IMAGE_TAG}"
    echo ""
    log_info "前端镜像构建完成!"
    echo "========================================"
}

# ============================================
# 主流程
# ============================================
main() {
    parse_args "$@"

    echo ""
    echo "========================================"
    echo "  前端镜像构建脚本"
    echo "  版本: ${APP_VERSION}"
    echo "========================================"
    echo ""

    check_env
    clean
    extract_dist
    build_image
    cleanup
    reload_container
    show_result
}

# 执行主流程
main "$@"
