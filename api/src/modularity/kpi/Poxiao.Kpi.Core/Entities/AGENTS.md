<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Entities

## Purpose
KPI 模块全部 SqlSugar 实体。按业务子领域分目录摆放，命名空间统一为 `Poxiao.Kpi.Core.Entitys`。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `MetricAnalysisTask/` | 归因分析任务实体 (see `MetricAnalysisTask/AGENTS.md`) |
| `MetricCategory/` | 指标分类树 + 分类计数 (see `MetricCategory/AGENTS.md`) |
| `MetricCog/` | 指标图链 (see `MetricCog/AGENTS.md`) |
| `MetricCov/` | 价值链节点 + 规则 (see `MetricCov/AGENTS.md`) |
| `MetricCovStatus/` | 价值链状态字典 (see `MetricCovStatus/AGENTS.md`) |
| `MetricDash/` | 指标仪表板 (see `MetricDash/AGENTS.md`) |
| `MetricDataIE/` | 动态建表登记 (see `MetricDataIE/AGENTS.md`) |
| `MetricDimension/` | 公共维度 (see `MetricDimension/AGENTS.md`) |
| `MetricGot/` | 指标思维图 (see `MetricGot/AGENTS.md`) |
| `MetricGraded/` | 指标分级 (see `MetricGraded/AGENTS.md`) |
| `MetricInfo/` | 指标定义（基础/派生/复合共表） (see `MetricInfo/AGENTS.md`) |
| `MetricNotice/` | 指标通知 (see `MetricNotice/AGENTS.md`) |
| `MetricTag/` | 指标标签 + 标签计数 (see `MetricTag/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 全部实体使用 `[SugarTable("metric_xxx", TableDescription = "...")]`，列名 snake_case 显式声明。
- 大部分继承 `CUDEntityBase`（含 `Id/IsDeleted/CreatedTime/...`）；少量较新表使用 `CUEntityBase`（无逻辑删除字段）。
- 枚举列使用 `SqlParameterDbType = typeof(EnumToStringConvert)`，落库为枚举名字符串。
- 不要随意修改 `CUEntityBase`/`CUDEntityBase` 选择，参考 `.cursorrules` 字段命名规则。

### Common patterns
- JSON 字段统一存 `string`/`string?`（DTO 端再反序列化为 `TableFieldOutput`、`MetricFilterDto` 等）。
- 不存在 `IsIgnore` 字段——若需新增扩展属性请用 `[SugarColumn(IsIgnore = true)]`。

## Dependencies
### Internal
- `../../../common/Poxiao.Common`（基类与转换器）
### External
- SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
