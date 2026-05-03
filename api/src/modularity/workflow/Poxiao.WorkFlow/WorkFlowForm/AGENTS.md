<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# WorkFlowForm

## Purpose
内置流程表单的 API 实现层。两个示例业务表单（请假申请、销售订单）通过独立动态控制器对外暴露 `api/workflow/Form/*`，演示如何把外部业务表单接入工作流引擎并存储到独立业务表（`WFORM_LEAVEAPPLY` / `WFORM_SALESORDER`）。

## Key Files
| File | Description |
|------|-------------|
| `LeaveApplyService.cs` | 请假申请 CRUD（信息 / 提交 / 更新），落库到 `LeaveApplyEntity`，Tag="WorkflowForm" Order=516 |
| `SalesOrderService.cs` | 销售订单 CRUD（含明细 entryList），落库到 `SalesOrderEntity` + `SalesOrderEntryEntity`，Tag="WorkflowForm" Order=532 |

## For AI Agents

### Working in this directory
- 业务表单实体放在 `Poxiao.WorkFlow.Entitys/Entity/WorkFlowForm/`，DTO 放在 `Poxiao.WorkFlow.Entitys/Dto/WorkFlowForm/<Form>/`，命名严格配对。
- 控制器仅注入 `ISqlSugarRepository<TEntity>` + `IUserManager` + `IFileManager` + `ICacheManager`；流程发起逻辑由前端先提交本表单数据 → 再通过 `FlowTaskService` 创建任务。
- 新增内置表单：复制现有 Service 改名，注意修改 `[ApiDescriptionSettings(Order=…)]` 避开既有段位，并在 `Interfaces/Service/` 增加对应 `I*Service`。

### Common patterns
- 类头有版本/作者/日期注释（保留 Poxiao 原始 header）。
- `GetInfo("0")` 约定返回 null 用于「新建」场景。
- 通过 `Adapt<TOutput>()` 把实体转换为 DTO。

## Dependencies
### Internal
- `Poxiao.WorkFlow.Interfaces`、`Poxiao.WorkFlow.Entitys`、`framework/Poxiao/*`
### External
- SqlSugar、Mapster

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
