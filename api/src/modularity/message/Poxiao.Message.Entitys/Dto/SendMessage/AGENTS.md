<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# SendMessage

## Purpose
发送配置（`MessageSendEntity`）的输出 DTO。一个发送配置聚合若干渠道（站内/邮件/短信/微信/钉钉/企微/Webhook）+ 模板 + 账号关系。

## Key Files
| File | Description |
|------|-------------|
| `SendMessageListOutput.cs` | 列表行；`messageType` 为 `List<MessageTypeModel>`，`templateJson` 透传完整 JSON 包 |
| `SendMessageInfoOutput.cs` | 详情输出（编辑表单使用） |

## For AI Agents

### Working in this directory
- `templateJson` 是 `dynamic/object`，前端渲染时需按渠道类型解析。新增渠道字段时同步 `MessageTypeModel` 枚举与渲染逻辑。
- 新增字段需更新 `SendMessageService` 的 Select/Mapster 映射。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
