<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# basic

## Purpose
Foundational framework pages that every authenticated session touches: login, session-timeout re-login, screen lock, home/dashboard portal, user profile, runtime error log, generic exception (404/etc.), iframe host, internal redirect bridge, and message records. These are not domain features — they back the layout shell.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `error-log/` | Runtime client-side error log table fed by `errorLogStore` (see `error-log/AGENTS.md`) |
| `exception/` | Generic 404 / no-permission page (see `exception/AGENTS.md`) |
| `home/` | 首页/Dashboard — visual portal layout host with charts and settings drawer (see `home/AGENTS.md`) |
| `iframe/` | iframe page host computing height from layout headers (see `iframe/AGENTS.md`) |
| `lock/` | Lock-screen with avatar + password unlock (see `lock/AGENTS.md`) |
| `login/` | 登录页 — password login, captcha, SSO, social, session-timeout re-login (see `login/AGENTS.md`) |
| `messageRecord/` | 消息中心列表 (公告/流程/系统/日程) with read tracking (see `messageRecord/AGENTS.md`) |
| `profile/` | 个人中心 — info, password, organize/position/subordinate, justAuth bind, log (see `profile/AGENTS.md`) |
| `redirect/` | Programmatic re-route helper used to refresh KeepAlive routes (see `redirect/AGENTS.md`) |

## For AI Agents

### Working in this directory
- These pages tie directly to `web/src/store/modules/{user,lock,permission,errorLog,app}` and `web/src/layouts/` — changes here often have layout-wide impact.
- Routes are declared in `web/src/router/routes/basic.ts` (or similar). Verify the route path before renaming a folder.
- Strings are i18n via `useI18n()` for stock framework messages and inlined Chinese for product-specific copy.

### Common patterns
- `useDesign('xxx-page')` to create scoped CSS prefix names.
- Drawer/Modal helpers (`useDrawer`, `useModal`, `usePopup`) from `/@/components/*`.
- Login flows inject `JNPF_TICKET` into SSO redirect URLs and poll `getTicketStatus`.

## Dependencies
### Internal
- `/@/api/basic/user`, `/@/api/system/message`, `/@/api/permission/userSetting`, `/@/store/modules/*`, `/@/components/{Form,Table,Modal,Drawer}`, `/@/layouts/default/content/useContentViewHeight`
### External
- `ant-design-vue`, `@vueuse/core`, `@ant-design/icons-vue`, `vue-router`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
