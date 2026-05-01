# nlq-agent: 路美实验室两阶段问答微服务

![Architecture](../diagrams/overall_architecture.png)

## 1. 项目简介

`nlq-agent` 是专为路美实验室数据分析系统设计的独立 Python 微服务。
它采用业界公认的**两阶段问答架构**（借鉴了 [WrenAI](https://github.com/Canner/WrenAI) 的核心设计理念），将处理流程拆分为：

1. **Stage 1 (Semantic & KG Agent)**：先懂业务。利用 Qdrant 检索判定规则、产品规格和指标定义，构建业务语义层。
2. **Stage 2 (Data & SQL Agent)**：再查数据。基于语义层的上下文和数据库 DDL，生成安全的只读 SQL 并执行，最终生成数据回答。

本服务完全兼容前端现有的 `<KgReasoningChain>` 组件，通过 SSE（Server-Sent Events）协议实时流式推送推理步骤（Reasoning Steps）和文本回答。

---

## 2. 核心特性

- **五类意图智能路由**：`statistical` / `trend` / `by_shift` / `root_cause` / `conceptual`，stage1 关键词预路由 + LLM 兜底分类，conceptual 直接答不走 SQL。
- **动态条件回填**：Stage 1 生成过滤条件（如"铁损 ≤ 0.80"），Stage 2 查询后回填实际值和满足状态，完美驱动前端渲染。
- **SQL 安全沙箱**：内置正则表达式拦截所有写操作，强制附加 `LIMIT` 保护，结合数据库只读账号实现双重安全。
- **自动 SQL 修正**：查询失败时自动将错误信息和 DDL 喂给 LLM 进行修正重试（最多 2 次）。
- **生产级输入硬化**：请求大小 / 用户消息长度 / 速率（30 req/min/IP）三层中间件，配合 `correlation_id` 透传与 `/metrics` Prometheus 暴露。
- **.NET 后端无缝集成**：提供 `/api/v1/sync/rules` 和 `/api/v1/sync/specs` 接口，支持业务规则变更时增量同步到 Qdrant。

---

## 3. 目录结构

```text
nlq-agent/
├── docker-compose.yml          # 完整服务编排（Agent + Qdrant + TEI）
├── Dockerfile                  # Agent 镜像构建
├── .env.example                # 环境变量配置模板
├── requirements.txt            # Python 依赖
├── pyproject.toml              # 项目配置
├── scripts/
│   └── init_semantic_layer.py  # 语义层初始化脚本（从 MySQL 同步到 Qdrant）
├── tests/                      # 测试用例
│   └── test_pipeline.py
├── packages/
│   └── shared-types/           # 与前端共享的 TypeScript 类型定义
│       └── reasoning-protocol.ts
└── src/
    ├── main.py                 # FastAPI 入口
    ├── api/                    # 路由与依赖注入
    │   ├── routes.py
    │   └── dependencies.py
    ├── core/                   # 核心配置
    │   └── settings.py
    ├── models/                 # 数据模型
    │   ├── schemas.py          # Pydantic 模型
    │   └── ddl.py              # 数据库 DDL 定义
    ├── pipelines/              # 核心业务逻辑
    │   ├── orchestrator.py     # 主编排器
    │   ├── stage1/
    │   │   └── semantic_kg_agent.py  # Stage 1 Agent
    │   └── stage2/
    │       └── data_sql_agent.py     # Stage 2 Agent
    ├── services/               # 基础设施服务
    │   ├── database.py         # MySQL 异步客户端
    │   ├── qdrant_service.py   # 向量检索客户端
    │   ├── embedding_client.py # TEI Embedding 客户端
    │   ├── llm_client.py       # OpenAI-compatible LLM 客户端
    │   └── sse_emitter.py      # SSE 事件发射器
    └── utils/                  # 工具类
        └── prompts.py          # 所有 Prompt 模板集中管理
```

---

## 4. 快速开始

### 4.1 环境准备

1. 复制配置模板并修改：
   ```bash
   cp .env.example .env
   # 编辑 .env，填入实际的 LLM_API_KEY 和 MYSQL_PASSWORD
   ```

2. 启动基础设施（Qdrant + TEI）：
   ```bash
   docker compose up -d qdrant tei
   ```

### 4.2 初始化语义层

在首次运行前，需要从业务数据库中读取判定规则和产品规格，向量化后写入 Qdrant。

```bash
# 建议在宿主机虚拟环境中执行
python -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt

# 运行初始化脚本
python scripts/init_semantic_layer.py
```

### 4.3 启动 Agent 服务

```bash
# 使用 Docker 启动
docker compose up -d nlq-agent

# 或者本地开发模式启动
uvicorn src.main:app --reload --port 18100
```

服务启动后，可通过 `http://localhost:18100/docs` 查看 Swagger API 文档。

---

## 5. API 接口说明

### 5.1 核心问答端点

**POST `/api/v1/chat/stream`**

处理用户提问，返回 SSE 事件流。

**请求体 (JSON):**
```json
{
  "messages": [
    {"role": "user", "content": "本月甲班的合格率是多少？"}
  ]
}
```

**响应 (text/event-stream):**
```text
data: {"type": "reasoning_step", "reasoning_step": {"kind": "rule", "label": "A类判定规则", ...}}

data: {"type": "reasoning_step", "reasoning_step": {"kind": "condition", "label": "铁损", "field": "F_PERF_PS_LOSS", "expected": "<= 0.80"}}

data: {"type": "reasoning_step", "reasoning_step": {"kind": "grade", "label": "合格率: 95.2%"}}

data: {"type": "text", "content": "本月甲班的合格率为"}
data: {"type": "text", "content": "95.2%。"}

data: {"type": "response_metadata", "response_payload": {"sql": "SELECT ...", "row_count": 1}}

data: {"type": "done"}
```

### 5.2 知识同步端点

由 .NET 后端在规则或规格发生变更时调用。

- **POST `/api/v1/sync/rules`**
- **POST `/api/v1/sync/specs`**

---

## 6. 开发与测试

```bash
# 默认（无 live 服务，无 load）— CI 跑这个
uv run pytest tests/ -m "not live_llm and not live_qdrant and not load"

# 带真 LLM/Qdrant
uv run pytest tests/ -m "live_llm or live_qdrant"

# 并发负载
uv run pytest tests/ -m load
```

测试 marker 在 `pyproject.toml [tool.pytest.ini_options]` 注册。

---

## 7. 生产部署

```bash
# 启动
docker compose -f docker-compose.production.yml --env-file .env.production up -d

# 升级
docker compose -f docker-compose.production.yml pull
docker compose -f docker-compose.production.yml up -d

# 监控
curl http://localhost:18100/health      # 健康检查
curl http://localhost:18100/metrics     # Prometheus 指标
docker compose logs -f nlq-agent        # 结构化 JSON 日志（设 LOG_FORMAT=json）
```

详细参考：

- [`API.md`](./API.md) — 完整 API 接口 + 错误码 + Production Deployment 段
- [`CONTRIBUTING.md`](./CONTRIBUTING.md) — 本地开发 + commit/branch 规范 + load test 跑法
- [`PRODUCTION_CHECKLIST.md`](./PRODUCTION_CHECKLIST.md) — 上线前 ✅/❌/⚠️ 清单（Security / Observability / Performance / Reliability / Operations）
- [`scripts/verify_env.py`](./scripts/verify_env.py) — `.env` / `.env.production` 配置验证 + 服务可达性 ping
- [`../docs/RUNBOOK_NLQ_E2E.md`](../docs/RUNBOOK_NLQ_E2E.md) — 真实 E2E 跑通流程 + 8 个故障排查 recipe

---

## 8. 架构设计参考

本项目的两阶段架构设计深度借鉴了 [WrenAI](https://github.com/Canner/WrenAI) 的 MDL（语义层）概念。详细的架构对比和时序图请参考：

- [两阶段问答流程时序图](../diagrams/two_stage_flow.png)
- [问题路由与处理路径](../diagrams/query_routing.png)
- [WrenAI 对比借鉴](../diagrams/wrenai_vs_luma.png)
