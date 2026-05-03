<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# PageSize

## Purpose
"页面设置" modal for the print template designer. Lets the user pick paper preset (A3/A4/A5/B4/B5/Custom), orientation (横向/纵向), and four-side margins in millimetres. Result is emitted to the parent `PrintDesign/index.vue` which adjusts the TinyMCE canvas dimensions.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `BasicModal`-wrapped form: 纸张设置 select, width/height inputs (enabled only for type `6` = custom), 方向 radio group, 页边距 (上/下/左/右 mm). |

## For AI Agents

### Working in this directory
- Width/height fields are read-only when `dataForm.type !== '6'`; preset values come from a `sizeMap` lookup table — keep IDs `1`-`6` stable.
- Modal width is fixed at 400px; emits `change` with the merged `dataForm` payload on submit.

### Common patterns
- Uses `useModalInner` for register/close/loading control.
- Chinese-only labels; wrap user-facing strings via the same convention if expanded.

## Dependencies
### Internal
- `/@/components/Modal` (`BasicModal`, `useModalInner`), `/@/hooks/web/useMessage`.
### External
- `ant-design-vue` (`a-form`, `a-input-number`, `a-radio-group`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
