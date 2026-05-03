<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Kpi.Core

## Purpose
KPI 模块核心程序集 (`net10.0`)。仅依赖 `Poxiao.Common`，承载 SqlSugar 实体、枚举和跨层模型，是 Application 与 Web.Core 的下层基础。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.Kpi.Core.csproj` | 引用 `Poxiao.Common/Poxiao.Infrastructure.csproj` |
| `GlobalUsings.cs` | 全局 `using`：SqlSugar、Newtonsoft.Json、`Poxiao.Infrastructure.*`、`System.ComponentModel` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Entities/` | SqlSugar 实体（按业务子领域分文件夹） (see `Entities/AGENTS.md`) |
| `Enums/` | 业务枚举（带 `[JsonConverter(typeof(EnumUseNameConverter<>))]` 与 `[Description]`） (see `Enums/AGENTS.md`) |
| `Model/` | 跨层共享模型（如 `DbSchemaOutput`） (see `Model/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 实体放在 `Poxiao.Kpi.Core.Entitys` namespace（注意拼写：`Entitys` 而非 `Entities`）。
- 列名一律 `snake_case`、显式 `[SugarColumn(ColumnName="...")]`；枚举字段使用 `SqlParameterDbType = typeof(EnumToStringConvert)` 入库为字符串。
- 表名以 `metric_` 前缀，例如 `metric_info/metric_category/metric_cov/metric_got/metric_dash/metric_graded`。

### Common patterns
- 实体继承 `CUDEntityBase`（含逻辑删除）或 `CUEntityBase`（仅创建/更新）；`MetricDataIETableCollectionEntity` 自定义最简结构。
- 枚举对外 JSON 都按 name 序列化，前端用字符串值。

## Dependencies
### Internal
- `../../common/Poxiao.Common`（实体基类、`EnumToStringConvert`、`TreeModel`、`SnowflakeIdHelper`）
### External
- SqlSugar, Newtonsoft.Json

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
