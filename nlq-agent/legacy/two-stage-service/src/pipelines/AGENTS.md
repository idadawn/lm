<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# pipelines

## Purpose
两阶段问答 Pipeline 的实现层。`PipelineOrchestrator` 负责意图路由 + 串联 Stage 1（语义/图谱检索）与 Stage 2（SQL 查询）+ SSE 事件流编排。是 nlq-agent 的核心业务逻辑。

## Key Files
| File | Description |
|------|-------------|
| `orchestrator.py` | `PipelineOrchestrator.stream_chat()`：意图分类 → 选择路径（statistical/root_cause→Stage1+2，conceptual→仅 Stage1，out_of_scope→fallback）→ 串行 yield SSE 事件 |
| `__init__.py` | 子包标记 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `stage1/` | Semantic & KG Agent（向量检索 + 语义解析） (see `stage1/AGENTS.md`) |
| `stage2/` | Data & SQL Agent（NL2SQL + 执行 + 修正 + 回答） (see `stage2/AGENTS.md`) |

## For AI Agents

### Working in this directory
- Orchestrator 必须保持 `AsyncIterator[str]`（已编码 SSE 行）作为输出契约，不要改回 `dict`。
- 不要在 orchestrator 中执行外部 IO（LLM / DB / Qdrant），把这些交给 Stage Agent；orchestrator 只做编排与异常兜底。
- 意图分类失败时回退到 `IntentType.OUT_OF_SCOPE`，并 emit `fallback` 类型 reasoning_step。
- Stage 间通过 `AgentContext` 传递结构化结果；禁止用全局变量或 module 状态。
- 新增 Stage（如 Stage 3 数据可视化）时遵循同样的目录划分（`stage3/<agent>.py`）。

### Common patterns
- `async for event in stage.run(...): yield event` 透传 SSE 子流。
- 顶层 try/except 兜底任何 stage 异常，emit `error` 事件并保证 `done` 仍发送。

## Dependencies
### Internal
- `src/services/*` (LLM/Qdrant/DB/SSE/Embedding)
- `src/models/schemas.py`、`src/utils/prompts.py`

### External
- 异步运行时（asyncio）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
