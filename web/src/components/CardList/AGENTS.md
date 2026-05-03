<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# CardList

## Purpose
Grid-style card list view backed by Ant `List`, with a built-in `BasicForm` search header, pagination, refresh button, and a slider-controlled "items per row" (`grid`) control. Used in places where tabular data is better browsed as cards.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Exports `CardList = withInstall(cardList)`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Implementation + demo data (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Caller supplies `data` via fetcher; component manages pagination state internally.
- Header slot lets pages inject custom toolbar buttons next to the grid-size slider.

### Common patterns
- `withInstall` barrel.

## Dependencies
### Internal
- `/@/components/Form` (`BasicForm`).
### External
- `ant-design-vue` (`List`, `Slider`, `Tooltip`, `Button`), `@ant-design/icons-vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
