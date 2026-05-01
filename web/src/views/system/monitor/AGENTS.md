<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# monitor

## Purpose
服务器监控 page — displays CPU, memory and JVM/.NET runtime stats from the API host, with auto-refreshing progress widgets.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Server info panel + CPU/memory/disk progress charts. |

## For AI Agents

### Working in this directory
- Read-only page; render values from a single `data` reactive object (system, cpu, mem, ...).
- `customColors(percentage)` thresholds drive color severity — keep semantically aligned with backend warning levels.

## Dependencies
### Internal
- API call(s) to `/@/api/system/monitor`
