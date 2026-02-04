#!/bin/bash
# ============================================
# 统一构建脚本
# 用途：构建前端和后端镜像
# ============================================

set -e

# ============================================
# 配置区域
# ============================================
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BUILD_WEB_SCRIPT="${SCRIPT_DIR}/build-web.sh"
BUILD_WEB_LOCAL_SCRIPT="${SCRIPT_DIR}/deploy/build-web-local.sh"
BUILD_API_SCRIPT="${SCRIPT_DIR}/build-api.sh"

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
    -w, --web            只构建前端
    -a, --api            只构建后端
    -r, --reload         构建完成后重新加载容器
    -v, --version VER    指定镜像版本号
    -t, --tag TAG        指定镜像标签
    --local              使用本地构建（跳过打包解压）
    -h, --help           显示此帮助信息

示例:
    $(basename "$0")                    # 构建前端和后端
    $(basename "$0") -w                 # 只构建前端
    $(basename "$0") -a                 # 只构建后端
    $(basename "$0") -r                 # 构建并重新加载容器
    $(basename "$0") -v 1.2.3 -r        # 指定版本并重新加载
    $(basename "$0") -w --local         # 本地构建前端（跳过打包解压）

EOF
}

# ============================================
# 解析参数
# ============================================
parse_args() {
    BUILD_WEB=true
    BUILD_API=true
    RELOAD=""
    VERSION=""
    TAG=""
    USE_LOCAL=false

    while [[ $# -gt 0 ]]; do
        case $1 in
            -w|--web)
                BUILD_WEB=true
                BUILD_API=false
                shift
                ;;
            -a|--api)
                BUILD_WEB=false
                BUILD_API=true
                shift
                ;;
            -r|--reload)
                RELOAD="-r"
                shift
                ;;
            -v|--version)
                VERSION="-v $2"
                shift 2
                ;;
            -t|--tag)
                TAG="-t $2"
                shift 2
                ;;
            --local)
                USE_LOCAL=true
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
}

# ============================================
# 检查脚本
# ============================================
check_scripts() {
    if [ "$BUILD_WEB" = true ]; then
        if [ "$USE_LOCAL" = true ] && [ ! -f "$BUILD_WEB_LOCAL_SCRIPT" ]; then
            log_error "前端本地构建脚本不存在: $BUILD_WEB_LOCAL_SCRIPT"
            exit 1
        elif [ "$USE_LOCAL" = false ] && [ ! -f "$BUILD_WEB_SCRIPT" ]; then
            log_error "前端构建脚本不存在: $BUILD_WEB_SCRIPT"
            exit 1
        fi
    fi

    if [ "$BUILD_API" = true ] && [ ! -f "$BUILD_API_SCRIPT" ]; then
        log_error "后端构建脚本不存在: $BUILD_API_SCRIPT"
        exit 1
    fi
}

# ============================================
# 主流程
# ============================================
main() {
    parse_args "$@"
    check_scripts

    echo ""
    echo "========================================"
    echo "  统一构建脚本"
    echo "========================================"
    echo ""

    local start_time=$(date +%s)

    # 构建前端
    if [ "$BUILD_WEB" = true ]; then
        echo ""
        if [ "$USE_LOCAL" = true ]; then
            log_step "【1/2】构建前端镜像（本地模式，跳过打包解压）..."
            echo ""
            bash "$BUILD_WEB_LOCAL_SCRIPT" $VERSION $TAG $RELOAD
        else
            log_step "【1/2】构建前端镜像..."
            echo ""
            bash "$BUILD_WEB_SCRIPT" $VERSION $TAG $RELOAD
        fi
    fi

    # 构建后端
    if [ "$BUILD_API" = true ]; then
        echo ""
        log_step "【2/2】构建后端镜像..."
        echo ""
        bash "$BUILD_API_SCRIPT" $VERSION $TAG $RELOAD
    fi

    local end_time=$(date +%s)
    local duration=$((end_time - start_time))

    echo ""
    echo "========================================"
    log_info "所有构建任务完成!"
    echo "耗时: ${duration} 秒"
    echo "========================================"
    echo ""
}

# 执行主流程
main "$@"
