<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# nlq-agent

## Purpose
路美实验室数据分析系统的两阶段自然语言问答（NL→SQL）微服务。基于 FastAPI + Python，借鉴 WrenAI 的 MDL 语义层思想，将用户自然语言问题拆分为：Stage 1（语义图谱检索）→ Stage 2（SQL 生成与执行），通过 SSE 流式回传 `ReasoningStep` 和文本回答给前端 `<KgReasoningChain>` 组件。该服务独立于 .NET 主后端，端口 18100。

## Key Files
| File | Description |
|------|-------------|
| `README.md` | 完整项目说明（架构、API、开发流程） |
| `Dockerfile` | Agent 镜像构建（Python 3.11 + uvicorn） |
| `docker-compose.yml` | 完整服务编排（agent + Qdrant + TEI） |
| `docker-compose.vectors.yml` | 仅向量基础设施编排（Qdrant + TEI 单独启动） |
| `requirements.txt` | Python 依赖（fastapi, qdrant-client, aiomysql, openai, pydantic v2） |
| `pyproject.toml` | 项目元数据与工具配置（black/isort/mypy） |
| `.env.example` | 环境变量模板（LLM/Qdrant/MySQL/TEI 配置） |
| `.env` | 本地实际配置（不入库） |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | FastAPI 应用源码：入口、路由、Pipeline、服务、数据模型（见 `src/AGENTS.md`） |
| `scripts/` | 运维脚本：从 MySQL 同步语义层到 Qdrant（见 `scripts/AGENTS.md`） |
| `skills/` | Claude-Code 风格的 5 个开发 Skills（见 `skills/AGENTS.md`） |
| `packages/` | 与前端共享的 TypeScript 类型定义（见 `packages/AGENTS.md`） |
| `tests/` | pytest 集成与单元测试（见 `tests/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 服务端口固定为 `18100`；修改前先确认前端 `web/src/api/nlqAgent.ts` 中的 baseURL。
- SSE 协议是前端契约，新增事件类型需同步更新 `packages/shared-types/src/reasoning-protocol.ts` 以及 `src/models/schemas.py`。
- 所有 SQL 必须通过 `DatabaseService.validate_sql()` 安全闸；只允许 SELECT，禁止写操作。
- 修改 Pipeline 行为后，运行 `pytest tests/ -v` 验证 SSE 协议字段不破坏。
- 业务知识（规则/规格/指标）变更后，需重跑 `scripts/init_semantic_layer.py` 或调用 `/api/v1/sync/*` 端点。

### Common patterns
- Pydantic v2 + 异步 (`async/await`) 全栈；FastAPI 通过 `Depends` 注入 LLM/Qdrant/DB 单例。
- LLM 调用走 OpenAI-compatible SDK，可指向本地 vLLM、TEI 兼容端点；JSON 模式用 `chat_json()`。
- 中文为主：日志、prompt、文档、字段名（display_name）一律中文。

## Dependencies
### Internal
- 前端 `web/src/api/nlqAgent.ts`（SSE 客户端）
- 移动端 `mobile/utils/sse-client.js`
- .NET 后端 MySQL 业务库（只读）

### External
- FastAPI / uvicorn / pydantic-settings
- qdrant-client (AsyncQdrantClient)
- aiomysql / openai (AsyncOpenAI) / httpx
- TEI (text-embeddings-inference, bge-m3, dim=1024)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
