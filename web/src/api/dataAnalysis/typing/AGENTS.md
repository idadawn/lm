<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# typing

## Purpose
Placeholder for FastGPT / data-analysis response shapes. Currently minimal — will grow once the endpoint contract stabilizes beyond `Promise<any>`.

## Key Files
| File | Description |
|------|-------------|
| `model.d.ts` | Tiny stub for analysis result/event types. |

## For AI Agents

### Working in this directory
- When adding concrete types, prefer ambient `declare namespace API.DataAnalysis` consistent with sibling `chart/typing/`.
- Reflect FastGPT's actual response envelope (`choices[].message.content`, etc.).

### Common patterns
- `.d.ts` only.

## Dependencies
### Internal
- Consumed by `../index.ts`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
