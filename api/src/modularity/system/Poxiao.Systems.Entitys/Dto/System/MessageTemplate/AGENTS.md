<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MessageTemplate (Dto)

## Purpose
站内信模板 DTO。提供 CRUD、详情、列表查询、选择器。

## Key Files
| File | Description |
|------|-------------|
| `MessageTemplateCrInput.cs` / `MessageTemplateUpInput.cs` | 创建/更新（含模板代码、标题、内容、变量） |
| `MessageTemplateInfoOutput.cs` | 详情 |
| `MessageTemplateListOutput.cs` / `MessageTemplateListQueryInput.cs` | 列表/查询条件 |
| `MessageTemplateSeletorOutput.cs` | 选择器（注意类名拼写 "Seletor"，历史保留） |

## For AI Agents

### Working in this directory
- 类名 `Seletor` 拼写错误，已被前端引用，不要重命名。
- 模板内容支持变量占位 `{$xxx$}`，由 `Message` 模块在发送时替换。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
