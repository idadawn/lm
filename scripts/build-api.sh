#!/bin/bash
# ============================================
# 瀹為獙瀹ゆ暟鎹垎鏋愮郴缁?- API 鏋勫缓鑴氭湰
# 浼樺寲鐗堟湰锛氬閲忔瀯寤?+ 骞惰缂栬瘧 + 缂撳瓨浼樺寲
# ============================================

set -e

# ============================================
# 閰嶇疆鍖哄煙
# ============================================
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
API_ENTRY_DIR="${PROJECT_ROOT}/api/src/application/Poxiao.API.Entry"
PUBLISH_DIR="${PROJECT_ROOT}/publish/api"
RUNTIME="linux-x64"
CONFIGURATION="Release"

# 璇诲彇鐗堟湰鍙?
VERSION_FILE="${PROJECT_ROOT}/VERSION"
if [ -f "$VERSION_FILE" ]; then
    APP_VERSION=$(cat "$VERSION_FILE" | tr -d '[:space:]')
else
    APP_VERSION="latest"
    log_warn "VERSION 鏂囦欢涓嶅瓨鍦紝浣跨敤鐗堟湰: $APP_VERSION"
fi

# 鏋勫缓缂撳瓨鐩綍
BUILD_CACHE_DIR="${PROJECT_ROOT}/.build-cache"
NUGET_PACKAGES_DIR="${BUILD_CACHE_DIR}/nuget"

# 棰滆壊杈撳嚭
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
# 妫€鏌ュ苟瀹夎 .NET SDK
# ============================================
check_and_install_dotnet() {
    log_step "妫€鏌?.NET SDK..."

    if command -v dotnet &> /dev/null; then
        local dotnet_version=$(dotnet --version 2>&1)
        log_info ".NET SDK 宸插畨瑁? $dotnet_version"
        return 0
    fi

    log_warn ".NET SDK 鏈畨瑁咃紝寮€濮嬭嚜鍔ㄥ畨瑁?.."

    DOTNET_VERSION="10.0.102"
    DOTNET_INSTALL_DIR="$HOME/dotnet"
    DOTNET_FILE="dotnet-sdk-${DOTNET_VERSION}-linux-x64.tar.gz"
    DOTNET_URL="https://builds.dotnet.microsoft.com/dotnet/Sdk/${DOTNET_VERSION}/${DOTNET_FILE}"
    TEMP_DIR="/tmp/dotnet_install"

    mkdir -p "$TEMP_DIR"
    cd "$TEMP_DIR"

    log_info "涓嬭浇 .NET SDK ${DOTNET_VERSION}..."
    if [ ! -f "$DOTNET_FILE" ]; then
        wget -O "$DOTNET_FILE" "$DOTNET_URL"
    fi

    mkdir -p "$DOTNET_INSTALL_DIR"

    log_info "瀹夎 .NET SDK 鍒?$DOTNET_INSTALL_DIR..."
    tar zxf "$DOTNET_FILE" -C "$DOTNET_INSTALL_DIR"

    export DOTNET_ROOT="$DOTNET_INSTALL_DIR"
    export PATH="$PATH:$DOTNET_INSTALL_DIR"

    if ! grep -q "DOTNET_ROOT=$DOTNET_INSTALL_DIR" ~/.bashrc 2>/dev/null; then
        echo "" >> ~/.bashrc
        echo "# .NET SDK" >> ~/.bashrc
        echo "export DOTNET_ROOT=$DOTNET_INSTALL_DIR" >> ~/.bashrc
        echo "export PATH=\$PATH:\$DOTNET_ROOT" >> ~/.bashrc
        log_info "宸插皢 .NET SDK 娣诲姞鍒?~/.bashrc"
    fi

    cd "$PROJECT_ROOT"
    rm -rf "$TEMP_DIR"

    if command -v dotnet &> /dev/null; then
        local installed_version=$(dotnet --version 2>&1)
        log_info ".NET SDK 瀹夎鎴愬姛: $installed_version"
    else
        log_error ".NET SDK 瀹夎澶辫触"
        exit 1
    fi
}

# ============================================
# 鍑嗗鏋勫缓缂撳瓨
# ============================================
prepare_build_cache() {
    log_step "鍑嗗鏋勫缓缂撳瓨..."

    mkdir -p "$BUILD_CACHE_DIR"
    mkdir -p "$NUGET_PACKAGES_DIR"

    # 閰嶇疆 NuGet 浣跨敤鏈湴缂撳瓨
    export NUGET_PACKAGES="$NUGET_PACKAGES_DIR"

    log_info "NuGet 缂撳瓨鐩綍: $NUGET_PACKAGES_DIR"
}

# ============================================
# 妫€鏌ユ槸鍚︽湁浠ｇ爜鍙樻洿
# ============================================
has_code_changes() {
    # 妫€鏌ユ槸鍚︽湁缂栬瘧杈撳嚭
    if [ -d "$PUBLISH_DIR" ] && [ -f "$PUBLISH_DIR/Poxiao.API.Entry.dll" ]; then
        return 1  # 鏃犲彉鏇?
    fi
    return 0  # 鏈夊彉鏇?
}

# ============================================
# 娓呯悊鏃у彂甯冩枃浠?
# ============================================
clean() {
    log_step "娓呯悊鏃у彂甯冩枃浠?.."
    if [ -d "$PUBLISH_DIR" ]; then
        rm -rf "$PUBLISH_DIR"
        log_info "宸叉竻鐞? $PUBLISH_DIR"
    fi
    mkdir -p "$PUBLISH_DIR"
}

# ============================================
# 蹇€熷彂甯?API锛堝閲忔瀯寤猴級
# ============================================
publish_fast() {
    log_step "蹇€熷彂甯?API锛堝閲忔瀯寤猴級..."
    log_info "椤圭洰鐩綍: $API_ENTRY_DIR"
    log_info "杈撳嚭鐩綍: $PUBLISH_DIR"
    log_info "杩愯鏃? $RUNTIME"
    log_info "閰嶇疆: $CONFIGURATION"

    cd "$API_ENTRY_DIR"

    # 浣跨敤浼樺寲鐨勬瀯寤哄弬鏁?
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

    log_info "鍙戝竷瀹屾垚!"
}

# ============================================
# 瀹屾暣鍙戝竷 API锛堢敤浜庨娆℃瀯寤烘垨閲嶅ぇ鍙樻洿锛?
# ============================================
publish_full() {
    log_step "瀹屾暣鍙戝竷 API..."
    log_info "椤圭洰鐩綍: $API_ENTRY_DIR"
    log_info "杈撳嚭鐩綍: $PUBLISH_DIR"
    log_info "杩愯鏃? $RUNTIME"
    log_info "閰嶇疆: $CONFIGURATION"

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

    log_info "鍙戝竷瀹屾垚!"
}

# ============================================
# 澶嶅埗蹇呰鏂囦欢
# ============================================
copy_resources() {
    log_step "Copy resources..."

    if [ -d "${PROJECT_ROOT}/api/resources" ]; then
        rm -rf "$PUBLISH_DIR/resources"
        cp -r "${PROJECT_ROOT}/api/resources" "$PUBLISH_DIR/"
        log_info "Copied: resources"
    else
        log_warn "resources not found: ${PROJECT_ROOT}/api/resources"
    fi

    if [ -d "${API_ENTRY_DIR}/Configurations" ]; then
        rm -rf "$PUBLISH_DIR/Configurations"
        cp -r "${API_ENTRY_DIR}/Configurations" "$PUBLISH_DIR/"
        log_info "Copied: Configurations"
    else
        log_warn "Configurations not found: ${API_ENTRY_DIR}/Configurations"
    fi

    if [ -d "${API_ENTRY_DIR}/lib" ]; then
        rm -rf "$PUBLISH_DIR/lib"
        cp -r "${API_ENTRY_DIR}/lib" "$PUBLISH_DIR/"
        log_info "Copied: lib"
    fi
}

# ============================================
# 鏄剧ず鍙戝竷淇℃伅
# ============================================
show_info() {
    log_step "鍙戝竷淇℃伅..."
    echo ""
    echo "鍙戝竷鐩綍: $PUBLISH_DIR"
    echo "澶у皬: $(du -sh "$PUBLISH_DIR" | cut -f1)"
    echo ""
}

# ============================================
# 娓呯悊鏋勫缓缂撳瓨
# ============================================
clean_cache() {
    log_step "娓呯悊鏋勫缓缂撳瓨..."
    if [ -d "$BUILD_CACHE_DIR" ]; then
        rm -rf "$BUILD_CACHE_DIR"
        log_info "宸叉竻鐞嗘瀯寤虹紦瀛?"
    fi
}

# ============================================
# 鏄剧ず甯姪淇℃伅
# ============================================
show_help() {
    echo "鐢ㄦ硶: $0 [閫夐」]"
    echo ""
    echo "閫夐」:"
    echo "  --fast, -f       蹇€熸瀯寤猴紙澧為噺缂栬瘧锛岄粯璁わ級"
    echo "  --full, -F       瀹屾暣鏋勫缓锛堝寘鎷緷璧栨仮澶嶏級"
    echo "  --clean-cache    娓呯悊鏋勫缓缂撳瓨"
    echo "  --help, -h       鏄剧ず姝ゅ府鍔╀俊鎭?"
    echo ""
    echo "绀轰緥:"
    echo "  $0              # 蹇€熸瀯寤猴紙鎺ㄨ崘锛?"
    echo "  $0 --full       # 瀹屾暣鏋勫缓"
    echo "  $0 --clean-cache # 娓呯悊缂撳瓨"
    echo ""
}

# ============================================
# 涓绘祦绋?
# ============================================
main() {
    local BUILD_MODE="fast"  # 榛樿浣跨敤蹇€熸瀯寤?

    # 瑙ｆ瀽鍛戒护琛屽弬鏁?
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
                log_error "鏈煡閫夐」: $1"
                show_help
                exit 1
                ;;
        esac
    done

    echo "=========================================="
    echo "API 鏋勫缓鑴氭湰 (妯″紡: $BUILD_MODE)"
    echo "=========================================="
    echo ""

    check_and_install_dotnet
    prepare_build_cache

    if [ "$BUILD_MODE" = "fast" ]; then
        if ! publish_fast; then
            log_warn "蹇€熸瀯寤哄け璐ワ紙鍙兘鏄緷璧栨湭杩樺師锛夛紝灏濊瘯鑷姩鍒囨崲鍒板畬鏁存瀯寤?.."
            publish_full
        fi
    else
        publish_full
    fi

    copy_resources
    show_info

    echo "=========================================="
    log_info "鏋勫缓瀹屾垚!"
    echo "=========================================="
}

main "$@"

