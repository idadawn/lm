<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# lab

## Purpose
检测室核心业务模块的页面集合，对应后端 `api/src/modularity/lab/`。覆盖外观特性、产品规格、原始/中间/磁性数据、月度报表与驾驶舱、单位/维度/严重等级/Excel 模板等所有 LIMS 维护与查询场景。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `appearance/` | 外观特性定义 + 语义匹配测试 (see `appearance/AGENTS.md`) |
| `appearanceCategory/` | 外观特性大类管理 (see `appearanceCategory/AGENTS.md`) |
| `dashboard/` | 实时生产驾驶舱 (see `dashboard/AGENTS.md`) |
| `intermediateData/` | 中间数据计算结果维护 (see `intermediateData/AGENTS.md`) |
| `intermediateDataFormula/` | 中间数据公式维护 (see `intermediateDataFormula/AGENTS.md`) |
| `intermediateDataJudgmentLevel/` | 中间数据判定等级维护 (see `intermediateDataJudgmentLevel/AGENTS.md`) |
| `magneticData/` | 磁性数据导入与浏览 (see `magneticData/AGENTS.md`) |
| `metric/` | 指标管理（占位，开发中）(see `metric/AGENTS.md`) |
| `monthly-dashboard/` | 月度驾驶舱（kebab-case 路径）(see `monthly-dashboard/AGENTS.md`) |
| `monthlyReport/` | 月度质量统计报表 (see `monthlyReport/AGENTS.md`) |
| `product/` | 产品规格定义 + 扩展属性 (see `product/AGENTS.md`) |
| `publicDimension/` | 公共维度（含版本管理）(see `publicDimension/AGENTS.md`) |
| `rawData/` | 原始数据 + Excel 步骤导入向导 (see `rawData/AGENTS.md`) |
| `reportConfig/` | 报告统计列配置 (see `reportConfig/AGENTS.md`) |
| `severityLevel/` | 特性等级（带 AI 匹配）(see `severityLevel/AGENTS.md`) |
| `template/` | Excel 导入模板管理 (see `template/AGENTS.md`) |
| `unit/` | 单位维度与单位定义 (see `unit/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 所有页面对应 `/@/api/lab/*` 接口；路由名通常 `lab-<feature>`。
- 多数页面采用 `page-content-wrapper` 三段布局或 `p-4 h-full flex flex-col gap-4` Tailwind 卡片布局，新页面应保持其中一种风格一致。
- 中间数据/公式/判定等级是强耦合三件套：编辑公式时需要同步检查判定等级。

### Common patterns
- BasicTable + useModal/usePopup + Form/Detail 组件复用。
- 搜索筛选写在页头工具栏；导入采用 `Step*` 向导（rawData/magneticData）。

## Dependencies
### Internal
- `/@/api/lab/*`, `/@/components/Table`, `/@/components/Modal`, `/@/components/Form`
### External
- `ant-design-vue`, `echarts`, `dayjs`, `vuedraggable`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
