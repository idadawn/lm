<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# style

## Purpose
Shared Less styles for `FlowParser` and related modals.

## Key Files
| File | Description |
|------|-------------|
| `index.less` | `.basic-flow-parser` selectors — header title ellipsis, urgent-tag colors, layout. |

## For AI Agents

### Working in this directory
- Imported by `FlowParser.vue` — do not scope styles with `<style scoped>` here; selectors must match the popup root class.
- Urgent levels (`.urgent1`/`.urgent2`/...) match `flowUrgentList` ids; keep numeric mapping intact.
