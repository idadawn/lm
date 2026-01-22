#!/bin/bash
# ============================================
# 自动构建设置脚本
# 用于配置 Git hooks 自动构建
# ============================================

set -e

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
# 配置区域
# ============================================
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
HOOKS_DIR="${PROJECT_ROOT}/.git/hooks"
AUTO_BUILD_FLAG="${PROJECT_ROOT}/.auto-build"

# ============================================
# 启用自动构建
# ============================================
enable_auto_build() {
    log_step "启用自动构建..."

    # 创建 hook 文件
    cat > "${HOOKS_DIR}/post-commit" <<'HOOK'
#!/bin/bash
# ============================================
# Git post-commit hook - 自动构建
# ============================================

PROJECT_ROOT="$(git rev-parse --show-toplevel)"
VERSION_FILE="${PROJECT_ROOT}/VERSION"
BUILD_FLAG="${PROJECT_ROOT}/.auto-build"

# 检查是否启用自动构建
if [ ! -f "$BUILD_FLAG" ]; then
    exit 0
fi

# 检查 VERSION 文件是否被修改
if git diff HEAD~1 HEAD --quiet -- "$VERSION_FILE"; then
    exit 0
fi

echo "=========================================="
echo "检测到版本号变更，开始自动构建..."
echo "=========================================="

# 读取新版本号
NEW_VERSION=$(cat "$VERSION_FILE" | tr -d '[:space:]')
echo "新版本: $NEW_VERSION"
echo ""

# 异步构建
(
    sleep 2
    cd "$PROJECT_ROOT"

    echo "[$(date)] 开始构建 Docker 镜像..." >> "${PROJECT_ROOT}/.build.log"
    ./scripts/build.sh >> "${PROJECT_ROOT}/.build.log" 2>&1

    echo "[$(date)] 构建完成！" >> "${PROJECT_ROOT}/.build.log"
    echo "镜像标签: lm-api:${NEW_VERSION}, lm-web:${NEW_VERSION}" >> "${PROJECT_ROOT}/.build.log"
) &

echo "后台构建任务已启动..."
echo "查看构建日志: tail -f ${PROJECT_ROOT}/.build.log"
HOOK

    chmod +x "${HOOKS_DIR}/post-commit"

    # 创建启用标记
    touch "$AUTO_BUILD_FLAG"

    log_info "自动构建已启用"
    echo ""
    log_info "现在每次提交代码时（如果 VERSION 文件变更）将自动构建"
    echo ""
    log_warn "如需禁用，运行: $0 disable"
}

# ============================================
# 禁用自动构建
# ============================================
disable_auto_build() {
    log_step "禁用自动构建..."

    if [ -f "$AUTO_BUILD_FLAG" ]; then
        rm "$AUTO_BUILD_FLAG"
        log_info "自动构建已禁用"
    else
        log_warn "自动构建未启用"
    fi
}

# ============================================
# 查看状态
# ============================================
show_status() {
    log_step "自动构建状态"

    if [ -f "$AUTO_BUILD_FLAG" ]; then
        echo "状态: ${GREEN}已启用${NC}"
        echo "Hook 文件: ${HOOKS_DIR}/post-commit"
    else
        echo "状态: ${YELLOW}未启用${NC}"
    fi
}

# ============================================
# 显示帮助
# ============================================
show_help() {
    echo "用法: $0 <命令>"
    echo ""
    echo "命令:"
    echo "  enable    启用自动构建"
    echo "  disable   禁用自动构建"
    echo "  status    查看状态"
    echo "  help      显示帮助"
    echo ""
    echo "说明:"
    echo "  当启用自动构建后，每次提交代码时"
    echo "  如果 VERSION 文件有变更，会自动触发构建"
    echo ""
    echo "示例:"
    echo "  $0 enable   # 启用"
    echo "  $0 disable  # 禁用"
    echo "  $0 status   # 查看状态"
    echo ""
}

# ============================================
# 主流程
# ============================================
main() {
    local command="${1:-status}"

    case "$command" in
        enable)
            enable_auto_build
            ;;
        disable)
            disable_auto_build
            ;;
        status)
            show_status
            ;;
        help|--help|-h)
            show_help
            ;;
        *)
            log_error "未知命令: $command"
            echo ""
            show_help
            exit 1
            ;;
    esac
}

main "$@"
