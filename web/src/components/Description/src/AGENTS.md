<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
TSX implementation of the schema-driven `Description` and the `useDescription` composable that lets parents call `setDescProps` imperatively.

## Key Files
| File | Description |
|------|-------------|
| `Description.vue` | TSX component. Props: `useCollapse`, `title`, `size`, `bordered`, `column`, `collapseOptions`, `schema`, `data`. Emits `register` (passes `{ setDescProps }`). Iterates `schema` → `Descriptions.Item` with `lodash.get(data, field)` and `render(value, data)`. |
| `useDescription.ts` | Composable returning `[register, { setDescProps }]`; guards re-registration in prod via `isProdMode`. |
| `typing.ts` | `DescItem`, `DescInstance`, `DescriptionProps`, `UseDescReturnType`. |

## For AI Agents

### Working in this directory
- `register` callback delivers the `DescInstance` (currently just `setDescProps`); add new methods to both the instance and `useDescription`'s `methods` proxy.
- When wrapping with collapse, `getCollapseOptions` defaults `canExpand: false`; pass `collapseOptions={canExpand:true}` to allow folding.

### Common patterns
- `renderItem()` filters out items where `show(data) === false`; null items are dropped after map.
- `labelMinWidth` overrides the label cell with a styled `<div>` to prevent line-break.

## Dependencies
### Internal
- `/@/components/Container` (CollapseContainer + CollapseContainerOptions), `/@/hooks/web/useDesign`, `/@/hooks/core/useAttrs`, `/@/utils/env` (isProdMode), `/@/utils/helper/tsxHelper` (getSlot).
### External
- `ant-design-vue` (Descriptions), `lodash-es`, `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
