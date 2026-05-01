<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Source for the `CardList` component. The `.vue` file implements grid + pagination + slider; `data.ts` provides fallback / demo records used during development and stories.

## Key Files
| File | Description |
|------|-------------|
| `CardList.vue` | Template using Ant `List` with responsive `:grid` (xs/sm/md/lg/xl/xxl) and `:pagination`; embeds `BasicForm` for search; toolbar with `Tooltip` + `Slider` + refresh `Button`. |
| `data.ts` | Static sample dataset used as default `data` source. |

## For AI Agents

### Working in this directory
- Avoid inline styles; use Tailwind utility classes (`p-2`, `bg-white`, `flex justify-end`) consistent with the rest of the codebase.
- The slider's `change` handler updates `grid` reactively; preserve this when refactoring.

### Common patterns
- Composition API `<script lang="ts" setup>` with a `paginationProp` reactive object.

## Dependencies
### Internal
- `/@/components/Form` (`BasicForm`, `useForm`).
### External
- `ant-design-vue` (`List`, `Slider`, `Tooltip`, `Button`), `@ant-design/icons-vue` (`TableOutlined`, `RedoOutlined`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
