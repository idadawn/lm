<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# BillRule (Dto)

## Purpose
单据编号规则 DTO。生成业务单号（如检测申请 LIMS-YYYYMMDD-NNNN）。提供 CRUD、详情、列表查询。

## Key Files
| File | Description |
|------|-------------|
| `BillRuleCrInput.cs` / `BillRuleUpInput.cs` | 规则创建/更新（前缀、日期格式、序号长度、流水重置策略等） |
| `BillRuleInfoOutput.cs` | 规则详情 |
| `BillRuleListOutput.cs` | 列表项 |
| `BillRuleListQueryInput.cs` | 列表查询条件 |

## For AI Agents

### Working in this directory
- 单号生成在 Service 端使用 `IBillRullService`（注意拼写）。DTO 字段调整需确保与规则解析一致。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
