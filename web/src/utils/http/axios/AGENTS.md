<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# axios

## Purpose
Concrete axios wrapper used by every `/@/api/**` module. Centralizes URL prefix injection, token header, error message dispatch (i18n), retry policy, request cancellation, response unwrapping, login redirect on 401/403, and date-param formatting. Exports the singleton `defHttp` plus the `createAxios` factory for non-default instances.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Builds the `defHttp` instance: configures `transform` (request hooks, response unwrap, error formatter, login redirect), default `RequestOptions`, and global config (`urlPrefix`, timeout, withCredentials). |
| `Axios.ts` | `VAxios` class — wraps an `AxiosInstance`, applies interceptors, handles upload/get/post/put/delete, integrates `AxiosCanceler`. |
| `axiosTransform.ts` | `AxiosTransform` interface + `CreateAxiosOptions` shape. |
| `axiosCancel.ts` | `AxiosCanceler` storing pending requests in a Map keyed by `method + url + params`; allows global cancel-all (used on logout / route change). |
| `axiosRetry.ts` | `AxiosRetry` strategy used by `transform.retryRequest`. |
| `checkStatus.ts` | HTTP status → i18n message router (401/403/404/405/408/500/501/502/503/504/505), with logout side-effect on 401. |
| `helper.ts` | `formatRequestDate`, `joinTimestamp` for query-string cache busting. |

## For AI Agents

### Working in this directory
- The convention `defHttp.get/post/put/delete({ url, data, params, headers }, options?)` is mandatory throughout `/@/api/**` — do not call `axios` directly.
- `urlPrefix` comes from `useGlobSetting()` (env-driven); avoid hard-coding `/api/...` prefixes if the env should drive them.
- 401 handling triggers `useUserStoreWithOut().logout()` then routes to login — only the response transform should do that, not call sites.
- Retry is opt-in per request via `RequestOptions.retryRequest`; don't blanket-enable.
- `isTransformResponse: false` lets callers receive the raw `Result` (used by import wizard endpoints that need full envelope).

### Common patterns
- Error messages localized via `useI18n` — never hard-code Chinese strings here.
- File downloads bypass the unwrap by passing `isReturnNativeResponse: true`.

## Dependencies
### Internal
- `/@/utils/auth`, `/@/utils/is`, `/@/utils`, `/@/store/modules/user`, `/@/store/modules/errorLog`, `/@/hooks/setting`, `/@/hooks/web/useI18n`, `/@/hooks/web/useMessage`, `/@/enums/httpEnum`.
### External
- `axios`, `qs`, `lodash-es`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
