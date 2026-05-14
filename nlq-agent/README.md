# nlq-agent

`nlq-agent` 是路美质量数据自然语言问数与知识图谱能力的独立工作区。当前后端主线已经统一到 **`services/agent-api`**，它承载 FastAPI、LangGraph Agent、知识图谱 API、Chat2SQL 兜底查询和 SSE 推理流协议。旧的两阶段 `src` 服务、旧测试、旧脚本和旧部署资产已经从主分支移除，避免新旧入口并存造成误用。

## 当前目录结构

| 路径 | 定位 | 当前状态 |
|---|---|---|
| `services/agent-api/` | 主后端服务，包含 FastAPI 应用、LangGraph 工作流、工具函数、模型和测试。 | **主线维护** |
| `apps/web/` | nlq-agent 独立前端实验工作区。 | 保留 |
| `packages/shared-types/` | 前后端共享 TypeScript 类型定义。 | 保留 |
| `docs/` | 产品、技术设计、知识图谱、变更记录、审查报告和路线图文档。 | **统一文档入口** |
| `package.json`、`pnpm-workspace.yaml`、`turbo.json` | 前端与共享包工作区配置。 | 保留 |

## 推荐开发入口

后端主线开发请直接进入 `services/agent-api`。根目录不再保留旧 Python `src` 包，也不再把 `uvicorn src.main:app` 作为启动方式。

```bash
cd nlq-agent/services/agent-api
uv sync --extra dev
uv run uvicorn app.main:app --reload --port 8000
```

测试主线后端时，运行 `services/agent-api/tests` 下的用例。

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
| `docs/architecture/legacy-two-stage-architecture.md` | 历史两阶段设计记录，仅用于理解演进背景，不再对应主分支运行代码。 |
| `docs/kg-validation-guide.md` | 知识图谱功能验证指南。 |
| `docs/kg-neo4j-design.md` | Neo4j 知识图谱设计文档。 |
| `docs/audits/nlq-agent-structure-audit.md` | 目录与实现审查报告。 |
| `docs/roadmap/development-plan.md` | 后续开发计划。 |

## 维护原则

本工作区后续遵循 **一条主线、一个后端入口、一组主测试** 的原则。新增后端能力默认放入 `services/agent-api/app`；新增主线测试默认放入 `services/agent-api/tests`；新增设计或审查材料默认放入 `docs` 的对应子目录。旧两阶段实现已经删除，若确需追溯历史代码，请通过 Git 历史查看删除前提交。
