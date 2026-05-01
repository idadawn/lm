<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# DataInterfaceLog (Dto)

## Purpose
数据接口调用日志 DTO。仅查询，不允许人工写入。提供列表输出与查询条件。

## Key Files
| File | Description |
|------|-------------|
| `DataInterfaceLogListOutput.cs` | 调用日志列表（接口名、参数、耗时、状态、错误信息） |
| `DataInterfaceLogListQuery.cs` | 查询条件（时间范围、接口、状态） |

## For AI Agents

### Working in this directory
- 日志只读：禁止在 DTO 中提供写入字段。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
