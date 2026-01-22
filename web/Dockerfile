# ============================================
# 实验室数据分析系统 - 前端 Dockerfile
# 简化版：直接拷贝本地构建的 dist 文件
# ============================================

FROM nginx:stable-alpine

# 使用阿里云 Alpine 镜像源
RUN sed -i 's/dl-cdn.alpinelinux.org/mirrors.aliyun.com/g' /etc/apk/repositories

# 安装 tzdata 设置时区
RUN apk add --no-cache \
    tzdata \
    curl \
    && cp /usr/share/zoneinfo/Asia/Shanghai /etc/localtime \
    && echo "Asia/Shanghai" > /etc/timezone

# 直接拷贝本地构建的 dist 文件
COPY dist /usr/share/nginx/html

# 复制 nginx 配置
COPY conf/nginx.docker.conf /etc/nginx/conf.d/default.conf

# 创建非 root 用户并设置必要的目录权限
RUN addgroup -g 1001 -S www-data 2>/dev/null || true && \
    adduser -S -D -H -u 1001 -h /usr/share/nginx/html -s /sbin/nologin -G www-data -g www-data www-data 2>/dev/null || \
    adduser -S -D -H -u 1001 -h /usr/share/nginx/html -s /sbin/nologin www-data && \
    mkdir -p /var/cache/nginx /var/log/nginx /run/nginx && \
    chown -R www-data:www-data /usr/share/nginx/html /var/cache/nginx /var/log/nginx /run

# 暴露端口
EXPOSE 80

# 健康检查
HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost/ || exit 1

# 切换到非 root 用户
USER www-data

CMD ["nginx", "-g", "daemon off;"]
