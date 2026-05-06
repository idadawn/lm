"""VannaApp: Qdrant + TEI + vLLM 组合实例。

MRO 设计要点（Architect Round 2 MUST-B）：
- 自定义 mixin (QdrantStoreMixin / TeiEmbedMixin / VllmLlmMixin) 必须放在
  VannaBase 之前（最左侧），确保自定义实现覆盖 VannaBase 默认实现。
- 各 mixin __init__ 必须显式调用，不依赖 super() 链，避免 MRO 歧义。
- MRO 解析顺序：VannaApp → QdrantStoreMixin → TeiEmbedMixin → VllmLlmMixin
  → VannaBase → object
"""

from __future__ import annotations

from vanna.base import VannaBase

from app.adapters.qdrant_store import QdrantStoreMixin
from app.adapters.tei_embed import TeiEmbedMixin
from app.adapters.vllm_llm import VllmLlmMixin


class VannaApp(QdrantStoreMixin, TeiEmbedMixin, VllmLlmMixin, VannaBase):
    """组合 Qdrant 向量库 + TEI embedding + vLLM 生成的 Vanna 实例。

    MRO 设计要点（Architect Round 2 MUST-B）：
    - 自定义 mixin (Qdrant/TEI/Vllm) 必须放在 VannaBase 之前（最左侧）
    - 这样自定义实现 override 默认 VannaBase 实现
    - 各 mixin __init__ 必须显式调用，不依赖 super() 链
    """

    def __init__(self, config: dict) -> None:
        VannaBase.__init__(self, config=config)
        QdrantStoreMixin.__init__(self, config=config)
        TeiEmbedMixin.__init__(self, config=config)
        VllmLlmMixin.__init__(self, config=config)
        self._emitter = None

    def attach_emitter(self, emitter: object) -> None:
        """Attach a reasoning emitter to the Qdrant store mixin."""
        self._emitter = emitter
        # Propagate to the store mixin's own reference so _emit() works.
        QdrantStoreMixin.attach_emitter(self, emitter)

    def detach_emitter(self) -> None:
        """Detach the reasoning emitter."""
        self._emitter = None
        QdrantStoreMixin.detach_emitter(self)


# ---------------------------------------------------------------------------
# Factory
# ---------------------------------------------------------------------------


def create_vanna_app() -> VannaApp:
    """Instantiate VannaApp from application settings.

    Reads configuration from app.config.get_settings() so that all
    secrets come from environment variables (Principle 5).

    Returns:
        A fully initialised VannaApp instance.
    """
    from app.config import get_settings  # noqa: PLC0415

    settings = get_settings()
    config: dict = {
        "qdrant_url": settings.qdrant_url,
        "tei_url": settings.tei_url,
        "vllm_url": settings.vllm_url,
        "vllm_model": settings.vllm_model,
        "llm_api_key": settings.llm_api_key,
        "llm_disable_thinking": settings.llm_disable_thinking,
        "embedding_dim": settings.tei_embedding_dim,
        "embedding_model": settings.embedding_model,
        "embedding_api_key": settings.embedding_api_key,
        "qdrant_collection": settings.qdrant_collection,
    }
    vn = VannaApp(config=config)

    # Wire Vanna's built-in MySQL connection so vn.run_sql() works.
    # Note: chat_stream.py enforces _check_sql_allowed() upstream of vn.run_sql,
    # so even though Vanna's default connection has no whitelist, the SELECT-only
    # invariant is still guaranteed at the request boundary.
    #
    # BOOTSTRAP_SKIP=1 — used in dev smoke tests when MySQL is unreachable;
    # service starts but vn.run_sql() will raise until reconnected.
    import os  # noqa: PLC0415

    if os.environ.get("BOOTSTRAP_SKIP", "").strip() != "1":
        vn.connect_to_mysql(
            host=settings.mysql_host,
            dbname=settings.mysql_database,
            user=settings.mysql_user,
            password=settings.mysql_password,
            port=settings.mysql_port,
        )
    return vn
