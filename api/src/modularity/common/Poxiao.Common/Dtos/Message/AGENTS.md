<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Message

## Purpose
Shared message-send DTOs used by the messaging infrastructure (站内信、邮件、短信、企业微信). `MessageSendModel` is the unified envelope passed into the dispatcher; `MessageSendParam` describes the per-field substitution values used to fill a message template.

## Key Files
| File | Description |
|------|-------------|
| `MessageSendModel.cs` | Envelope: `id`、`toUser` (recipient list)、`paramJson` (template params)、`msgTemplateName`、`messageType`、`templateId`、`accountConfigId`、`sendConfigId`. |
| `MessageSendParam.cs` | Per-template-field value: `field`、`fieldName`、`templateCode`、`templateId`、`templateName`、`templateType`、`value`、`relationField`、`isSubTable`. |

## For AI Agents

### Working in this directory
- Namespace `Poxiao.Infrastructure.Dtos.Message`.
- `messageType` is a string code (e.g. "Email", "SMS", "WeChat") — keep it loose so new channels can be added without enum churn.
- `paramJson` is a typed list, not a raw JSON string, despite the name — preserved for legacy compatibility.
- Add `[SuppressSniffer]` to any new DTO added here.

### Common patterns
- camelCase props with Chinese XML comments matching the upstream entity columns.
- `isSubTable` flags template fields whose values come from a sub-table row instead of the main entity.

## Dependencies
### Internal
- Consumed by message dispatch services (in `Manager/` / `Service/Message/` of the message module).
### External
- None.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
