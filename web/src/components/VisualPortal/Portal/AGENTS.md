<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Portal

## Purpose
Runtime widgets used by the visual portal. Each subdirectory is one widget kind (matching a `jnpfKey`) — they are rendered both inside the designer (via `Parser.vue`) and on the production portal page. Every widget wraps its content in `<a-card class="portal-card-box">` with a shared `CardHeader`.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `CardHeader/` | Shared title bar with icon, color, font, optional right-link button. |
| `HCarousel/` | Image carousel (`activeData.option.carousel*`). |
| `HChart/` | Generic ECharts widget (bar/line/pie/...) via `useEChart`. |
| `HCommonFunc/` | Quick-link grid of common functions. |
| `HDataBoard/` | KPI tiles with icon + value + unit. |
| `HEmail/` | Latest-email list, links to `/emailDetail?id=`. |
| `HIframe/` | Embedded iframe content. |
| `HImage/` | Single image with overlay text and link. |
| `HMapChart/` | Map ECharts with drill-down breadcrumb. |
| `HNotice/` | Announcements (table or card style). |
| `HRankList/` | Ranking list with cup/badge medal styles. |
| `HSchedule/` | Calendar via `@fullcalendar/vue3` plus form/detail modals. |
| `HTableList/` | Generic table-list driven by data interface. |
| `HText`, `HTimeAxis`, `HTodo`, `HTodoList`, `HVideo`, `Layout`, `Link` | Other widgets (text block, timeline, todo, video, container/grid, web-link wrapper). |

## For AI Agents

### Working in this directory
- Widgets are intentionally thin — they bind `activeData` and delegate to `useCommon` / `useEChart` / `useTable` (in `../Design/hooks`). Resist adding business logic here; extend a hook instead.
- Templates use `getValue` / `list` / `getOption` / `getColumns` returned by the shared hooks; keep variable names stable for slot continuity.
- The empty state asset is `assets/images/portal-nodata.png` — reuse it everywhere.

### Common patterns
- `<a-card class="portal-card-box">` outer chrome + `<CardHeader>` slot.
- `defineProps(['activeData'])` only; no events emitted upward (state changes round-trip via parent state for designer).

## Dependencies
### Internal
- `../Design/hooks/{useCommon,useEChart,useTable}`, `./CardHeader`, `./Link`
- `/@/api/onlineDev/portal`, `/@/utils/jnpf`

### External
- `ant-design-vue`, `echarts`, `@fullcalendar/*`, `dayjs`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
