# nlq-agent

`nlq-agent` 是路美质量数据自然语言问数与知识图谱能力的独立工作区。当前主线已经统一到 **`services/agent-api`**，它承载 FastAPI、LangGraph Agent、知识图谱 API、Chat2SQL 兜底查询和 SSE 推理流协议。旧的两阶段 `src` 服务已经从根目录移出，归档到 **`legacy/two-stage-service`**，避免新旧后端入口并存造成误用。

## 当前目录结构

| 路径 | 定位 | 当前状态 |
|---|---|---|
| `services/agent-api/` | 主后端服务，包含 FastAPI 应用、LangGraph 工作流、工具函数、模型和测试。 | **主线维护** |
| `apps/web/` | nlq-agent 独立前端实验工作区。 | 保留 |
| `packages/shared-types/` | 前后端共享 TypeScript 类型定义。 | 保留 |
| `docs/` | 产品、技术设计、知识图谱、变更记录、审查报告和路线图文档。 | **统一文档入口** |
| `legacy/two-stage-service/` | 旧两阶段 Python 服务、旧测试、旧脚本和旧 Docker/Compose 资产。 | 只读归档 |
| `package.json`、`pnpm-workspace.yaml`、`turbo.json` | 前端与共享包工作区配置。 | 保留 |

## 推荐开发入口

后端主线开发请直接进入 `services/agent-api`。根目录不再保留旧 Python `src` 包，也不再把 `uvicorn src.main:app` 作为默认启动方式。

```bash
cd nlq-agent/services/agent-api
uv sync --extra dev
uv run uvicorn app.main:app --reload --port 8000
```

测试主线后端时，优先运行 `services/agent-api/tests` 下的用例。

```bash
cd nlq-agent/services/agent-api
uv run pytest
```

前端和共享类型仍使用 pnpm 工作区管理。

```bash
cd nlq-agent
pnpm install
pnpm type-check
```

## 文档导航

| 文档 | 内容 |
|---|---|
| `docs/development-guide.md` | nlq-agent 开发指南。 |
| `docs/architecture/langgraph-agent-api-architecture.md` | 当前 LangGraph / Agent API 架构说明。 |
| `docs/architecture/legacy-two-stage-architecture.md` | 旧两阶段架构记录，仅作历史参考。 |
| `docs/kg-validation-guide.md` | 知识图谱功能验证指南。 |
| `docs/kg-neo4j-design.md` | Neo4j 知识图谱设计文档。 |
| `docs/audits/nlq-agent-structure-audit.md` | 本次目录与实现审查报告。 |
| `docs/roadmap/development-plan.md` | 后续开发计划。 |

## 维护原则

本工作区后续应遵循 **一条主线、一个后端入口、一组主测试** 的原则。新增后端能力默认放入 `services/agent-api/app`；新增主线测试默认放入 `services/agent-api/tests`；新增设计或审查材料默认放入 `docs` 的对应子目录。除非明确做历史兼容验证，不应继续在 `legacy/two-stage-service` 中新增功能。
