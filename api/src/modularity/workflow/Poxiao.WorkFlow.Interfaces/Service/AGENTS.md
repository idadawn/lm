<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Service

## Purpose
工作流引擎的动态 API 服务接口集合。Furion `IDynamicApiController` 通过这些接口为外部模块提供方法级别的可注入访问点（HTTP 路由由具体实现的 `[Route]` 决定）。

## Key Files
| File | Description |
|------|-------------|
| `IFlowEngineService.cs` | 流程设计接口（如 `GetFlowFormList()` 返回流程树） |
| `IFlowTaskService.cs` | 流程任务发起接口（`Create(FlowTaskSubmitModel)`） |
| `IFlowTemplateService.cs` | 流程模板查询接口（列表 / 详情 / JSON） |
| `ILeaveApplyService.cs` | 内置请假申请表单接口 |
| `ISalesOrderService.cs` | 内置销售订单表单接口 |

## For AI Agents

### Working in this directory
- 新增动态 API 控制器时同步添加接口文件，方便其它模块（lab、system 等）通过 DI 调用工作流功能。
- 接口仅暴露其它模块需要复用的方法；纯前端 HTTP 接口可不暴露 Interface（保留 implementation）。

### Common patterns
- 文件命名 `I<Service>.cs`，与 `Poxiao.WorkFlow/Service/<Service>.cs` 严格对应。

## Dependencies
### Internal
- `Poxiao.WorkFlow.Entitys/Dto/FlowEngine`、`Dto/WorkFlowForm/*`、`Poxiao.Infrastructure.Models.WorkFlow`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
