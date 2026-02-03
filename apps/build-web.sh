#!/bin/bash
# ============================================
# 前端打包脚本
# 用途：构建前端项目并输出到发布目录
# ============================================

set -e

# ============================================
# 配置区域
# ============================================
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
WEB_DIR="${PROJECT_ROOT}/web"
PUBLISH_DIR="${PROJECT_ROOT}/publish/web"

# 默认配置
BUILD_MODE="production"    # production | test
SKIP_INSTALL=false         # 是否跳过依赖安装
SKIP_DOCKER=false          # 是否跳过 Docker 镜像构建
CLEAN_CACHE=false          # 是否清理缓存

# 读取版本号
VERSION_FILE="${PROJECT_ROOT}/VERSION"
APP_VERSION=$(cat "$VERSION_FILE" 2>/dev/null | tr -d '[:space:]' || echo "latest")

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
    -m, --mode MODE      构建模式: production(默认) | test
    --skip-install       跳过依赖安装（使用已有 node_modules）
    --skip-docker        跳过 Docker 镜像构建
    --clean-cache        构建前清理 Vite 缓存
    -h, --help           显示此帮助信息

示例:
    $(basename "$0")                    # 生产环境构建
    $(basename "$0") -m test            # 测试环境构建
    $(basename "$0") --skip-install     # 跳过依赖安装（更快）
    $(basename "$0") --clean-cache      # 清理缓存后构建

EOF
}

# ============================================
# 解析参数
# ============================================
parse_args() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            -m|--mode)
                BUILD_MODE="$2"
                shift 2
                ;;
            --skip-install)
                SKIP_INSTALL=true
                shift
                ;;
            --skip-docker)
                SKIP_DOCKER=true
                shift
                ;;
            --clean-cache)
                CLEAN_CACHE=true
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

    # 验证构建模式
    if [[ "$BUILD_MODE" != "production" && "$BUILD_MODE" != "test" ]]; then
        log_error "不支持的构建模式: $BUILD_MODE"
        log_info "支持的模式: production, test"
        exit 1
    fi
}

# ============================================
# 环境检查
# ============================================
check_env() {
    log_step "检查环境..."

    # 检查 Node.js
    if ! command -v node &> /dev/null; then
        log_error "Node.js 未安装，请先安装 Node.js 18+"
        exit 1
    fi

    local node_version=$(node -v | sed 's/v//g')
    local major_version=$(echo "$node_version" | cut -d'.' -f1)
    if [ "$major_version" -lt 18 ]; then
        log_error "Node.js 版本过低: $node_version，需要 18+"
        exit 1
    fi
    log_info "Node.js: $node_version"

    # 检查 pnpm
    if ! command -v pnpm &> /dev/null; then
        log_error "pnpm 未安装，请先安装 pnpm"
        log_info "安装命令: npm install -g pnpm"
        exit 1
    fi
    log_info "pnpm: $(pnpm -v)"

    # 检查项目目录
    if [ ! -d "$WEB_DIR" ]; then
        log_error "前端项目目录不存在: $WEB_DIR"
        exit 1
    fi

    if [ ! -f "$WEB_DIR/package.json" ]; then
        log_error "前端项目 package.json 不存在"
        exit 1
    fi

    log_info "环境检查通过"
}

# ============================================
# 清理函数
# ============================================
clean() {
    log_step "清理旧的构建文件..."

    # 清理发布目录
    if [ -d "$PUBLISH_DIR" ]; then
        rm -rf "$PUBLISH_DIR"
        log_info "已清理发布目录"
    fi
    mkdir -p "$PUBLISH_DIR"

    # 清理 Vite 缓存（可选）
    if [ "$CLEAN_CACHE" = true ]; then
        log_info "清理 Vite 缓存..."
        rm -rf "$WEB_DIR/node_modules/.vite"
        rm -rf "$WEB_DIR/node_modules/.cache"
    fi

    # 清理旧的 dist
    if [ -d "$WEB_DIR/dist" ]; then
        rm -rf "$WEB_DIR/dist"
        log_info "已清理 dist 目录"
    fi
}

# ============================================
# 安装依赖
# ============================================
install_deps() {
    if [ "$SKIP_INSTALL" = true ]; then
        log_step "跳过依赖安装 (--skip-install)"
        
        # 检查 node_modules 是否存在
        if [ ! -d "$WEB_DIR/node_modules" ]; then
            log_warn "node_modules 不存在，无法进行跳过安装"
            log_info "将自动执行依赖安装"
            SKIP_INSTALL=false
        else
            return 0
        fi
    fi

    log_step "安装依赖..."
    cd "$WEB_DIR"

    # 配置国内镜像加速
    pnpm config set registry https://registry.npmmirror.com 2>/dev/null || true

    # 安装依赖
    if ! pnpm install; then
        log_error "依赖安装失败"
        exit 1
    fi

    log_info "依赖安装完成"
}

# ============================================
# 执行构建
# ============================================
build() {
    log_step "开始构建 [模式: $BUILD_MODE]..."
    cd "$WEB_DIR"

    local build_cmd=""
    if [ "$BUILD_MODE" = "production" ]; then
        build_cmd="pnpm run build"
    else
        build_cmd="pnpm run build:test"
    fi

    log_info "执行: $build_cmd"
    
    if ! eval "$build_cmd"; then
        log_error "构建失败！"
        exit 1
    fi

    # 检查构建产物
    if [ ! -d "$WEB_DIR/dist" ]; then
        log_error "构建产物不存在: $WEB_DIR/dist"
        exit 1
    fi

    log_info "构建成功"
}

# ============================================
# 复制构建产物到发布目录
# ============================================
copy_dist() {
    log_step "复制构建产物到发布目录..."

    mkdir -p "$PUBLISH_DIR"
    cp -r "$WEB_DIR/dist/"* "$PUBLISH_DIR/"

    # 添加版本信息
    echo "$APP_VERSION" > "$PUBLISH_DIR/VERSION"
    echo "$(date '+%Y-%m-%d %H:%M:%S')" > "$PUBLISH_DIR/BUILD_TIME"

    log_info "构建产物已复制到: $PUBLISH_DIR"
}

# ============================================
# 构建 Docker 镜像
# ============================================
build_docker_image() {
    if [ "$SKIP_DOCKER" = true ]; then
        log_step "跳过 Docker 镜像构建 (--skip-docker)"
        return 0
    fi

    if ! command -v docker &> /dev/null; then
        log_warn "Docker 未安装，跳过镜像构建"
        return 0
    fi

    log_step "构建 Docker 镜像..."
    cd "$PROJECT_ROOT"

    # 构建镜像
    if ! docker build --progress=plain -t "lm-web:${APP_VERSION}" -f web/Dockerfile web/; then
        log_error "Docker 镜像构建失败"
        exit 1
    fi

    # 打 latest 标签
    docker tag "lm-web:${APP_VERSION}" "lm-web:latest"

    log_info "Docker 镜像构建完成:"
    log_info "  - lm-web:${APP_VERSION}"
    log_info "  - lm-web:latest"
}

# ============================================
# 显示构建结果
# ============================================
show_result() {
    log_step "构建结果"
    
    echo ""
    echo "========================================"
    echo "版本: $APP_VERSION"
    echo "模式: $BUILD_MODE"
    echo "========================================"
    echo ""
    echo "发布目录: $PUBLISH_DIR"
    echo "目录大小: $(du -sh "$PUBLISH_DIR" 2>/dev/null | cut -f1 || echo 'N/A')"
    echo ""
    echo "文件列表:"
    ls -la "$PUBLISH_DIR" 2>/dev/null || echo "  (目录为空)"
    echo ""
    echo "========================================"
    log_info "前端打包完成!"
    echo "========================================"
}

# ============================================
# 主流程
# ============================================
main() {
    parse_args "$@"

    echo ""
    echo "========================================"
    echo "  前端打包脚本"
    echo "  版本: $APP_VERSION | 模式: $BUILD_MODE"
    echo "========================================"
    echo ""

    check_env
    clean
    install_deps
    build
    copy_dist
    build_docker_image
    show_result
}

# 执行主流程
main "$@"
