<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricDash

## Purpose
指标仪表板 DTO。`MetricDashService`/`MetricDashController` 用来保存/恢复仪表板的整张表单 JSON，并提供拖拽指标的可行性校验入参。

## Key Files
| File | Description |
|------|-------------|
| `MetricDashCrInput.cs` | 新建/Upsert 入参：`gotId`、`gotType`、`formJson` |
| `MetricDashUpInput.cs` | 更新入参 |
| `MetricDashListQueryInput.cs` | 列表查询参数 |
| `MetricDashListOutput.cs` | 列表项 |
| `MetricDashInfoOutput.cs` | 详情（含整张 `formJson`） |
| `MetricDashDragInput.cs` | 拖拽校验入参：当前指标 + 目标已存在指标列表 |

## For AI Agents

### Working in this directory
- `formJson` 是整个表单 JSON 字符串，由前端整段提交；服务层不做字段级解析。
- `Create` 服务中若 `gotId` 已存在会自动转 `Update`，DTO 字段保持松散类型 (`string?`)。

### Common patterns
- 与 `MetricDashEntity` 一一对应，存储列名为 `form_json`。
- `gotType` 来源 `GotType` 枚举（`Cov`/`Dash`），但 DTO 用字符串以兼容前端。

## Dependencies
### Internal
- `Services/MetricDash`、`Services/MetricInfo`
### External
- Newtonsoft.Json

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
