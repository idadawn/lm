<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# login

## Purpose
Login experience: split left-panel branding + right-panel form. Supports password login with optional captcha, third-party social logins via JNPF ticket flow, SSO via iframe poller, multi-tenant social account picker, and a session-timeout re-login overlay.

## Key Files
| File | Description |
|------|-------------|
| `Login.vue` | Page entry — left panel (logo, slogan "让世界更节能 让生活更美好"), right panel hosts `LoginForm`, copyright from `appStore.getSysConfigInfo` |
| `LoginForm.vue` | Main form — password + captcha (`/api/oauth/ImageCode/{len}/{ts}`), Enter-to-submit, social login window, SSO modal, tenant picker, captures `prevLogin` notification with last login info |
| `LoginFormTitle.vue` | (Currently unused) form title fragment |
| `SessionTimeoutLogin.vue` | Overlay version of `Login` rendered on top of the SPA when token expired; reloads page if user identity changes |
| `sso-redirect.vue` | `/sso-redirect` handler — reads `?token` and `?redirect` from query, calls `userStore.updateToken`, `router.replace` |
| `useLogin.ts` | `useLoginState`, `useFormValid`, `useFormRules` (account/password/code/sms/mobile rules + register / reset-password branches), `LoginStateEnum` |
| `index.less` | Shared layout / animation styles for the login chrome |

## For AI Agents

### Working in this directory
- Passwords always hashed with `encryptByMd5` before calling `userStore.login` — preserve this on every change.
- SSO/social: `getTicket()` issues a `JNPF_TICKET`; the form polls `getTicketStatus` on a 1s interval — clear with `clearTimer()` to avoid leaks.
- Captcha is requested only when `getConfig(account)` returns `enableVerificationCode === true`.
- The brand strings ("下一代实验室智能分析平台" etc.) are product copy — coordinate translations before editing.

### Common patterns
- State held in a single `reactive<State>` object, deconstructed with `toRefs` for template binding.
- Modal helpers: `BasicModal` + `useModal` for `jnpf-sso-modal` and `jnpf-tenant-social-modal`.

## Dependencies
### Internal
- `/@/api/basic/user` (`getLoginConfig`, `getTicket`, `getTicketStatus`, `socialsLogin`, `getConfig`), `/@/store/modules/{user,app,permission}`, `/@/components/{Application,Modal}`, `/@/utils/{cache,cipher,dateUtil}`, `/@/enums/pageEnum`
### External
- `ant-design-vue`, `@vueuse/core` (`onKeyStroke`), `vue-router`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
