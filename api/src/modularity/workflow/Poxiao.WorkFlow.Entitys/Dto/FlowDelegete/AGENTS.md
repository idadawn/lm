<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# FlowDelegete

## Purpose
流程委托规则的 CRUD DTO（被 `FlowDelegateService` 使用）。支持「发起委托」与「审批委托」两类（`type`：0 发起 / 1 审批），按指定流程 / 分类 + 时间区间 + 委托人/被委托人记录委托关系。

> 注意：目录名拼写为 `FlowDelegete`，与代码内 `FlowDelegate*` 实体不同；这是历史遗留，不要重命名。

## Key Files
| File | Description |
|------|-------------|
| `FlowDelegeteCrInput.cs` | 创建委托：userId/userName、toUserId/toUserName、flowId/flowName/flowCategory、startTime/endTime、type、description |
| `FlowDelegeteUpInput.cs` | 更新委托规则 |
| `FlowDelegeteInfoOutput.cs` | 委托规则详情输出 |
| `FlowDelegeteListOutput.cs` | 列表条目（含状态、时间区间） |
| `FlowDelegateQuery.cs` | 列表查询过滤（注意此文件用 Delegate 拼写） |

## For AI Agents

### Working in this directory
- 新增字段需要同步 `FlowDelegateEntity`（`FLOW_DELEGATE` 表，列前缀 `F_`）。
- `type` 使用字符串 `"0" / "1"`，保持与历史前端一致。

### Common patterns
- 时间字段使用 `DateTime?`（与 Comment/Engine 用 long? 不同）。

## Dependencies
### Internal
- `DependencyInjection`（`SuppressSniffer`）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
