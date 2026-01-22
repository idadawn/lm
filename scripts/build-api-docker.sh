#!/bin/bash
# ============================================
# 实验室数据分析系统 - API Docker 构建脚本
# 使用 Docker 容器构建，无需本地安装 .NET SDK
# ============================================

set -e

# ============================================
# 配置区域
# ============================================
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PUBLISH_DIR="${PROJECT_ROOT}/publish/api"
DOTNET_VERSION="10.0"
CONFIGURATION="Release"
RUNTIME="linux-x64"

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
# 检查 Docker
# ============================================
check_docker() {
    if ! command -v docker &> /dev/null; then
        log_error "Docker 未安装，请先安装 Docker"
        exit 1
    fi
    log_info "Docker 已安装: $(docker --version)"
}

# ============================================
# 清理旧发布文件
# ============================================
clean() {
    log_step "清理旧发布文件..."
    if [ -d "$PUBLISH_DIR" ]; then
        rm -rf "$PUBLISH_DIR"
        log_info "已清理: $PUBLISH_DIR"
    fi
    mkdir -p "$PUBLISH_DIR"
}

# ============================================
# 使用 Docker 构建
# ============================================
build_with_docker() {
    log_step "使用 Docker 构建 API..."

    # 创建临时容器进行构建
    docker run --rm \
        -v "$PROJECT_ROOT:/src" \
        -w "/src" \
        mcr.microsoft.com/dotnet/sdk:10.0 \
        bash -c "
            cd api/src/application/Poxiao.API.Entry && \
            dotnet publish Poxiao.API.Entry.csproj \
                -c $CONFIGURATION \
                -r $RUNTIME \
                --self-contained false \
                -o /src/publish/api \
                /p:PublishTrimmed=false
        "

    log_info "发布完成!"
}

# ============================================
# 复制必要文件
# ============================================
copy_resources() {
    log_step "复制必要文件..."

    # 复制 resources 目录
    if [ -d "${PROJECT_ROOT}/api/resources" ]; then
        cp -r "${PROJECT_ROOT}/api/resources" "$PUBLISH_DIR/"
        log_info "已复制: resources"
    else
        log_warn "resources 目录不存在: ${PROJECT_ROOT}/api/resources"
    fi

    # 确保 Configurations 目录存在
    if [ ! -d "$PUBLISH_DIR/Configurations" ]; then
        cp -r "${PROJECT_ROOT}/api/src/application/Poxiao.API.Entry/Configurations" "$PUBLISH_DIR/"
        log_info "已复制: Configurations"
    fi
}

# ============================================
# 显示发布信息
# ============================================
show_info() {
    log_step "发布信息..."
    echo ""
    echo "发布目录: $PUBLISH_DIR"
    echo "大小: $(du -sh "$PUBLISH_DIR" 2>/dev/null | cut -f1 || echo 'unknown')"
    echo ""
    log_info "现在可以运行以下命令构建 API 镜像:"
    echo ""
    echo "  docker build -t lm-api:latest -f api/Dockerfile.build ."
    echo ""
    echo "或使用一键部署:"
    echo ""
    echo "  ./scripts/deploy-api-docker.sh build"
    echo ""
}

# ============================================
# 主流程
# ============================================
main() {
    echo "=========================================="
    echo "API Docker 构建脚本"
    echo "=========================================="
    echo ""

    check_docker
    clean
    build_with_docker
    copy_resources
    show_info

    echo "=========================================="
    log_info "构建完成!"
    echo "=========================================="
}

main "$@"
