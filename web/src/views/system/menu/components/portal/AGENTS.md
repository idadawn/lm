<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# portal

## Purpose
门户管理 popup — Web/App portal-page management for the selected subsystem, with portal-config and transfer-between-subsystems modals.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Web/App tabs of portal pages. |
| `Form.vue` | Portal entry edit form. |
| `PortalModal.vue` | Portal configuration designer modal. |
| `TransferModal.vue` | Modal to move a portal entry to a different subsystem/module. |

## For AI Agents

### Working in this directory
- `PortalModal` may contain heavy designer code — keep lazy-mount via `destroyOnClose`.
- Transfer flow must call API after confirming target module; do not optimistically update the local list.
