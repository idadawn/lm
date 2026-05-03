<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# HDataBoard

## Purpose
KPI / data-board tiles (数据看板). Each tile is `web-link` showing an icon (with translucent background derived from `iconColor`), a numeric value with custom font, a unit, and a name label.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Renders `list` from `useCommon` as a flex grid (`100 / rowNumber + '%'`). Each item shows `item.num`, `item.unit`, `item.fullName` with separately configurable font/size/weight/color. Icon color is RGB→RGBA via `.replace('rgb','rgba').replace(')',',0.1)')`. |

## For AI Agents

### Working in this directory
- The RGB→RGBA replacement assumes `iconColor` is in `rgb(r,g,b)` form — keep `RDataBoardModal.vue` producing that format.
- `styleType` controls layout but value/unit/name field names (`num`, `unit`, `fullName`) are fixed; the dataset in `dataMap.ts` mirrors them.
- Configurable font groups: `valueFontSize/Weight/Color`, `unitFontSize/Weight/Color`, `labelFontSize/Weight/Color` — preserve the trio.

### Common patterns
- All visual config inlined as `:style` objects.

## Dependencies
### Internal
- `../../Design/hooks/useCommon`, `../CardHeader`, `../Link`

### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
