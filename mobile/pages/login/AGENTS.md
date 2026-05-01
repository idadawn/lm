<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# login

## Purpose
uni-app login page for the LIMS mobile client (检测室数据分析). Collects 账号 / 密码 / optional 验证码, supports "记住密码", and submits credentials to the backend auth endpoint.

## Key Files
| File | Description |
|------|-------------|
| `login.vue` | Login form: account / password / captcha (`needCode`, `codeImgUrl`, `refreshCode`), remember-me toggle, submit button with `loading` state, and `onAccountBlur` handler that probes whether captcha is required. |

## For AI Agents

### Working in this directory
- Captcha is rendered conditionally via `needCode`; flip via the captcha-required check in `onAccountBlur` — don't always show it.
- Strings are Chinese (UI policy); keep both labels and placeholders in Chinese.
- "记住密码" persists credentials locally — make sure any change still encrypts/encodes the stored password (do not store plaintext in `uni.setStorage`).

### Common patterns
- Two-step auth: blur on account → fetch captcha state → optionally show captcha → submit.
- Form state lives under `form: { account, password, code }`; submit button uses `:loading` and `@click` on a `<button>`.

## Dependencies
### Internal
- Auth API client (likely under `mobile/api` or `@/utils/request`), Pinia user store.
### External
- uni-app runtime.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
