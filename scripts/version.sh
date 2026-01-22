#!/bin/bash
# ============================================
# 版本管理脚本
# 用于管理项目版本号
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
# 配置
# ============================================
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
VERSION_FILE="${PROJECT_ROOT}/VERSION"

# ============================================
# 显示当前版本
# ============================================
show_version() {
    if [ -f "$VERSION_FILE" ]; then
        local version=$(cat "$VERSION_FILE" | tr -d '[:space:]')
        log_info "当前版本: $version"
        return 0
    else
        log_error "VERSION 文件不存在"
        return 1
    fi
}

# ============================================
# 设置新版本
# ============================================
set_version() {
    local new_version="$1"

    if [ -z "$new_version" ]; then
        log_error "请指定新版本号"
        echo "用法: $0 set <版本号>"
        echo "示例: $0 set 1.0.1"
        return 1
    fi

    # 验证版本号格式 (语义化版本)
    if ! [[ "$new_version" =~ ^[0-9]+\.[0-9]+\.[0-9]+(-[a-zA-Z0-9.]+)?$ ]]; then
        log_error "无效的版本号格式: $new_version"
        echo "版本号格式应为: X.Y.Z 或 X.Y.Z-预发布标识"
        echo "示例: 1.0.0, 1.0.1-beta, 2.0.0-rc.1"
        return 1
    fi

    local old_version=""
    if [ -f "$VERSION_FILE" ]; then
        old_version=$(cat "$VERSION_FILE" | tr -d '[:space:]')
    fi

    echo "$new_version" > "$VERSION_FILE"

    log_step "版本已更新"
    if [ -n "$old_version" ]; then
        log_info "旧版本: $old_version"
    fi
    log_info "新版本: $new_version"
    echo ""
    log_warn "请提交 VERSION 文件的变更到 Git"
    echo "  git add VERSION"
    echo "  git commit -m \"chore: bump version to $new_version\""
}

# ============================================
# 增加版本号
# ============================================
bump_version() {
    local part="$1"  # major, minor, or patch

    if [ -z "$part" ]; then
        part="patch"
    fi

    if [ ! -f "$VERSION_FILE" ]; then
        log_error "VERSION 文件不存在"
        return 1
    fi

    local current_version=$(cat "$VERSION_FILE" | tr -d '[:space:]')

    # 解析版本号
    local major=$(echo "$current_version" | cut -d'.' -f1)
    local minor=$(echo "$current_version" | cut -d'.' -f2)
    local patch=$(echo "$current_version" | cut -d'.' -f3 | cut -d'-' -f1)

    case "$part" in
        major)
            major=$((major + 1))
            minor=0
            patch=0
            ;;
        minor)
            minor=$((minor + 1))
            patch=0
            ;;
        patch)
            patch=$((patch + 1))
            ;;
        *)
            log_error "无效的版本部分: $part"
            echo "用法: $0 bump [major|minor|patch]"
            return 1
            ;;
    esac

    local new_version="${major}.${minor}.${patch}"
    set_version "$new_version"
}

# ============================================
# 获取版本历史
# ============================================
version_history() {
    log_step "版本历史（最近 10 次）"
    echo ""

    # 从 Git log 中提取版本变更
    if [ -d "$PROJECT_ROOT/.git" ]; then
        git log --all --oneline --grep="bump version" -10 2>/dev/null || log_warn "无法获取 Git 历史"
    else
        log_warn "不是 Git 仓库"
    fi
}

# ============================================
# 显示帮助
# ============================================
show_help() {
    echo "用法: $0 <命令> [参数]"
    echo ""
    echo "命令:"
    echo "  show                  显示当前版本"
    echo "  set <版本号>          设置新版本"
    echo "  bump [major|minor|patch]  增加版本号（默认 patch）"
    echo "  history              显示版本历史"
    echo "  help                 显示此帮助"
    echo ""
    echo "示例:"
    echo "  $0 show              # 显示当前版本"
    echo "  $0 set 1.0.1         # 设置版本为 1.0.1"
    echo "  $0 bump patch        # 增加补丁版本 (1.0.0 -> 1.0.1)"
    echo "  $0 bump minor        # 增加次版本 (1.0.0 -> 1.1.0)"
    echo "  $0 bump major        # 增加主版本 (1.0.0 -> 2.0.0)"
    echo ""
}

# ============================================
# 主流程
# ============================================
main() {
    local command="${1:-show}"

    case "$command" in
        show)
            show_version
            ;;
        set)
            set_version "$2"
            ;;
        bump)
            bump_version "$2"
            ;;
        history)
            version_history
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
