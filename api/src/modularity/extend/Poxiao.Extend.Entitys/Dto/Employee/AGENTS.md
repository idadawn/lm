<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Employee

## Purpose
职员模块 DTO。仅做基础列表 + 导入流程：列表项、查询、导入入参/导入结果。

## Key Files
| File | Description |
|------|-------------|
| `EmployeeListQuery.cs` | 列表查询（分页 + 关键字） |
| `EmployeeListOutput.cs` | 列表项（编号/姓名/性别/部门/职位/电话...） |
| `ImportDataInput.cs` | NPOI 导入数据行入参 |
| `ImportDataOutput.cs` | 导入结果（成功/失败行数 + 错误明细） |

## For AI Agents

### Working in this directory
- 与 `EmployeeService.cs` 的 NPOI 导入对接，新增列时需同时改实体 + 模板。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
