---
name: nlq-two-stage
description: 路美 NLQ Agent 两阶段问答 Pipeline 完整开发与调试（Stage1 语义图谱 + Stage2 SQL 数据查询）
argument-hint: "<需要实现或调试的功能描述，例如：实现合格率统计的两阶段查询>"
level: 4
---

<Purpose>
本 Skill 专为路美项目 `nlq-agent` Python 微服务设计，指导 Claude Code 在 `nlq-agent/` 目录下实现、调试或扩展"两阶段问答 Pipeline"。

两阶段架构核心思想：
- **Stage 1（Semantic & KG Agent）**：从 Qdrant 向量库检索业务知识（判定规则、产品规格、指标定义），通过 SSE 实时推送 `ReasoningStep` 给前端 `<KgReasoningChain>` 组件渲染推理链，输出结构化 `AgentContext`。
- **Stage 2（Data & SQL Agent）**：接收 `AgentContext`，结合 DDL 生成安全只读 SQL，执行查询，回填 `condition` 步骤的 `actual` 值，流式输出最终自然语言回答。

SSE 协议与前端 `web/src/api/nlqAgent.ts` 和 `mobile/utils/sse-client.js` 完全兼容，无需改动前端代码。
</Purpose>

<Project_Structure>
```
nlq-agent/
├── src/
│   ├── main.py                          # FastAPI 应用入口（端口 18100）
│   ├── core/settings.py                 # 配置（OpenAI / Qdrant / MySQL）
│   ├── models/
│   │   ├── schemas.py                   # Pydantic 模型（AgentContext, ReasoningStep 等）
│   │   └── ddl.py                       # 数据库 DDL + 预定义 SQL 模板
│   ├── services/
│   │   ├── llm_client.py                # OpenAI 兼容 LLM 客户端
│   │   ├── embedding_client.py          # Embedding 服务
│   │   ├── qdrant_service.py            # Qdrant 向量检索
│   │   ├── database.py                  # MySQL 异步连接池
│   │   └── sse_emitter.py               # SSE 事件发射器（协议层）
│   ├── utils/prompts.py                 # 6 套 Prompt 模板
│   └── pipelines/
│       ├── orchestrator.py              # 主编排器（路由 + 串联两阶段）
│       ├── stage1/semantic_kg_agent.py  # Stage 1 实现
│       └── stage2/data_sql_agent.py     # Stage 2 实现
├── scripts/init_semantic_layer.py       # 语义层初始化（MySQL → Qdrant）
└── tests/test_pipeline.py               # 测试套件
```

**关键约定**：
- 服务端口：`18100`（与 `.env.example` 和 `docker-compose.yml` 一致）
- SSE 端点：`POST /api/v1/query`（流式）、`POST /api/v1/query/sync`（同步，仅测试用）
- 健康检查：`GET /health`
- 知识库同步：`POST /api/v1/knowledge/sync`（.NET 后端回调触发）
</Project_Structure>

<Use_When>
- 需要实现或修改 Stage 1 语义检索逻辑（Qdrant 搜索、推理步骤生成）
- 需要实现或修改 Stage 2 SQL 生成逻辑（NL2SQL、SQL 修正循环）
- 需要添加新的统计查询类型（如新增磁性数据分析）
- 需要调试两阶段 Pipeline 的 SSE 流式输出
- 需要扩展 DDL 或 Prompt 模板
- 需要端到端联调前端 `<KgReasoningChain>` 组件
</Use_When>

<Do_Not_Use_When>
- 只需要初始化 Qdrant 语义层 → 使用 `nlq-qdrant-init` skill
- 只需要调试 SQL 生成问题 → 使用 `nlq-sql-debug` skill
- 只需要配置意图路由规则 → 使用 `nlq-query-router` skill
- 只需要更新语义层（MDL）定义 → 使用 `nlq-semantic-layer` skill
</Do_Not_Use_When>

<SSE_Protocol>
前端通过 `EventSource` 消费以下事件类型（定义于 `web/src/types/reasoning-protocol.d.ts`）：

```typescript
// 推理步骤（Stage 1 实时推送）
event: reasoning_step
data: {
  "type": "reasoning_step",
  "step": {
    "id": "step_001",
    "kind": "retrieval" | "condition" | "conclusion" | "fallback",
    "title": "检索判定规则",
    "content": "从知识库检索到 A 类硅钢片判定标准...",
    "status": "running" | "done" | "error",
    "metadata": { "source": "JUDGMENT_LEVEL", "score": 0.92 },
    // condition 类型额外字段（Stage 2 回填）
    "expected": "铁损 P17/50 ≤ 1.08 W/kg",
    "actual": "1.05 W/kg",       // Stage 2 执行 SQL 后回填
    "satisfied": true              // Stage 2 执行 SQL 后回填
  }
}

// 流式文本（Stage 2 最终回答）
event: text
data: { "type": "text", "delta": "本月共检测..." }

// 元数据（完成时）
event: response_metadata
data: {
  "type": "response_metadata",
  "sql": "SELECT ...",
  "rows_count": 42,
  "execution_time_ms": 156,
  "stage1_sources": ["JUDGMENT_LEVEL", "PRODUCT_SPEC"]
}

// 错误
event: error
data: { "type": "error", "message": "...", "code": "SQL_GENERATION_FAILED" }
```
</SSE_Protocol>

<Execution_Policy>
1. 始终先读取现有代码文件，理解当前实现状态，再进行修改
2. 修改 `schemas.py` 后，同步检查 `orchestrator.py` 中的类型引用
3. 修改 Prompt 模板时，同时更新 `tests/test_pipeline.py` 中的相关断言
4. 新增统计查询类型时，必须同步更新 `ddl.py` 中的 `PREDEFINED_SQL_TEMPLATES`
5. 所有 SQL 必须通过 `_validate_sql_safety()` 安全检查（白名单 + LIMIT 注入）
6. SSE 事件顺序必须严格遵守：`reasoning_step`(多个) → `text`(流式) → `response_metadata`
</Execution_Policy>

<Steps>
## Phase 1 — 理解现状

1. 读取核心文件，建立完整上下文：
   ```
   src/models/schemas.py          # 数据模型
   src/pipelines/orchestrator.py  # 主编排逻辑
   src/pipelines/stage1/semantic_kg_agent.py
   src/pipelines/stage2/data_sql_agent.py
   src/utils/prompts.py           # Prompt 模板
   ```
2. 确认用户的具体需求属于哪个模块（Stage1 / Stage2 / 路由 / DDL / Prompt）

## Phase 2 — 实现或修改

根据需求类型选择对应的实现路径：

### 路径 A：新增统计查询类型
1. 在 `src/models/ddl.py` 的 `PREDEFINED_SQL_TEMPLATES` 中添加新模板
2. 在 `src/utils/prompts.py` 的 `INTENT_CLASSIFICATION_PROMPT` 中添加新意图类型
3. 在 `src/pipelines/orchestrator.py` 的 `_route_intent()` 中添加路由规则
4. 在 `src/pipelines/stage2/data_sql_agent.py` 的 `_try_predefined_template()` 中添加匹配逻辑

### 路径 B：优化 Stage 1 语义检索
1. 修改 `src/pipelines/stage1/semantic_kg_agent.py` 中的 `_search_knowledge()` 方法
2. 调整 `src/utils/prompts.py` 中的 `SEMANTIC_EXTRACTION_PROMPT`
3. 验证 `ReasoningStep` 的 `kind` 字段是否正确（`retrieval` / `condition` / `conclusion`）

### 路径 C：优化 Stage 2 SQL 生成
1. 修改 `src/pipelines/stage2/data_sql_agent.py` 中的 `_generate_sql()` 方法
2. 调整 `src/utils/prompts.py` 中的 `SQL_GENERATION_PROMPT` 和 `SQL_CORRECTION_PROMPT`
3. 确认 SQL 修正循环（最多 2 次）逻辑正确

### 路径 D：调试 SSE 流式输出
1. 检查 `src/services/sse_emitter.py` 中的事件格式
2. 使用 `curl` 测试端点：
   ```bash
   curl -N -X POST http://localhost:18100/api/v1/query \
     -H "Content-Type: application/json" \
     -d '{"question": "本月A类合格率是多少", "context": {}}'
   ```
3. 对照 `web/src/types/reasoning-protocol.d.ts` 验证事件格式

## Phase 3 — 测试验证

```bash
cd nlq-agent

# 单元测试
python -m pytest tests/ -v

# 启动服务（需要 .env 配置）
uvicorn src.main:app --reload --port 18100

# 健康检查
curl http://localhost:18100/health

# 端到端测试
curl -N -X POST http://localhost:18100/api/v1/query \
  -H "Content-Type: application/json" \
  -d '{"question": "本月硅钢片A类合格率是多少？", "context": {"month": "2024-01"}}'
```

## Phase 4 — 代码质量

```bash
# 类型检查
mypy src/ --ignore-missing-imports

# 格式化
black src/ tests/
isort src/ tests/
```
</Steps>

<Key_Data_Models>
```python
# AgentContext — Stage1 输出，Stage2 输入
class AgentContext(BaseModel):
    intent: IntentType          # "statistical" | "root_cause" | "concept" | "out_of_scope"
    question: str
    kg_answer: str              # Stage1 的业务解释文本
    filters: QueryFilters       # 结构化过滤条件（时间范围、产品型号等）
    reasoning_steps: list[ReasoningStep]
    knowledge_sources: list[KnowledgeSource]

# ReasoningStep — SSE 推送的推理步骤
class ReasoningStep(BaseModel):
    id: str
    kind: Literal["retrieval", "condition", "conclusion", "fallback"]
    title: str
    content: str
    status: Literal["running", "done", "error"] = "done"
    metadata: dict = {}
    # condition 类型专用（Stage2 回填）
    expected: str | None = None
    actual: str | None = None
    satisfied: bool | None = None

# QueryFilters — 结构化查询过滤条件
class QueryFilters(BaseModel):
    time_range: TimeRange | None = None
    product_model: str | None = None
    batch_no: str | None = None
    shift: str | None = None
    judgment_level: str | None = None
    thresholds: dict[str, float] = {}
```
</Key_Data_Models>

<Qdrant_Collections>
语义层包含以下 Qdrant 集合（由 `scripts/init_semantic_layer.py` 初始化）：

| 集合名 | 内容 | 对应 MySQL 表 |
|--------|------|---------------|
| `luma_judgment_rules` | 判定等级规则（A/B/C/D 类标准） | `LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL` |
| `luma_product_specs` | 产品规格属性（铁损/磁感阈值） | `LAB_PRODUCT_SPEC` + `LAB_PRODUCT_SPEC_ATTRIBUTE` |
| `luma_metric_definitions` | 指标定义（合格率/叠片系数等计算公式） | 静态配置 |
| `luma_formula_rules` | 中间数据计算公式 | `LAB_INTERMEDIATE_DATA_FORMULA` |

向量维度：`1536`（text-embedding-3-small）
相似度阈值：`0.72`（低于此值触发 fallback）
</Qdrant_Collections>

<Common_Issues>
**问题 1：Stage 2 SQL 生成错误**
- 检查 `src/models/ddl.py` 中的 DDL 是否与实际数据库表结构一致
- 确认 `SQL_GENERATION_PROMPT` 中包含了正确的表名和字段名
- 查看 SQL 修正循环的错误日志

**问题 2：SSE 事件前端无法解析**
- 检查 `sse_emitter.py` 中事件格式，确保 `event:` 和 `data:` 字段名与前端 `nlqAgent.ts` 中的 `onReasoningStep`/`onText` 回调一致
- 确认 `Content-Type: text/event-stream` 响应头正确设置

**问题 3：Qdrant 检索命中率低**
- 运行 `scripts/init_semantic_layer.py` 重新初始化语义层
- 调整 `qdrant_service.py` 中的 `score_threshold`（默认 0.72）
- 检查 embedding 模型是否与初始化时一致

**问题 4：condition 步骤的 actual 值未回填**
- 确认 `data_sql_agent.py` 中的 `_backfill_conditions()` 方法被正确调用
- 检查 SQL 查询结果的字段映射是否正确
</Common_Issues>

<Final_Checklist>
- [ ] SSE 事件格式与 `web/src/types/reasoning-protocol.d.ts` 完全一致
- [ ] SQL 通过安全检查（无 DROP/INSERT/UPDATE/DELETE）
- [ ] `condition` 步骤的 `actual` 和 `satisfied` 字段在 Stage 2 正确回填
- [ ] Qdrant 未命中时正确生成 `fallback` 步骤
- [ ] 所有 SQL 查询包含 `LIMIT` 子句（最大 1000 行）
- [ ] 服务在 `http://localhost:18100/health` 返回 200
- [ ] 单元测试全部通过（`python -m pytest tests/ -v`）
</Final_Checklist>
