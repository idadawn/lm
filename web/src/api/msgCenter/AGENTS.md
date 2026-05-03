<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# msgCenter

## Purpose
消息中心 — covers per-channel account config, message templates with test-send, send config, and message-monitor logs. Backend module: `/api/message/*`.

## Key Files
| File | Description |
|------|-------------|
| `msgTemplate.ts` | Template CRUD + copy + `testSendMail` + `getMsgTypeList(type)` (1=msgType, 2=channel, 3=webhook, 4=source). Path `/api/message/MessageTemplateConfig`. |
| `accountConfig.ts` | Channel account config (mail server, SMS, webhook). |
| `sendConfig.ts` | Send-config records linking template + recipients. |
| `msgMonitor.ts` | Send history / monitor list. |

## For AI Agents

### Working in this directory
- The "type" param on `getMsgTypeList` is a small enum (1-4) baked into backend; document at call site to avoid magic numbers.
- `testMsgTemplate` is `testSendMail` despite the function name — historical; preserve.
- Pair with notification settings exposed by backend `Poxiao.Message` module.

### Common patterns
- `Api.Prefix + '/' + action` URL composition.

## Dependencies
### Internal
- `/@/utils/http/axios`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
