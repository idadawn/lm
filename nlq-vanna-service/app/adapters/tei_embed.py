"""OpenAI-compatible embedding mixin for Vanna.

虽然文件/类名仍叫 TEI（避免改 vanna_app.py 的 mixin 链），但内部实现已改为
OpenAI `/v1/embeddings` 格式，可对接：
  - OpenAI 官方  https://api.openai.com/v1
  - 智谱 BigModel https://open.bigmodel.cn/api/paas/v4
  - DashScope (通义) https://dashscope.aliyuncs.com/compatible-mode/v1
  - SiliconFlow  https://api.siliconflow.cn/v1
  - 本地 vLLM / Ollama / TEI(--enable-openai-compat)
  - 任何遵循 OpenAI Embeddings API 协议的服务

请求格式：
  POST {base_url}/embeddings  body={"input": <str|list[str]>, "model": <str>}
  Authorization: Bearer <api_key>

返回格式：
  {"data": [{"embedding": [...], "index": 0, "object": "embedding"}], "model": "...", "usage": {...}}
"""

from __future__ import annotations

import httpx


class TeiEmbedMixin:
    """OpenAI-compatible embedding mixin.

    Expected config keys:
        tei_url (str):           Base URL ending in /v1 (or compatible),
                                 e.g. "https://api.openai.com/v1"
        embedding_model (str):   Model identifier the provider expects.
        embedding_api_key (str): Bearer token (empty = no auth header).
        embedding_dim (int):     Expected vector length; verified at startup.
    """

    def __init__(self, config: dict) -> None:
        # base_url 必须以 /v1（或等价路径）结尾。我们直接拼 /embeddings。
        self._embed_base: str = config["tei_url"].rstrip("/")
        self._embedding_model: str = str(config.get("embedding_model", "")).strip()
        self._embedding_api_key: str = str(config.get("embedding_api_key", "")).strip()
        self._embedding_dim: int = int(config["embedding_dim"])
        self._http: httpx.Client = httpx.Client(timeout=60)

    def _headers(self) -> dict[str, str]:
        h: dict[str, str] = {"Content-Type": "application/json"}
        if self._embedding_api_key:
            h["Authorization"] = f"Bearer {self._embedding_api_key}"
        return h

    # ------------------------------------------------------------------
    # Startup validation
    # ------------------------------------------------------------------

    def verify_dim(self) -> None:
        """发一次最小探测请求，断言返回向量长度等于 config['embedding_dim']。

        Raises:
            RuntimeError: 维度不一致或无法探测时抛出（fast-fail）。
        """
        if not self._embedding_model:
            raise RuntimeError(
                "embedding_model 未配置；OpenAI 兼容模式必须设置 EMBEDDING_MODEL"
            )
        try:
            vec = self.generate_embedding("dim_probe")
        except Exception as exc:
            raise RuntimeError(
                f"embedding endpoint probe failed: {exc}. "
                f"Check EMBEDDING_BASE_URL / EMBEDDING_API_KEY / EMBEDDING_MODEL."
            ) from exc

        server_dim = len(vec)
        if server_dim != self._embedding_dim:
            raise RuntimeError(
                f"embedding_dim mismatch: server returned {server_dim}, "
                f"config has {self._embedding_dim}. "
                f"Update EMBEDDING_DIM (formerly TEI_EMBEDDING_DIM) to match the model."
            )

    # ------------------------------------------------------------------
    # Vanna abstract: generate_embedding
    # ------------------------------------------------------------------

    def generate_embedding(self, data: str, **kwargs: object) -> list[float]:
        """Call OpenAI-compatible /embeddings and return the single vector.

        Args:
            data: The text to embed.

        Returns:
            A flat list of floats representing the embedding vector.
        """
        resp = self._http.post(
            f"{self._embed_base}/embeddings",
            headers=self._headers(),
            json={"input": data, "model": self._embedding_model},
        )
        resp.raise_for_status()
        body = resp.json()
        # OpenAI shape: {"data": [{"embedding": [...], "index": 0}], ...}
        return list(body["data"][0]["embedding"])
