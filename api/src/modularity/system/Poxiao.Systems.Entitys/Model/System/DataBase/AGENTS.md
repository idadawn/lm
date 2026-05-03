<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# DataBase (Model)

## Purpose
描述外部/本地数据库表与字段的抽象 Model。配合 `DbLink`/`DataBase`/`DataSync`/`PrintDev` 等服务在多库场景下做元数据探查、表结构差异比对与动态查询。

## Key Files
| File | Description |
|------|-------------|
| `DbTableModel.cs` | 表元信息：表名、注释、行数、列集合 |
| `DbTableFieldModel.cs` | 字段元信息：字段名、类型、长度、可空、注释、是否主键等 |
| `DbTableAndFieldModel.cs` | 表 + 字段集合的复合结构，用于一次返回 |
| `DynamicDbTableModel.cs` | 动态运行期表结构，承载从 SqlSugar 反射出的表/字段 |

## For AI Agents

### Working in this directory
- 多数据库兼容（SQL Server / MySQL / Oracle）：字段类型字符串保持小写，类型名以源库原始名为准。
- 与前端"表设计器"相关，字段命名（camelCase）不要随意调整。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
