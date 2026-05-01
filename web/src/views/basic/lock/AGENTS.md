<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# lock

## Purpose
Full-screen lock overlay. Shows a large clock by default; clicking the unlock label flips to a password panel where the user re-authenticates via `lockStore.unLock(md5(password))`. Provides an escape hatch back to login that clears the lock state.

## Key Files
| File | Description |
|------|-------------|
| `LockPage.vue` | Main lock UI — clock + meridiem + password entry, avatar from `userStore`, MD5 cipher, `goLogin` resets via `userStore.logout(true) + lockStore.resetLockInfo()` |
| `index.vue` | Thin wrapper exporting `LockPage` for the lock route |
| `useNow.ts` | `useNow(immediate)` reactive `{year, month, day, hour, minute, meridiem, week}` ticker used by both lock and login pages |

## For AI Agents

### Working in this directory
- Always hash passwords with `encryptByMd5` (`/@/utils/cipher`) before calling `lockStore.unLock` — the backend expects an MD5-hashed value.
- The avatar URL is built as `apiUrl + userInfo.userName` (NB: not avatar field) — confirm with backend before changing.
- Keep z-index `@lock-page-z-index` (defined in `web/src/design`) above all other panels.

### Common patterns
- `useDesign('lock-page')` for the BEM-style `&__hour`, `&__minute`, `&-entry` Less blocks.
- Responsive font-sizes via media queries against Less screen tokens (`@screen-md`, `@screen-lg`, …).

## Dependencies
### Internal
- `/@/store/modules/lock`, `/@/store/modules/user`, `/@/hooks/setting`, `/@/hooks/web/{useDesign,useI18n,useMessage}`, `/@/utils/cipher`
### External
- `ant-design-vue` (`Input.Password`, `Avatar`), `@ant-design/icons-vue` (`LockOutlined`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
