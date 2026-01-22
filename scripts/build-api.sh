#!/bin/bash
# ============================================
# 实验室数据分析系统 - API 本地构建脚本
# 用于在本地发布 API 到指定目录，供 Docker 构建
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

    # 检查是否已安装 dotnet
    if command -v dotnet &> /dev/null; then
        local dotnet_version=$(dotnet --version 2>&1)
        log_info ".NET SDK 已安装: $dotnet_version"
        return 0
    fi

    log_warn ".NET SDK 未安装，开始自动安装..."

    # 配置
    DOTNET_VERSION="10.0.102"
    DOTNET_INSTALL_DIR="$HOME/dotnet"
    DOTNET_FILE="dotnet-sdk-${DOTNET_VERSION}-linux-x64.tar.gz"
    DOTNET_URL="https://builds.dotnet.microsoft.com/dotnet/Sdk/${DOTNET_VERSION}/${DOTNET_FILE}"
    TEMP_DIR="/tmp/dotnet_install"

    # 创建临时目录
    mkdir -p "$TEMP_DIR"
    cd "$TEMP_DIR"

    # 下载 .NET SDK
    log_info "下载 .NET SDK ${DOTNET_VERSION}..."
    if [ ! -f "$DOTNET_FILE" ]; then
        wget -O "$DOTNET_FILE" "$DOTNET_URL"
    fi

    # 创建安装目录
    mkdir -p "$DOTNET_INSTALL_DIR"

    # 解压
    log_info "安装 .NET SDK 到 $DOTNET_INSTALL_DIR..."
    tar zxf "$DOTNET_FILE" -C "$DOTNET_INSTALL_DIR"

    # 设置环境变量
    export DOTNET_ROOT="$DOTNET_INSTALL_DIR"
    export PATH="$PATH:$DOTNET_INSTALL_DIR"

    # 添加到 ~/.bashrc（如果还没有）
    if ! grep -q "DOTNET_ROOT=$DOTNET_INSTALL_DIR" ~/.bashrc 2>/dev/null; then
        echo "" >> ~/.bashrc
        echo "# .NET SDK" >> ~/.bashrc
        echo "export DOTNET_ROOT=$DOTNET_INSTALL_DIR" >> ~/.bashrc
        echo "export PATH=\$PATH:\$DOTNET_ROOT" >> ~/.bashrc
        log_info "已将 .NET SDK 添加到 ~/.bashrc"
    fi

    # 清理临时文件
    cd "$PROJECT_ROOT"
    rm -rf "$TEMP_DIR"

    # 验证安装
    if command -v dotnet &> /dev/null; then
        local installed_version=$(dotnet --version 2>&1)
        log_info ".NET SDK 安装成功: $installed_version"
    else
        log_error ".NET SDK 安装失败"
        exit 1
    fi
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
# 发布 API
# ============================================
publish() {
    log_step "发布 API..."
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
        /p:PublishTrimmed=false

    log_info "发布完成!"
}

# ============================================
# 复制必要文件
# ============================================
copy_resources() {
    log_step "复制必要文件..."

    # 复制 resources 目录
    if [ -d "${PROJECT_ROOT}/api/resources" ]; then
        cp -r "${PROJECT_ROOT}/api/resources" "$PUBLISH_DIR/"
        log_info "已复制: resources"
    else
        log_warn "resources 目录不存在: ${PROJECT_ROOT}/api/resources"
    fi

    # 确保 Configurations 目录存在
    if [ ! -d "$PUBLISH_DIR/Configurations" ]; then
        cp -r "${API_ENTRY_DIR}/Configurations" "$PUBLISH_DIR/"
        log_info "已复制: Configurations"
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
    log_info "现在可以运行以下命令构建 Docker 镜像:"
    echo ""
    echo "  docker build -t lm-api:latest -f api/Dockerfile ."
    echo ""
}

# ============================================
# 主流程
# ============================================
main() {
    echo "=========================================="
    echo "API 本地构建脚本"
    echo "=========================================="
    echo ""

    check_and_install_dotnet
    clean
    publish
    copy_resources
    show_info

    echo "=========================================="
    log_info "构建完成!"
    echo "=========================================="
}

main "$@"
