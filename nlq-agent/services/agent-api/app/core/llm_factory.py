"""LLM 工厂模块."""

from langchain_openai import ChatOpenAI

from app.core.config import settings


def get_llm(
    model_name: str | None = None,
    require_function_calling: bool = False,
    streaming: bool = True,
) -> ChatOpenAI:
    """获取 LLM 实例.

    通过 OpenAI 兼容网关统一接入多种模型（DeepSeek / SiliconFlow / LiteLLM 等）。
    空 model_name 回落到 settings.DEFAULT_MODEL_NAME。

    内置 3 次重试 + 较长超时，覆盖 DeepSeek 高峰期 “System is too busy (503)”
    这类瞬时过载错误——openai SDK 会自动按指数退避重试 5xx / 429。

    Args:
        model_name: 模型名称；空时取 .env 的 DEFAULT_MODEL_NAME
        require_function_calling: 是否需要函数调用支持（影响 temperature）
        streaming: 是否启用流式输出。chat2sql 内部的 schema_pick/column_pick/sql_draft
                   这些 JSON 解析步骤应该传 False，避免中间产物被 SSE 当 text 转发；
                   最终面向用户的 narrative 才用 True。

    Returns:
        ChatOpenAI: LangChain LLM 实例
    """
    resolved = model_name or settings.DEFAULT_MODEL_NAME
    return ChatOpenAI(
        model=resolved,
        base_url=settings.LITELLM_BASE_URL,
        api_key=settings.LITELLM_API_KEY,
        streaming=streaming,
        temperature=0 if require_function_calling else 0.1,
        # DeepSeek 高峰期 503 / 429 通常 10-30 秒恢复，重试 3 次 + 长超时已经足够吃掉一次抖动。
        max_retries=3,
        timeout=90,
    )
