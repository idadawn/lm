"""vLLM LLM mixin for Vanna.

Calls the vLLM OpenAI-compatible /v1/chat/completions endpoint.
"""

from __future__ import annotations

import httpx


class VllmLlmMixin:
    """Mixin that provides LLM generation via a vLLM HTTP endpoint.

    Expected config keys:
        vllm_url (str):  OpenAI-compatible base URL ending in /v1
                         (vLLM / DeepSeek / OpenAI / 智谱 / DashScope)
        vllm_model (str): Model ID to pass in the request body
        llm_api_key (str): Bearer token (DeepSeek/OpenAI 必填；自托管 vLLM 留空)
        llm_disable_thinking (bool): 仅 Qwen3+ vLLM 部署时设 True
    """

    def __init__(self, config: dict) -> None:
        self._vllm_url: str = config["vllm_url"].rstrip("/")
        self._model: str = config["vllm_model"]
        self._api_key: str = str(config.get("llm_api_key", "")).strip()
        self._disable_thinking: bool = bool(config.get("llm_disable_thinking", False))
        self._http: httpx.Client = httpx.Client(timeout=120)

    # ------------------------------------------------------------------
    # Vanna abstract: message helpers
    # ------------------------------------------------------------------

    def system_message(self, message: str) -> dict[str, str]:
        return {"role": "system", "content": message}

    def user_message(self, message: str) -> dict[str, str]:
        return {"role": "user", "content": message}

    def assistant_message(self, message: str) -> dict[str, str]:
        return {"role": "assistant", "content": message}

    # ------------------------------------------------------------------
    # Vanna abstract: submit_prompt
    # ------------------------------------------------------------------

    def submit_prompt(self, prompt: list[dict], **kwargs: object) -> str:
        """Call vLLM /v1/chat/completions and return the assistant content.

        Args:
            prompt: List of message dicts (role/content), as returned by
                    system_message / user_message / assistant_message.

        Returns:
            The text content of the first choice's message.
        """
        body: dict = {
            "model": self._model,
            "messages": prompt,
            "temperature": 0.1,
            "max_tokens": 1024,
        }
        # 仅 Qwen3+ vLLM 自托管时启用：禁用 thinking 把 token 留给 SQL。
        # DeepSeek/OpenAI/云端 API 不支持此 param，会 400。
        if self._disable_thinking:
            body["chat_template_kwargs"] = {"enable_thinking": False}

        headers = {"Content-Type": "application/json"}
        if self._api_key:
            headers["Authorization"] = f"Bearer {self._api_key}"

        resp = self._http.post(
            f"{self._vllm_url}/chat/completions",
            headers=headers,
            json=body,
        )
        resp.raise_for_status()
        msg = resp.json()["choices"][0]["message"]
        # Qwen reasoning models put final answer in 'content', thoughts in
        # 'reasoning'. Some configs only emit reasoning and leave content null.
        return (msg.get("content") or msg.get("reasoning") or "")
