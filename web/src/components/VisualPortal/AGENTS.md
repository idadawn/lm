<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# VisualPortal

## Purpose
Drag-and-drop visual portal (门户) builder and runtime. Lets admins assemble a dashboard from cards, charts, schedules, lists, banners, etc., persists the layout JSON, and renders it on home pages. Mirrors the JNPF "在线开发 / 门户" feature.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Design/` | Designer modal: grid layout editor, toolbox, right-side property panels (see `Design/AGENTS.md`). |
| `Portal/` | Runtime widget components (`HChart`, `HCarousel`, `HDataBoard`, ...) consumed by both designer preview and the actual portal page. |
| `style/` | Shared LESS for both design and runtime sides. |

## For AI Agents

### Working in this directory
- The unique discriminator on every layout node is `jnpfKey` (e.g. `'card'`, `'tab'`, `'barChart'`, `'mapChart'`, `'rankList'`, `'notice'`, `'todo'`, `'iframe'`, `'image'`, `'commonFunc'`). Adding a new widget means: register in `Design/helper/componentMap.ts`, render via `Design/components/Parser.vue`, build a `Portal/Hxxx/` runtime, and add a property panel under `Design/rightComponents/`.
- Layout uses `vue3-grid-layout` for PC and `vuedraggable` (App view).
- Every runtime widget wraps content in `<a-card class="portal-card-box">` with a `CardHeader` slot — keep the visual chrome consistent.

### Common patterns
- `activeData` is the per-node config object passed everywhere; `activeData.option`, `activeData.card`, `activeData.title` are the canonical buckets.
- Data sources resolved through `getDataInterfaceRes` (`/@/api/systemData/dataInterface`) — same convention as form designer.
- ECharts via `useECharts` hook in `/@/hooks/web/useECharts`.

## Dependencies
### Internal
- `/@/api/onlineDev/portal`, `/@/api/systemData/dataInterface`
- `/@/components/{Modal,Form,Container}`, `/@/hooks/web/useECharts`

### External
- `ant-design-vue`, `echarts`, `@fullcalendar/*`, `vuedraggable`, `dayjs`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
