<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-29 | Updated: 2026-04-29 -->

# changes

## Purpose
逐功能实施提案。每次开发新功能/重构前先在这里加一份 `NNN-feature.md`，描述背景、范围、技术方案、验收标准、影响文档；评审通过再开 `feature/*` 分支。文件不删除，状态字段在头部更新（`草稿/评审中/开发中/已完成`）。

## Key Files

| File | Description |
|------|-------------|
| `000-template.md` | 提案模板，新建文件时复制此文件 |
| `001-query-agent-mvp.md` | QueryAgent MVP — 指标查询完整闭环（首个 P0 提案） |

## For AI Agents

### Working In This Directory
- 新文件命名 `NNN-kebab-case.md`，编号严格递增不复用。
- 完成后状态改为 `已完成`、补充"实际变更与原方案差异"段，但不要删除原内容（保留历史决策依据）。
- 模板必填段：背景与目标 / 范围（做+不做） / 技术方案 / 验收标准 / 影响的文档。

### Testing Requirements
- 文档无自动化测试。建议提案合入前 lint 中文标点（与 `docs/TDD.md` 风格一致）。

### Common Patterns
- 中文为主，模板里的关键词不翻译（`Tool`、`@tool`、`SSE`、`StreamEvent`）。
- 用 markdown 表格列"做"与"不做"，便于 reviewer 一眼看清范围。

## Dependencies

### Internal
- 提案完成后通常会更新 `../PRD.md` / `../TDD.md` 对应章节。

### External
- 无。
