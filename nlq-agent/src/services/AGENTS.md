<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# services

## Purpose
nlq-agent 的外部资源客户端层。把 LLM、Embedding、Qdrant、MySQL、SSE 协议封装为可注入的异步服务对象，供 pipelines 与 api 层消费。这一层是 IO 边界——所有外部网络/数据库调用都集中在此。

## Key Files
| File | Description |
|------|-------------|
| `llm_client.py` | `LLMClient`：基于 `AsyncOpenAI`，支持 vLLM / OpenAI 兼容端点，提供 chat 与 stream chat |
| `embedding_client.py` | `EmbeddingClient`：对接 TEI（text-embeddings-inference），支持单条/批量 embed，bge-m3 输出 1024 维 |
| `qdrant_service.py` | `QdrantService`：`AsyncQdrantClient` 封装；`ensure_collections` / `upsert` / `search` / 按 payload 过滤 |
| `database.py` | `DatabaseService`：`aiomysql` 连接池 + `_FORBIDDEN_PATTERNS` 安全白名单，仅允许 SELECT |
| `sse_emitter.py` | `SSEEmitter`：把 `ReasoningStep` / 文本 / metadata / error / done 序列化为 `data: {...}\n\n` SSE 行 |
| `__init__.py` | 子包标记 |

## For AI Agents

### Working in this directory
- 所有客户端构造函数零参（从 `get_settings()` 读取），便于在 `dependencies.py` 中按固定顺序初始化。
- 长连接资源（`httpx.AsyncClient`、aiomysql Pool、Qdrant 客户端）需要在 `shutdown_services()` 中显式关闭；新增服务时遵循同样契约。
- `database.py` 的禁词正则 `_FORBIDDEN_PATTERNS` 是最后一道安全防线——禁止放宽。如需写操作，请新建独立服务而非改动它。
- `SSEEmitter` 序列化必须是 `data: <json>\n\n` 双换行；前端解析依赖之。
- Qdrant collection 名从 `Settings` 读取，禁止硬编码。

### Common patterns
- 全异步 (`async def`) + try/except 局部包裹，错误向上抛由 stage agent 处理。
- 日志使用模块级 logger，关键路径打印连接信息（隐藏密码）。

## Dependencies
### Internal
- `src/core/settings.py`、`src/models/schemas.py`

### External
- `openai`, `qdrant-client`, `aiomysql`, `httpx`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
