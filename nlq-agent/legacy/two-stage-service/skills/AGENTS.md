<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# skills

## Purpose
nlq-agent 配套的 5 个 oh-my-claudecode 风格开发 Skills 目录。每个子目录是一个 `SKILL.md`（YAML frontmatter + 结构化指令块），Claude Code 会按 `name` 注册并通过 `level` 决定优先级。这些 skills 把"如何在 nlq-agent 中实现/调试某类任务"沉淀为可复用的标准操作流程（SOP），覆盖 Pipeline 全流程开发与运维。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `nlq-two-stage/` | 两阶段问答 Pipeline 整体开发与调试（level 4，最高优先级） |
| `nlq-query-router/` | Stage 1 入口的意图识别与查询路由配置（level 2） |
| `nlq-semantic-layer/` | 语义层 / MDL 文档设计与 Qdrant 知识维护（level 3） |
| `nlq-sql-debug/` | Stage 2 NL2SQL 错误诊断与 Prompt/模板修复（level 3） |
| `nlq-qdrant-init/` | Qdrant 集合初始化、增量同步、检索质量排查（level 2） |

## For AI Agents

### Working in this directory
- 每个 skill 自治：仅修改对应的 `SKILL.md` 即可；不要在 skills/ 添加 Python 源码或共享工具。
- frontmatter 字段约定：`name`（kebab-case）、`description`、`argument-hint`、`level`（1=基础到 4=高优）。
- 编写新 skill 时遵循现有结构：`<Purpose>` / `<Use_When>` / `<Do_Not_Use_When>` / `<Steps>` / `<Final_Checklist>`。
- 各 skill 的"路由表"（什么场景用哪个 skill）维护在 `nlq-two-stage` 的 `<Do_Not_Use_When>` 块——新增 skill 须同步更新。

### Common patterns
- skills 引用具体源文件路径（如 `src/pipelines/orchestrator.py`），便于 Claude 直接定位。
- 中文叙述 + 英文标识符；包含 curl / pytest / python -c 形式的可执行示例。

## Dependencies
### Internal
- 所有 skills 均假设工作目录为仓库根 `/data/project/lm/`，引用 `nlq-agent/...` 子路径。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
