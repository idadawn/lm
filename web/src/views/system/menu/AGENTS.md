<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# menu

## Purpose
系统菜单 / 子系统管理 — top-level page that lists subsystems (`getSystemList`) and opens nested authorization, menu, and portal management drawers. Central hub for permission configuration.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Subsystem table with status (启用/禁用) and links into menu/portal/authorize subviews. |
| `Form.vue` | Subsystem edit form. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | Authorize/menu/portal sub-modules (see `components/AGENTS.md`). |

## For AI Agents

### Working in this directory
- `defineOptions({ name: 'system-menu' })` — used by route caching.
- Cross-cutting: this page chains into `components/menu`, `components/portal`, and the four `*Authorize` drawers; opening order and `moduleId` propagation matters — keep the existing register/openDrawer flow.

## Dependencies
### Internal
- `/@/api/system/system`, `/@/components/Table`, `/@/components/Modal`, `/@/components/Popup`
