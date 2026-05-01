<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# docs

## Purpose
End-user documentation for the 检测室数据分析系统 (Laboratory Data Analysis System). Holds the master Chinese user manual and the screenshot library that backs every figure reference inside it. The manual targets non-technical lab operators using the deployed web UI at `http://47.105.59.151:8928`.

## Key Files
| File | Description |
|------|-------------|
| `实验室数据分析系统-使用说明书.md` | 主用户手册（约 15,000 字、51 张图引用，6 个主章节：系统概述 / 登录与首页 / 基础管理 / 数据管理 / 报表与分析 / 常见问题）。 |
| `使用说明书-完成报告.md` | 文档与截图交付状态报告（截图覆盖率、缺失截图清单、对应自动化脚本映射）。 |
| `screenshots/README.md` | 截图目录索引与命名规约说明。 |
| `screenshots/01-系统界面概览.png` | 顶层封面图。 |
| `screenshots/02-登录页面.png` | 登录页（由 `scripts/screenshot.js` 自动生成）。 |
| `screenshots/03-生产驾驶舱.png` | 首页生产驾驶舱（lab dashboard）。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `screenshots/` | PNG screenshot library, organised into `基础管理/`, `数据管理/`, `报表分析/` matching the manual's chapter structure. |

## For AI Agents

### Working in this directory
- The manual references images via relative paths like `./screenshots/基础管理/03-01-...png`. Preserve the **Chinese subdirectory names** and the leading `NN-MM-` ordering — they are load-bearing for the markdown links and the `md_to_docx.py` converter in `../scripts/`.
- Do **not** re-shoot screenshots manually here; regenerate via the Playwright crawlers in `../scripts/screenshot*.js` (login: `admin / lm@2025` against the staging URL embedded in those scripts).
- Four import-wizard screenshots (`04-05` through `04-08`) are flagged as manual-only in `使用说明书-完成报告.md` because they sit behind a native file-picker; leave them alone unless you have human-in-the-loop access.
- When updating the manual, also update the completion report's checklist to keep delivery status truthful.

### Common patterns
- Chapter / section IDs in filenames map 1:1 to manual headings, e.g. `03-XX-基础管理-...`, `04-XX-数据管理-...`, `05-XX-报表分析-...`.
- All prose is Chinese; code blocks and field names follow the backend's `F_XXX` SqlSugar conventions described in `../.cursorrules`.

## Dependencies
### Internal
- `../scripts/screenshot*.js` — Playwright-based generators that populate `screenshots/`.
- `../scripts/md_to_docx.py` — converts the manual to DOCX using `Microsoft YaHei` fonts and inlines images from `screenshots/`.
- Backend module names (`lab/`, `kpi/`, etc.) — manual section titles must stay aligned with `../api/src/modularity/` and `../web/src/views/`.

### External
- Markdown viewers / pandoc-compatible renderers; the docx pipeline depends on `python-docx`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
