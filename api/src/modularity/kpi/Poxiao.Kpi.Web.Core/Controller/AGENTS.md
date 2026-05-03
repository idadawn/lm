<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Controller

## Purpose
KPI 模块全部 HTTP 控制器集合。每个业务子领域一个控制器，构造注入对应 `I*Service`，转发请求并按 ErrorCode 抛 `Oops.Oh`。

## Key Files
| File | Description |
|------|-------------|
| `KpiController.cs` | 指标对话 (`api/kpi/v1/Chat`)：`getall`/`metric` 给 LLM 用，`[AllowAnonymous]` |
| `MesController.cs` | MES 订单生产进度示例接口 (`api/mes`)，`[AllowAnonymous]`；当前包含一段大块注释样本数据 |
| `AnalysisDataController.cs` | 统计分析 (`api/kpi/v1/AnalysisData`)：`list` 正态/直方、`rb` X̄-R |
| `MetricCategoryController.cs` | 指标分类 CRUD + `selector` 树 |
| `MetricCogController.cs` | 指标图链 CRUD |
| `MetricCovController.cs` / `MetricCovRuleController.cs` / `MetricCovStatusController.cs` | 价值链节点、规则、状态字典 CRUD |
| `MetricDashController.cs` | 仪表板 Upsert/Get/拖拽校验 |
| `MetricDataController.cs` | 指标取数：`{metricId}` 单值、`POST` 图表、`POST more` 多指标合并 |
| `MetricDataIEController.cs` | 数据导入导出 (`getCreate`/`createTable` 等)；类名为 `ExcelContriller`(原文拼写) |
| `MetricDimensionController.cs` | 公共维度 CRUD |
| `MetricGotController.cs` | 思维图 CRUD（按类型分页） |
| `MetricGradedController.cs` | 指标分级 CRUD |
| `MetricInfoController.cs` | 指标定义主入口（基础指标 + 数据源/Schema/字段/聚合查询） |
| `MetricInfo4CompositeController.cs` | 复合指标 CRUD + 公式校验 |
| `MetricInfo4DeriveController.cs` | 派生指标 CRUD |
| `MetricNoticeController.cs` | 指标通知 CRUD + 模板列表 |
| `MetricTagController.cs` | 标签 CRUD |
| `MetricAnalysisController.cs` | 归因分析任务接口 |

## For AI Agents

### Working in this directory
- 所有控制器路由统一 `api/kpi/v1/[controller]`；`Tag/Name/Order` 用 `[ApiDescriptionSettings]` 标注以便 OpenAPI 分组。
- 一律 `IDynamicApiController` 而非 `ControllerBase`，由动态 API 框架自动生成 Swagger。
- 业务校验返回值用 `if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000/1001/1002)`。

### Common patterns
- 路由示例：`HttpPost("")`/`HttpGet("{id}")`/`HttpGet("list")`/`HttpPut("{id}")`/`HttpDelete("{id}")`/`HttpGet("selector")`。
- `KpiController.GetMetricDataAsync` 使用 `time.GetTimeRange()` 拓展自然语言时间区间，便于 AI 调用。
- `ExcelContriller`/`MesController` 名字保留历史拼写，重构时如需更名要同时同步前端调用。

## Dependencies
### Internal
- `../../Poxiao.Kpi.Application`（接口与 DTO）
### External
- Microsoft.AspNetCore.Mvc, Microsoft.AspNetCore.Authorization, Furion `IDynamicApiController`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
