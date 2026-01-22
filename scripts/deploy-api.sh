#!/bin/bash
# ============================================
# 实验室数据分析系统 - API 一键部署脚本
# 用于 Docker 环境
# ============================================

set -e

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
# 检查依赖
# ============================================
check_dependencies() {
    log_step "检查依赖..."

    if ! command -v docker &> /dev/null; then
        log_error "Docker 未安装"
        exit 1
    fi

    if ! command -v docker-compose &> /dev/null; then
        log_error "Docker Compose 未安装"
        exit 1
    fi

    log_info "依赖检查通过（使用 Docker 构建 .NET 项目）"
}

# ============================================
# 构建发布 API（使用 Docker）
# ============================================
build_api() {
    log_step "使用 Docker 构建 API..."
    ./scripts/build-api-docker.sh
}

# ============================================
# 构建 Docker 镜像
# ============================================
build_image() {
    log_step "构建 Docker 镜像..."
    docker build -t lm-api:latest -f api/Dockerfile.build .
    log_info "镜像构建完成"
}

# ============================================
# 启动基础设施服务
# ============================================
start_infrastructure() {
    log_step "启动基础设施服务..."
    docker-compose -f docker-compose.infrastructure.yml up -d
    log_info "基础设施服务已启动"

    # 等待服务就绪
    log_info "等待服务就绪..."
    sleep 5
}

# ============================================
# 启动 API 服务
# ============================================
start_api() {
    log_step "启动 API 服务..."
    docker-compose -f docker-compose.app.yml up -d
    log_info "API 服务已启动"
}

# ============================================
# 显示状态
# ============================================
show_status() {
    log_step "服务状态..."
    echo ""
    docker-compose -f docker-compose.infrastructure.yml ps
    echo ""
    docker-compose -f docker-compose.app.yml ps
    echo ""
    log_info "API 地址: http://localhost:9530"
    log_info "查看日志: docker-compose -f docker-compose.app.yml logs -f api"
}

# ============================================
# 主流程
# ============================================
main() {
    local action=${1:-full}

    echo "=========================================="
    echo "API 部署脚本"
    echo "=========================================="
    echo ""

    case "$action" in
        full)
            check_dependencies
            build_api
            build_image
            start_infrastructure
            start_api
            show_status
            ;;
        build)
            build_api
            build_image
            ;;
        start)
            start_infrastructure
            start_api
            show_status
            ;;
        stop)
            log_step "停止服务..."
            docker-compose -f docker-compose.app.yml down
            docker-compose -f docker-compose.infrastructure.yml down
            log_info "服务已停止"
            ;;
        restart)
            log_step "重启服务..."
            docker-compose -f docker-compose.app.yml restart api
            show_status
            ;;
        logs)
            docker-compose -f docker-compose.app.yml logs -f api
            ;;
        status)
            show_status
            ;;
        *)
            echo "用法: $0 {full|build|start|stop|restart|logs|status}"
            echo ""
            echo "选项:"
            echo "  full    - 完整部署（默认）：构建 -> 镜像 -> 启动"
            echo "  build   - 仅构建 API 和镜像"
            echo "  start   - 仅启动服务"
            echo "  stop    - 停止所有服务"
            echo "  restart - 重启 API 服务"
            echo "  logs    - 查看 API 日志"
            echo "  status  - 查看服务状态"
            exit 1
            ;;
    esac

    echo ""
    echo "=========================================="
    log_info "部署完成!"
    echo "=========================================="
}

main "$@"
