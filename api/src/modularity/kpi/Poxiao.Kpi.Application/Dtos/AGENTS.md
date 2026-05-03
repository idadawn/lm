<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Dtos

## Purpose
KPI 应用层 DTO 根目录。按子领域分文件夹存放新增/更新/查询/列表/详情等数据传输对象，对应 `Services/` 与 `Controller/` 的输入输出契约。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `AnalysisData/` | 控制图、Xbar-R、正态/直方图等统计分析输出 (see `AnalysisData/AGENTS.md`) |
| `Chat/` | 指标对话接口（自然语言 → 指标值/图表数据）DTO (see `Chat/AGENTS.md`) |
| `MetricAnalysis/` | 指标归因分析任务 DTO (see `MetricAnalysis/AGENTS.md`) |
| `MetricCategory/` | 指标分类树 DTO (see `MetricCategory/AGENTS.md`) |
| `MetricCog/` | 指标图链 DTO (see `MetricCog/AGENTS.md`) |
| `MetricCov/` | 指标价值链与规则 DTO (see `MetricCov/AGENTS.md`) |
| `MetricCovStatus/` | 价值链节点状态 DTO (see `MetricCovStatus/AGENTS.md`) |
| `MetricDash/` | 指标仪表板表单/拖拽 DTO (see `MetricDash/AGENTS.md`) |
| `MetricData/` | 指标取数与图表数据 DTO (see `MetricData/AGENTS.md`) |
| `MetricDataIE/` | 指标数据导入导出与建表模板 DTO (see `MetricDataIE/AGENTS.md`) |
| `MetricDimension/` | 公共维度 DTO (see `MetricDimension/AGENTS.md`) |
| `MetricGot/` | 指标思维图 DTO (see `MetricGot/AGENTS.md`) |
| `MetricIGrade/` | 指标分级 DTO (see `MetricIGrade/AGENTS.md`) |
| `MetricInfo/` | 指标定义（基础/派生/复合）DTO，含 `Composite/Db/Derive/` 三类子目录 (see `MetricInfo/AGENTS.md`) |
| `MetricNotice/` | 指标消息通知 DTO (see `MetricNotice/AGENTS.md`) |
| `MetricTag/` | 指标标签 DTO (see `MetricTag/AGENTS.md`) |
| `RealData/` | 实时数据查询 DTO (see `RealData/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 命名约定：`*CrInput`(创建) / `*UpInput`(更新) / `*ListQueryInput`(列表查询) / `*InfoOutput`(详情) / `*ListOutput`(列表项) / `*Output`(通用输出)。
- DTO 标注 `[SuppressSniffer]`，每个属性使用 `[JsonProperty("camelCase")]` 暴露给前端。
- 含枚举属性时引用 `Poxiao.Kpi.Core.Enums`，不要在 DTO 内重复定义。

### Common patterns
- 列表/树形 DTO 继承 `TreeModel`，配合 `.ToTree("-1")` 扩展输出层级。
- 复杂 JSON 字段（dimensions/filters/format/timeDimensions）在 DTO 用强类型 `List<TableFieldOutput>`、`MetricFilterDto`、`DataModelFormat`、`MetricTimeDimensionDto`，由 `Mapper/` 序列化到实体的 string 字段。

## Dependencies
### Internal
- `../../Poxiao.Kpi.Core`（枚举、`DbSchemaOutput`）
- `Poxiao.Infrastructure`（`PagedResultDto`、`TableFieldOutput`、`DBAggType`、`MetricFilterDto` 等）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
