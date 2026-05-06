# =============================================================================
# Multi-stage Dockerfile for nlq-vanna-service
# Builder: ghcr.io/astral-sh/uv:python3.12-bookworm-slim (uv bundled)
# Runtime: python:3.12-slim
# =============================================================================

# ---------------------------------------------------------------------------
# Stage 1: builder — install dependencies with uv
# ---------------------------------------------------------------------------
FROM ghcr.io/astral-sh/uv:python3.12-bookworm-slim AS builder

WORKDIR /app

# Copy dependency manifests first (layer cache: only re-runs when deps change)
COPY pyproject.toml uv.lock ./

# Install runtime deps only (no project source yet) — frozen for reproducibility
RUN uv sync --frozen --no-install-project --no-dev

# Copy source code
COPY app/ ./app/

# Install the project itself into the venv
RUN uv sync --frozen --no-dev

# ---------------------------------------------------------------------------
# Stage 2: runtime — slim final image
# ---------------------------------------------------------------------------
FROM python:3.12-slim AS runtime

WORKDIR /app

# Copy the fully-populated virtual environment from builder
COPY --from=builder /app/.venv /app/.venv

# Copy application source
COPY --from=builder /app/app ./app

# Ensure the venv's bin is on PATH
ENV PATH="/app/.venv/bin:$PATH"

# Default port (overridable via SERVICE_PORT env and --port flag)
EXPOSE 8088

# NOTE: --workers 1 is intentional.
# Vanna's generate_sql() uses internal state that has NOT been fully audited
# for thread-safety across multiple OS processes sharing the same Qdrant
# collection. Running multiple workers risks race conditions during knowledge
# bootstrap and concurrent generate_sql calls. Use a single worker + async
# concurrency (asyncio.to_thread) instead.
CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8088", "--workers", "1"]
