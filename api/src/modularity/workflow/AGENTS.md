<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# workflow

## Purpose
工作流引擎模块（Poxiao.WorkFlow）。提供流程设计、流程发起、审批、抄送、委托、监控、催办、加签、子流程、定时器、超时与变更复活等完整 BPM 能力，并内置「请假申请 / 销售订单」两个示例业务表单。引擎数据基于 SqlSugar 持久化，节点/经办/记录三层模型支撑或签、会签、条件分支、依次审批等审批方式。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Poxiao.WorkFlow/` | 实现层：服务（Controller）、Manager、Repository、内置流程表单 (see `Poxiao.WorkFlow/AGENTS.md`) |
| `Poxiao.WorkFlow.Entitys/` | 实体、DTO、枚举、Mapster Mapper 与流程节点属性模型 (see `Poxiao.WorkFlow.Entitys/AGENTS.md`) |
| `Poxiao.WorkFlow.Interfaces/` | 对外接口契约：Manager / Repository / Service (see `Poxiao.WorkFlow.Interfaces/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 三个项目按 .NET 模块化分层，依赖关系：`Poxiao.WorkFlow` → `Poxiao.WorkFlow.Interfaces` → `Poxiao.WorkFlow.Entitys`。新增能力时先在 Interfaces 定义契约，再在 Entitys 补 DTO/实体，最后在 Poxiao.WorkFlow 实现。
- 流程实体均继承 `CLDEntityBase`，遗留字段用 `[SugarColumn(ColumnName = "F_XXX")]` 重写；新表请遵循 `.cursorrules`。
- 状态、节点类型、经办类型集中在 `Poxiao.WorkFlow.Entitys/Enum`，禁止在业务代码内硬编码数字状态。

### Common patterns
- Controller 层 `*Service.cs` 通过 `IDynamicApiController` + `ApiDescriptionSettings` 自动注册路由 `api/workflow/*`。
- 审批主流程由 `FlowTaskManager` 编排：`Save / Submit / Audit / Reject / Recall / Revoke / Cancel / Transfer / Press / Change / Suspend / Restore`。
- 节点属性序列化为 JSON 存于 `FLOW_TASK.F_FLOWTEMPLATEJSON` 与 `FLOW_TEMPLATE` 表。

## Dependencies
### Internal
- `framework/Poxiao/*`、`api/src/modularity/system`、`api/src/modularity/visualdev`、`api/src/modularity/message`
### External
- SqlSugar、Mapster、Furion(DynamicApiController/FriendlyException)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
