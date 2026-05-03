<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
nlq-agent 微服务的 Python 源码根目录。承载 FastAPI 应用、两阶段问答 Pipeline（Stage 1 语义检索 + Stage 2 SQL 查询）、外部服务客户端（LLM / Embedding / Qdrant / MySQL）以及 Pydantic 数据模型。监听端口 18100，对外仅暴露 `/api/v1/chat/stream` 等少量端点。

## Key Files
| File | Description |
|------|-------------|
| `main.py` | FastAPI 应用工厂 + lifespan 生命周期 + uvicorn 启动入口 |
| `__init__.py` | 包初始化（空，仅为 Python 包识别） |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `api/` | HTTP 路由与 DI（依赖注入容器） (see `api/AGENTS.md`) |
| `core/` | 全局配置（Settings / `.env` 加载） (see `core/AGENTS.md`) |
| `models/` | Pydantic Schemas + DDL/SQL 模板常量 (see `models/AGENTS.md`) |
| `pipelines/` | Stage1/Stage2 Agent + Orchestrator (see `pipelines/AGENTS.md`) |
| `services/` | 外部服务客户端 (LLM/Embedding/Qdrant/MySQL/SSE) (see `services/AGENTS.md`) |
| `utils/` | Prompt 模板与通用工具 (see `utils/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 严格遵循分层依赖方向：`api → pipelines → services → models/core/utils`；禁止反向依赖（services 不可 import pipelines）。
- 新增模块时优先放进现有子目录，避免顶层文件膨胀；只有 `main.py` 例外。
- 全部使用 `from __future__ import annotations` 启用 PEP 563，并采用 Python 3.10+ 联合类型语法（`A | B`）。
- 日志使用 `logging.getLogger(__name__)`；禁止 print 调试。
- 服务单例由 `api/dependencies.py` 在 lifespan 内初始化与销毁，新增服务必须在 `init_services` / `shutdown_services` 中注册。

### Common patterns
- 异步优先（`async def`）+ `httpx.AsyncClient` / `aiomysql.Pool` / `AsyncOpenAI` / `AsyncQdrantClient`。
- Pydantic v2 BaseModel 定义所有跨进程数据。

## Dependencies
### Internal
- 测试入口 `nlq-agent/tests/`
- 协议同步 `nlq-agent/packages/shared-types/`

### External
- FastAPI, uvicorn, pydantic / pydantic-settings, openai, qdrant-client, aiomysql, httpx

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
