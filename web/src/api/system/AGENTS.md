<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# system

## Purpose
System-management module APIs — menu, area (regions), system config, advanced query, billing rules, button/column/data/form authorization, cache mgmt, common-words, log, message dispatch, monitor, print-design, schedule task, system info, version. Targets `/api/system/*`.

## Key Files
| File | Description |
|------|-------------|
| `menu.ts` | Menu CRUD + selector + `ModuleBySystem/{systemId}` listing + `/{id}/Action/Export` export. Used by both the menu management page and dynamic backend menu provider. |
| `sysConfig.ts` | System-wide configuration CRUD. |
| `area.ts` | 行政区划 (region) tree. |
| `advancedQuery.ts` | Saved advanced-query definitions. |
| `authorize.ts` / `buttonAuthorize.ts` / `columnAuthorize.ts` / `dataAuthorize.ts` / `formAuthorize.ts` | Per-resource RBAC grants. |
| `billRule.ts` | 单据规则 (auto-numbering rules). |
| `cache.ts` | Cache list / clear. |
| `commonWords.ts` | 常用语 CRUD. |
| `log.ts` | System operation logs. |
| `message.ts` | System bulletin / broadcast messages. |
| `monitor.ts` | Server monitor (CPU/memory/runtime). |
| `printDev.ts` | Print template designer. |
| `system.ts` | Sub-system definitions. |
| `task.ts` | Scheduled tasks (Quartz / Hangfire wrapper). |
| `version.ts` | Version info / changelog endpoint. |

## For AI Agents

### Working in this directory
- Menu paths use `/Selector/${id}/${systemId}` — preserve the dual-segment shape (backend route expects both).
- Authorization endpoints come in pairs (button / column / data / form) — keep them parallel for the generic perm-grant component.
- `exportMenu` is GET-only (no body) — backend streams XML/JSON.

### Common patterns
- Each file declares an inner `enum Api { Prefix = '/api/system/Xxx' }` then exports CRUD functions.

## Dependencies
### Internal
- `/@/utils/http/axios`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
