<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-29 | Updated: 2026-04-29 -->

# docs

## Purpose
项目长期文档。`PRD.md` / `TDD.md` 是产品需求与技术设计的全量来源；`changes/` 是逐功能的实施提案（"开发前必读"）；`KNOWLEDGE_GRAPH.md` / `knowledge-graph.md` 描述 Neo4j 节点/关系 schema。

## Key Files

| File | Description |
|------|-------------|
| `PRD.md` | 产品需求文档：业务目标、用户故事、四类问题（查询/归因/洞察/假设）、验收标准 |
| `TDD.md` | 技术设计：完整目录结构、API 设计、Agent 状态机、SQL 安全策略、SSE 协议 |
| `KNOWLEDGE_GRAPH.md` | KG 节点/关系/Cypher 示例（顶层版） |
| `knowledge-graph.md` | KG 详细字段说明（小写 alias，与上方互补） |

## Subdirectories

| Directory | Purpose |
|-----------|---------|
| `changes/` | 逐功能实施提案 + `000-template.md` 模板（见 `changes/AGENTS.md`） |

## For AI Agents

### Working In This Directory
- 任何代码改动若动到 PRD/TDD 的范围（新指标、新 Agent、新 SSE 字段、新 KG 节点），必须同步更新 PRD/TDD 对应章节，否则后续 AI 编程工具读旧文档会做出错误推断。
- `changes/` 单文件命名 `NNN-feature.md`（递增编号），完成后状态由 `草稿/评审中/开发中` 改为 `已完成`，不要删文件。

### Testing Requirements
- 文档无自动化测试。如改了 KG schema，建议手工跑 `scripts/test_kg_api.py` 验证 Cypher 语句仍可用。

### Common Patterns
- 中文为主、英文术语保留（如 `LangGraph`、`SSE`、`F_FORMULA_ID`），不要翻译。
- 表格优先于自然段（结构化更利于 LLM 抽取）。

## Dependencies

### Internal
- 数据库 schema 来源是父项目 `D:/project/lm/api/src/modularity/lab/Entity/`，本目录的 KG 文档必须与那边对齐。

### External
- 无运行时依赖。
