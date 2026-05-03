<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# HEmail

## Purpose
Latest-emails widget. Fetches the user's recent emails via `getEmailList` and renders a clickable list, each row being a `<router-link>` to `/emailDetail?id=...`. Times are formatted via `toDateText` from project utils.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Fetches list `onMounted`, renders rows with `fullName` + `creatorTime`, falls back to the shared `portal-nodata.png` empty state. |

## For AI Agents

### Working in this directory
- The route target `/emailDetail?id=` is hardcoded; if the route schema changes, update both here and the email module.
- Time formatting is `toDateText(creatorTime)` from `/@/utils/jnpf` — use it consistently for relative-time display.
- Empty state must reuse `assets/images/portal-nodata.png` for visual consistency across portal widgets.

### Common patterns
- Direct `getEmailList` call in `onMounted`; no abstraction layer (keep it simple — emails widget is read-only).

## Dependencies
### Internal
- `/@/api/onlineDev/portal` (`getEmailList`), `/@/utils/jnpf` (`toDateText`), `../CardHeader`

### External
- `vue`, `vue-router` (for `<router-link>`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
