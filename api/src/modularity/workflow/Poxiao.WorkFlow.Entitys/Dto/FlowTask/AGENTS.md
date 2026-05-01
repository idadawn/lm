<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# FlowTask

## Purpose
流程任务（FLOW_TASK）创建 / 更新 / 详情 DTO，被 `FlowTaskService` 控制器使用。承载发起表单数据、紧急程度、提交/保存状态等。

## Key Files
| File | Description |
|------|-------------|
| `FlowTaskCrInput.cs` | 创建/保存任务入参：flowId、id、formData(object)、status(0 保存 /1 提交)、flowUrgent；继承 `FlowTaskOtherModel` 复用候选/委托/抄送字段 |
| `FlowTaskUpInput.cs` | 更新任务入参（草稿编辑） |
| `FlowTaskInfoOutput.cs` | 任务详情输出 |

## For AI Agents

### Working in this directory
- `formData` 类型为 `object`，由前端按表单设计动态序列化，业务侧使用 Newtonsoft / System.Text.Json 解析。
- `FlowTaskOtherModel` 来源于 `Poxiao.Infrastructure.Models.WorkFlow`，包含 candidateList / delegateUserList / errorRuleUserList 等公共字段；新增公共字段优先放到该基类。

### Common patterns
- `status = 0` 表示草稿、`status = 1` 表示提交，FlowTaskService 据此分发到 `Save` 或 `Submit`。

## Dependencies
### Internal
- `Poxiao.Infrastructure.Models.WorkFlow`（`FlowTaskOtherModel`）、`DependencyInjection`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
