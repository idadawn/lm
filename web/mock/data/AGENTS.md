<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# data

## Purpose
Canned response payloads consumed by `../routes/*.js` — large pre-baked JSON-shaped JS modules covering login config, dictionary data, dashboard nodes, chart series, and indicator trees. Each export key in these files corresponds to a URL key in `../routes/url.js`.

## Key Files
| File | Description |
|------|-------------|
| `common.js` | ~400KB module — auth/dictionary/system mocks: `getLoginConfig`, `admin`, `Login`, `CurrentUser`, `SysConfig`, `All` (dictionary), `Base`, `getNodes`, `getNodeElements`, `getOptimalNodeElements`, `getWarningNodeElements`, `getChartsDataList`, `getChartsFormatData`, `getMetricSchema`, etc. |
| `chart.js` | ~160KB — chart datasets for analysis/dashboard mocks. |
| `chart2.js` | ~1.2MB — extended chart fixtures (large series). |
| `createModel.js` | Indicator-tree mocks — `getIndicatorTreeList`, `getIndicatorValueChainList` (Cov tree of 净利润 KPIs), `getAllIndicatorList`. Standard envelope `{ code:200, msg, data, extras, timestamp }`. |
| `json.js` | Reference snippets (params/result shape comments) for dashboard, chart, filter endpoints — reference only, not exported. |
| `test.js` | Empty placeholder. |

## For AI Agents

### Working in this directory
- Files use CommonJS (`module.exports = { ... }`); not ES modules. Keep that — Koa server is plain Node.
- Data is **shape-faithful** to the real backend response envelope; new fixtures must match `{ code, msg, data, extras, timestamp }`.
- Chinese strings (`'操作成功'`, KPI names like `'净利润达到去年的10倍'`) are intentional — preserve UTF-8.
- IDs use stringified snowflakes (e.g. `'501601491832799173'`); keep them as strings to avoid JS precision loss.
- `chart2.js` is intentionally large; do not regenerate from scratch — append/edit only.

### Common patterns
- Each export key matches a `../routes/url.js` URL constant of the same name.
- Tree-shaped data uses `children: []`, `parentId`, `hasChildren`, `isLeaf`.

## Dependencies
### Internal
- Consumed by `../routes/common.js`, `../routes/createModel.js`, `../routes/chart.js`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
