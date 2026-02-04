#!/bin/bash
# ============================================
# Universal Build & Rebuild Script
# 支持选择性构建: API、Web或全部
# Usage: ./rebuild.sh [mode]
#   mode可选: api | web | all (默认: all)
#   支持选项: -c | --clean  先清理再构建
# Example:
#   ./rebuild.sh api          # 只构建API
#   ./rebuild.sh web          # 只构建Web
#   ./rebuild.sh all          # 构建全部
#   ./rebuild.sh -c api       # 清理并构建API
#   ./rebuild.sh --clean all  # 清理并构建全部
# ============================================

set -e

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$SCRIPT_DIR"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_section() {
    echo -e "\n${BLUE}[==== $1 ====]${NC}"
}

# 解析参数
CLEAN_MODE=false
BUILD_MODE="all"

for arg in "$@"; do
    case $arg in
        -c|--clean)
            CLEAN_MODE=true
            ;;
        api|web|all)
            BUILD_MODE=$arg
            ;;
        *)
            log_warn "未知参数: $arg"
            ;;
    esac
done

log_info "构建模式: $BUILD_MODE"
log_info "清理模式: $CLEAN_MODE"

# 检查脚本是否存在
if [ ! -f "build-api.sh" ]; then
    log_error "build-api.sh 不存在"
    exit 1
fi

if [ ! -f "build-web-turbo.sh" ]; then
    log_error "build-web-turbo.sh 不存在"
    exit 1
fi

# 清理函数
clean_builds() {
    log_section "清理构建产物"

    # 清理API构建产物
    if [ -d "apps/api" ]; then
        log_info "清理 API 构建产物..."
        rm -rf apps/api
    fi

    # 清理Web构建产物
    if [ -d "apps/dist" ]; then
        log_info "清理 Web 构建产物..."
        rm -rf apps/dist
    fi

    # 清理Web缓存
    if [ -d "web/dist" ]; then
        log_info "清理 dist 缓存..."
        rm -rf web/dist
    fi

    if [ -d "web/node_modules/.vite" ]; then
        log_info "清理 Vite 缓存..."
        rm -rf web/node_modules/.vite
    fi

    if [ -d "web/node_modules/.cache" ]; then
        log_info "清理 缓存目录..."
        rm -rf web/node_modules/.cache
    fi

    if [ -d "web/.turbo" ]; then
        log_info "清理 Turbo 缓存..."
        rm -rf web/.turbo
    fi

    log_info "清理完成"
}

# 构建函数
build_api() {
    log_section "开始构建 API"

    # 检查dotnet
    if ! command -v dotnet &> /dev/null; then
        log_error "未找到 dotnet 命令，请先安装 .NET SDK"
        return 1
    fi

    # 执行构建
    if ./build-api.sh; then
        log_info "✅ API 构建成功"
        return 0
    else
        log_error "❌ API 构建失败"
        return 1
    fi
}

build_web() {
    log_section "开始构建 Web (Turbo模式)"

    # 检查pnpm
    if ! command -v pnpm &> /dev/null; then
        log_error "未找到 pnpm 命令，请先安装 pnpm (npm install -g pnpm)"
        return 1
    fi

    # 执行构建
    if ./build-web-turbo.sh; then
        log_info "✅ Web 构建成功"
        return 0
    else
        log_error "❌ Web 构建失败"
        return 1
    fi
}

# 主流程
main() {
    local start_time=$(date +%s)
    local success=true

    # 如果启用清理模式，先清理
    if [ "$CLEAN_MODE" = true ]; then
        clean_builds
    fi

    # 根据构建模式执行构建
    case $BUILD_MODE in
        api)
            if ! build_api; then
                success=false
            fi
            ;;
        web)
            if ! build_web; then
                success=false
            fi
            ;;
        all)
            if ! build_api; then
                success=false
            fi

            if $success && ! build_web; then
                success=false
            fi
            ;;
        *)
            log_error "未知构建模式: $BUILD_MODE"
            exit 1
            ;;
    esac

    # 显示统计信息
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))

    echo -e "\n"
    log_section "构建统计"
    echo "  总耗时: ${duration} 秒"
    echo "  清理模式: $CLEAN_MODE"

    if [ -d "apps/api" ]; then
        echo "  API 产物大小:"
        du -sh apps/api 2>/dev/null || echo "  API 大小计算失败"
    fi

    if [ -d "apps/dist" ]; then
        echo "  Web 产物大小:"
        du -sh apps/dist 2>/dev/null || echo "  Web 大小计算失败"
        echo "  Web 文件数量:"
        find apps/dist -type f | wc -l
    fi

    if $success; then
        log_info "🎉 构建完成！执行时间: ${duration}秒"
        exit 0
    else
        log_error "💥 构建失败！结束时间: ${duration}秒"
        exit 1
    fi
}

# 执行主流程
main