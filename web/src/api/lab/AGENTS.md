<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# lab

## Purpose
化验室 (laboratory) — the project's primary domain. API client for raw-data import/CRUD, intermediate-data calculation/judgment, appearance & feature catalogs, formulas, units, products & specs, monthly quality reports, magnetic data, severity levels, dashboard KPI fetch, AI-assist endpoints, and Excel template management.

## Key Files
| File | Description |
|------|-------------|
| `rawData.ts` | 原始数据导入向导 + CRUD: stepwise import session API (`createImportSession`, step1 upload+parse, step2 productSpec, step3 appearanceFeature, step4 review/preview), simple-import, log query, append/overwrite strategy. |
| `dashboard.ts` | `生产驾驶舱` KPIs: total weight, qualified rate, lamination factor avg/trend, warnings, quality distribution. |
| `appearance.ts` / `appearanceCategory.ts` / `appearanceFeature.ts` / `featureLearning.ts` | 外观 / 外观特征 / 特征学习。 |
| `intermediateData.ts` / `intermediateDataColor.ts` / `intermediateDataFormula.ts` / `intermediateDataJudgmentLevel.ts` | 中间数据计算与判定: row/page query, recompute, color rules, formula CRUD, judgment-level config. |
| `metric.ts` | 检测指标定义 CRUD。 |
| `monthlyQualityReport.ts` | 月度质量报表生成 / 导出。 |
| `magneticData.ts` | 磁性数据导入 + 查询。 |
| `product.ts` / `productSpec.ts` / `productSpecPublicAttribute.ts` | 产品 / 规格 / 公共属性。 |
| `publicDimension.ts` | 化验域公共维度。 |
| `reportConfig.ts` | 指标列表配置。 |
| `severityLevel.ts` | 严重等级配置。 |
| `unit.ts` | 单位 CRUD + 分类查询 (used by `/@/utils/lab/unit`). |
| `excelTemplate.ts` | Excel 模板 CRUD (导入/导出向导用). |
| `ai.ts` | 化验域 AI 辅助 (推理 / 智能填充). |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `model/` | Calculation/judgment log + color models (see `model/AGENTS.md`). |
| `types/` | Concrete TS interfaces for rawData/excelTemplate/formula/metric (see `types/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Backend module is `Poxiao.Lab.*` (see `api/src/modularity/lab/`). API prefixes follow `/api/lab/{kebab-case}` (e.g. `/api/lab/raw-data`, `/api/lab/raw-data-import-session`).
- Import wizard uses `isTransformResponse: false, errorMessageMode: 'none'` for some calls because it parses the full envelope itself (e.g. session creation returns string id directly).
- File downloads use `downloadByData` from `/@/utils/file/download`; pass through `responseType: 'blob'` and `isReturnNativeResponse: true`.
- For CLDEntityBase-aware shapes, see `.cursorrules`. Field naming inside payloads mixes camelCase and Pascal — preserve what backend returns.
- The dashboard payload shape is canonical for the 生产驾驶舱 page; avoid drift between this file and `views/lab/monthly-dashboard/*`.

### Common patterns
- One file per backend resource — keep granularity even when small.
- Prefer concrete typed signatures in newer files (rawData.ts, dashboard.ts) over `Promise<any>`.

## Dependencies
### Internal
- `/@/utils/http/axios`, `/@/utils/file/download`, `/@/enums/httpEnum`, `./types/*`, `./model/*`, `./productSpec`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
