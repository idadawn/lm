<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# utils

## Purpose
nlq-agent 的通用工具模块。当前主要承载 LLM Prompt 模板的集中管理——包含意图分类、Stage 1 语义解析、Stage 2 SQL 生成与修正、概念回答、最终回答共 6+ 套模板，所有 LLM 调用统一从此处取 prompt。

## Key Files
| File | Description |
|------|-------------|
| `prompts.py` | 全部 Prompt 模板：`INTENT_CLASSIFICATION_*`、`STAGE1_SEMANTIC_*`、`STAGE2_SQL_GENERATION_*`、`STAGE2_SQL_CORRECTION_*`、`CONCEPTUAL_ANSWER_*`、`FINAL_ANSWER_*` |
| `__init__.py` | 子包标记 |

## For AI Agents

### Working in this directory
- 所有 prompt 必须是 Python 字符串常量，命名遵循 `<阶段>_<用途>_<SYSTEM|USER>` 三段式，便于 grep 定位。
- 用 `str.format()` / f-string 注入变量；占位符（如 `{question}` / `{ddl}` / `{context}`）必须在调用方 100% 提供，避免 KeyError。
- 中文术语（铁损、磁感、A 类等）保持原样不要英化；few-shot 示例使用真实 DDL 列名（`F_PERF_PS_LOSS` 等）。
- 修改 prompt 时联动更新 `nlq-sql-debug` / `nlq-query-router` SKILL 文档中的引用片段。
- 不要把业务知识（具体阈值、判定规则）硬编码进 prompt——这些应来自 Stage 1 的 Qdrant 检索。

### Common patterns
- SYSTEM prompt 描述角色与输出格式，USER prompt 注入运行时变量。
- 输出要求 JSON 时附带 schema 描述，方便 Pydantic 反序列化校验。

## Dependencies
### Internal
- 调用方：`src/pipelines/orchestrator.py`、`stage1/semantic_kg_agent.py`、`stage2/data_sql_agent.py`

### External
- 无（纯字符串常量）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
