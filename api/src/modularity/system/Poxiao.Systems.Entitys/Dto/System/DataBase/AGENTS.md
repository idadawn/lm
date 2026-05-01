<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# DataBase (Dto)

## Purpose
数据库（外部库）表/字段管理 DTO。配合 `DbLink` 提供库内表清单、字段选择器、表数据预览、字段批量保存等接口。

## Key Files
| File | Description |
|------|-------------|
| `DatabaseTableListOutput.cs` | 表清单（表名、注释、行数） |
| `DatabaseTableFieldsSelectorOutput.cs` | 字段选择器（用于配置时选列） |
| `DatabaseTablePreviewOutput.cs` / `DatabaseTablePreviewQuery.cs` | 表数据预览（分页查询） |
| `DatabaseTableUpInput.cs` | 表结构/注释批量更新 |

## For AI Agents

### Working in this directory
- 多库（SQL Server/MySQL/Oracle）字段类型不统一，DTO 仅承载字符串类型名；前端按类型映射控件。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
