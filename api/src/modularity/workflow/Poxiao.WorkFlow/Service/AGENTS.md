<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Service

## Purpose
工作流引擎的对外动态 API 控制器层。每个 `*Service.cs` 通过 `IDynamicApiController` + `[ApiDescriptionSettings(Tag="WorkflowEngine", Order=…)]` 暴露 `api/workflow/Engine/*` 接口，覆盖流程设计、模板、待办、已办、监控、抄送、评论、委托、撤回 / 撤销等场景。

## Key Files
| File | Description |
|------|-------------|
| `FlowEngineService.cs` | 流程设计 CRUD（FLOW_ENGINE）：列表、信息、发布、导入导出、表单元数据 |
| `FlowTemplateService.cs` | 已发布流程模板（FLOW_TEMPLATE）查询、版本、协管、JSON 详情 |
| `FlowTaskService.cs` | 流程任务发起 / 保存 / 更新入口（POST/PUT），调度 FlowTaskManager |
| `FlowBeforeService.cs` | 待我审批 / 批量审批 / 我已审批列表与详情、审批提交、加签 |
| `FlowLaunchService.cs` | 我发起的流程列表与撤回（FlowLaunchActionWithdrawInput） |
| `FlowMonitorService.cs` | 流程监控：列表、强制流转、删除、节点变更 |
| `FlowCommentService.cs` | 流程评论 CRUD |
| `FlowDelegateService.cs` | 委托规则 CRUD（发起 / 审批委托） |
| `FlowFormService.cs` | 流程表单元数据查询（formData / propertyJson 等） |
| `VersionService.cs` | 工作流模块版本信息 |

## For AI Agents

### Working in this directory
- 控制器构造仅依赖 `Manager` / `Service` 接口与 `IUserManager`，不要直接访问 SqlSugar；数据访问走 Manager 或对应 Repository。
- HTTP 动词与 RESTful 命名保持一致：`GET` 列表/详情、`POST` 创建/动作、`PUT` 更新、`DELETE` 删除；动作型接口（撤回、撤销、催办）使用 `POST` + 子路径。
- 异常一律 `try { … } catch (AppFriendlyException ex) { throw Oops.Oh(ex.ErrorCode, ex.Args); }`，不要直接 throw 系统异常。

### Common patterns
- 类标注 `[ApiDescriptionSettings(Tag="WorkflowEngine", Name="…", Order=30x)]`，Order 段位 30x 用于引擎、5xx 用于内置表单。
- 入参 DTO 命名 `*CrInput / *UpInput / *ListQuery`，输出 `*ListOutput / *InfoOutput`，与 `Poxiao.WorkFlow.Entitys/Dto/*` 一一对应。
- `IUserManager.UserId` 作为发起人 / 委托人解析依据。

## Dependencies
### Internal
- `Poxiao.WorkFlow.Interfaces`、`Poxiao.WorkFlow.Entitys`、`framework/Poxiao/*`、`modularity/system`、`modularity/visualdev`、`modularity/message`
### External
- SqlSugar、Mapster、Furion(DynamicApiController/FriendlyException)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
