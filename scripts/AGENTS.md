<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# scripts

## Purpose
Repo-root utility scripts for three workflows: (1) Windows release/deploy entry points that delegate to `publish/` sub-scripts, (2) Playwright crawlers that produce the screenshot library under `../docs/screenshots/`, and (3) cross-language consistency tooling (markdown→docx export, `reasoning-protocol` drift check between `nlq-agent/`, `web/`, and `mobile/`).

## Key Files
| File | Description |
|------|-------------|
| `build-release.ps1` | Release entry point — forwards `Target/Version/Incremental/BaseVersion/SkipBuild/Deploy/Clean` to `publish/publish.ps1`. Targets: `all|api|worker|web`. |
| `deploy-release.ps1` | Deploy entry point — forwards `Version/Component/Environment/SkipBackup/SkipHealthCheck` to `publish/deploy.ps1`. Environments: `development|test|staging|production`. |
| `check-reasoning-protocol-sync.ps1` | CI guard that SHA-256 pins `nlq-agent/packages/shared-types/src/reasoning-protocol.ts` against `web/src/types/reasoning-protocol.d.ts` and `mobile/types/reasoning-protocol.d.ts` via an `// upstream-sha: <hex>` comment in the first 5 lines. Exits 1 on drift, 2 if upstream is missing. |
| `md_to_docx.py` | Converts the Chinese user manual to DOCX with `Microsoft YaHei` fonts; inlines images from a screenshots directory. Invocation: `python md_to_docx.py <input.md> <output.docx> [screenshots_dir]`. |
| `screenshot.js` | Baseline Playwright crawler — logs in (`admin / lm@2025`), navigates the `lab/*` routes, and writes PNGs into `../docs/screenshots/{基础管理,数据管理,报表分析}/`. |
| `screenshot-v2.js` … `screenshot-v5.js` | Iterative successors that broaden coverage (added modules, scrolling, modal capture). `v5` is the current "main" capture per the docs completion report. |
| `screenshot-modals.js`, `screenshot-modal.js` | Capture pop-up dialogs (新增 / 编辑 / 导入向导). |
| `screenshot-template.js`, `screenshot-template-edit.js` | Excel import-template configuration screens. |
| `screenshot-import.js` | Import-wizard step screenshots (file-dialog steps must be captured manually). |
| `screenshot-missing.js`, `screenshot-missing-v2.js`, `screenshot-more.js`, `screenshot-final.js`, `screenshot-mapping.js`, `screenshot-fix.js`, `fix-05-09.js` | Patch scripts for filling gaps after each iteration; produce only the deltas listed in `../docs/使用说明书-完成报告.md`. |

## For AI Agents

### Working in this directory
- The PowerShell scripts target Windows operators. They expect a `publish/` sibling that is **not** in this repo — do not invent stubs; failures with "发布脚本不存在" are by design when run outside the release host.
- Playwright scripts are hard-coded to the staging deployment `http://47.105.59.151:8928` and the credentials `admin / lm@2025`. Do **not** repoint them to production. If you fork one, copy `screenshot-v5.js` (the latest baseline) rather than `screenshot.js`.
- When you change `nlq-agent/packages/shared-types/src/reasoning-protocol.ts`, you must also update the pinned hash in `web/src/types/reasoning-protocol.d.ts` and `mobile/types/reasoning-protocol.d.ts`. Run `pwsh scripts/check-reasoning-protocol-sync.ps1` from the repo root before committing.
- `md_to_docx.py` resolves images via `Path(screenshots_dir) / img_path.replace('./screenshots/', '').replace('/', '\\')` — the backslash replacement is Windows-only; on Linux pass an already-resolved absolute path.

### Common patterns
- All Playwright scripts share the same skeleton: `chromium.launch({headless:true})`, viewport 1920×1080, `delay(ms)` helper, login → goto → `waitForLoadState('networkidle')` → `page.screenshot({fullPage:false})`. Filenames follow `NN-MM-<功能>-<场景>.png`.
- PowerShell entry scripts use a forward-only param adapter (build a `$publishParams` hashtable and splat with `@publishParams`), so adding a new switch only needs a single `if` block.

## Dependencies
### Internal
- `../docs/screenshots/` — output target for all `screenshot*.js`.
- `../nlq-agent/packages/shared-types/src/reasoning-protocol.ts` — upstream of the sync check.
- `../web/src/types/reasoning-protocol.d.ts`, `../mobile/types/reasoning-protocol.d.ts` — downstream copies.
- `publish/publish.ps1`, `publish/deploy.ps1` — referenced but not present in this repo (operator-side).

### External
- `playwright` (`chromium`) for the JS screenshot crawlers.
- `python-docx` for `md_to_docx.py`.
- `nssm` (Windows service manager) — invoked indirectly through `publish/deploy.ps1` and the sibling `../deploy/scripts/`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
