# ============================================
# 前端 Web 镜像 - 基于 Nginx 的静态文件服务
# ============================================
FROM nginx:1.27-alpine

# 安装必要的工具
RUN apk add --no-cache curl

# 复制 Nginx 配置
COPY web/nginx.conf /etc/nginx/conf.d/default.conf

# 复制前端构建产物
COPY dist /usr/share/nginx/html

# 健康检查
HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

# 暴露端口
EXPOSE 80

# 启动 Nginx
CMD ["nginx", "-g", "daemon off;"]
