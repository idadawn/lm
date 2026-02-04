#!/bin/bash
# ============================================
# 下载CDN依赖到本地脚本
# Usage: ./download-cdn-libs.sh
# ============================================

set -e

# 配置
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CDN_DIR="${SCRIPT_DIR}/web/public/cdn"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
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

# 创建CDN目录
log_info "Creating CDN directory..."
mkdir -p "$CDN_DIR/vue"
mkdir -p "$CDN_DIR/vue-router"
mkdir -p "$CDN_DIR/vue-demi"
mkdir -p "$CDN_DIR/pinia"
mkdir -p "$CDN_DIR/axios"
mkdir -p "$CDN_DIR/dayjs"
mkdir -p "$CDN_DIR/echarts"

# 下载Vue（使用可用版本）
log_info "Downloading Vue..."
curl -o "$CDN_DIR/vue/vue.global.prod.min.js" -L "https://unpkg.com/vue@3/dist/vue.global.prod.min.js" || log_warn "Vue download failed"

# 下载其他依赖
log_info "Downloading Vue Router..."
curl -o "$CDN_DIR/vue-router/vue-router.global.min.js" -L "https://unpkg.com/vue-router@4/dist/vue-router.global.min.js" || log_warn "Vue Router download failed"

log_info "Downloading Vue Demi..."
curl -o "$CDN_DIR/vue-demi/index.iife.min.js" -L "https://unpkg.com/vue-demi@0.14.5/lib/index.iife.min.js" || log_warn "Vue Demi download failed"

log_info "Downloading Pinia..."
curl -o "$CDN_DIR/pinia/pinia.iife.min.js" -L "https://unpkg.com/pinia@2/dist/pinia.iife.min.js" || log_warn "Pinia download failed"

log_info "Downloading Axios..."
curl -o "$CDN_DIR/axios/axios.min.js" -L "https://unpkg.com/axios@1/dist/axios.min.js" || log_warn "Axios download failed"

log_info "Downloading Dayjs..."
curl -o "$CDN_DIR/dayjs/dayjs.min.js" -L "https://unpkg.com/dayjs@1/dayjs.min.js" || log_warn "Dayjs download failed"

log_info "Downloading Echarts..."
curl -o "$CDN_DIR/echarts/echarts.min.js" -L "https://unpkg.com/echarts@5/dist/echarts.min.js" || log_warn "Echarts download failed"

# 检查下载结果
log_info "CDN download complete!"
ls -la "$CDN_DIR"

# 创建HTML模版更新脚本
cat > "${SCRIPT_DIR}/update-local-cdn.sh" << 'EOF'
#!/bin/bash
# 更新dist/index.html使用本地CDN引用

WEB_DIST="${SCRIPT_DIR}/web/dist/index.html"
BACKUP="${WEB_DIST}.backup"

# 备份原文件
cp "$WEB_DIST" "$BACKUP"

# 替换CDN引用为本地文件
sed -i 's|https://unpkg.com/vue@3.4.24/vue.global.prod.min.js|/cdn/vue/vue.global.prod.min.js|g' "$WEB_DIST" 2>/dev/null || true
sed -i 's|https://unpkg.com/vue@3.5.27/dist/vue.global.prod.min.js|/cdn/vue/vue.global.prod.min.js|g' "$WEB_DIST" 2>/dev/null || true
sed -i 's|https://unpkg.com/vue@3/dist/vue.global.prod.min.js|/cdn/vue/vue.global.prod.min.js|g' "$WEB_DIST" 2>/dev/null || true
sed -i 's|https://unpkg.com/vue-router@4.3.2/vue-router.global.min.js|/cdn/vue-router/vue-router.global.min.js|g' "$WEB_DIST"
sed -i 's|https://unpkg.com/vue-demi@0.14.5/lib/index.iife.min.js|/cdn/vue-demi/index.iife.min.js|g' "$WEB_DIST"
sed -i 's|https://unpkg.com/pinia@2.1.7/pinia.iife.min.js|/cdn/pinia/pinia.iife.min.js|g' "$WEB_DIST"
sed -i 's|https://unpkg.com/axios@1.6.8/axios.min.js|/cdn/axios/axios.min.js|g' "$WEB_DIST"
sed -i 's|https://unpkg.com/dayjs@1.11.10/dayjs.min.js|/cdn/dayjs/dayjs.min.js|g' "$WEB_DIST"
sed -i 's|https://unpkg.com/echarts@5.5.0/echarts.min.js|/cdn/echarts/echarts.min.js|g' "$WEB_DIST"

echo "✅ Local CDN references updated!"
EOF

chmod +x "${SCRIPT_DIR}/update-local-cdn.sh"

log_info "✅ CDN libraries downloaded successfully!"
log_info "Next steps:"
echo "  1. Run ./update-local-cdn.sh to update index.html"
echo "  2. Rebuild your application"
echo "  3. All CDN dependencies will work offline!"