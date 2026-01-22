#!/bin/bash
# ============================================
# 实验室数据分析系统 - API 构建脚本
# 优化版本：增量构建 + 并行编译 + 缓存优化
# ============================================

set -e

# ============================================
# 配置区域
# ============================================
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
API_ENTRY_DIR="${PROJECT_ROOT}/api/src/application/Poxiao.API.Entry"
PUBLISH_DIR="${PROJECT_ROOT}/publish/api"
RUNTIME="linux-x64"
CONFIGURATION="Release"

# 读取版本号
VERSION_FILE="${PROJECT_ROOT}/VERSION"
if [ -f "$VERSION_FILE" ]; then
    APP_VERSION=$(cat "$VERSION_FILE" | tr -d '[:space:]')
else
    APP_VERSION="latest"
    log_warn "VERSION 文件不存在，使用版本: $APP_VERSION"
fi

# 构建缓存目录
BUILD_CACHE_DIR="${PROJECT_ROOT}/.build-cache"
NUGET_PACKAGES_DIR="${BUILD_CACHE_DIR}/nuget"

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
# 检查并安装 .NET SDK
# ============================================
check_and_install_dotnet() {
    log_step "检查 .NET SDK..."

    if command -v dotnet &> /dev/null; then
        local dotnet_version=$(dotnet --version 2>&1)
        log_info ".NET SDK 已安装: $dotnet_version"
        return 0
    fi

    log_warn ".NET SDK 未安装，开始自动安装..."

    DOTNET_VERSION="10.0.102"
    DOTNET_INSTALL_DIR="$HOME/dotnet"
    DOTNET_FILE="dotnet-sdk-${DOTNET_VERSION}-linux-x64.tar.gz"
    DOTNET_URL="https://builds.dotnet.microsoft.com/dotnet/Sdk/${DOTNET_VERSION}/${DOTNET_FILE}"
    TEMP_DIR="/tmp/dotnet_install"

    mkdir -p "$TEMP_DIR"
    cd "$TEMP_DIR"

    log_info "下载 .NET SDK ${DOTNET_VERSION}..."
    if [ ! -f "$DOTNET_FILE" ]; then
        wget -O "$DOTNET_FILE" "$DOTNET_URL"
    fi

    mkdir -p "$DOTNET_INSTALL_DIR"

    log_info "安装 .NET SDK 到 $DOTNET_INSTALL_DIR..."
    tar zxf "$DOTNET_FILE" -C "$DOTNET_INSTALL_DIR"

    export DOTNET_ROOT="$DOTNET_INSTALL_DIR"
    export PATH="$PATH:$DOTNET_INSTALL_DIR"

    if ! grep -q "DOTNET_ROOT=$DOTNET_INSTALL_DIR" ~/.bashrc 2>/dev/null; then
        echo "" >> ~/.bashrc
        echo "# .NET SDK" >> ~/.bashrc
        echo "export DOTNET_ROOT=$DOTNET_INSTALL_DIR" >> ~/.bashrc
        echo "export PATH=\$PATH:\$DOTNET_ROOT" >> ~/.bashrc
        log_info "已将 .NET SDK 添加到 ~/.bashrc"
    fi

    cd "$PROJECT_ROOT"
    rm -rf "$TEMP_DIR"

    if command -v dotnet &> /dev/null; then
        local installed_version=$(dotnet --version 2>&1)
        log_info ".NET SDK 安装成功: $installed_version"
    else
        log_error ".NET SDK 安装失败"
        exit 1
    fi
}

# ============================================
# 准备构建缓存
# ============================================
prepare_build_cache() {
    log_step "准备构建缓存..."

    mkdir -p "$BUILD_CACHE_DIR"
    mkdir -p "$NUGET_PACKAGES_DIR"

    # 配置 NuGet 使用本地缓存
    export NUGET_PACKAGES="$NUGET_PACKAGES_DIR"

    log_info "NuGet 缓存目录: $NUGET_PACKAGES_DIR"
}

# ============================================
# 检查是否有代码变更
# ============================================
has_code_changes() {
    # 检查是否有编译输出
    if [ -d "$PUBLISH_DIR" ] && [ -f "$PUBLISH_DIR/Poxiao.API.Entry.dll" ]; then
        return 1  # 无变更
    fi
    return 0  # 有变更
}

# ============================================
# 清理旧发布文件
# ============================================
clean() {
    log_step "清理旧发布文件..."
    if [ -d "$PUBLISH_DIR" ]; then
        rm -rf "$PUBLISH_DIR"
        log_info "已清理: $PUBLISH_DIR"
    fi
    mkdir -p "$PUBLISH_DIR"
}

# ============================================
# 快速发布 API（增量构建）
# ============================================
publish_fast() {
    log_step "快速发布 API（增量构建）..."
    log_info "项目目录: $API_ENTRY_DIR"
    log_info "输出目录: $PUBLISH_DIR"
    log_info "运行时: $RUNTIME"
    log_info "配置: $CONFIGURATION"

    cd "$API_ENTRY_DIR"

    # 使用优化的构建参数
    dotnet publish "Poxiao.API.Entry.csproj" \
        -c "$CONFIGURATION" \
        -r "$RUNTIME" \
        --self-contained false \
        -o "$PUBLISH_DIR" \
        --no-restore \
        /p:IncrementalBuild=true \
        /p:SkipCompilerExecution=true \
        /p:UseSharedCompilation=true \
        /p:Deterministic=false \
        /p:OptimizeModular=true \
        /p:BuildInParallel=true \
        -m:1 \
        -v q || return 1

    log_info "发布完成!"
}

# ============================================
# 完整发布 API（用于首次构建或重大变更）
# ============================================
publish_full() {
    log_step "完整发布 API..."
    log_info "项目目录: $API_ENTRY_DIR"
    log_info "输出目录: $PUBLISH_DIR"
    log_info "运行时: $RUNTIME"
    log_info "配置: $CONFIGURATION"

    cd "$API_ENTRY_DIR"

    dotnet publish "Poxiao.API.Entry.csproj" \
        -c "$CONFIGURATION" \
        -r "$RUNTIME" \
        --self-contained false \
        -o "$PUBLISH_DIR" \
        /p:IncrementalBuild=true \
        /p:UseSharedCompilation=true \
        /p:Deterministic=false \
        /p:OptimizeModular=true \
        /p:BuildInParallel=true \
        -m:1 \
        -v q

    log_info "发布完成!"
}

# ============================================
# 复制必要文件
# ============================================
copy_resources() {
    log_step "复制必要文件..."

    if [ -d "${PROJECT_ROOT}/api/resources" ]; then
        cp -r "${PROJECT_ROOT}/api/resources" "$PUBLISH_DIR/"
        log_info "已复制: resources"
    else
        log_warn "resources 目录不存在: ${PROJECT_ROOT}/api/resources"
    fi

    if [ ! -d "$PUBLISH_DIR/Configurations" ]; then
        cp -r "${API_ENTRY_DIR}/Configurations" "$PUBLISH_DIR/"
        log_info "已复制: Configurations"
    fi

    if [ -d "${API_ENTRY_DIR}/lib" ]; then
        cp -r "${API_ENTRY_DIR}/lib" "$PUBLISH_DIR/"
        log_info "已复制: lib"
    fi
}

# ============================================
# 显示发布信息
# ============================================
show_info() {
    log_step "发布信息..."
    echo ""
    echo "发布目录: $PUBLISH_DIR"
    echo "大小: $(du -sh "$PUBLISH_DIR" | cut -f1)"
    echo ""
}

# ============================================
# 清理构建缓存
# ============================================
clean_cache() {
    log_step "清理构建缓存..."
    if [ -d "$BUILD_CACHE_DIR" ]; then
        rm -rf "$BUILD_CACHE_DIR"
        log_info "已清理构建缓存"
    fi
}

# ============================================
# 显示帮助信息
# ============================================
show_help() {
    echo "用法: $0 [选项]"
    echo ""
    echo "选项:"
    echo "  --fast, -f       快速构建（增量编译，默认）"
    echo "  --full, -F       完整构建（包括依赖恢复）"
    echo "  --clean-cache    清理构建缓存"
    echo "  --help, -h       显示此帮助信息"
    echo ""
    echo "示例:"
    echo "  $0              # 快速构建（推荐）"
    echo "  $0 --full       # 完整构建"
    echo "  $0 --clean-cache # 清理缓存"
    echo ""
}

# ============================================
# 主流程
# ============================================
main() {
    local BUILD_MODE="fast"  # 默认使用快速构建

    # 解析命令行参数
    while [[ $# -gt 0 ]]; do
        case $1 in
            --fast|-f)
                BUILD_MODE="fast"
                shift
                ;;
            --full|-F)
                BUILD_MODE="full"
                shift
                ;;
            --clean-cache)
                clean_cache
                exit 0
                ;;
            --help|-h)
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

    echo "=========================================="
    echo "API 构建脚本 (模式: $BUILD_MODE)"
    echo "=========================================="
    echo ""

    check_and_install_dotnet
    prepare_build_cache

    if [ "$BUILD_MODE" = "fast" ]; then
        if ! publish_fast; then
            log_warn "快速构建失败（可能是依赖未还原），尝试自动切换到完整构建..."
            publish_full
        fi
    else
        publish_full
    fi

    copy_resources
    show_info

    echo "=========================================="
    log_info "构建完成!"
    echo "=========================================="
}

main "$@"
