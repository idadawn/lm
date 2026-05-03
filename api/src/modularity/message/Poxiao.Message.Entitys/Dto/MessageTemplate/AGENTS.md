<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MessageTemplate

## Purpose
消息模板（`MessageTemplateEntity`）的列表与查询 DTO。模板可同时关联短信变量（`SmsFieldModel`）与通用模板参数（`TemplateParamModel`）。

## Key Files
| File | Description |
|------|-------------|
| `MessageTemplateQuery.cs` | 模板查询参数（messageSource / templateType / enabledMark / Keyword） |
| `MessageTemplateListOutput.cs` | 列表输出，包含 `smsFieldList` / `templateParamList` 两个明细集合 |

## For AI Agents

### Working in this directory
- 模板列表行内嵌 SMS 与参数列表，避免前端再发请求；后端通过 `WhereIF` + 子查询/Include 一次性带出。
- `MessageTypeModel` / `SendTemplateModel` 在 `Model/MessageTemplate/` 中，不要在 DTO 里重复定义。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
