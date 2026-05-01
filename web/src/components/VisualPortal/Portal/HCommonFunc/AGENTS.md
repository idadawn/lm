<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# HCommonFunc

## Purpose
"Common functions" quick-link grid (常用功能). Renders an icon-grid of shortcuts where each item is a `web-link`. Supports configurable `rowNumber`, `showBorder`, vertical (`styleType==1`) vs horizontal (`styleType==2`) item layout, and label font controls.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Iterates `list` (from `useCommon`), each entry is a `web-link` styled via `activeData.option.labelFontSize/Weight/Color`, icon color from `item.iconBgColor`. Width is `100 / rowNumber + '%'` for grid behavior. |

## For AI Agents

### Working in this directory
- Items are configured in the designer via `RCommonFunModal.vue`; the field shape `{ icon, iconBgColor, fullName, linkType, urlAddress, linkTarget, type, propertyJson }` must stay aligned.
- `styleType==2` flips to horizontal item layout via `item-horizontal-box` class — keep both cases when restyling.

### Common patterns
- Single template; no JS logic beyond `useCommon` destructure.

## Dependencies
### Internal
- `../../Design/hooks/useCommon`, `../CardHeader`, `../Link`

### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
