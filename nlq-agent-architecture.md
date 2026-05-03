# 路美项目「知识图谱 → 数据查询」两阶段问答架构设计方案

> **文档定位**：本文档为 `nlq-agent` 独立 Python 微服务的架构设计规范，面向开发者，覆盖设计思想、协议规范、目录结构、核心代码实现及与现有系统的集成方案。

---

## 1. 背景与设计目标

路美项目（实验室数据分析系统）的核心业务是对硅钢带材的检测数据进行质量判定与分析。用户的自然语言查询往往包含强业务语义，例如：

- "为什么炉号 1丙20260110-1 是 C 级？"
- "本月 Ps 铁损超标的炉次有哪些？"
- "120 规格的 A 级判定标准是什么？"

这类查询的难点在于，**直接进行 NL2SQL 转换时，LLM 缺乏对"C 级"、"铁损上限"等业务概念的先验理解**，极易生成错误的 SQL 或产生无意义的回答。

业界公认的最佳实践是：**先通过知识图谱/向量检索理解业务语义，再基于业务上下文生成精准的数据查询**。本方案将这一实践落地为路美项目的独立 Python 微服务（`nlq-agent`），并完美对接现有前端的 `<KgReasoningChain>` 组件。

---

## 2. 两阶段架构核心思想

整个问答流程被明确拆分为两个串行的 Agent：

| 阶段 | 名称 | 核心职责 | 输入 | 输出 |
|------|------|----------|------|------|
| **Stage 1** | Semantic & KG Agent（语义解析与图谱检索） | 理解意图，从 Qdrant 检索业务规则与判定标准 | 用户自然语言问题 | 业务解释文本 + 结构化过滤条件 + `ReasoningStep[]` |
| **Stage 2** | Data & SQL Agent（数据查询与分析） | 基于业务上下文生成 SQL，执行查询，生成最终回答 | Stage 1 的业务上下文 + 过滤条件 | 最终自然语言回答（流式文本） |

**核心原则**：Stage 1 的输出是 Stage 2 的输入，两个 Agent 之间通过一个结构化的 `AgentContext` 对象传递状态，而非让 Stage 2 重新理解用户问题。

---

## 3. 系统整体架构

### 3.1 服务定位

`nlq-agent` 是一个独立的 Python 微服务，与现有的 .NET 后端（`api/`）并行运行，不侵入现有代码。

```
┌─────────────────────────────────────────────────────────────────┐
│                        客户端层                                   │
│  ┌──────────────────────┐    ┌──────────────────────────────┐   │
│  │  Web (Vue 3)         │    │  Mobile (UniApp)              │   │
│  │  streamNlqChat()     │    │  streamNlqChat()              │   │
│  │  <KgReasoningChain>  │    │  <kg-reasoning-chain>        │   │
│  └──────────┬───────────┘    └──────────────┬───────────────┘   │
└─────────────┼────────────────────────────────┼───────────────────┘
              │ SSE (POST /api/v1/chat/stream)  │
              ▼                                 ▼
┌─────────────────────────────────────────────────────────────────┐
│                   nlq-agent (Python, 端口 18100)                  │
│                                                                   │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  FastAPI 路由层 + SSE 流式响应                             │    │
│  └──────────────────────────┬──────────────────────────────┘    │
│                              │                                    │
│  ┌───────────────────────────▼────────────────────────────────┐  │
│  │  Stage 1: Semantic & KG Agent                               │  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  │  │
│  │  │ 意图解析      │→ │ Qdrant 检索  │→ │ 推理链生成       │  │  │
│  │  │ (LLM)        │  │ (bge-m3 向量) │  │ (ReasoningStep)  │  │  │
│  │  └──────────────┘  └──────────────┘  └──────────────────┘  │  │
│  └───────────────────────────┬────────────────────────────────┘  │
│                               │ AgentContext                      │
│  ┌────────────────────────────▼───────────────────────────────┐  │
│  │  Stage 2: Data & SQL Agent                                  │  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  │  │
│  │  │ SQL 生成     │→ │ SQL 执行     │→ │ 结果分析与总结   │  │  │
│  │  │ (LLM + DDL)  │  │ (只读查询)   │  │ (LLM)            │  │  │
│  │  └──────────────┘  └──────────────┘  └──────────────────┘  │  │
│  └────────────────────────────────────────────────────────────┘  │
└────────────────────────────────────┬────────────────────────────┘
                                      │
              ┌───────────────────────┼───────────────────────┐
              ▼                       ▼                        ▼
    ┌──────────────────┐   ┌──────────────────┐   ┌──────────────────┐
    │  Qdrant (6333)   │   │  vLLM (8082)     │   │  业务数据库      │
    │  向量知识库       │   │  Qwen2.5-7b      │   │  (MySQL/SQLSvr)  │
    └──────────────────┘   └──────────────────┘   └──────────────────┘
```

### 3.2 目录结构

```
nlq-agent/
├── packages/
│   └── shared-types/
│       └── src/
│           └── reasoning-protocol.ts   # 与前端共享的类型定义（上游源）
├── services/
│   └── agent-api/                      # FastAPI 服务主体
│       ├── main.py                     # 服务入口
│       ├── config.py                   # 配置（Qdrant、vLLM、DB 连接）
│       ├── routers/
│       │   └── chat.py                 # /api/v1/chat/stream 路由
│       ├── agents/
│       │   ├── kg_agent.py             # Stage 1: 语义 & 图谱 Agent
│       │   └── sql_agent.py            # Stage 2: 数据 & SQL Agent
│       ├── models/
│       │   ├── protocol.py             # ReasoningStep 等 Pydantic 模型
│       │   └── context.py              # AgentContext 跨阶段传递对象
│       ├── tools/
│       │   ├── qdrant_retriever.py     # Qdrant 向量检索工具
│       │   └── db_executor.py          # 安全 SQL 执行工具
│       └── prompts/
│           ├── kg_system.txt           # Stage 1 系统提示词
│           └── sql_system.txt          # Stage 2 系统提示词
└── scripts/
    └── ingest_knowledge.py             # 知识入库脚本（将业务规则向量化）
```

---

## 4. 协议规范：与前端 `<KgReasoningChain>` 对接

### 4.1 推理步骤类型定义

前端 `<KgReasoningChain>` 组件消费的数据类型由 `shared-types/src/reasoning-protocol.ts` 定义，Python 侧必须严格对齐：

```python
# services/agent-api/models/protocol.py

from __future__ import annotations
from typing import Literal, Optional, Union
from pydantic import BaseModel

# 对应 TypeScript 的 ReasoningStepKind
ReasoningStepKind = Literal["record", "spec", "rule", "condition", "grade", "fallback"]

class ReasoningStep(BaseModel):
    """与前端 KgReasoningChain 组件对接的推理步骤，
    严格对应 shared-types/src/reasoning-protocol.ts。"""
    kind: ReasoningStepKind
    label: str
    detail: Optional[str] = None
    satisfied: Optional[bool] = None
    field: Optional[str] = None
    expected: Optional[str] = None
    actual: Optional[Union[str, float, int]] = None
    meta: Optional[dict] = None
```

### 4.2 SSE 事件流格式

`/api/v1/chat/stream` 接口必须返回 `Content-Type: text/event-stream`，每个事件为 `data: <json>\n\n` 格式：

| 事件类型 | 触发时机 | JSON 结构 | 前端处理 |
|----------|----------|-----------|----------|
| `reasoning_step` | Stage 1 每生成一个推理步骤时 | `{"type": "reasoning_step", "reasoning_step": {...}}` | `onReasoningStep(step)` → 追加到 `steps[]` |
| `text` | Stage 2 流式输出最终回答时 | `{"type": "text", "content": "..."}` | `onText(chunk)` → 追加到消息气泡 |
| `response_metadata` | 全部完成后发送 | `{"type": "response_metadata", "response_payload": {"reasoning_steps": [...]}}` | `onResponseMetadata(payload)` → 用完整列表覆盖 |
| `error` | 任意阶段发生错误时 | `{"type": "error", "error": "错误描述"}` | `onError(err)` |
| `done` | 流结束时 | `{"type": "done"}` | `onDone()` |

---

## 5. Stage 1 实现：语义解析与图谱检索 Agent

### 5.1 知识入库（预处理）

在 `nlq-agent` 能够检索知识之前，需要将路美项目的业务规则向量化并存入 Qdrant。入库的数据来源于 .NET 后端的数据库，主要包括以下实体：

| 数据来源（.NET 实体） | 对应数据库表 | 向量化内容 | Qdrant Collection |
|----------------------|-------------|-----------|-------------------|
| `IntermediateDataJudgmentLevelEntity` | `LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL` | 等级名称 + 判定条件 JSON | `lm_judgment_rules` |
| `ProductSpecEntity` + `ProductSpecAttributeEntity` | `LAB_PRODUCT_SPEC` + `LAB_PRODUCT_SPEC_ATTRIBUTE` | 规格代码 + 属性名称 + 属性值 | `lm_product_specs` |
| `IntermediateDataFormulaEntity` | `LAB_INTERMEDIATE_DATA_FORMULA` | 公式名称 + 公式表达式 + 备注 | `lm_formulas` |

```python
# scripts/ingest_knowledge.py（示意）

from qdrant_client import QdrantClient
from qdrant_client.models import PointStruct, VectorParams, Distance
import requests

QDRANT_URL = "http://localhost:6333"
TEI_ENDPOINT = "http://localhost:8081"  # bge-m3 嵌入服务

def embed(texts: list[str]) -> list[list[float]]:
    """调用 TEI 服务获取文本向量。"""
    resp = requests.post(f"{TEI_ENDPOINT}/embed", json={"inputs": texts})
    return resp.json()

def ingest_judgment_rules(rules: list[dict]):
    """将判定规则向量化并存入 Qdrant。"""
    client = QdrantClient(url=QDRANT_URL)
    texts = [
        f"产品规格 {r['product_spec_name']} 的 {r['name']} 等级：{r['description']}。"
        f"判定条件：{r['condition']}"
        for r in rules
    ]
    vectors = embed(texts)
    points = [
        PointStruct(
            id=i,
            vector=vectors[i],
            payload={
                "kind": "judgment_rule",
                "grade_name": rules[i]["name"],
                "product_spec_id": rules[i]["product_spec_id"],
                "product_spec_name": rules[i]["product_spec_name"],
                "condition_json": rules[i]["condition"],
                "quality_status": rules[i]["quality_status"],
                "priority": rules[i]["priority"],
            }
        )
        for i in range(len(rules))
    ]
    client.upsert(collection_name="lm_judgment_rules", points=points)
```

### 5.2 Stage 1 Agent 核心实现

```python
# services/agent-api/agents/kg_agent.py

from __future__ import annotations
import json
from typing import AsyncIterator
from qdrant_client import AsyncQdrantClient
from openai import AsyncOpenAI
from models.protocol import ReasoningStep
from models.context import AgentContext

class KGAgent:
    """Stage 1: 语义解析与知识图谱检索 Agent。
    
    职责：
    1. 解析用户意图，提取关键实体（炉号、规格、指标名称等）
    2. 向 Qdrant 检索相关的业务规则与产品规格
    3. 生成 ReasoningStep 推理链，实时推送给前端
    4. 输出结构化的 AgentContext，供 Stage 2 使用
    """

    def __init__(self, qdrant: AsyncQdrantClient, llm: AsyncOpenAI, config: dict):
        self.qdrant = qdrant
        self.llm = llm
        self.config = config

    async def run(
        self,
        user_query: str,
        session_id: str,
    ) -> AsyncIterator[ReasoningStep | AgentContext]:
        """执行 Stage 1，以异步生成器方式流式输出推理步骤，最后 yield AgentContext。"""

        # ── 步骤 1：意图解析，提取实体 ──────────────────────────────────────
        entity_prompt = f"""从以下用户问题中提取关键实体，以 JSON 格式返回。
字段说明：
- furnace_no: 炉号（如 "1丙20260110-1"），没有则为 null
- product_spec: 产品规格代码（如 "120"、"142"），没有则为 null  
- metric_names: 涉及的指标名称列表（如 ["Ps铁损", "带宽"]），没有则为 []
- query_type: 查询类型，可选 "why_grade"(为什么是某等级) | "stat_query"(统计查询) | "trend_query"(趋势查询) | "general"(通用)

用户问题：{user_query}

只返回 JSON，不要有任何额外文字。"""

        entity_resp = await self.llm.chat.completions.create(
            model=self.config["model_id"],
            messages=[{"role": "user", "content": entity_prompt}],
            temperature=0.1,
        )
        entities = json.loads(entity_resp.choices[0].message.content)

        # ── 步骤 2：如果有炉号，生成"命中记录"推理步骤 ─────────────────────
        if entities.get("furnace_no"):
            yield ReasoningStep(
                kind="record",
                label=f"命中检测记录：炉号 {entities['furnace_no']}",
                meta={"furnace_no": entities["furnace_no"]},
            )

        # ── 步骤 3：检索产品规格 ─────────────────────────────────────────────
        spec_context = ""
        if entities.get("product_spec"):
            spec_results = await self._search_qdrant(
                collection="lm_product_specs",
                query=f"产品规格 {entities['product_spec']} 的属性和标准",
                limit=3,
            )
            if spec_results:
                spec_text = spec_results[0].payload.get("spec_summary", "")
                spec_context = spec_text
                yield ReasoningStep(
                    kind="spec",
                    label=f"产品规格 {entities['product_spec']}",
                    detail=spec_text[:200],
                )

        # ── 步骤 4：检索判定规则 ─────────────────────────────────────────────
        rule_context = ""
        conditions: list[dict] = []
        query_for_rules = user_query
        if entities.get("product_spec"):
            query_for_rules = f"产品规格 {entities['product_spec']} 的判定规则 {user_query}"

        rule_results = await self._search_qdrant(
            collection="lm_judgment_rules",
            query=query_for_rules,
            limit=5,
        )

        for rule in rule_results:
            payload = rule.payload
            grade_name = payload.get("grade_name", "")
            condition_json = payload.get("condition_json", "{}")

            yield ReasoningStep(
                kind="rule",
                label=f"判定规则：{grade_name}（优先级 {payload.get('priority', '?')}）",
                detail=f"质量状态：{payload.get('quality_status', '')}",
            )
            rule_context += f"\n{grade_name} 等级规则：{condition_json}"

            # 解析条件，生成 condition 步骤（仅当有炉号时，表示在评估该炉号）
            if entities.get("furnace_no") and condition_json:
                try:
                    cond_obj = json.loads(condition_json)
                    for field, constraint in (cond_obj.get("conditions") or {}).items():
                        conditions.append({
                            "field": field,
                            "operator": constraint.get("operator"),
                            "threshold": constraint.get("value"),
                            "grade": grade_name,
                        })
                        yield ReasoningStep(
                            kind="condition",
                            label=f"{field} {constraint.get('operator')} {constraint.get('value')}",
                            field=field,
                            expected=f"{constraint.get('operator')} {constraint.get('value')}",
                            # actual 值需要 Stage 2 查询后回填，此处先不填
                        )
                except (json.JSONDecodeError, AttributeError):
                    pass

        # ── 步骤 5：构建 AgentContext，传递给 Stage 2 ────────────────────────
        context = AgentContext(
            user_query=user_query,
            session_id=session_id,
            entities=entities,
            spec_context=spec_context,
            rule_context=rule_context,
            conditions=conditions,
            reasoning_steps=[],  # 将在 orchestrator 中收集完整列表
        )
        yield context

    async def _search_qdrant(self, collection: str, query: str, limit: int = 5):
        """向 Qdrant 发起向量检索。"""
        import requests
        # 先获取查询向量
        embed_resp = requests.post(
            f"{self.config['tei_endpoint']}/embed",
            json={"inputs": [query]},
        )
        query_vector = embed_resp.json()[0]
        results = await self.qdrant.search(
            collection_name=collection,
            query_vector=query_vector,
            limit=limit,
        )
        return results
```

---

## 6. Stage 2 实现：数据查询与分析 Agent

### 6.1 核心数据库表 DDL 摘要

Stage 2 的 SQL 生成需要注入以下关键表的 DDL，以便 LLM 正确生成查询：

```sql
-- 中间数据表（核心分析表）
CREATE TABLE LAB_INTERMEDIATE_DATA (
    F_Id            VARCHAR(50)  NOT NULL PRIMARY KEY,
    F_FURNACE_NO    VARCHAR(100),           -- 原始炉号
    F_PROD_DATE     DATETIME,               -- 生产日期
    F_LINE_NO       INT,                    -- 产线号
    F_SHIFT         VARCHAR(10),            -- 班次（甲/乙/丙）
    F_WIDTH         DECIMAL(18,6),          -- 带宽 (mm)
    F_PERF_PS_LOSS  DECIMAL(18,6),          -- Ps铁损 (W/kg)
    F_PERF_SS_POWER DECIMAL(18,6),          -- Ss激磁功率 (VA/kg)
    F_PERF_HC       DECIMAL(18,6),          -- Hc (A/m)
    F_LAMINATION_FACTOR DECIMAL(18,6),      -- 叠片系数 (%)
    F_SINGLE_COIL_WEIGHT DECIMAL(18,6),     -- 单卷重量 (kg)
    F_LABELING      VARCHAR(50),            -- 判定等级（A/B/C/性能不合等）
    F_PRODUCT_SPEC_ID VARCHAR(50),          -- 产品规格ID
    F_DELETE_MARK   INT DEFAULT 0           -- 软删除标记
);

-- 原始数据表
CREATE TABLE LAB_RAW_DATA (
    F_Id            VARCHAR(50)  NOT NULL PRIMARY KEY,
    F_FURNACE_NO    VARCHAR(100),
    F_DETECTION_DATE DATETIME,
    F_WIDTH         DECIMAL(18,6),
    F_COIL_WEIGHT   DECIMAL(18,6),
    F_BREAK_COUNT   INT,
    F_DELETE_MARK   INT DEFAULT 0
);
```

### 6.2 Stage 2 Agent 核心实现

```python
# services/agent-api/agents/sql_agent.py

from __future__ import annotations
import re
from typing import AsyncIterator
from openai import AsyncOpenAI
from models.protocol import ReasoningStep
from models.context import AgentContext
from tools.db_executor import SafeDBExecutor

# 注入 LLM 的 DDL 上下文（精简版，只包含与业务查询相关的表和字段）
DDL_CONTEXT = """
-- 中间数据表（核心）
LAB_INTERMEDIATE_DATA:
  F_FURNACE_NO: 炉号
  F_PROD_DATE: 生产日期
  F_LINE_NO: 产线号(1,2,3...)
  F_SHIFT: 班次(甲/乙/丙)
  F_WIDTH: 带宽(mm)
  F_PERF_PS_LOSS: Ps铁损(W/kg)
  F_PERF_SS_POWER: Ss激磁功率(VA/kg)
  F_PERF_HC: Hc(A/m)
  F_LAMINATION_FACTOR: 叠片系数(%)
  F_SINGLE_COIL_WEIGHT: 单卷重量(kg)
  F_LABELING: 判定等级(A/B/C/性能不合/其他不合)
  F_PRODUCT_SPEC_ID: 产品规格ID
  F_DELETE_MARK: 软删除标记(0=正常)

-- 产品规格表
LAB_PRODUCT_SPEC:
  F_Id: 规格ID
  F_CODE: 规格代码(如120/142/170)
  F_NAME: 规格名称
"""

class SQLAgent:
    """Stage 2: 数据查询与分析 Agent。
    
    职责：
    1. 接收 Stage 1 的 AgentContext（含业务解释与过滤条件）
    2. 生成安全的只读 SQL 查询
    3. 执行查询并获取结果
    4. 结合业务上下文生成最终的自然语言回答（流式输出）
    """

    def __init__(self, llm: AsyncOpenAI, db: SafeDBExecutor, config: dict):
        self.llm = llm
        self.db = db
        self.config = config

    async def run(self, context: AgentContext) -> AsyncIterator[str | ReasoningStep]:
        """执行 Stage 2，流式输出最终回答文本，并在必要时补充 condition 步骤的 actual 值。"""

        # ── 步骤 1：构建 SQL 生成 Prompt ─────────────────────────────────────
        conditions_desc = ""
        if context.conditions:
            conditions_desc = "\n已知过滤条件（来自知识图谱）：\n" + "\n".join(
                f"  - {c['field']} {c['operator']} {c['threshold']} （{c['grade']} 等级规则）"
                for c in context.conditions
            )

        sql_prompt = f"""你是一个 SQL 专家，负责为实验室数据分析系统生成只读查询。

数据库表结构：
{DDL_CONTEXT}

业务背景（来自知识图谱检索）：
{context.rule_context}
{context.spec_context}
{conditions_desc}

用户问题：{context.user_query}

要求：
1. 只生成 SELECT 语句，不允许 INSERT/UPDATE/DELETE/DROP
2. 必须包含 WHERE F_DELETE_MARK = 0 或 F_DELETE_MARK IS NULL
3. 如果问题涉及特定炉号，使用 WHERE F_FURNACE_NO = '...' 精确匹配
4. 如果问题是统计性质，使用 GROUP BY + COUNT/SUM/AVG
5. 结果集不超过 200 行，使用 LIMIT 200
6. 只返回 SQL 语句，不要有任何解释

SQL："""

        sql_resp = await self.llm.chat.completions.create(
            model=self.config["model_id"],
            messages=[{"role": "user", "content": sql_prompt}],
            temperature=0.1,
        )
        raw_sql = sql_resp.choices[0].message.content.strip()
        # 清理 Markdown 代码块
        sql = re.sub(r"```sql\s*|\s*```", "", raw_sql).strip()

        # ── 步骤 2：安全校验并执行 SQL ──────────────────────────────────────
        query_result = await self.db.execute_readonly(sql)

        # ── 步骤 3：如果有炉号查询，补充 condition 步骤的 actual 值 ──────────
        if context.entities.get("furnace_no") and query_result.rows:
            row = query_result.rows[0]
            for cond in context.conditions:
                field = cond["field"]
                col_map = {
                    "F_PERF_PS_LOSS": "f_perf_ps_loss",
                    "F_WIDTH": "f_width",
                    "F_PERF_SS_POWER": "f_perf_ss_power",
                    "F_LAMINATION_FACTOR": "f_lamination_factor",
                }
                col_key = col_map.get(field, field.lower())
                actual_val = row.get(col_key)
                if actual_val is not None:
                    # 判断是否满足条件
                    satisfied = self._evaluate_condition(
                        actual_val, cond["operator"], cond["threshold"]
                    )
                    # 发送补充的 condition 步骤（含 actual 值）
                    yield ReasoningStep(
                        kind="condition",
                        label=f"{field} {cond['operator']} {cond['threshold']}",
                        field=field,
                        expected=f"{cond['operator']} {cond['threshold']}",
                        actual=actual_val,
                        satisfied=satisfied,
                    )

        # ── 步骤 4：生成最终自然语言回答（流式输出）────────────────────────
        result_summary = self._format_result(query_result)
        answer_prompt = f"""你是路美实验室数据分析系统的智能助手。

用户问题：{context.user_query}

业务背景：
{context.rule_context[:500]}

查询结果：
{result_summary}

请根据以上信息，用简洁专业的中文回答用户的问题。
- 如果是"为什么是某等级"的问题，明确指出哪些条件满足、哪些不满足
- 如果是统计问题，直接给出数字和结论
- 回答控制在 200 字以内"""

        stream = await self.llm.chat.completions.create(
            model=self.config["model_id"],
            messages=[{"role": "user", "content": answer_prompt}],
            temperature=0.7,
            stream=True,
        )
        async for chunk in stream:
            delta = chunk.choices[0].delta.content
            if delta:
                yield delta  # 文本块，由 orchestrator 包装为 SSE text 事件

    def _evaluate_condition(self, actual, operator: str, threshold) -> bool:
        """评估实际值是否满足条件。"""
        try:
            a, t = float(actual), float(threshold)
            return {
                ">=": a >= t, "<=": a <= t, ">": a > t,
                "<": a < t, "=": a == t, "!=": a != t,
            }.get(operator, False)
        except (TypeError, ValueError):
            return False

    def _format_result(self, result) -> str:
        """将查询结果格式化为文本摘要。"""
        if not result.rows:
            return "查询结果为空。"
        if len(result.rows) == 1:
            return "查询到 1 条记录：\n" + "\n".join(
                f"  {k}: {v}" for k, v in result.rows[0].items()
            )
        return f"查询到 {len(result.rows)} 条记录，前 3 条：\n" + "\n".join(
            str(row) for row in result.rows[:3]
        )
```

---

## 7. 主路由与 SSE 编排

```python
# services/agent-api/routers/chat.py

from __future__ import annotations
import json
from fastapi import APIRouter
from fastapi.responses import StreamingResponse
from pydantic import BaseModel
from agents.kg_agent import KGAgent
from agents.sql_agent import SQLAgent
from models.protocol import ReasoningStep
from models.context import AgentContext

router = APIRouter()

class ChatRequest(BaseModel):
    messages: list[dict]
    session_id: str | None = None
    model_name: str | None = None

def sse_event(data: dict) -> str:
    """将字典序列化为 SSE 格式字符串。"""
    return f"data: {json.dumps(data, ensure_ascii=False)}\n\n"

@router.post("/api/v1/chat/stream")
async def chat_stream(req: ChatRequest):
    """两阶段问答主入口，返回 SSE 流。"""
    user_query = next(
        (m["content"] for m in reversed(req.messages) if m["role"] == "user"),
        ""
    )
    session_id = req.session_id or "default"

    async def generate():
        all_reasoning_steps: list[ReasoningStep] = []
        agent_context: AgentContext | None = None

        # ── Stage 1: KG Agent ────────────────────────────────────────────────
        kg_agent = KGAgent(qdrant=get_qdrant(), llm=get_llm(), config=get_config())
        async for item in kg_agent.run(user_query, session_id):
            if isinstance(item, ReasoningStep):
                all_reasoning_steps.append(item)
                yield sse_event({
                    "type": "reasoning_step",
                    "reasoning_step": item.model_dump(exclude_none=True),
                })
            elif isinstance(item, AgentContext):
                agent_context = item

        if agent_context is None:
            yield sse_event({"type": "error", "error": "Stage 1 未返回上下文"})
            yield sse_event({"type": "done"})
            return

        # ── Stage 2: SQL Agent ───────────────────────────────────────────────
        sql_agent = SQLAgent(llm=get_llm(), db=get_db(), config=get_config())
        async for item in sql_agent.run(agent_context):
            if isinstance(item, ReasoningStep):
                # Stage 2 补充的 condition 步骤（含 actual 值）
                all_reasoning_steps.append(item)
                yield sse_event({
                    "type": "reasoning_step",
                    "reasoning_step": item.model_dump(exclude_none=True),
                })
            elif isinstance(item, str):
                # 最终回答文本块
                yield sse_event({"type": "text", "content": item})

        # ── 发送完整推理链元数据 ─────────────────────────────────────────────
        yield sse_event({
            "type": "response_metadata",
            "response_payload": {
                "reasoning_steps": [s.model_dump(exclude_none=True) for s in all_reasoning_steps],
                "session_id": session_id,
            },
        })
        yield sse_event({"type": "done"})

    return StreamingResponse(
        generate(),
        media_type="text/event-stream",
        headers={
            "Cache-Control": "no-cache",
            "X-Accel-Buffering": "no",
        },
    )
```

---

## 8. 典型交互示例

以用户问题"**为什么炉号 1丙20260110-1 是 C 级？**"为例，完整的 SSE 事件流如下：

```
data: {"type": "reasoning_step", "reasoning_step": {"kind": "record", "label": "命中检测记录：炉号 1丙20260110-1"}}

data: {"type": "reasoning_step", "reasoning_step": {"kind": "spec", "label": "产品规格 120", "detail": "带宽标准 119.5mm，检测列数 13"}}

data: {"type": "reasoning_step", "reasoning_step": {"kind": "rule", "label": "判定规则：C级（优先级 1）", "detail": "质量状态：合格"}}

data: {"type": "reasoning_step", "reasoning_step": {"kind": "condition", "label": "带宽 >= 119.5", "field": "F_WIDTH", "expected": ">= 119.5", "actual": 119.8, "satisfied": true}}

data: {"type": "reasoning_step", "reasoning_step": {"kind": "condition", "label": "Ps铁损 <= 1.30", "field": "F_PERF_PS_LOSS", "expected": "<= 1.30", "actual": 1.46, "satisfied": false}}

data: {"type": "reasoning_step", "reasoning_step": {"kind": "grade", "label": "1/2 条满足，主要差距在 Ps铁损上限；按规则归入 C 级"}}

data: {"type": "text", "content": "根据产品规格 120 的 C 级判定规则，该炉号共需满足 2 个条件："}
data: {"type": "text", "content": "带宽（119.8mm）满足 ≥119.5mm 的要求，但 Ps 铁损（1.46 W/kg）超过了 ≤1.30 W/kg 的上限。"}
data: {"type": "text", "content": "因此，该炉号被判定为 C 级（合格）。"}

data: {"type": "response_metadata", "response_payload": {"reasoning_steps": [...完整列表...]}}

data: {"type": "done"}
```

前端 `<KgReasoningChain>` 组件将实时渲染上述推理步骤，效果如下：

| 步骤 | 类型标签 | 内容 | 状态标签 |
|------|---------|------|---------|
| 1 | 命中记录 | 命中检测记录：炉号 1丙20260110-1 | — |
| 2 | 产品规格 | 产品规格 120 | — |
| 3 | 判定规则 | 判定规则：C级（优先级 1） | — |
| 4 | 条件评估 | 带宽 >= 119.5 | **满足**（绿色） |
| 5 | 条件评估 | Ps铁损 <= 1.30 | **不满足**（红色） |
| 6 | 最终结论 | 1/2 条满足，按规则归入 C 级 | — |

---

## 9. 容错与降级策略

| 场景 | 处理策略 | 前端展示 |
|------|---------|---------|
| Qdrant 未检索到相关规则 | 生成 `kind: "fallback"` 步骤，Stage 2 直接进行通用数据查询 | 显示"降级"标签，提示知识库未覆盖该问题 |
| SQL 生成失败（语法错误） | 最多重试 2 次，每次将错误信息反馈给 LLM 自我修正 | 若仍失败，返回友好错误提示 |
| SQL 执行超时（> 10s） | 中断查询，返回超时错误 | 提示用户缩小查询范围 |
| vLLM 服务不可用 | 返回 HTTP 503，前端 `onError` 回调处理 | 显示"服务暂时不可用"提示 |
| 查询结果为空 | Stage 2 直接告知用户无数据，不生成空回答 | 文本气泡显示"未找到相关数据" |

---

## 10. 与现有系统的集成方案

### 10.1 知识库同步触发

.NET 后端在以下操作发生时，需通知 `nlq-agent` 更新 Qdrant 知识库：

- `IntermediateDataJudgmentLevelService` 中的判定规则 **新增/修改/删除**
- `ProductSpecService` 中的产品规格 **新增/修改**

推荐通过 **HTTP 回调**实现：.NET 后端在数据变更后，向 `nlq-agent` 的 `/api/v1/admin/sync-knowledge` 接口发送 POST 请求，触发增量向量化。

### 10.2 前端接入（零改动）

现有的前端代码（`web/src/api/nlqAgent.ts` 和 `mobile/utils/sse-client.js`）**无需任何修改**，只需确保环境变量 `VITE_NLQ_AGENT_API_BASE`（Web 端）或 `NLQ_AGENT_API_BASE`（移动端）指向 `nlq-agent` 服务地址即可。

### 10.3 部署配置

在现有 `docker-compose.yml` 中追加 `nlq-agent` 服务：

```yaml
# 追加到 docker-compose.yml
services:
  nlq-agent:
    build:
      context: ./nlq-agent/services/agent-api
    image: lm-nlq-agent:latest
    container_name: lm-nlq-agent
    restart: unless-stopped
    ports:
      - "18100:18100"
    environment:
      - QDRANT_URL=http://qdrant:6333
      - VLLM_ENDPOINT=http://vllm:8082/v1
      - TEI_ENDPOINT=http://tei:8081
      - DB_CONNECTION_STRING=${DB_CONNECTION_STRING}
      - MODEL_ID=/data/qwen2.5-7b
    networks:
      - lm-apps-network
    depends_on:
      - qdrant
```

---

## 11. 实施路线图

本架构建议按以下顺序分阶段实施：

**第一阶段（基础能力）**：完成 `shared-types` 包的 Python 镜像（`models/protocol.py`），搭建 FastAPI 服务骨架，实现 SSE 流式响应，并用 Fixture 数据验证前端 `<KgReasoningChain>` 的渲染效果。

**第二阶段（知识入库）**：编写 `ingest_knowledge.py` 脚本，将 `LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL` 和 `LAB_PRODUCT_SPEC` 中的业务规则向量化并存入 Qdrant，完成 Stage 1 的检索能力。

**第三阶段（SQL 能力）**：实现 Stage 2 的 SQL 生成与执行，完成"为什么是某等级"类查询的端到端闭环，并补充 `condition` 步骤的 `actual` 值回填逻辑。

**第四阶段（统计查询）**：扩展 Stage 2，支持统计类查询（如"本月合格率"、"各班次铁损对比"），并优化 SQL 生成的 Prompt 策略。

**第五阶段（知识同步）**：实现 .NET 后端的知识库变更回调，确保 Qdrant 中的业务规则与数据库保持实时同步。
