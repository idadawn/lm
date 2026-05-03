<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# AdvancedQuery (Dto)

## Purpose
高级查询方案 DTO。允许用户在列表页保存自定义筛选/排序方案，下次直接复用。提供创建、详情、列表输出。

## Key Files
| File | Description |
|------|-------------|
| `AdvancedQuerySchemeCrInput.cs` | 创建查询方案（含目标模块、字段、运算符、值） |
| `AdvancedQuerySchemeInfoOutput.cs` | 方案详情 |
| `AdvancedQuerySchemeListOutput.cs` | 我的方案列表 |

## For AI Agents

### Working in this directory
- 字段表达 (operator/value/logic) 序列化为 JSON 存 `BASE_ADVANCEDQUERY.F_QUERYJSON`，DTO 字段名要与前端方案构造器保持一致。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
