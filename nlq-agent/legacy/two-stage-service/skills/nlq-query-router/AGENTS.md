<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# nlq-query-router

## Purpose
OMC skill：聚焦 NLQ Agent 的 **意图识别与查询路由** 模块。指导 Claude 修改 `src/pipelines/orchestrator.py::_route_intent()` 与 `src/utils/prompts.py` 中的 `INTENT_CLASSIFICATION_PROMPT`，配置 `statistical` / `root_cause` / `concept` / `out_of_scope` 四类意图的分类规则与处理路径。

## Key Files
| File | Description |
|------|-------------|
| `SKILL.md` | Skill 主文档：四类意图定义表、三层路由决策（关键词 → LLM → fallback）、常见路由错配排查思路 |

## For AI Agents

### Working in this directory
- 修改路由逻辑时必须同步两处：`orchestrator.py` 中的关键词列表 + `prompts.py` 中的分类提示词；不要只改一边。
- 新增意图类型必须先扩展 `src/models/schemas.py::IntentType` 枚举，再补 prompt few-shot，再增 orchestrator 分支。
- 关键词快捷路由优先级最高，目的是"省 LLM 调用"，因此关键词必须高精低召（误判代价大）。
- `out_of_scope` 不要执行 Stage 2，直接返回安全 fallback 文案，避免泄漏 DDL 给越界问题。
- Skill 描述中的"统计/根因/概念"分类边界不可随意调整，前端 KgReasoningChain 的渲染依赖之。

### Common patterns
- SKILL 用表格列举意图 → 触发场景 → 处理路径 → 示例问题，新增意图时沿用此表结构。

## Dependencies
### Internal
- `nlq-agent/src/pipelines/orchestrator.py`、`src/models/schemas.py`、`src/utils/prompts.py`
- 调用方：`nlq-agent/src/api/routes.py::chat_stream`

### External
- LLM (OpenAI 兼容端点 / vLLM)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
