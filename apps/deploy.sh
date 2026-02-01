#!/bin/bash
# ============================================
# 应用服务器部署脚本
# ============================================

set -e

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# 打印带颜色的消息
print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# 检查 Docker 是否安装
check_docker() {
    if ! command -v docker &> /dev/null; then
        print_error "Docker 未安装，请先安装 Docker"
        exit 1
    fi

    if ! docker compose version &> /dev/null; then
        print_error "Docker Compose 未安装，请先安装 Docker Compose"
        exit 1
    fi

    print_info "Docker 环境检查通过"
}

# 检查环境变量文件
check_env_file() {
    if [ ! -f ".env.apps" ]; then
        print_error ".env.apps 文件不存在！"
        print_error "请先复制并配置环境变量文件"
        exit 1
    fi

    # 加载环境变量
    source .env.apps 2>/dev/null || true

    # 检查关键配置
    if [ -z "$INFRA_HOST" ] || [ "$INFRA_HOST" = "10.0.0.5" ]; then
        print_warn "INFRA_HOST 未配置或使用默认值，请检查 .env.apps"
        print_warn "基础环境服务器内网 IP: $INFRA_HOST"
    fi

    print_info "使用 .env.apps 配置文件"
    print_info "基础环境服务器: $INFRA_HOST"
}

# 检查镜像是否存在
check_images() {
    print_info "检查 Docker 镜像..."

    if ! docker images | grep -q "lm-api"; then
        print_warn "lm-api 镜像不存在，请先构建 API 镜像"
        print_warn "运行: docker build -t lm-api -f ../api/Dockerfile ."
    fi

    if ! docker images | grep -q "lm-web"; then
        print_warn "lm-web 镜像不存在，请先构建 Web 镜像"
        print_warn "运行: docker build -t lm-web ../web"
    fi
}

# 测试基础环境连接
test_infra_connection() {
    print_info "测试基础环境连接..."

    local infra_host="${INFRA_HOST:-10.0.0.5}"
    local mysql_port="${INFRA_MYSQL_PORT:-3307}"
    local redis_port="${INFRA_REDIS_PORT:-6380}"

    if ping -c 1 -W 2 "$infra_host" &> /dev/null; then
        print_info "基础环境服务器 $infra_host 可访问"
    else
        print_warn "无法 ping 通基础环境服务器 $infra_host"
        print_warn "请确保内网连接正常"
    fi
}

# 启动服务
start_services() {
    print_info "启动应用服务..."

    docker compose up -d

    print_info "等待服务启动..."
    sleep 10

    print_info "服务状态："
    docker compose ps
}

# 显示服务信息
show_info() {
    echo ""
    print_info "========================================"
    print_info "应用服务部署完成"
    print_info "========================================"
    echo ""
    print_info "服务访问地址："
    echo "  - API:        http://localhost:${API_PORT:-9530}"
    echo "  - Nginx:      http://localhost:${NGINX_PORT:-8923}"
    echo ""
    print_info "请在浏览器中访问: http://localhost:${NGINX_PORT:-8923}"
    echo ""
    print_info "常用命令："
    echo "  查看日志: docker compose logs -f <服务名>"
    echo "  停止服务: docker compose down"
    echo "  重启服务: docker compose restart"
    echo ""
}

# 主流程
main() {
    echo "========================================"
    echo "   应用服务器部署脚本"
    echo "========================================"
    echo ""

    check_docker
    check_env_file
    check_images
    test_infra_connection
    start_services
    show_info
}

# 执行主流程
main
