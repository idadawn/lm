<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Source for the `Basic` primitives. Each file is a small standalone Vue component; `BasicHelp` is the most feature-rich, wrapping an Ant Tooltip with optional indexed list rendering.

## Key Files
| File | Description |
|------|-------------|
| `BasicArrow.vue` | Rotatable arrow icon for expand/collapse indicators. |
| `BasicTitle.vue` | Section title with optional `helpMessage` / `span` decoration. |
| `BasicCaption.vue` | Subdued caption / secondary text. |
| `BasicHelp.vue` | `QuestionCircleFilled` + Tooltip; supports `text`, `list[]`, `showIndex`, `maxWidth`, `color`, `placement`. |

## For AI Agents

### Working in this directory
- Keep these components dependency-free of feature modules — they are leaf-level.
- Use `defineComponent` + `<script lang="tsx">` (as in `BasicHelp`) only when JSX/TSX is required for slot composition.

### Common patterns
- `useDesign(prefixCls)` for class names; `getPopupContainer` for tooltip mounting.

## Dependencies
### Internal
- `/@/hooks/web/useDesign`, `/@/utils` (`getPopupContainer`, `is*`), `/@/utils/helper/tsxHelper` (`getSlot`).
### External
- `ant-design-vue` (`Tooltip`), `@ant-design/icons-vue` (`QuestionCircleFilled`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
