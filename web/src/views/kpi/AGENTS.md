<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# kpi

## Purpose
KPI/指标管理子模块根目录。围绕指标定义、维度管理、价值链建模、状态管理、看板展示与统计分析工具（正态分布、SPC、KPI 对比）。是 LIMS 系统中「检测数据 → 指标度量 → 业务洞察」一线工作台。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `analyticalTools/` | 分析工具集（Tabs：正态分布 / SPC / KPI 对比） (see `analyticalTools/AGENTS.md`) |
| `bulletinBoard/` | 价值链看板：标签切换 + 多个 MindMap 思维导图展示 (see `bulletinBoard/AGENTS.md`) |
| `createModel/` | 价值链/指标模型创建与编辑（含图谱编辑器） (see `createModel/AGENTS.md`) |
| `dataAnalysis/` | 数据分析（曲线分析上传 + AI 一键分析对接 fastgpt） (see `dataAnalysis/AGENTS.md`) |
| `dataSourceConfig/` | 数据源配置占位页 (see `dataSourceConfig/AGENTS.md`) |
| `dimension/` | 公共维度管理（树表 + 维度新建/编辑） (see `dimension/AGENTS.md`) |
| `indicatorDefine/` | 指标定义（含 OrgTree 等共享组件） |
| `indicatorOverview/` | 指标总览页 |
| `matchingRate/` | 匹配率分析 |
| `statusManagement/` | 状态管理 |

## For AI Agents

### Working in this directory
- API 入口：`/@/api/createModel/model`、`/@/api/dimension/model`、`/@/api/dataAnalysis`、`/@/api/basic/charts` 等。注意 `dimension/` 与 `createModel/` 的 components 子目录目前直接引用 `views/basic/home/components/optimalManagement`，跨视图依赖较深。
- 看板/编辑器使用 `/@/components/MindMap` 与 `/@/components/ZEditor`，二次开发请优先扩展组件而不是在视图里重写。
- 命名约定：很多页面 `defineOptions({ name: 'permission-organize' })` 是历史遗留；新增请改为更精确的 `kpi-xxx`。

### Common patterns
- `BasicTable` + `BasicModal`/`BasicPopup` 组合做 CRUD
- 通过 `useBaseStore` 拉字典缓存
- 使用 `lodash-es`（`isEmpty`、`find`、`uniqWith`）+ `ResultEnum`/`GotTypeEnum` 等枚举

## Dependencies
### Internal
- `/@/components/Table`, `/@/components/Modal`, `/@/components/Popup`, `/@/components/Tree`, `/@/components/MindMap`, `/@/components/ZEditor`
### External
- `ant-design-vue`, `echarts`, `mathjs`, `lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
