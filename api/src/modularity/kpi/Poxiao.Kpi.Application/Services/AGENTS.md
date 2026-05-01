<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Services

## Purpose
KPI 模块业务服务实现根目录。每个子领域（与 `Dtos/` 子目录一一对应）放一对 `I*Service.cs` 接口与 `*Service.cs` 实现，由控制器注入。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `AnalysisData/` | 统计图（正态/直方/Xbar-R）服务 (see `AnalysisData/AGENTS.md`) |
| `MetricAnalysis/` | 指标归因任务服务 (see `MetricAnalysis/AGENTS.md`) |
| `MetricCategory/` | 指标分类树服务 (see `MetricCategory/AGENTS.md`) |
| `MetricCog/` | 指标图链服务 (see `MetricCog/AGENTS.md`) |
| `MetricCov/` | 价值链 + 价值链规则服务 (see `MetricCov/AGENTS.md`) |
| `MetricCovStatus/` | 价值链状态字典服务 (see `MetricCovStatus/AGENTS.md`) |
| `MetricDash/` | 仪表板服务 (see `MetricDash/AGENTS.md`) |
| `MetricData/` | 指标取数与图表数据服务 (see `MetricData/AGENTS.md`) |
| `MetricDataIE/` | 指标数据导入导出 / 动态建表服务 (see `MetricDataIE/AGENTS.md`) |
| `MetricDimension/` | 公共维度服务 (see `MetricDimension/AGENTS.md`) |
| `MetricGot/` | 思维图服务 (see `MetricGot/AGENTS.md`) |
| `MetricIGrade/` | 指标分级服务 (see `MetricIGrade/AGENTS.md`) |
| `MetricInfo/` | 指标定义（基础/派生/复合）+ 数据源服务 (see `MetricInfo/AGENTS.md`) |
| `MetricNotice/` | 指标通知服务 (see `MetricNotice/AGENTS.md`) |
| `MetricTag/` | 指标标签服务 (see `MetricTag/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 服务必须实现 `ITransient` 才能被 DI 容器解析。
- 数据库交互全部通过 `ISqlSugarRepository<TEntity>` 或 `ISqlSugarClient`/`ITenant`，不要直接 new SqlSugarClient。
- 对于跨服务调用，注入接口（如 `IMetricInfoService`、`IDbService`）而非实现类。

### Common patterns
- CRUD 模板：`AsQueryable().WhereIF(...).Select(...).ToPagedListAsync` → `PagedResultDto<T>.SqlSugarPageResult(data)`。
- 软删除：`CallEntityMethod(x => x.Delete()).UpdateColumns(it => new { it.DeleteTime, it.DeleteUserId, it.IsDeleted })`。
- 异常使用 `Oops.Oh(ErrorCode.K100xx)`。

## Dependencies
### Internal
- `../Dtos/**`、`../../Poxiao.Kpi.Core/**`、`Poxiao.Common`、`Poxiao.Systems.Interfaces`
### External
- SqlSugar, Mapster, NCalcSync, MathNet.Numerics, NPOI, Microsoft.Extensions.Logging

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
