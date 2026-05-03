"""
LLM 客户端封装

统一封装 OpenAI-compatible API 调用，支持 vLLM / OpenAI / 其他兼容端点。
提供同步调用和流式调用两种模式。
"""

from __future__ import annotations

import json
import logging
from typing import AsyncIterator

from openai import AsyncOpenAI

from src.core.settings import get_settings

logger = logging.getLogger(__name__)


class LLMClient:
    """异步 LLM 客户端，基于 OpenAI SDK。"""

    def __init__(self) -> None:
        settings = get_settings()
        self._client = AsyncOpenAI(
            base_url=settings.llm_base_url,
            api_key=settings.llm_api_key,
        )
        self._model = settings.llm_model
        self._temperature = settings.llm_temperature
        self._max_tokens = settings.llm_max_tokens

    async def chat(
        self,
        messages: list[dict[str, str]],
        *,
        temperature: float | None = None,
        max_tokens: int | None = None,
        response_format: dict | None = None,
    ) -> str:
        """
        非流式调用 LLM，返回完整文本。

        Args:
            messages: OpenAI 格式的消息列表
            temperature: 覆盖默认温度
            max_tokens: 覆盖默认最大 token 数
            response_format: JSON mode 配置（如 {"type": "json_object"}）

        Returns:
            LLM 生成的完整文本
        """
        kwargs: dict = {
            "model": self._model,
            "messages": messages,
            "temperature": temperature or self._temperature,
            "max_tokens": max_tokens or self._max_tokens,
        }
        if response_format:
            kwargs["response_format"] = response_format

        try:
            response = await self._client.chat.completions.create(**kwargs)
            if not response.choices:
                logger.error("LLM 调用返回空 choices")
                raise RuntimeError("LLM 返回空 choices")
            return response.choices[0].message.content or ""
        except Exception as e:
            logger.error("LLM 调用失败: %s", e)
            raise

    async def chat_stream(
        self,
        messages: list[dict[str, str]],
        *,
        temperature: float | None = None,
        max_tokens: int | None = None,
    ) -> AsyncIterator[str]:
        """
        流式调用 LLM，逐 chunk 返回文本。

        Yields:
            每个 chunk 的文本内容
        """
        try:
            stream = await self._client.chat.completions.create(
                model=self._model,
                messages=messages,
                temperature=temperature or self._temperature,
                max_tokens=max_tokens or self._max_tokens,
                stream=True,
            )
            async for chunk in stream:
                if not chunk.choices:
                    continue
                delta = chunk.choices[0].delta
                if not delta:
                    continue
                content = delta.content
                if not content:
                    continue
                yield content
        except Exception as e:
            logger.error("LLM 流式调用失败: %s", e)
            raise

    async def chat_json(
        self,
        messages: list[dict[str, str]],
        *,
        temperature: float | None = None,
    ) -> dict:
        """
        调用 LLM 并解析 JSON 响应。

        Returns:
            解析后的 dict
        """
        raw = await self.chat(
            messages,
            temperature=temperature,
            response_format={"type": "json_object"},
        )
        # 尝试提取 JSON（处理 LLM 可能包裹 markdown code block 的情况）
        text = raw.strip()
        if text.startswith("```"):
            lines = text.split("\n")
            text = "\n".join(lines[1:-1])
        return json.loads(text)

    async def health_check(self) -> bool:
        """检查 LLM 服务是否可用。"""
        try:
            await self.chat(
                [{"role": "user", "content": "ping"}],
                max_tokens=5,
            )
            return True
        except Exception:
            return False
