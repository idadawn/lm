<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# core

## Purpose
nlq-agent 核心配置层。基于 pydantic-settings 提供单例 `Settings`，从环境变量 / `.env` 加载 LLM、Embedding、Qdrant、MySQL、SSE 等所有运行时参数。所有模块通过 `get_settings()` 获取配置，便于测试时替换。

## Key Files
| File | Description |
|------|-------------|
| `settings.py` | `Settings` BaseSettings 类 + `@lru_cache get_settings()`；包含 LLM (vLLM / OpenAI)、TEI Embedding、Qdrant collection 名、MySQL 连接、检索 top_k、向量维度（bge-m3=1024）等 |
| `__init__.py` | 子包标记 |

## For AI Agents

### Working in this directory
- 所有可调参数必须通过 `Settings` 字段暴露，禁止散落 `os.getenv` 调用。
- 默认值要尽量"开发态可用"（如 `port=18100`、本地 vLLM `http://127.0.0.1:8082/v1`），生产配置通过环境变量覆盖。
- 新增字段时给出合理 default + 中文 inline 注释；保持节区分隔注释（`# ── XXX ──`）。
- `get_settings()` 用 `@lru_cache` 缓存实例；测试中如需替换，调用 `get_settings.cache_clear()`。
- Collection 名 (`collection_rules` / `collection_specs` / `collection_metrics`) 必须与 `nlq-semantic-layer` skill 文档保持一致。

### Common patterns
- 字段分组：服务基础 / LLM / Embedding / Qdrant / MySQL / 业务参数。
- 类型与默认值同时声明，避免 BaseSettings 推断异常。

## Dependencies
### Internal
- 全局调用方：`src/api/dependencies.py`、`src/services/*`、`src/pipelines/*`

### External
- `pydantic`、`pydantic-settings`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
