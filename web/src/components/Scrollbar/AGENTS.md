<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Scrollbar

## Purpose
Custom scrollbar component ported from element-ui. Wraps native scroll with thin overlay tracks (`bar.ts`) so the layout doesn't reserve native scrollbar gutters. The default mode is `native: false` (driven by `componentSetting.scrollbar`).

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Re-exports `Scrollbar` SFC and `ScrollbarType` (note the leading "copy from element-ui" comment). |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Implementation: `Scrollbar.vue`, `bar.ts`, `util.ts`, `types.d.ts` (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Default `native` is sourced from `/@/settings/componentSetting`; do not hardcode in SFC.
- `tag` prop lets consumers swap the scroll-content root element (default `div`).

### Common patterns
- Slot-only API; no v-model. Consumers pass content as default slot.

## Dependencies
### Internal
- `/@/utils/event` (`addResizeListener`, `removeResizeListener`), `/@/settings/componentSetting`.
### External
- `vue` only (no third-party scroll lib).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
