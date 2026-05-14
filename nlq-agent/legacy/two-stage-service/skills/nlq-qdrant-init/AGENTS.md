<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# nlq-qdrant-init

## Purpose
oh-my-claudecode skill 包：指导 Claude 执行路美 NLQ Agent 的 **Qdrant 语义层初始化**。覆盖全量重建与增量更新两种模式，把 MySQL 业务表（判定规则、产品规格）转成向量写入 Qdrant，是部署 nlq-agent 的前置步骤。

## Key Files
| File | Description |
|------|-------------|
| `SKILL.md` | Skill 主文档：`<Purpose>` / `<Use_When>` / `<Prerequisites>` / 步骤化命令清单（健康检查、Embedding 维度校验、初始化脚本调用） |

## For AI Agents

### Working in this directory
- 编辑 `SKILL.md` 时保留 frontmatter 三字段：`name`、`description`、`argument-hint`（不可改字段名）。
- `level: 2` 表示中等复杂度（需要预检环境）；不要无故升级 level。
- 执行示例命令前后必须做"健康检查"（Qdrant `/health`、MySQL `SELECT COUNT(*)`、Embedding 维度），与 SKILL 中的检查顺序保持一致。
- 引用的脚本路径以 `nlq-agent/scripts/init_semantic_layer.py` 为准；如脚本重命名，必须同步本 SKILL。
- 只描述"如何执行初始化"，不要在此 SKILL 内嵌业务知识扩展；那是 `nlq-semantic-layer` skill 的职责。

### Common patterns
- HTML-like 标签 `<Purpose>` / `<Use_When>` 是 OMC skill 的语义化分区，不是 markdown 标准语法。
- 命令块都带预期输出，便于 Claude 自检。

## Dependencies
### Internal
- 配套脚本 `nlq-agent/scripts/init_semantic_layer.py`
- 数据来源 `nlq-agent/src/services/qdrant_service.py`、`embedding_client.py`

### External
- Qdrant、TEI/OpenAI Embedding、MySQL

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
