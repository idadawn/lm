<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# notice

## Purpose
公告/通知 management — publish, edit and view system notices with three-state status (已发送 / 存草稿 / 已过期).

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Notice list with status tag and table actions. |
| `Form.vue` | Notice editor (title, content, recipients, validity). |
| `Detail.vue` | Read-only detail viewer. |

## For AI Agents

### Working in this directory
- API: `/@/api/system/message` — `getNoticeList`, `delNotice`, `release` (publish action).
- `enabledMark` uses tri-state: 1=success, 0=warning(draft), other=expired; keep `a-tag` mapping intact.

## Dependencies
### Internal
- `/@/api/system/message`, `/@/components/Table`, `/@/components/Popup`
