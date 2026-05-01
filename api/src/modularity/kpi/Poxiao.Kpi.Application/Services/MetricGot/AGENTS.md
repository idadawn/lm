<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricGot

## Purpose
指标思维图 (GOT) 服务。管理价值链 (`Cov`) 与仪表板 (`Dash`) 两类思维图条目，并按标签反查 GotId。

## Key Files
| File | Description |
|------|-------------|
| `IMetricGotService.cs` | CRUD、按类型分页查询、按 tag 反查 GotId |
| `MetricGotService.cs` | 实现：`GotType` 与字符串 `Type` 字段比较 (`type.ToString().Equals(x.Type)`)；标签字段是逗号分隔的 ID 串，运行时 split 后联表 `MetricTagsEntity` 显示名称 |

## For AI Agents

### Working in this directory
- `MetricTag` 字段为字符串，用 `,` 分隔多个 tagId；`GetListAsync` 返回时会拼接 tag 名称用于显示。
- 列表查询按 `Sidx + " " + Sort` 动态排序，请确保上游字段在白名单内。

### Common patterns
- `pageList.List.ForEach(x => x.TypeStr = x.Type?.GetDescription())` 把枚举转为中文描述。
- 关闭警告 `#pragma warning disable SA1519`（控制流大括号）。

## Dependencies
### Internal
- `../MetricTag`、`../../../Poxiao.Kpi.Core/Entities/MetricGot`、`Enums/GotType`
### External
- SqlSugar, Mapster

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
