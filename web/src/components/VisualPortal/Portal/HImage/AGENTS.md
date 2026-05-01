<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# HImage

## Purpose
Single-image portal widget with optional overlay text and link wrapping. Supports `object-fit` (`imageFillStyle`), default text (`textDefaultValue`) styling (font color/size/align/background/weight/style), and `web-link` for click-through.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Renders `<img :src="getValue">` inside `web-link`. Bottom overlay div is configurable. Falls back to the shared empty state when `getValue` is empty. |

## For AI Agents

### Working in this directory
- `getValue` returns the resolved image URL from `useCommon` (may be a static `imageUrl` or an interface result). Do not call APIs directly here.
- The component re-renders when `key` changes — used to force `<img>` reload after URL changes; keep `:key="key"` binding.
- Overlay font controls live on `activeData.option.text*` — mirror these in `RTextSet.vue` if extended.

### Common patterns
- Pure-template component, no methods.

## Dependencies
### Internal
- `../../Design/hooks/useCommon`, `../CardHeader`, `../Link`

### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
