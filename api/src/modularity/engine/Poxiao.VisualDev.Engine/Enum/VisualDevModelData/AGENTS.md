<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# VisualDevModelData

## Purpose
可视化开发"数据缓存"类别枚举。给前端控件下发数据时，约定数据来源类型：字典、静态、动态接口、级联值、查询字段映射、时间控件、可视化数据列表等。

## Key Files
| File | Description |
|------|-------------|
| `vModelType.cs` | 枚举 `vModelType`，每个成员带 `[Description]` 标签（dictionary / static / keyJsonMap / value / dynamic / timeControl / list） |

## For AI Agents

### Working in this directory
- 描述字符串与前端 JSON 约定一致，禁止直接 rename；如需新增类型先在前端 `dataType` 列表中确认。

## Dependencies
### External
- `System.ComponentModel`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
