<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# kpi

## Purpose
KPI/指标管理与分析模块。提供基础/派生/复合三类指标的定义、价值链(COV)、思维图(GOT)、仪表板(Dash)、分级(Graded)、统计分析(Xbar-R 控制图、归因)、消息通知与导入导出能力。是检测室数据分析系统中支撑业务度量与可视化的核心模块。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Poxiao.Kpi.Application/` | DTO、Mapster 映射器、应用服务 (see `Poxiao.Kpi.Application/AGENTS.md`) |
| `Poxiao.Kpi.Core/` | SqlSugar 实体、枚举与跨层模型 (see `Poxiao.Kpi.Core/AGENTS.md`) |
| `Poxiao.Kpi.Web.Core/` | `IDynamicApiController` 控制器，承载 `api/kpi/v1/*` 接口 (see `Poxiao.Kpi.Web.Core/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 三层结构 Web.Core → Application → Core；上层只依赖下层。新增功能保持此分层，不在 Web.Core 直接写业务逻辑。
- 实体继承 `CUEntityBase` 或 `CUDEntityBase`（来自 `Poxiao.Common`），列名使用 snake_case 通过 `[SugarColumn(ColumnName="...")]` 显式声明，请遵守 `.cursorrules` 与项目根 `CLAUDE.md`。
- 业务模块沿 `Metric{Domain}` 子目录拆分（如 `MetricCategory/MetricCog/MetricCov/MetricCovStatus/MetricDash/MetricData/MetricDataIE/MetricDimension/MetricGot/MetricGraded/MetricInfo/MetricNotice/MetricTag` 等），保持 Dto + Service + Entity + Controller 平行命名。
- 错误码使用 `ErrorCode.K100xx` 系列，由 `Oops.Oh` 抛出。

### Common patterns
- 服务实现 `ITransient` 并通过 `ISqlSugarRepository<TEntity>` 访问数据；Mapster 通过 `Adapt<>()` 与 `IRegister` 注册转换。
- 树形数据（指标分类、价值链）使用 `CategoryIdTree`/`CovTreeId` 拼接祖先路径，`-1` 表示根节点。
- JSON 字段（`format`/`dimensions`/`filters`/`time_dimensions`）以字符串落库，DTO 端通过 Mapper 序列化/反序列化。

## Dependencies
### Internal
- `api/src/modularity/common/Poxiao.Common`（实体基类、枚举、SqlSugar 扩展）
- `api/src/modularity/system/Poxiao.Systems.Interfaces`（数据库连接 `IDbLinkService`、用户管理 `IUserManager`）
- `api/src/modularity/message`（指标通知模板）

### External
- SqlSugar (ORM)
- Mapster (DTO mapping)
- NCalcSync (复合指标公式解析)
- MathNet.Numerics (统计分析、控制图)
- MailKit (通知邮件)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
