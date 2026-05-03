<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Button

## Purpose
Enhanced Ant Design Vue button package. Provides a `BasicButton` (alias `AButton`) supporting `preIcon`/`postIcon` slots and shared style overrides, plus two confirm-dialog variants used heavily by toolbars and table action columns.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Exports `Button`, `PopConfirmButton`, `ModelConfirmButton`, and the `ButtonProps` type derived from `buttonProps`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | `.vue` impls + `props.ts` shared prop schema (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Always extend `buttonProps` (in `src/props.ts`) when adding new shared props rather than redeclaring them per variant.
- For destructive ops, prefer `PopConfirmButton`; for multi-step / typed confirms, use `ModelConfirmButton`.

### Common patterns
- `withInstall` barrel + `ExtractPropTypes` for the public `ButtonProps` type.

## Dependencies
### Internal
- `/@/utils` (`withInstall`), `/@/hooks/core/useAttrs`.
### External
- `ant-design-vue` (`Button`, `Popconfirm`, `Modal`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
