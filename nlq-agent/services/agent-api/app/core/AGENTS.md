<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-29 | Updated: 2026-04-29 -->

# core

## Purpose
横切关注点：配置、数据库连接、LLM 工厂、上游鉴权、日志。被几乎所有其它子包依赖，所以这里只能依赖标准库 + 第三方包，不能反向 import `agents/` `tools/` `api/`。

## Key Files

| File | Description |
|------|-------------|
| `config.py` | `Settings(BaseSettings)` 单例 — 读 nlq-agent 根目录 `.env`；含 DB / Redis / LiteLLM / vLLM / OpenAI / Gemini / Ollama / JWT / Poxiao 上游 / Neo4j / LangSmith / CORS 的全部字段；`DEFAULT_MODEL_NAME=deepseek-chat` |
| `database.py` | `engine = create_async_engine(settings.DATABASE_URL, pool_size, max_overflow, pool_pre_ping=True)` + `AsyncSessionLocal` sessionmaker + `get_db_session()` async generator |
| `llm_factory.py` | `get_llm(model_name=None, require_function_calling=False)` → `ChatOpenAI(base_url=LITELLM_BASE_URL, api_key=LITELLM_API_KEY, streaming=True)`；空名回落 `DEFAULT_MODEL_NAME` |
| `auth.py` | `validate_chat_auth(auth_context)` — 调上游 `POXIAO_API_BASE_URL/api/oauth/CurrentUser` 校验 Bearer token；权限 token 在 `REQUIRED_PERMISSION_MODULES`（默认 `lab`）；缓存 `AUTH_CACHE_TTL_SECONDS` 秒 |
| `logger.py` | `setup_logger("nlq-agent")` — stdout handler + `%(asctime)s - %(name)s - %(levelname)s - %(message)s`；模块级单例 `logger` |
| `__init__.py` | 包标记 |

## For AI Agents

### Working In This Directory
- 加新配置项：在 `Settings` 类加字段（含默认值）→ 在 nlq-agent 根 `.env.example` 同步加占位 → 在调用方 `from app.core.config import settings` 用 `settings.NEW_FIELD`。
- 新增 LLM 提供方：优先在 `litellm_config.yaml` 注册（统一通过 LiteLLM 网关），不要 `from langchain_xxx import ChatXxx` 绕过 `llm_factory`。
- 数据库：用 `AsyncSessionLocal()` async with，不要直接 `engine.connect()`（破坏 pool 语义）。
- 鉴权缓存键是 access_token 字符串本身（明文存 token），多 worker 部署前要换成 hash 或 Redis。

### Testing Requirements
- 单测：`tests/unit/`下没有专门的 core/ 测试文件，`config.py`/`database.py` 通过被测模块间接覆盖；`auth.py` 改动建议补 monkey-patched httpx 单测。

### Common Patterns
- `Settings.Config.env_file` 用绝对路径（向上 5 级到 nlq-agent/）— 任何脚本不在 `services/agent-api/` 启动也能读到 `.env`。
- `engine` 的 `echo=settings.APP_ENV == "development"`：开发时打印所有 SQL；生产关掉避免日志洪峰。

## Dependencies

### Internal
- 无（本目录是 utility 层，反向被所有人依赖）。

### External
- `pydantic-settings`、`sqlalchemy[asyncio]+aiomysql`、`langchain-openai`、`httpx`、`fastapi`（仅 `HTTPException`）。
