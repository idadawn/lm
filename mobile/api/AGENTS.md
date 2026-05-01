<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# api

## Purpose
Thin API client modules that wrap `utils/http.js` for the backend's REST endpoints. Each function returns a Promise resolving to the unified `{ code, data, msg }` envelope. The mobile app reuses Web-side endpoints unchanged — these wrappers exist mainly to centralize URLs and to hide form-encoding / multipart-upload quirks specific to `uni.request`.

## Key Files
| File | Description |
|------|-------------|
| `user.js` | OAuth + permission endpoints: `loginApi` (form-urlencoded with `origin=app`), `getUserInfoApi`, `doLogoutApi`, `getConfigApi`/`getLoginConfigApi` for captcha/login policy, `getUserSettingInfoApi`/`updateUserInfoApi` for profile, `updatePasswordApi`, plus avatar helpers (`uploadAvatarFile` uses `uni.uploadFile` with `Poxiao-Origin: app`, then `updateAvatarApi`). |
| `dashboard.js` | Lab dashboard reads: `getMonthlyReport`, `getLaminationTrend`, `getThicknessCorrelation`, `getDailyProduction`. All map to `/api/lab/*` endpoints reused from the Web dashboard. |

## For AI Agents

### Working in this directory
- New API modules must import from `@/utils/http.js` (`request`, `get`, `post`) — never call `uni.request` directly except for streaming or multipart cases that `http.js` doesn't cover (mirror the avatar upload pattern in `user.js`).
- `loginApi` and `getConfigApi` pass `needToken: false` — preserve this when adding pre-login endpoints.
- For form-urlencoded POSTs, set `headers: { 'Content-Type': 'application/x-www-form-urlencoded' }`; `http.js` will serialize the body.
- Endpoint paths follow the .NET backend's PascalCase routing (e.g. `/api/oauth/Login`, `/api/permission/Users/Current/BaseInfo`) — do not lowercase them.
- Group new endpoints by feature domain (`user.js`, `dashboard.js`, `chat.js`, etc.) rather than by HTTP verb.

### Common patterns
- Function names end with `Api` for token-required calls; helper utilities (e.g. `uploadAvatarFile`) drop the suffix.
- Avatar upload reads token from `uni.getStorageSync('lm_app_token')` directly because `uni.uploadFile` doesn't go through the shared interceptor.

## Dependencies
### Internal
- `@/utils/http.js` — request wrapper with token + 401 redirect.
- Backend modules under `api/src/modularity/system/` (oauth, permission) and `api/src/modularity/lab/` (dashboard).

### External
- `uni.request`, `uni.uploadFile`, `uni.getStorageSync`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
