<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# dataSource

## Purpose
数据源 (database connection) management — registers external DB connections (host/port/dbType) consumed by `dataModel`, `dataInterface`, authorize forms, and other systemData pages.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Table of connections with create/edit/test actions. |
| `Form.vue` | Connection form (driver, host, port, credentials, schema). |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 内置数据源 picker (`sourceAdd.vue`) (see `components/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Connection records are referenced by id throughout the codebase — never rename `dbType`, `host`, `port` fields without updating `getDataSourceSelector` consumers.
- Sensitive credential values must not be logged or echoed in preview popups.

## Dependencies
### Internal
- `/@/api/systemData/dataSource`, `/@/components/Table`, `/@/components/Modal`
