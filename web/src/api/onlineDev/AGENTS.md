<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# onlineDev

## Purpose
在线开发 (online-dev) workbench API — visual page designer (visualDev), portal management, dashboard atlas, data report builder, big-screen (dataV) wiring, and short-link generator. Targets `/api/visualdev/*`, `/api/system/atlas`, `/api/system/PortalManage`.

## Key Files
| File | Description |
|------|-------------|
| `portal.ts` | Portal CRUD + atlas + dashboard + schedule prefixes; manages portal pages and per-tenant home portals. |
| `visualDev.ts` | Visual designer schema + CRUD for online-built forms/lists. |
| `dataReport.ts` | Data report (printable report) endpoints. |
| `dataV.ts` | Big-screen DataV configuration. |
| `shortLink.ts` | Short link gen + redirect lookup. |

## For AI Agents

### Working in this directory
- Multiple base prefixes per module are normal (portal.ts has Portal/Atlas/Dashboard/Schedule/PortalManage). Keep them as `enum Api` block at top.
- Generated forms produced here run through `views/onlineDev/*` — keep API <-> view contract aligned.

### Common patterns
- Standard CRUD verbs + a few extra actions (copy, preview, publish).

## Dependencies
### Internal
- `/@/utils/http/axios`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
