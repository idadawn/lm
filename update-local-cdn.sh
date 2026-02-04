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
