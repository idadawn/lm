<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# helper

## Purpose
Pure helpers grouped by concern: tree manipulation (list↔tree, find paths, recursive map), theme palette generation (Ant Design color algorithms), JNPF-style tool helpers (guid, deep utilities), and a TSX-only helper for slot rendering.

## Key Files
| File | Description |
|------|-------------|
| `treeHelper.ts` | `listToTree`, `treeToList`, `findNode` / `findPath` / `findPathAll`, `treeMap`, `eachTree` — config-driven (`id`, `pid`, `children` keys). |
| `themes.ts` | Generates color palette steps used by `logics/theme` (lighten/darken/mix). |
| `toolHelper.ts` | `guid()`, deep utilities, JNPF-flavored helpers used by online-dev components. |
| `tsxHelper.tsx` | `getSlot(slots, name, data)` for ergonomic slot-forwarding in TSX components. |

## For AI Agents

### Working in this directory
- `treeHelper` defaults assume `id`/`parentId`/`children` — pass a config when backend uses different keys.
- `findPath` returns the path including the matched node; `findPathAll` returns all matches.
- Themes helpers depend on `/@/utils/color` math; keep them in sync with `logics/theme/util.ts`.

### Common patterns
- Generic-typed (`<T = any>`); pass the row shape at call site for IntelliSense.
- `cloneDeep` (lodash-es) input arrays before mutating during transforms.

## Dependencies
### Internal
- `/@/utils/color`.
### External
- `lodash-es`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
