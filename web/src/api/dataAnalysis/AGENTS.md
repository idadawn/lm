<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# dataAnalysis

## Purpose
"一键分析" / FastGPT integration — sends user prompts to a configured FastGPT chat-completion endpoint and returns the assistant payload. Used by the data-analysis quick-action button on KPI / dashboard views.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `completions(params)` — posts to `${fastgpt}/chat/completions` with chatId (guid), variables (uid, name='kpi'), and the user's `prompt` as a single user-role message. Sets `Authorization: Bearer ...` and `withToken: false` to bypass project token. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `typing/` | Result type stubs (currently minimal) (see `typing/AGENTS.md`). |

## For AI Agents

### Working in this directory
- The FastGPT host + bearer token are hard-coded; consider externalizing to env (`VITE_FASTGPT_*`) before production. Do not commit real secrets.
- `withToken: false` is critical — adding the project Authorization header would break the FastGPT auth.
- For richer NLQ flows (reasoning + SQL), use `/@/api/nlqAgent.ts` instead.

### Common patterns
- Single-prompt → single-response; `stream: false`.

## Dependencies
### Internal
- `/@/utils/helper/toolHelper` (`guid`), `/@/utils/http/axios`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
