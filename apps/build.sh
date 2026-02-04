#!/bin/bash
# ============================================
# Docker 镜像构建脚本
# 使用 host 网络模式和详细输出
# ============================================

set -e

# 颜色定义
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

# 加载环境变量
if [ -f .env ]; then
    export $(cat .env | grep -v '^#' | xargs)
fi

# 设置默认值
IMAGE_TAG=${IMAGE_TAG:-latest}
API_IMAGE=${API_IMAGE:-lm-api}
WEB_IMAGE=${WEB_IMAGE:-lm-web}

log_info "开始构建镜像..."
log_info "版本: $IMAGE_TAG"

# 构建 API 镜像
log_info "正在构建 API 镜像: $API_IMAGE:$IMAGE_TAG"
docker build \
    --progress=plain \
    --network=host \
    -f Dockerfile \
    -t ${API_IMAGE}:${IMAGE_TAG} \
    .

log_success "API 镜像构建完成"

# 构建 Web 镜像
log_info "正在构建 Web 镜像: $WEB_IMAGE:$IMAGE_TAG"
docker build \
    --progress=plain \
    --network=host \
    -f web/Dockerfile \
    -t ${WEB_IMAGE}:${IMAGE_TAG} \
    .

log_success "Web 镜像构建完成"

log_success "所有镜像构建完成！"
echo ""
echo "镜像列表:"
docker images | grep -E "REPOSITORY|${API_IMAGE}|${WEB_IMAGE}"
