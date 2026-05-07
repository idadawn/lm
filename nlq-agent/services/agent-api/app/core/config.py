"""应用配置模块."""

import os

from pydantic_settings import BaseSettings

# 获取项目根目录（nlq-agent 目录）
# __file__ = .../services/agent-api/app/core/config.py
# 向上5级 = nlq-agent/
BASE_DIR = os.path.dirname(
    os.path.dirname(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))))
)


class Settings(BaseSettings):
    """应用配置类.

    从环境变量读取配置，支持 .env 文件。
    """

    # 应用配置
    APP_ENV: str = "development"
    APP_HOST: str = "0.0.0.0"
    APP_PORT: int = 8000
    APP_RELOAD: bool = True

    # 数据库配置
    DATABASE_URL: str = "mysql+aiomysql://root:password@localhost:3306/lab_db"
    DATABASE_POOL_SIZE: int = 10
    DATABASE_MAX_OVERFLOW: int = 20

    # Redis 配置
    REDIS_URL: str = "redis://localhost:6379/0"
    REDIS_PASSWORD: str = ""  # 单独配置，方便引用

    # LiteLLM 网关配置
    LITELLM_BASE_URL: str = "http://localhost:4000"
    LITELLM_API_KEY: str = "sk-litellm-master-key"
    # 默认模型——前端不再传 model_name 时由后端兜底（值需匹配 LITELLM_BASE_URL 提供方的实际模型名）。
    DEFAULT_MODEL_NAME: str = "deepseek-chat"

    # vLLM 配置
    VLLM_BASE_URL: str = "http://localhost:8080/v1"
    VLLM_API_KEY: str = "token-xxx"

    # 直连 LLM 配置（备选）
    OPENAI_API_KEY: str = ""
    OPENAI_BASE_URL: str = "https://api.openai.com/v1"
    GEMINI_API_KEY: str = ""
    OLLAMA_BASE_URL: str = "http://localhost:11434"

    # JWT 认证配置
    JWT_SECRET_KEY: str = "change-this-to-a-random-secret-at-least-32-chars"
    JWT_ALGORITHM: str = "HS256"
    JWT_EXPIRE_MINUTES: int = 1440

    # 上游主系统认证校验配置
    POXIAO_API_BASE_URL: str = "http://localhost:10089"
    POXIAO_CURRENT_USER_PATH: str = "/api/oauth/CurrentUser"
    AUTH_REQUIRED: bool = False
    AUTH_VALIDATE_UPSTREAM: bool = True
    AUTH_CACHE_TTL_SECONDS: int = 300
    REQUIRED_PERMISSION_MODULES: str = "lab"

    # LangSmith 可观测性
    LANGCHAIN_TRACING_V2: bool = False
    LANGCHAIN_API_KEY: str = ""
    LANGCHAIN_PROJECT: str = "nlq-agent"
    LANGCHAIN_ENDPOINT: str = "https://api.smith.langchain.com"

    # Neo4j 知识图谱配置
    NEO4J_URI: str = "bolt://localhost:7687"
    NEO4J_USER: str = "neo4j"
    NEO4J_PASSWORD: str = "password"
    NEO4J_DATABASE: str = "neo4j"
    NEO4J_ENABLED: bool = False  # 默认关闭，需要手动启用

    # Sentry 错误追踪
    SENTRY_DSN: str = ""

    # CORS 配置
    CORS_ORIGINS: list[str] = ["*"]

    # 前端配置（由 Next.js 使用，后端忽略）
    NEXT_PUBLIC_API_BASE_URL: str = "http://localhost:8000"
    NEXT_PUBLIC_APP_NAME: str = "NLQ-Agent"

    class Config:
        env_file = os.path.join(BASE_DIR, ".env")
        env_file_encoding = "utf-8"


settings = Settings()
