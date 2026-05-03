<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# systemData

## Purpose
"系统数据" submodule — config for data sources, REST data interfaces (with OAuth credentials), data models, dictionary, and common-fields. Enables online-dev forms / reports to bind to arbitrary backends.

## Key Files
| File | Description |
|------|-------------|
| `dataInterface.ts` | Data interface (HTTP API definition) CRUD + selector + test-call endpoints. Path `/api/system/DataInterface`. |
| `dataSource.ts` | Database connection config CRUD. |
| `dataModel.ts` | Online data-model definitions (table schemas / DTOs). |
| `dictionary.ts` | 字典 CRUD + tree query. |
| `interfaceOauth.ts` | OAuth credentials attached to data interfaces. |
| `commonFields.ts` | Reusable field templates injected into forms. |

## For AI Agents

### Working in this directory
- Dictionary tree is heavily consumed by online-dev forms; keep `getDictionaryTree` performant — UI re-fetches per form open.
- `dataInterface` test-call endpoint should not run against real backends without explicit user trigger.
- OAuth credentials may be surfaced in the UI; never log raw secrets.

### Common patterns
- Standard `getXxxList`, `createXxx`, `updateXxx`, `getInfo`, `delXxx` per file.

## Dependencies
### Internal
- `/@/utils/http/axios`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
