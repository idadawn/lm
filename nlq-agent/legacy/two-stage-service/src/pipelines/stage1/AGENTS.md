<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# stage1

## Purpose
两阶段问答的 **Stage 1：Semantic & KG Agent**。负责意图分类、向 Qdrant 检索业务知识（判定规则/产品规格/指标定义）、语义解析（提取过滤条件与目标指标），并通过 SSE 实时推送 `reasoning_step` 给前端 KgReasoningChain 渲染推理链。

## Key Files
| File | Description |
|------|-------------|
| `semantic_kg_agent.py` | `SemanticKGAgent.run()`：LLM 意图分类 → Embedding 向量化问题 → Qdrant 检索 Top-K → LLM 语义解析 → 输出 `AgentContext` + 流式推送多个 ReasoningStep |
| `__init__.py` | 子包标记 |

## For AI Agents

### Working in this directory
- 推送顺序约束：先 `record` / `spec` / `rule`（事实步骤），再 `condition`（待校验），最后传给 Stage 2 回填 `actual` / `satisfied`。
- 概念类问题（IntentType.CONCEPTUAL）不要构造 `AgentContext.filters`，直接由 orchestrator 走"概念回答"分支。
- Qdrant 检索 top_k 默认 5（来自 settings），增大需评估 LLM context 预算。
- 错误处理：检索为空时必须 emit 一个 `fallback` step 而不是静默继续，让用户看到"未找到相关知识"。
- 严禁在 Stage 1 直接读 MySQL；表数据是 Stage 2 的职责。

### Common patterns
- 使用 `STAGE1_SEMANTIC_SYSTEM` / `STAGE1_SEMANTIC_USER` prompt 模板，输出 JSON 由 Pydantic `AgentContext` 校验。
- `async for` 输出 SSE 行，调用方串到 orchestrator。

## Dependencies
### Internal
- `src/services/qdrant_service.py`、`embedding_client.py`、`llm_client.py`、`sse_emitter.py`
- `src/utils/prompts.py`、`src/models/schemas.py`

### External
- LLM / Embedding / Qdrant

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
