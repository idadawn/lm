<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# dataInterface

## Purpose
数据接口管理页面 — provides CRUD over `/api/system/DataInterface` with category tree on the left and table on the right. Used to define HTTP/SQL data interfaces consumable by forms and dashboards.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | List page with `BasicLeftTree` (接口分类) + `BasicTable`, supports add/import (`.bd`)/export/copy. |
| `Form.vue` | Edit drawer/popup for creating/updating an interface definition. |
| `Preview.vue` | Preview popup that runs the interface and shows response. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | `FieldForm` and `PageExplainModal` sub-modals (see `components/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Page component name: `systemData-dataInterface` (registered via `defineOptions`).
- Use `usePopup` (not `useModal`) for `Form` / `Preview` integration; mirrors sibling pages in `systemData/`.
- API surface lives in `/@/api/systemData/dataInterface`; do not reach into other modules' APIs.

### Common patterns
- `BasicLeftTree` + `BasicTable` two-pane layout via `.page-content-wrapper-{left,center}`.
- `enabledMark` rendered as success/error `a-tag` (启用/禁用) — convention shared across systemData pages.

## Dependencies
### Internal
- `/@/components/Tree`, `/@/components/Table`, `/@/components/Popup`
- `/@/api/systemData/dataInterface`
### External
- `ant-design-vue` (a-tag, a-button)
