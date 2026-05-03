<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# FlowComment

## Purpose
流程评论模块的 CRUD DTO，承载任务下的文本/图片/附件评论。被 `FlowCommentService` 控制器使用。

## Key Files
| File | Description |
|------|-------------|
| `FlowCommentCrInput.cs` | 创建评论入参：taskId / text / image / file |
| `FlowCommentUpInput.cs` | 更新评论入参 |
| `FlowCommentInfoOutput.cs` | 单条评论详情输出 |
| `FlowCommentListOutput.cs` | 评论列表条目（含创建人 / 时间） |
| `FlowCommentListQuery.cs` | 评论列表查询（按 taskId 等过滤） |

## For AI Agents

### Working in this directory
- 新增字段须同步更新 `FlowCommentEntity`（`FLOW_COMMENT` 表）。
- 图片与附件以 JSON 字符串形式存储，前端传入时已经过文件管理器上传得到的引用。

### Common patterns
- 字段全部 camelCase + 可空，`[SuppressSniffer]` 标注。

## Dependencies
### Internal
- `DependencyInjection`（`SuppressSniffer`）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
