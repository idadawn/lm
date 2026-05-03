<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# pages

## Purpose
All routed screens for the mobile client. Layout splits into three tabBar tabs (`index`, `chat`, `mine`) plus auxiliary detail pages (`login`, `profile`, `password`, `kg-demo`, `server`). Routes are declared in `mobile/pages.json`; folder/file naming follows the uni-app convention `pages/<name>/<name>.vue`.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `chat/` | NLQ chat tab with KG reasoning chain (see `chat/AGENTS.md`) |
| `index/` | 生产驾驶舱 (production cockpit) home tab (see `index/AGENTS.md`) |
| `kg-demo/` | Offline + live KG reasoning demo (see `kg-demo/AGENTS.md`) |
| `login/` | OAuth login with captcha and `origin=app` flag (see `login/AGENTS.md`) |
| `mine/` | "我的" profile/menu tab (see `mine/AGENTS.md`) |
| `password/` | 修改密码 with old + new + captcha (see `password/AGENTS.md`) |
| `profile/` | 个人资料 with avatar upload (see `profile/AGENTS.md`) |
| `server/` | 服务器配置 page for switching API base URL (see `server/AGENTS.md`) |

## For AI Agents

### Working in this directory
- Adding a new page: create `pages/<name>/<name>.vue`, register it in `pages.json` under `pages` (and `tabBar.list` only if it is a tab), then provide `navigationBarTitleText`.
- Tab pages are reached with `uni.switchTab`; detail pages with `uni.navigateTo`. Logout / token expiry uses `uni.reLaunch('/pages/login/login')`.
- Pages should not import api modules directly when possible — keep API calls in `@/api/*` and consume via `<script setup>` imports.
- 中文 UI strings only; comments and identifiers stay English-friendly.

### Common patterns
- `<script setup>` Composition API for new pages (`login`, `mine`, `profile`, `password`, `server`); legacy Options API still appears in `kg-demo`.
- Skeleton loaders are inline (`index.vue`) rather than a shared component.
- Rate / KPI colors derived from `getRateColor()` style helpers per page.

## Dependencies
### Internal
- `@/api/*` for backend calls.
- `@/utils/*` for HTTP, storage, MD5, SSE, update.
- `@/components/kg-reasoning-chain` for chat + demo.

### External
- `@dcloudio/uni-app` lifecycle hooks (`onShow`, `onLoad`, `onPullDownRefresh`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
