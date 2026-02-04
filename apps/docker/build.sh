#!/bin/bash
# ============================================
# Web 前端镜像构建脚本
# ============================================

set -e

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 默认配置
IMAGE_NAME="${IMAGE_NAME:-lm-web}"
IMAGE_TAG="${IMAGE_TAG:-latest}"
REGISTRY="${REGISTRY:-}"

# 完整镜像名称
if [ -n "$REGISTRY" ]; then
    FULL_IMAGE_NAME="${REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}"
else
    FULL_IMAGE_NAME="${IMAGE_NAME}:${IMAGE_TAG}"
fi

# 打印带颜色的信息
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# 显示帮助信息
show_help() {
    cat << EOF
Web 前端镜像构建脚本

用法: $0 [选项]

选项:
    -t, --tag TAG       设置镜像标签 (默认: latest)
    -r, --registry URL  设置镜像仓库地址
    -n, --name NAME     设置镜像名称 (默认: lm-web)
    -p, --push          构建完成后推送到镜像仓库
    -l, --load          加载 docker image 到本地 (用于 docker buildx)
    --platform PLATFORM 设置构建平台 (默认: linux/amd64,linux/arm64)
    -h, --help          显示帮助信息

环境变量:
    IMAGE_NAME          镜像名称
    IMAGE_TAG           镜像标签
    REGISTRY            镜像仓库地址

示例:
    # 基本构建
    $0

    # 指定标签构建
    $0 -t v1.0.0

    # 构建并推送到私有仓库
    $0 -t v1.0.0 -r registry.example.com --push

    # 多平台构建并推送
    $0 -t v1.0.0 --platform linux/amd64,linux/arm64 -r registry.example.com --push
EOF
}

# 检查依赖
check_dependencies() {
    if ! command -v docker &> /dev/null; then
        log_error "Docker 未安装"
        exit 1
    fi
}

# 检查构建产物
check_dist() {
    # 优先检查 web/dist/，如果不存在则检查 ../dist/
    if [ -d "dist" ] && [ -f "dist/index.html" ]; then
        log_info "使用 web/dist/ 目录"
    elif [ -d "../dist" ] && [ -f "../dist/index.html" ]; then
        log_info "检测到 ../dist/ 目录，复制到 web/dist/"
        rm -rf dist
        cp -r ../dist dist
    else
        log_error "dist/ 目录不存在，请先构建前端项目"
        exit 1
    fi

    if [ ! -f "dist/index.html" ]; then
        log_warning "dist/index.html 不存在，请确认前端项目已正确构建"
    fi
}

# 构建镜像
build_image() {
    log_info "开始构建镜像: ${FULL_IMAGE_NAME}"
    
    # 检查是否使用 buildx
    if docker buildx version &> /dev/null; then
        log_info "使用 Docker Buildx 构建"
        
        # 创建 builder（如果不存在）
        if ! docker buildx inspect multiplatform &> /dev/null; then
            docker buildx create --name multiplatform --use --bootstrap || true
        fi
        
        local buildx_args=""
        
        if [ "$PUSH" = "true" ]; then
            buildx_args="$buildx_args --push"
        fi
        
        if [ "$LOAD" = "true" ]; then
            buildx_args="$buildx_args --load"
        fi
        
        if [ -n "$PLATFORM" ]; then
            buildx_args="$buildx_args --platform $PLATFORM"
        fi
        
        # shellcheck disable=SC2086
        docker buildx build \
            $buildx_args \
            --tag "$FULL_IMAGE_NAME" \
            --build-arg BUILD_DATE="$(date -u +'%Y-%m-%dT%H:%M:%SZ')" \
            --build-arg VERSION="$IMAGE_TAG" \
            .
    else
        log_info "使用标准 Docker 构建"
        docker build \
            --tag "$FULL_IMAGE_NAME" \
            --build-arg BUILD_DATE="$(date -u +'%Y-%m-%dT%H:%M:%SZ')" \
            --build-arg VERSION="$IMAGE_TAG" \
            .
    fi
    
    log_success "镜像构建完成: ${FULL_IMAGE_NAME}"
}

# 推送镜像
push_image() {
    if [ "$PUSH" != "true" ]; then
        return
    fi
    
    if [ -z "$REGISTRY" ]; then
        log_warning "未设置镜像仓库地址，跳过推送"
        return
    fi
    
    log_info "推送镜像到仓库: ${FULL_IMAGE_NAME}"
    docker push "$FULL_IMAGE_NAME"
    log_success "镜像推送完成"
}

# 显示镜像信息
show_image_info() {
    log_info "镜像信息:"
    echo "  名称: $IMAGE_NAME"
    echo "  标签: $IMAGE_TAG"
    echo "  仓库: ${REGISTRY:-<本地>}"
    echo "  完整名称: $FULL_IMAGE_NAME"
    
    if [ "$PUSH" = "true" ]; then
        echo "  推送: 是"
    else
        echo "  推送: 否"
    fi
    
    if docker images "$FULL_IMAGE_NAME" --format "{{.Size}}" 2>/dev/null | grep -q .; then
        echo "  大小: $(docker images "$FULL_IMAGE_NAME" --format "{{.Size}}")"
    fi
}

# 主函数
main() {
    PUSH="false"
    LOAD="false"
    PLATFORM=""
    
    # 解析参数
    while [[ $# -gt 0 ]]; do
        case $1 in
            -t|--tag)
                IMAGE_TAG="$2"
                shift 2
                ;;
            -r|--registry)
                REGISTRY="$2"
                shift 2
                ;;
            -n|--name)
                IMAGE_NAME="$2"
                shift 2
                ;;
            -p|--push)
                PUSH="true"
                shift
                ;;
            -l|--load)
                LOAD="true"
                shift
                ;;
            --platform)
                PLATFORM="$2"
                shift 2
                ;;
            -h|--help)
                show_help
                exit 0
                ;;
            *)
                log_error "未知参数: $1"
                show_help
                exit 1
                ;;
        esac
    done
    
    # 更新完整镜像名称
    if [ -n "$REGISTRY" ]; then
        FULL_IMAGE_NAME="${REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}"
    else
        FULL_IMAGE_NAME="${IMAGE_NAME}:${IMAGE_TAG}"
    fi
    
    # 执行构建流程
    check_dependencies
    check_dist
    show_image_info
    build_image
    push_image
    
    log_success "构建流程完成!"
    echo ""
    echo "运行镜像:"
    echo "  docker run -d -p 8080:80 --name ${IMAGE_NAME} ${FULL_IMAGE_NAME}"
}

main "$@"
