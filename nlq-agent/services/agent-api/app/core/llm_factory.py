"""LLM 工厂模块."""

from langchain_openai import ChatOpenAI

from app.core.config import settings


def get_llm(
    model_name: str | None = None,
    require_function_calling: bool = False,
) -> ChatOpenAI:
    """获取 LLM 实例.

    通过 LiteLLM 网关统一接入多种模型。空 model_name 回落到 settings.DEFAULT_MODEL_NAME。

    Args:
        model_name: 模型名称；空时取 .env 的 DEFAULT_MODEL_NAME（默认 deepseek-chat）
        require_function_calling: 是否需要函数调用支持

    Returns:
        ChatOpenAI: LangChain LLM 实例
    """
    resolved = model_name or settings.DEFAULT_MODEL_NAME
    return ChatOpenAI(
        model=resolved,
        base_url=settings.LITELLM_BASE_URL,
        api_key=settings.LITELLM_API_KEY,
        streaming=True,
        temperature=0 if require_function_calling else 0.1,
    )
