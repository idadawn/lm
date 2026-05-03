<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# interfaceOauth

## Purpose
接口授权 (OAuth/AppKey) management — issue and manage outbound interface authorizations (app credentials, validity period, scope). List, edit, log, and empower clients.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Authorization list with status (启用/禁用) and per-row actions. |
| `Form.vue` | Auth definition form. |
| `Log.vue` | Per-auth call log popup. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | `Empower` (assign interfaces) and `Info` modals (see `components/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Page uses `usePopup` for Form/Log/Empower — keep modal-vs-popup choice consistent.
- `formatToDate` from `/@/utils/dateUtil` formats validity columns; reuse it instead of inline dayjs.

## Dependencies
### Internal
- `/@/api/systemData/interfaceOauth`, `/@/components/Popup`, `/@/components/Modal`
