# ============================================
# 后端 API 镜像 - 使用已发布的 .NET 应用
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

EXPOSE 9530

# 安装 curl 用于健康检查
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# 复制已发布的 API 应用程序
COPY api/ /app/

# 健康检查
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:9530/health || exit 1

# 启动应用
ENTRYPOINT ["dotnet", "Poxiao.API.Entry.dll"]