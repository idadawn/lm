"""
Application configuration via Pydantic Settings.

All values are read from environment variables (or a .env file loaded by
the process runner).  Use get_settings() — not Settings() directly — to
get the cached singleton.

Principle 5: Secrets come from environment — DB connection strings and model
endpoints are NEVER hardcoded in source.
"""

from __future__ import annotations

from functools import lru_cache

from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        case_sensitive=False,
        extra="ignore",
    )

    # ------------------------------------------------------------------
    # MySQL
    # ------------------------------------------------------------------
    mysql_host: str = "localhost"
    mysql_port: int = 3306
    mysql_user: str = "root"
    mysql_password: str = ""
    mysql_database: str = "lumei"

    # ------------------------------------------------------------------
    # Qdrant
    # ------------------------------------------------------------------
    qdrant_url: str = "http://localhost:6333"
    qdrant_collection: str = "nlq_vanna_knowledge"

    # ------------------------------------------------------------------
    # Embedding endpoint (OpenAI-compatible — supports OpenAI/智谱/DashScope/
    # SiliconFlow/Ollama/本地 vLLM 等任何遵循 /v1/embeddings 协议的服务)
    #
    # tei_url:          OpenAI-compatible base URL ending in /v1
    # embedding_model:  Model identifier the provider expects
    # embedding_api_key: Bearer token (empty = no auth header)
    # tei_embedding_dim: Expected vector dimension; verified on startup
    # ------------------------------------------------------------------
    tei_url: str = "https://api.openai.com/v1"
    embedding_model: str = ""
    embedding_api_key: str = ""
    tei_embedding_dim: int = 1536  # text-embedding-3-small 默认值；按所选模型调整

    # ------------------------------------------------------------------
    # vLLM
    # ------------------------------------------------------------------
    # OpenAI-compatible LLM endpoint — vLLM / DeepSeek / OpenAI / 智谱 / DashScope
    vllm_url: str = "http://localhost:8082/v1"
    vllm_model: str = ""
    # API key（DeepSeek/OpenAI 等云端服务必填；自托管 vLLM 通常留空）
    llm_api_key: str = ""
    # 是否禁用 thinking（仅 Qwen3+ vLLM 部署时生效；DeepSeek/OpenAI 设 false 让其忽略）
    llm_disable_thinking: bool = False

    # ------------------------------------------------------------------
    # Service
    # ------------------------------------------------------------------
    service_port: int = 8088
    uvicorn_workers: int = 1

    # ------------------------------------------------------------------
    # Knowledge versioning
    # ------------------------------------------------------------------
    knowledge_version: str = "v1"

    # ------------------------------------------------------------------
    # Auth (optional)
    # Empty string / None → auth disabled (no Bearer check).
    # Non-empty → strict Bearer check on all non-health endpoints.
    # ------------------------------------------------------------------
    auth_token: str | None = None

    # ------------------------------------------------------------------
    # Logging
    # ------------------------------------------------------------------
    log_level: str = "INFO"


@lru_cache(maxsize=1)
def get_settings() -> Settings:
    """Return the cached application settings singleton."""
    return Settings()
