<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# basic

## Purpose
Foundational APIs used at app boot: OAuth (login / logout / current-user / lock-screen / SSO ticket / socials login), common utility endpoints, and chart data wrappers used by the home dashboard.

## Key Files
| File | Description |
|------|-------------|
| `user.ts` | OAuth: `loginApi`, `getUserInfo`, `doLogout`, `unlock`, `getConfig(account)`, `updatePasswordMessage`, `getLoginConfig`, `getTicket` / `getTicketStatus`, `socialsLogin`. Uses `/api/oauth/*` prefix. |
| `charts.ts` | Mock-server-backed chart data: `getChartsDataList`, `getHomeChartsDataList`, `getAnalysisChartsData`, `getChartsFormatData` (typed via `API.GetChartsParams`). |
| `common.ts` | Misc shared helpers (region/dict/file lookups). |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `model/` | TS interfaces — `LoginParams`, `BackMenu`, `GetUserInfoModel` etc. (see `model/AGENTS.md`). |

## For AI Agents

### Working in this directory
- `loginApi` POSTs as `application/x-www-form-urlencoded` (`ContentTypeEnum.FORM_URLENCODED`) — do not change to JSON without backend coordination.
- Logout is a `GET` (not POST) — matches backend.
- The chart endpoints intentionally point at `VITE_MOCK_SERVER`; real prod chart data lives in `extend/` and `lab/dashboard.ts`.

### Common patterns
- Functions named by HTTP intent (`loginApi`, `getUserInfo`, `doLogout`).

## Dependencies
### Internal
- `/@/utils/http/axios`, `/@/enums/httpEnum`, `./model/userModel`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
