#!/bin/bash
# ============================================
# 前端镜像构建脚本
# 用途：从 web 目录构建前端并构建 Docker 镜像
# ============================================

set -e

# ============================================
# 配置区域
# ============================================
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$(dirname "$SCRIPT_DIR")")"
WEB_DIR="${PROJECT_ROOT}/web"
WEB_DEPLOY_DIR="${PROJECT_ROOT}/deploy/web"
DOCKERFILE="${WEB_DEPLOY_DIR}/Dockerfile"

# dist.zip 路径（如果跳过了构建，可直接指定 dist.zip 路径）
DIST_ZIP="${DIST_ZIP:-${PROJECT_ROOT}/web/dist.zip}"

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
    -f, --file PATH      直接指定 dist.zip 文件路径（跳过构建）
    -v, --version VER    指定镜像版本号 (默认: ${APP_VERSION})
    -t, --tag TAG        指定镜像标签 (默认: 与版本号相同)
    -r, --reload         构建完成后重新加载容器
    --fast               使用快速构建模式（禁用压缩，更快）
    --skip-install       跳过依赖安装（如果 node_modules 已存在）
    --skip-build         跳过前端构建，使用已有 dist.zip
    --skip-cleanup       构建后不清理临时文件
    -h, --help           显示此帮助信息

示例:
    $(basename "$0")                    # 构建前端并构建镜像
    $(basename "$0") -r                 # 构建并重新加载容器
    $(basename "$0") --fast             # 快速构建（推荐）
    $(basename "$0") --fast --skip-install -r  # 快速构建+跳过安装+重启
    $(basename "$0") --skip-build       # 跳过构建，使用现有 dist.zip

EOF
    echo ""
    echo "构建加速建议:"
    echo "  1. 日常开发测试使用 --fast 模式（最快）"
    echo "     $(basename "$0") --fast --skip-install -r"
    echo ""
    echo "  2. 首次构建后，后续可跳过依赖安装"
    echo "     $(basename "$0") --skip-install -r"
    echo ""
    echo "  3. 生产环境发布使用标准模式"
    echo "     $(basename "$0") -v 1.2.3"
    echo ""
}

# ============================================
# 解析参数
# ============================================
parse_args() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            -f|--file)
                DIST_ZIP="$2"
                SKIP_BUILD=true
                shift 2
                ;;
            --skip-build)
                SKIP_BUILD=true
                shift
                ;;
            --fast)
                FAST_BUILD=true
                shift
                ;;
            --skip-install)
                SKIP_INSTALL=true
                shift
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

        # 如果跳过了构建，检查 dist.zip 是否存在
    if [ "$SKIP_BUILD" = true ]; then
        if [ -z "$DIST_ZIP" ] || [ ! -f "$DIST_ZIP" ]; then
            log_error "构建产物不存在: ${DIST_ZIP:-未指定}"
            log_info "请确保 dist.zip 已放置在正确位置"
            exit 1
        fi
        log_info "构建产物: $DIST_ZIP"
        log_info "文件大小: $(du -sh "$DIST_ZIP" 2>/dev/null | cut -f1)"
    else
        # 检查 web 目录是否存在
        if [ ! -d "$WEB_DIR" ]; then
            log_error "Web 目录不存在: $WEB_DIR"
            exit 1
        fi
        
        # 检查 node 和 pnpm
        if ! command -v node &> /dev/null; then
            log_error "Node.js 未安装，请先安装 Node.js"
            exit 1
        fi
        
        if ! command -v pnpm &> /dev/null; then
            log_error "pnpm 未安装，请先安装 pnpm"
            exit 1
        fi
        
        log_info "Node.js: $(node --version)"
        log_info "pnpm: $(pnpm --version)"
        
        if [ "$FAST_BUILD" = true ]; then
            log_info "快速构建模式: 已启用"
        fi
        if [ "$SKIP_INSTALL" = true ]; then
            log_info "跳过依赖安装: 已启用"
        fi
    fi

    # 检查 Dockerfile 是否存在
    if [ ! -f "$DOCKERFILE" ]; then
        log_error "Dockerfile 不存在: $DOCKERFILE"
        exit 1
    fi

    log_info "环境检查通过"
}

# ============================================
# 清理旧的构建文件
# ============================================
clean() {
    log_step "清理旧的构建文件..."

    # 清理旧的 dist.zip
    if [ -f "${WEB_DEPLOY_DIR}/dist.zip" ]; then
        rm -f "${WEB_DEPLOY_DIR}/dist.zip"
        log_info "已清理部署目录的 dist.zip"
    fi
    
    # 如果构建前端，清理 web 目录的 dist
    if [ "$SKIP_BUILD" != true ] && [ -d "${WEB_DIR}/dist" ]; then
        rm -rf "${WEB_DIR}/dist"
        log_info "已清理 web 目录的 dist"
    fi
}

# ============================================
# 构建前端项目
# ============================================
check_node_modules() {
    # 检查 node_modules 是否存在且看起来完整
    if [ ! -d "${WEB_DIR}/node_modules" ]; then
        return 1
    fi
    
    # 检查关键包是否存在
    if [ ! -d "${WEB_DIR}/node_modules/vite" ] || [ ! -d "${WEB_DIR}/node_modules/vue" ]; then
        return 1
    fi
    
    return 0
}

build_web() {
    if [ "$SKIP_BUILD" = true ]; then
        return 0
    fi
    
    log_step "构建前端项目..."
    
    cd "$WEB_DIR"
    
    # 依赖安装
    if [ "$SKIP_INSTALL" = true ]; then
        if check_node_modules; then
            log_info "跳过依赖安装（node_modules 已存在）"
        else
            log_warn "node_modules 不存在或不完整，执行安装..."
            SKIP_INSTALL=false
        fi
    fi
    
    if [ "$SKIP_INSTALL" != true ]; then
        log_info "安装依赖..."
        # 使用 --frozen-lockfile 确保一致性，如果有 lock 文件
        if [ -f "pnpm-lock.yaml" ]; then
            if ! pnpm install --frozen-lockfile; then
                log_warn "锁定文件安装失败，尝试普通安装..."
                if ! pnpm install; then
                    log_error "依赖安装失败"
                    exit 1
                fi
            fi
        else
            if ! pnpm install; then
                log_error "依赖安装失败"
                exit 1
            fi
        fi
        log_info "依赖安装完成"
    fi
    
    # 构建项目
    if [ "$FAST_BUILD" = true ]; then
        log_info "执行快速构建（build:fast）..."
        log_info "  - 禁用 gzip/brotli 压缩"
        log_info "  - 禁用图片压缩"
        log_info "  - 禁用旧版浏览器兼容"
        if ! pnpm run build:fast; then
            log_error "前端构建失败"
            exit 1
        fi
    else
        log_info "执行标准构建..."
        if ! pnpm run build; then
            log_error "前端构建失败"
            exit 1
        fi
    fi
    
    log_info "前端构建完成"
}

# ============================================
# 复制构建产物到部署目录
# ============================================
prepare_dist() {
    log_step "准备构建产物..."
    
    if [ "$SKIP_BUILD" = true ]; then
        # 使用指定的 dist.zip
        log_info "复制 dist.zip..."
        cp "$DIST_ZIP" "${WEB_DEPLOY_DIR}/dist.zip"
    else
        # 从构建好的 dist 打包
        if [ ! -d "${WEB_DIR}/dist" ]; then
            log_error "构建产物目录不存在: ${WEB_DIR}/dist"
            exit 1
        fi
        
        log_info "打包 dist 目录..."
        cd "${WEB_DIR}"
        zip -r "${WEB_DEPLOY_DIR}/dist.zip" dist/
    fi
    
    log_info "准备完成"
}

# ============================================
# 构建 Docker 镜像
# ============================================
build_image() {
    log_step "构建 Docker 镜像..."
    cd "$WEB_DEPLOY_DIR"

    log_info "镜像名称: ${IMAGE_NAME}:${IMAGE_TAG}"
    log_info "Dockerfile: $DOCKERFILE"
    log_info "构建路径: $WEB_DEPLOY_DIR"

    # 构建镜像
    if ! docker build -f "$DOCKERFILE" --progress=plain --network=host -t "${IMAGE_NAME}:${IMAGE_TAG}" "$WEB_DEPLOY_DIR"; then
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
        log_info "临时文件保留在: ${WEB_DEPLOY_DIR}/dist.zip"
        return 0
    fi

    log_step "清理临时文件..."

    if [ -f "${WEB_DEPLOY_DIR}/dist.zip" ]; then
        rm -f "${WEB_DEPLOY_DIR}/dist.zip"
        log_info "已清理临时 dist.zip"
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

    local START_TIME=$(date +%s)
    
    check_env
    clean
    build_web
    prepare_dist
    build_image
    cleanup
    reload_container
    
    local END_TIME=$(date +%s)
    local DURATION=$((END_TIME - START_TIME))
    local MIN=$((DURATION / 60))
    local SEC=$((DURATION % 60))
    
    show_result
    
    echo ""
    log_info "总耗时: ${MIN}分${SEC}秒"
    echo "========================================"
}

# 执行主流程
main "$@"
