<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Apps.Entitys

## Purpose
App 模块的实体与传输对象工程。包含 `BASE_APPDATA` 表对应的 `AppDataEntity`，以及在 App 与移动端之间流转的 Dto 结构。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Entity/` | SqlSugar 实体定义 (见 `Entity/AGENTS.md`) |
| `Dto/` | 输入/输出 Dto，全部 `[SuppressSniffer]` (见 `Dto/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 实体继承 `OCDEntityBase`（旧式基类，使用 `F_OBJECTID` / `F_OBJECTDATA` 等大写字段名），对应历史 `BASE_APPDATA` 表。新建实体若使用新表，请遵循 `.cursorrules` 的 `CLDEntityBase` 规范，并审视是否仍需 `[SugarColumn(ColumnName = ...)]` 重写。
- DTO 字段统一 camelCase，可空类型加 `?`，与前端 TS 类型对齐。
- 不要在此工程引用任何 service 工程；它仅承载数据结构。

### Common patterns
- DTO 类标 `[SuppressSniffer]` 避免反射扫描；属性附中文 XML 注释。

## Dependencies
### Internal
- `Poxiao.Infrastructure.Contracts`（提供 `OCDEntityBase`）
- `Poxiao.Infrastructure.Models.User`（`PositionInfoModel`）

### External
- `SqlSugar`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
