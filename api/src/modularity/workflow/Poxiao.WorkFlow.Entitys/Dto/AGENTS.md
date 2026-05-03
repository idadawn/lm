<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Dto

## Purpose
工作流模块所有 HTTP 入参 / 出参 DTO 集中目录。按业务子域分包：流程设计、模板、待审、已发起、监控、任务、评论、委托、表单以及内置业务表单（请假 / 销售订单）。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `FlowBefore/` | 待我审批 / 我已审批列表查询、详情、审批记录 (see `FlowBefore/AGENTS.md`) |
| `FlowComment/` | 流程评论 CRUD DTO (see `FlowComment/AGENTS.md`) |
| `FlowDelegete/` | 流程委托规则 CRUD DTO（注意目录名拼写为 Delegete） (see `FlowDelegete/AGENTS.md`) |
| `FlowEngine/` | 流程设计 CRUD、导入导出、字段元数据 (see `FlowEngine/AGENTS.md`) |
| `FlowForm/` | 流程表单元数据列表 DTO (see `FlowForm/AGENTS.md`) |
| `FlowLaunch/` | 我发起的列表 / 撤回 DTO (see `FlowLaunch/AGENTS.md`) |
| `FlowMonitor/` | 流程监控列表 / 删除 DTO (see `FlowMonitor/AGENTS.md`) |
| `FlowTask/` | 任务创建 / 更新 / 详情 DTO (see `FlowTask/AGENTS.md`) |
| `FlowTemplate/` | 模板列表、树、JSON 详情、协管、导入 DTO (see `FlowTemplate/AGENTS.md`) |
| `WorkFlowForm/` | 内置业务表单（请假、销售订单）DTO (see `WorkFlowForm/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 命名约定：`*ListQuery`（继承 `PageInputBase`）/ `*ListOutput` / `*InfoOutput` / `*CrInput` / `*UpInput`。
- 字段使用 camelCase（与前端 TypeScript 对齐），全部用可空类型 `?` 表达可选。
- 新增 DTO 必须打 `[SuppressSniffer]`，并放在与功能匹配的子目录。

### Common patterns
- 列表查询统一携带 `flowId / flowCategory / templateId / startTime / endTime / status / flowUrgent` 等通用字段。
- 流程模板与表单 JSON 字段通常命名为 `flowTemplateJson / formData / propertyJson / tableJson / draftJson`。

## Dependencies
### Internal
- `Poxiao.WorkFlow.Entitys/Model/*`、`framework/Poxiao/Infrastructure.Filter`、`DependencyInjection`
### External
- 无（纯 POCO）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
