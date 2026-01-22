#!/bin/bash
# ============================================
# 实验室数据分析系统 - 前端 Docker 部署脚本
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

    log_info "依赖检查通过"
}

# ============================================
# 构建前端
# ============================================
build_web() {
    log_step "构建前端..."
    ./scripts/build-web.sh
}

# ============================================
# 构建 Docker 镜像
# ============================================
build_image() {
    log_step "构建 Docker 镜像..."
    docker build -t lm-web:latest -f web/Dockerfile.build .
    log_info "镜像构建完成"
}

# ============================================
# 启动前端服务
# ============================================
start_web() {
    log_step "启动前端服务..."

    # 检查是否已在 docker-compose 中定义
    if [ -f "docker-compose.web.yml" ]; then
        docker-compose -f docker-compose.web.yml up -d
    else
        # 使用 docker run 启动
        docker run -d \
            --name lm-web \
            --network lm-network \
            -p 80:80 \
            --restart unless-stopped \
            lm-web:latest
    fi

    log_info "前端服务已启动"
}

# ============================================
# 停止服务
# ============================================
stop_service() {
    log_step "停止前端服务..."

    if [ -f "docker-compose.web.yml" ]; then
        docker-compose -f docker-compose.web.yml down
    else
        docker stop lm-web 2>/dev/null || true
        docker rm lm-web 2>/dev/null || true
    fi

    log_info "前端服务已停止"
}

# ============================================
# 重启服务
# ============================================
restart_service() {
    log_step "重启前端服务..."
    stop_service
    start_web
}

# ============================================
# 查看日志
# ============================================
show_logs() {
    if [ -f "docker-compose.web.yml" ]; then
        docker-compose -f docker-compose.web.yml logs -f web
    else
        docker logs -f lm-web
    fi
}

# ============================================
# 查看状态
# ============================================
show_status() {
    log_step "服务状态..."
    echo ""
    if [ -f "docker-compose.web.yml" ]; then
        docker-compose -f docker-compose.web.yml ps
    else
        docker ps | grep lm-web || echo "前端服务未运行"
    fi
    echo ""
    log_info "前端地址: http://localhost"
    log_info "查看日志: ./scripts/deploy-web.sh logs"
}

# ============================================
# 完整部署
# ============================================
deploy_full() {
    check_dependencies
    build_web
    build_image
    start_web
    show_status
}

# ============================================
# 显示帮助
# ============================================
show_help() {
    echo "用法: $0 [命令]"
    echo ""
    echo "命令:"
    echo "  full     完整部署（构建 + 镜像 + 启动）"
    echo "  build    构建前端和镜像"
    echo "  start    启动服务"
    echo "  stop     停止服务"
    echo "  restart  重启服务"
    echo "  logs     查看日志"
    echo "  status   查看状态"
    echo "  help     显示帮助"
    echo ""
    echo "示例:"
    echo "  $0 full      # 完整部署"
    echo "  $0 build     # 仅构建"
    echo "  $0 logs      # 查看日志"
    echo ""
}

# ============================================
# 主流程
# ============================================
main() {
    local COMMAND="${1:-full}"

    case "$COMMAND" in
        full)
            deploy_full
            ;;
        build)
            check_dependencies
            build_web
            build_image
            ;;
        start)
            check_dependencies
            start_web
            ;;
        stop)
            stop_service
            ;;
        restart)
            restart_service
            ;;
        logs)
            show_logs
            ;;
        status)
            show_status
            ;;
        help|--help|-h)
            show_help
            ;;
        *)
            log_error "未知命令: $COMMAND"
            show_help
            exit 1
            ;;
    esac
}

main "$@"
