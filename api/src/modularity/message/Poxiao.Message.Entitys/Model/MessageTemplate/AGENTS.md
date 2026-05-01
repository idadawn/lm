<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MessageTemplate

## Purpose
模板与发送配置的内部 Model，用于 `MessageManager` 与控制器之间的传递（避免前端 DTO 反向污染服务层）。

## Key Files
| File | Description |
|------|-------------|
| `MessageTypeModel.cs` | 消息类型（站内/邮件/短信/微信/钉钉/企微/Webhook）描述 |
| `SendTemplateModel.cs` | 发送配置-模板-账号汇聚视图（id/sendConfigId/templateId/accountConfigId 等） |
| `SmsFieldModel.cs` | 短信变量（占位符 → 取值字段） |
| `TemplateParamModel.cs` | 通用模板参数（含默认值） |

## For AI Agents

### Working in this directory
- 这些 Model 同时作为 DTO 嵌套结构使用（见 `Dto/MessageTemplate/MessageTemplateListOutput.cs`），更改字段名会影响前端。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
