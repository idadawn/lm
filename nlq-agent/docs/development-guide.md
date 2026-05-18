# nlq-agent 开发指南

> **版本**: v2.0 | **日期**: 2026-05-16
>
> 本文档是路美实验室"知识图谱 + 自然语言问数"系统的开发与运维指南，涵盖后端 Agent 架构、Neo4j 知识图谱、前端 Ontology 可视化三个层面。

---

## 目录

1. [架构核心思想](#1-架构核心思想)
2. [两阶段问答架构](#2-两阶段问答架构)
3. [统计问题的智能路由](#3-统计问题的智能路由)
4. [语义层数据模型](#4-语义层数据模型)
5. [前端 Ontology 可视化](#5-前端-ontology-可视化)
6. [项目目录结构](#6-项目目录结构)
7. [关键代码设计详解](#7-关键代码设计详解)
8. [Neo4j 知识图谱](#8-neo4j-知识图谱)
9. [部署与运维](#9-部署与运维)
10. [实施路线图](#10-实施路线图)
11. [与现有代码库的集成点](#11-与现有代码库的集成点)

---

## 1. 架构核心思想

### 1.1 知识图谱的不可替代性

用户的统计问题看似简单（如"本月合格率是多少？"），但 LLM 直接面对数据库 DDL 时，并不知道什么是"合格率"、什么是"A 类产品"。WrenAI 项目（GitHub 15k+ Star）的核心设计理念正是**语义层（Semantic Layer / MDL）**——在生成 SQL 之前，必须先让 LLM "懂业务"。

在路美项目中，知识图谱存储的正是这些业务定义：

| 语义组件 | 存储内容 | 示例 | 向量库 Collection |
| :--- | :--- | :--- | :--- |
| **判定规则** | A/B/不合格的阈值条件 | 铁损 ≤ 0.80 W/kg → A类 | `luma_rules` |
| **产品规格** | 各牌号的参数标准 | 120 规格：标准厚度 0.23mm | `luma_specs` |
| **指标定义** | 统计指标的计算公式 | 合格率 = 三项全合格数/总数 | `luma_metrics` |

### 1.2 WrenAI 架构映射

![WrenAI 与路美架构映射](../diagrams/wrenai_vs_luma.png)

WrenAI 的三层 Pipeline（Indexing → Retrieval → Generation）被映射到路美的两阶段架构中。当 SQL 执行失败时，自动将错误信息喂给 LLM 进行修正重试。

---

## 2. 两阶段问答架构

![整体架构图](../diagrams/overall_architecture.png)

整个系统由 FastAPI 微服务（`services/agent-api`）承载，内部串联三个 Agent：

| Agent | 职责 | 对应文件 |
|-------|------|---------|
| **QueryAgent** | 意图分类 + 路由（统计查询 / 根因分析 / 概念解释 / 超出范围） | `app/agents/query_agent.py` |
| **Chat2SQL Agent** | Schema 选取 → 列选取 → SQL 起草 → 验证 → 执行 → 结果总结 | `app/agents/chat2sql_agent.py` |
| **RootCause Agent** | 单条记录的判等级根因解释（炉号 → 检测指标 → 判定公式 → 阈值 → 规格 → 等级） | `app/agents/root_cause_agent.py` |

三个阶段通过 SSE 事件流实时推送推理步骤到前端 `<KgReasoningPanel>` 组件。

---

## 3. 统计问题的智能路由

![查询路由分类图](../diagrams/query_routing.png)

| 意图类型 | 典型问题 | Agent 行为 |
| :--- | :--- | :--- |
| **statistical** | 本月合格率/各班次产量对比 | QueryAgent → Chat2SQL（生成 GROUP BY 聚合 SQL） |
| **root_cause** | 为什么昨天合格率低/哪些炉号超标 | QueryAgent → RootCauseAgent（遍历 KG 判定路径） |
| **conceptual** | A类是什么/铁损定义 | QueryAgent 直接返回知识（**跳过** SQL 查询） |
| **out_of_scope** | 天气怎么样 | 返回"超出范围"提示 |

路由逻辑实现在 `app/agents/query_agent.py` 的 `query_agent_node` 中。

---

## 4. 语义层数据模型

![MDL 语义层数据模型](../diagrams/mdl_semantic_layer.png)

语义层的初始化由 `app/knowledge_graph/schema_loader.py` 完成。该脚本从 MySQL 中读取判定规则、产品规格和公式定义，格式化为自然语言文本后向量化写入 Qdrant。此外，还包含预定义指标（合格率、产量、铁损、叠片系数、平均厚度）。

---

## 5. 前端 Ontology 可视化

前端知识图谱使用 `relation-graph-vue3` 渲染，以**带材(Ribbon)**为根节点，力导向布局展开产品规格、检测数据、炉号解析等业务节点。

**独立文档**：`nlq-agent/docs/kg-frontend-ontology.md`

| 节点层级 | 节点类型 | 数据来源 |
|---------|---------|---------|
| L1 根节点 | Ribbon（带材） | 虚拟节点 |
| L2 产品层 | ProductSpec（产品规格） | `lab_product_spec` |
| L2 数据层 | RawDataImport（叠片数据） | `lab_raw_data` |
| L2 数据层 | MagneticDataImport（单片性能） | `lab_magnetic_raw_data` |
| L2 标识层 | FurnaceNoInput / FurnaceNoParsed | 炉号解析规则 |
| L3 属性层 | SpecAttribute（扩展属性） | `lab_product_spec_attribute` |
| L3 字段层 | TemplateField（表字段） | `INFORMATION_SCHEMA.COLUMNS` |
| L3 计算层 | IntermediateData（中间数据） | `lab_intermediate_data` |
| L3 解析层 | FurnaceNoField（炉号字段） | 炉号正则分组 |

---

## 6. 项目目录结构

```text
nlq-agent/
├── docker-compose.yml          # 服务编排（Agent + Qdrant + TEI + Neo4j）
├── Dockerfile                  # Agent 镜像构建
├── .env.example                # 环境变量模板
├── pyproject.toml              # Python 项目配置（uv 管理依赖）
├── uv.lock                     # 依赖锁定文件
├── README.md
│
├── packages/
│   └── shared-types/
│       └── src/
│           └── reasoning-protocol.ts  # 与前端共享的类型定义
│
├── services/
│   └── agent-api/              # FastAPI 后端服务
│       ├── pyproject.toml
│       ├── app/
│       │   ├── main.py         # FastAPI 入口 + 生命周期管理
│       │   │
│       │   ├── api/            # HTTP 路由层
│       │   │   ├── chat.py     # /chat/stream SSE 端点
│       │   │   ├── health.py   # /health 健康检查
│       │   │   ├── kg.py       # /kg/* 知识图谱 API（含 getOntology）
│       │   │   └── sync.py     # /sync/* 数据同步
│       │   │
│       │   ├── core/           # 核心基础设施
│       │   │   ├── config.py   # Pydantic Settings（环境变量）
│       │   │   ├── database.py # SQLAlchemy 异步 MySQL 连接池
│       │   │   ├── llm_factory.py   # LLM 工厂（LiteLLM 网关统一接入）
│       │   │   ├── logger.py   # 结构化日志
│       │   │   ├── auth.py     # JWT / 上游认证校验
│       │   │   └── sentry_integration.py
│       │   │
│       │   ├── models/         # 数据模型
│       │   │   └── schemas.py  # Pydantic 数据模型
│       │   │
│       │   ├── agents/         # LangGraph Agent
│       │   │   ├── chat2sql_agent.py   # Chat2SQL：schema → SQL → 执行 → 总结
│       │   │   ├── query_agent.py      # QueryAgent：意图分类 + 路由 + 指标查询
│       │   │   ├── root_cause_agent.py # RootCauseAgent：单条记录判等级根因
│       │   │   └── graph.py            # LangGraph 编排（串联各 Agent）
│       │   │
│       │   ├── tools/          # Agent 工具集
│       │   │   ├── sql_tools.py   # SQL 安全执行（白名单 + AST 校验）
│       │   │   ├── graph_tools.py # KG 遍历工具（炉号 → 指标 → 规则 → 阈值）
│       │   │   └── query_tools.py # 查询工具（指标定义、规则获取、交检配置）
│       │   │
│       │   └── knowledge_graph/    # Neo4j 知识图谱模块
│       │       ├── manager.py      # 生命周期管理（初始化/刷新/关闭）
│       │       ├── neo4j_graph.py  # Neo4j 后端实现
│       │       ├── queries.py      # Cypher 查询封装
│       │       ├── schema_loader.py# 语义层初始化（MySQL → Qdrant）
│       │       └── base.py         # 抽象基类
│       │
│       ├── tests/              # 单元测试 + 集成测试
│       └── scripts/            # 运维脚本（逐步重建 KG 等）
│
└── docs/
    ├── development-guide.md        # 本文档
    └── kg-frontend-ontology.md     # 前端可视化 Ontology 规格
```

### 6.1 核心模块职责速查

| 模块 | 文件 | 核心职责 |
| :--- | :--- | :--- |
| **Chat2SQL Agent** | `app/agents/chat2sql_agent.py` | Schema 选取 → 列选取 → SQL 起草 → 白名单验证 → 安全执行 → 结果总结 |
| **Query Agent** | `app/agents/query_agent.py` | 意图分类 + 路由 + 指标查询 + 规则查询 + 一次交检 |
| **RootCause Agent** | `app/agents/root_cause_agent.py` | 炉号实体抽取 → KG 遍历 → 判定路径解释 |
| **LangGraph 编排** | `app/agents/graph.py` | 串联 Query / Chat2SQL / RootCause Agent |
| **SQL 安全** | `app/tools/sql_tools.py` | 正则白名单（仅 SELECT）+ AST 校验 + 自动 LIMIT 501 |
| **KG 遍历** | `app/tools/graph_tools.py` | Cypher 查询：炉号 → 记录 → 规则 → 条件评估 |
| **查询工具** | `app/tools/query_tools.py` | 指标定义获取、规则获取、交检配置查询 |
| **LLM 工厂** | `app/core/llm_factory.py` | 通过 LiteLLM 网关统一接入多种模型 |
| **语义层加载** | `app/knowledge_graph/schema_loader.py` | MySQL 表结构 + 规则/公式 → Qdrant 向量化 |
| **KG 管理** | `app/knowledge_graph/manager.py` | Neo4j 连接池 + 初始化/刷新/关闭 |

---

## 7. 关键代码设计详解

### 7.1 Agent 编排流程

`app/agents/graph.py` 定义了 LangGraph 状态机和 Agent 编排逻辑：

```
用户提问
    ↓
QueryAgent（意图分类）
    ├── conceptual → 直接回答（跳过 SQL）
    ├── statistical → Chat2SQL Agent
    ├── root_cause → RootCause Agent
    └── out_of_scope → 拒绝回答
```

### 7.2 SQL 安全机制

`app/tools/sql_tools.py` 实现了三层安全防护：

1. **正则白名单**：只允许 `SELECT` 开头的语句，拦截所有 `INSERT/UPDATE/DELETE/DROP/ALTER` 等写操作。
2. **AST 校验**：解析 SQL AST，禁止子查询中的写操作和危险函数。
3. **自动 LIMIT**：如果 SQL 中没有 `LIMIT` 子句，自动追加 `LIMIT 501`（超过 500 行截断）。

### 7.3 SQL 修正循环

当 SQL 执行失败时，Chat2SQL Agent 会自动进入修正循环（最多 2 次）：

```
SQL 生成 → 执行 → 失败？
                    ↓ 是
              将原始 SQL + 错误信息 + DDL 喂给 LLM
                    ↓
              LLM 输出修正后的 SQL
                    ↓
              重新执行 → 再次失败？→ 再修正一次
                                        ↓
                                   最终失败 → fallback
```

### 7.4 SSE 事件流

`app/api/chat.py` 的 `/chat/stream` 端点通过 SSE 实时推送推理步骤：

| SSE 事件类型 | 前端回调 | 触发阶段 |
| :--- | :--- | :--- |
| `reasoning_step` | `onReasoningStep(step)` | 任意阶段 |
| `text` | `onText(chunk)` | 最终回答 |
| `response_metadata` | `onResponseMetadata(payload)` | 结束 |
| `error` | `onError(msg)` | 错误 |
| `done` | `onDone()` | 完成 |

### 7.5 条件步骤的两阶段填充

以"铁损是否达标？"为例：

**Stage 1 发射**（只有 expected，无 actual）：
```json
{"kind": "condition", "label": "铁损阈值", "field": "F_PERF_PS_LOSS", "expected": "<= 0.80"}
```

**Stage 2 回填**（查询后补充 actual 和 satisfied）：
```json
{"kind": "condition", "label": "铁损阈值", "field": "F_PERF_PS_LOSS", "expected": "<= 0.80", "actual": 0.75, "satisfied": true}
```

---

## 8. Neo4j 知识图谱

### 8.1 数据模型

| 实体类型 | 说明 | 来源表 |
|---------|------|--------|
| ProductSpec | 产品规格 | `lab_product_spec` |
| Formula | 指标公式 | `lab_intermediate_data_formula` |
| JudgmentRule | 判定规则 | `lab_intermediate_data_judgment_level` |
| SpecAttribute | 规格扩展属性 | `lab_product_spec_attribute` |
| ReportConfig | 报表配置 | `lab_report_config` |

| 关系类型 | 说明 | 示例 |
|---------|------|------|
| `HAS_RULE` | 规格拥有判定规则 | `(ProductSpec)-[:HAS_RULE]->(JudgmentRule)` |
| `EVALUATES` | 规则评估指标 | `(JudgmentRule)-[:EVALUATES]->(Formula)` |
| `HAS_ATTRIBUTE` | 规格有扩展属性 | `(ProductSpec)-[:HAS_ATTRIBUTE]->(SpecAttribute)` |

### 8.2 配置

```bash
# .env 文件（位于 nlq-agent/.env）
NEO4J_URI=bolt://localhost:7687
NEO4J_USER=neo4j
NEO4J_PASSWORD=password
NEO4J_DATABASE=neo4j
NEO4J_ENABLED=true   # 启用知识图谱（默认 false，避免启动时全量构建超时）
```

> **注意**：`NEO4J_ENABLED=false` 时，`getOntology()` 接口仍可正常工作（直接查 MySQL）。仅在需要 Neo4j 图谱重建时设为 `true`。

### 8.3 Docker 部署 Neo4j

```bash
docker run -d \
  --name neo4j-lm \
  -p 7474:7474 -p 7687:7687 \
  -e NEO4J_AUTH=neo4j/password \
  -v neo4j_data:/data \
  neo4j:5.15-community
```

### 8.4 知识图谱 API

```bash
# 健康检查
GET /api/v1/kg/health

# 获取本体数据（供前端可视化初始化，直接查 MySQL，不依赖 Neo4j）
GET /api/v1/kg/ontology

# 刷新知识图谱（Neo4j 全量重建）
POST /api/v1/kg/refresh

# 获取所有产品规格
GET /api/v1/kg/specs

# 获取规格详情
GET /api/v1/kg/specs/{spec_code}

# 获取规格判定规则
GET /api/v1/kg/specs/{spec_code}/rules

# 获取所有指标
GET /api/v1/kg/metrics

# 搜索判定规则
GET /api/v1/kg/rules/search?keyword=带厚
```

### 8.5 使用示例

```python
from app.knowledge_graph.manager import get_knowledge_graph
from app.knowledge_graph.queries import get_spec_judgment_rules

graph = get_knowledge_graph()
rules = await get_spec_judgment_rules(graph, "120")
print(rules)
```

### 8.6 维护与故障排查

**每日刷新：**
```bash
curl -X POST http://localhost:18100/api/v1/kg/refresh
```

**监控项：**
- Neo4j 内存使用
- 图谱构建日志
- 数据一致性

**知识图谱未初始化：**
检查日志中的错误信息，确认 Neo4j 连接配置正确。若 Neo4j 初始化超时，可临时设置 `NEO4J_ENABLED=false` 启动服务。

---

## 9. 部署与运维

### 9.1 Docker Compose 一键部署

```bash
# 1. 配置环境变量
cp .env.example .env
vim .env  # 填入 LLM_API_KEY, MYSQL_PASSWORD 等

# 2. 启动基础设施
docker compose up -d qdrant tei neo4j

# 3. 初始化语义层
cd services/agent-api
uv run python -m app.knowledge_graph.schema_loader

# 4. 启动 Agent（跳过 Neo4j 初始化，避免超时）
NEO4J_ENABLED=false uv run uvicorn app.main:app --host 0.0.0.0 --port 18100
```

### 9.2 服务端口规划

| 服务 | 端口 | 说明 |
| :--- | :--- | :--- |
| nlq-agent | 18100 | FastAPI 问答微服务 |
| Qdrant REST | 6333 | 向量数据库 REST API |
| Qdrant gRPC | 6334 | 向量数据库 gRPC |
| TEI | 8001 | Embedding 服务 |
| Neo4j HTTP | 7474 | Neo4j Browser |
| Neo4j Bolt | 7687 | Neo4j 驱动协议 |

### 9.3 .NET 后端集成

在 .NET 后端的判定规则或产品规格 CRUD 操作中，添加 HTTP 回调触发知识图谱同步：

```csharp
await _httpClient.PostAsJsonAsync(
    $"{nlqAgentUrl}/api/v1/sync/rules",
    new { action = "upsert", data = changedRules }
);
```

---

## 10. 实施路线图

| 阶段 | 工作内容 | 状态 |
| :--- | :--- | :--- |
| **Phase 1** | 部署 Qdrant + TEI + Neo4j，运行语义层初始化 | ✅ |
| **Phase 2** | 开发 QueryAgent（意图分类 + 路由 + 指标查询） | ✅ |
| **Phase 3** | 开发 Chat2SQL Agent（SQL 生成 + 执行 + 修正） | ✅ |
| **Phase 4** | 开发 RootCause Agent（单条记录根因分析） | ✅ |
| **Phase 5** | SSE 协议对接，与前端联调 | ✅ |
| **Phase 6** | 前端知识图谱可视化（relation-graph-vue3） | ✅ 进行中 |
| **Phase 7** | Prompt 调优 + 预定义模板扩充 + 性能优化 | 持续 |

---

## 11. 与现有代码库的集成点

| 集成点 | 现有文件 | 改动说明 |
| :--- | :--- | :--- |
| 前端 SSE 客户端 | `web/src/api/nlqAgent.ts` | **无需改动**，协议完全兼容 |
| 前端推理链组件 | `web/src/components/KgReasoningChain/` | **无需改动** |
| 前端知识图谱 | `web/src/views/lab/knowledge-graph/` | 持续迭代中 |
| 移动端 SSE 客户端 | `mobile/utils/sse-client.js` | **无需改动** |
| 类型定义 | `web/src/types/reasoning-protocol.d.ts` | **无需改动**（由 shared-types 同步） |
| .NET AI Controller | `api/src/modularity/ai/` | 添加 nlq-agent 代理路由 |
| .NET 规则 Service | `api/src/modularity/lab/` | 添加变更后的 HTTP 回调 |
| Docker Compose | 项目根目录 `docker-compose.yml` | 添加 nlq-agent / Neo4j 服务定义 |
| 环境变量 | `.env.example` | 添加 `NLQ_AGENT_URL` 等配置 |
