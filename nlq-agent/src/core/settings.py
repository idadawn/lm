"""
nlq-agent 全局配置

通过环境变量或 .env 文件加载，所有配置项均有合理默认值。
"""

from __future__ import annotations

from functools import lru_cache

from pydantic import Field
from pydantic_settings import BaseSettings


class Settings(BaseSettings):
    """应用全局配置，自动从环境变量 / .env 文件读取。"""

    # ── 服务基础 ──────────────────────────────────────────────
    app_name: str = "nlq-agent"
    app_version: str = "0.1.0"
    host: str = "0.0.0.0"
    port: int = 18100
    debug: bool = False
    log_level: str = "INFO"

    # ── LLM 推理引擎 ─────────────────────────────────────────
    # 默认使用本地 vLLM，也可切换为 OpenAI 兼容端点
    llm_base_url: str = "http://127.0.0.1:8082/v1"
    llm_api_key: str = "EMPTY"
    llm_model: str = "Qwen2.5-7B-Instruct"
    llm_temperature: float = 0.1
    llm_max_tokens: int = 4096

    # ── Embedding 服务（TEI / text-embeddings-inference）─────
    embedding_base_url: str = "http://127.0.0.1:8081"
    embedding_model: str = "bge-m3"

    # ── Qdrant 向量数据库 ────────────────────────────────────
    qdrant_host: str = "127.0.0.1"
    qdrant_port: int = 6333
    qdrant_grpc_port: int = 6334
    qdrant_api_key: str | None = None

    # Qdrant Collection 名称
    collection_rules: str = "luma_rules"
    collection_specs: str = "luma_specs"
    collection_metrics: str = "luma_metrics"

    # 向量维度（bge-m3 默认 1024）
    embedding_dim: int = 1024
    # 检索 Top-K
    retrieval_top_k: int = 5

    # ── MySQL 业务数据库 ──────────────────────────────────────
    mysql_host: str = "127.0.0.1"
    mysql_port: int = 3307
    mysql_user: str = "root"
    mysql_password: str = ""
    mysql_database: str = "lumei"
    mysql_charset: str = "utf8mb4"

    # SQL 执行安全：最大返回行数
    sql_max_rows: int = 500
    # SQL 执行超时（秒）
    sql_timeout: int = 30

    # ── CORS ──────────────────────────────────────────────────────
    cors_allow_origins: str = ""  # comma-separated, empty → ["*"]

    # ── Error tracking ────────────────────────────────────────────
    sentry_dsn: str | None = None  # set SENTRY_DSN to enable

    # ── Admin token（bulk resync 端点鉴权）─────────────────────
    sync_admin_token: str = ""

    # ── .NET API 回调（语义层同步）────────────────────────────
    dotnet_api_base: str = "http://127.0.0.1:9530"

    model_config = {
        "env_file": ".env",
        "env_file_encoding": "utf-8",
        "case_sensitive": False,
        "extra": "ignore",
    }


@lru_cache
def get_settings() -> Settings:
    """单例获取配置对象（进程生命周期内缓存）。"""
    return Settings()
