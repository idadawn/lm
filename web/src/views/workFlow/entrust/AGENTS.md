<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# entrust

## Purpose
流程委托 — manage 委托发起 (entrust outgoing) and 委托接收 flows. Search + tabs + status tags identical to other flow lists.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Tabs (委托发起 / 委托接收) sharing search filters and approval status tags. |
| `Form.vue` | Edit form for an entrust rule (delegate, scope, validity). |
| `AddModal.vue` | Picker shown when launching a new entrusted flow. |

## For AI Agents

### Working in this directory
- 状态 tags must match the workflow conventions: 1=等待审核 ... 6=已被挂起.
- Tab switching uses `onTabClick`; the inactive tab table is destroyed (`destroyInactiveTabPane`) — keep that to avoid stale data.

## Dependencies
### Internal
- `/@/api/workFlow/entrust`, `/@/components/Popup`
