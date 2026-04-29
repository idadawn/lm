# ═══════════════════════════════════════════════════════════════
# nlq-agent Dockerfile
# 多阶段构建：减小最终镜像体积
# ═══════════════════════════════════════════════════════════════

# ── Stage 1: 依赖安装 ────────────────────────────────────────
FROM python:3.11-slim AS builder

WORKDIR /app

# 安装系统依赖
RUN apt-get update && apt-get install -y --no-install-recommends \
    gcc \
    default-libmysqlclient-dev \
    && rm -rf /var/lib/apt/lists/*

# 安装 Python 依赖
COPY requirements.txt .
RUN pip install --no-cache-dir --prefix=/install -r requirements.txt

# ── Stage 2: 运行时镜像 ──────────────────────────────────────
FROM python:3.11-slim

WORKDIR /app

# 从 builder 复制已安装的依赖
COPY --from=builder /install /usr/local

# 复制应用代码
COPY src/ ./src/
COPY scripts/ ./scripts/

# 创建非 root 用户
RUN useradd -m -r nlqagent && chown -R nlqagent:nlqagent /app
USER nlqagent

# 环境变量
ENV PYTHONUNBUFFERED=1
ENV PYTHONPATH=/app

# 暴露端口
EXPOSE 18100

# 健康检查
HEALTHCHECK --interval=30s --timeout=10s --start-period=15s --retries=3 \
    CMD python -c "import httpx; r = httpx.get('http://localhost:18100/health'); r.raise_for_status()" || exit 1

# 启动命令
CMD ["uvicorn", "src.main:app", "--host", "0.0.0.0", "--port", "18100", "--workers", "2"]
