<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# SmsTemplate

## Purpose
SMS模板（短信模板）DTOs for the system module — used by the controller/service stack to create, update, list, query and run send-tests on `base_sms_template` entities. Supports multi-provider SMS (腾讯云、阿里云) configuration via `company` field.

## Key Files
| File | Description |
|------|-------------|
| `SmsTemplateCrInput.cs` | 创建/编辑请求 — provider选择、`fullName`、`enCode`、`signContent`、第三方`templateId`、`endpoint`、`region`、`enabledMark`. |
| `SmsTemplateUpInput.cs` | 启停切换 update input. |
| `SmsTemplateListQueryInput.cs` | List query filters (keyword/enabled). |
| `SmsTemplateListOutput.cs` | List rows for grid display. |
| `SmsTemplateInfoOutput.cs` | Detail view including `appId`, `signContent`, full provider config. |
| `SmsTemplateSendTestInput.cs` | Inherits CrInput, adds `phoneNumbers` + `parameters` Dictionary for live send-test. |

## For AI Agents

### Working in this directory
- Property names are camelCase to match the frontend JSON contract; do not switch to PascalCase.
- All classes are decorated with `[SuppressSniffer]` (from `Poxiao.DependencyInjection`) to skip auto-DI scanning — keep the attribute on new DTOs.
- Don't add audit fields here (`F_CREATORTIME` etc.); these are pure transport objects, not entities.
- New provider fields belong in `SmsTemplateCrInput`/`SmsTemplateInfoOutput` mirror pair; keep them in sync.

### Common patterns
- `*CrInput` (create/edit), `*UpInput` (partial), `*ListOutput`/`*InfoOutput` (output split by view), `*ListQueryInput` (search filters), `*SendTestInput` reusing CrInput as base.
- Chinese XML doc comments throughout describing the underlying `base_sms_template` columns.

## Dependencies
### Internal
- `Poxiao.DependencyInjection` — `[SuppressSniffer]` attribute.
- Sibling `SysConfig` DTOs share SMS provider concepts (Ali/Tencent keys).
### External
- None (POCOs only).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
