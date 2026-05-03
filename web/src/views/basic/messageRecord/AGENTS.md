<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# messageRecord

## Purpose
消息中心列表 — paged list of system / 流程 / 公告 / 日程 messages for the current user, with read tracking, batch delete, and per-row routing into the correct viewer (workflow form, schedule detail, or notice detail).

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Tabs (全部/系统/流程/公告/日程) + keyword search + `BasicTable` over `getMessageList`; `handleView` calls `readInfo`, opens `/@/views/system/notice/Detail.vue` for notices, `ScheduleDetail` for schedules, or routes to `/workFlow/entrust` / `/workFlowDetail?config=...` for flows (with base64-encrypted payload) |

## For AI Agents

### Working in this directory
- Tab `activeKey` is also the message `type` (`'1'` 公告, `'2'` 流程, `'3'` 系统, `'4'` 日程) and `'0'` means "all" → empty filter. Keep this mapping when extending.
- Workflow detail URLs encrypt the body via `encryptByBase64` then `encodeURIComponent`. Both `/@/views/system/notice/Detail.vue` and `/@/components/VisualPortal/Portal/HSchedule/Detail.vue` are imported directly — do not lazy-load without verifying registration.
- `readInfo` flips `record.isRead` locally — also mutate the store/badge counter elsewhere if you add new view paths.

### Common patterns
- `defineOptions({ name: 'messageRecord' })` so KeepAlive cache is keyed by name.
- `getSearchInfo` computed merges keyword + type as the table search payload.

## Dependencies
### Internal
- `/@/api/system/message`, `/@/components/{Form,Table,Modal}`, `/@/views/system/notice/Detail.vue`, `/@/components/VisualPortal/Portal/HSchedule/Detail.vue`, `/@/utils/cipher`
### External
- `ant-design-vue` (`a-tabs`, `a-tag`), `vue-router`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
