<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Description

## Purpose
Wrapper around `ant-design-vue` `Descriptions` that drives content from a `schema` array (similar to `BasicForm`'s schema model). Optionally embeds the description block inside a `CollapseContainer` so detail pages get a foldable, titled section. Used widely by detail/preview pages in the lab module.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Re-exports `Description`, `useDescription`, and types (`DescItem`, `DescInstance`, `DescriptionProps`, `UseDescReturnType`). |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | TSX component + `useDescription` composable + types (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Schema items support `field`, `label`, `span`, `show(data)`, `render(value, data)`, `labelMinWidth`, `labelStyle`, `contentMinWidth` — extend the type, do not freelance prop names.
- Use `useDescription()` to obtain `[register, { setDescProps }]` and bind via `@register`; mirrors the pattern of `useDrawer`/`useModal` elsewhere in the codebase.

### Common patterns
- Default responsive columns: `{ xxl:4, xl:3, lg:3, md:3, sm:2, xs:1 }`.
- When `useCollapse` is true and `title` is set, the component renders inside `CollapseContainer` with `canExpand: false` by default.

## Dependencies
### Internal
- `/@/components/Container`, `/@/hooks/web/useDesign`, `/@/hooks/core/useAttrs`, `/@/utils/helper/tsxHelper`, `/@/utils/is`.
### External
- `ant-design-vue` (Descriptions), `lodash-es` (get), `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
