<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# HRankList

## Purpose
Ranking-list widget (排行榜). Four visual styles: ranking medals (`styleType==1`), badges (`==2`), cup podium (`==3`), large cup with side list (`==4`). All driven by data from `useTable`.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `styleType==1/2` render `<a-table>` with a custom `rank` column (top-3 use `rank{1,2,3}.png` / `badge{1,2,3}.png` images). `==3/4` render the cup podium UI with `cup{1,2,3}.png` and a styled side list. |

## For AI Agents

### Working in this directory
- Top-3 medal/cup images live under `assets/images/rankList/`. New styles must add fallback images consistently and keep the `index < 3` special case.
- All styles share the `useTable` hook for data; columns expected to include a numeric `value`/`amount` and a name field — coordinate with `RTableSet.vue`.
- Do not replace `<img>` with icon font — designs are pixel-art images intentional for the cup/badge feel.

### Common patterns
- `<template v-if="getOption.styleType == N">` cascade.
- Computed `getItemStyle`, `getColumnsStyle`, `getTypeStyle` from the hook.

## Dependencies
### Internal
- `../../Design/hooks/useTable`, `../CardHeader`

### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
