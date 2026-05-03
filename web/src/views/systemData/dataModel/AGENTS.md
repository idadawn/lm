<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# dataModel

## Purpose
数据模型管理 — defines logical data models (虚拟表) over a chosen data source. List page shows expandable rows of model fields and supports `.bdb` import, copy, export, and a 常用字段 drawer.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Table with expandable child-table of fields; entry point for add/edit/preview/import. |
| `Form.vue` | Model form drawer (basics + fields editor). |
| `Preview.vue` | Renders the model output for verification. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 常用字段 reusable-fields drawer (see `components/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Reuses `getDataSourceSelector` from `systemData/dataSource` — keep that cross-module dependency narrow (selector only).
- Expanded rows lazy-load fields via `getDataModelFieldList`; preserve the `record.childTableLoading` flag pattern.

### Common patterns
- `useDrawer` for `常用字段` (drawer-style picker), `usePopup` for full-screen Form/Preview.

## Dependencies
### Internal
- `/@/api/systemData/dataModel`, `/@/api/systemData/dataSource`
- `/@/components/Table`, `/@/components/Drawer`, `/@/components/Popup`
