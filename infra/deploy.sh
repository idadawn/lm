#!/bin/bash
# ============================================
# 基础环境服务器部署脚本
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
    if [ ! -f ".env" ]; then
        print_warn ".env 文件不存在，使用默认配置"
    else
        print_info "使用 .env 配置文件"
    fi
}

# 检查模型目录
check_models_dir() {
    local models_path="${MODELS_HOST_PATH:-/opt/models}"
    if [ ! -d "$models_path" ]; then
        print_warn "模型目录 $models_path 不存在，AI 服务可能无法启动"
        print_warn "请确保已下载模型文件到 $models_path 目录"
    else
        print_info "模型目录存在: $models_path"
    fi
}

# 启动服务
start_services() {
    print_info "启动基础环境服务..."

    docker compose up -d

    print_info "等待服务启动..."
    sleep 5

    print_info "服务状态："
    docker compose ps
}

# 显示服务信息
show_info() {
    echo ""
    print_info "========================================"
    print_info "基础环境服务部署完成"
    print_info "========================================"
    echo ""
    print_info "服务访问地址："
    echo "  - MySQL:      localhost:${MYSQL_PORT:-3307}"
    echo "  - Redis:      localhost:${REDIS_PORT:-6380}"
    echo "  - Qdrant:     localhost:${QDRANT_HTTP_PORT:-6333}"
    echo "  - TEI:        localhost:${TEI_PORT:-8081}"
    echo "  - vLLM:       localhost:${VLLM_PORT:-8082}"
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
    echo "   基础环境服务器部署脚本"
    echo "========================================"
    echo ""

    check_docker
    check_env_file
    check_models_dir
    start_services
    show_info
}

# 执行主流程
main
