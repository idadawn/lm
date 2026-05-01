<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ScreenDataSource

## Purpose
大屏数据源（`VisualDBEntity` → `BLADE_VISUAL_DB`）DTO，以及运行时动态执行 SQL 查询的入参。

## Key Files
| File | Description |
|------|-------------|
| `ScreenDataSourceCrInput.cs` | 创建数据源（name/driverClass/url/username/password/remark） |
| `ScreenDataSourceUpInput.cs` | 更新数据源 |
| `ScreenDataSourceInfoOutput.cs` | 详情 |
| `ScreenDataSourceListOutput.cs` | 列表行（password 字段也返回，前端展示连接信息）|
| `ScreenDataSourceListQueryInput.cs` | 列表查询参数 |
| `ScreenDataSourceSeleectOutput.cs` | 选择器输出（注意原文件名拼写 `Seleect`） |
| `ScreenDataSourceDynamicQueryInput.cs` | 大屏组件运行时动态查询入参（数据源 id + SQL/参数） |

## For AI Agents

### Working in this directory
- 动态查询走 `ScreenDataSourceService` 内的临时 `ISqlSugarClient`，必须做 SQL 注入防护（仅允许 SELECT，黑名单 ; / DROP / UPDATE / DELETE 等）。
- `password` 在列表中明文返回，前端弹窗时需手工脱敏。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
