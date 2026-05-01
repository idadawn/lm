<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# tableDemo

## Purpose
扩展模块下的「表格示例库」根目录，集中演示 `BasicTable` + `useTable` 在不同场景下的能力：通用表格、复杂表头、合并单元格、列锁定、分组、行内编辑、批注、打印、签收、汇总统计、树形等。所有子目录共享同一份后端假数据接口 `/@/api/extend/table`，并复用 `commonForm/` 的项目录入弹窗，作为 LIMS 平台研发人员复制粘贴的模板。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `commonForm/` | 通用项目新建/编辑 BasicModal 表单（其他示例的共享 Form） (see `commonForm/AGENTS.md`) |
| `commonTable/` | 标准 BasicTable + 搜索 + 操作列示例 (see `commonTable/AGENTS.md`) |
| `complexHeader/` | 复杂多级表头示例 (see `complexHeader/AGENTS.md`) |
| `extension/` | 行展开嵌套子表示例 (see `extension/AGENTS.md`) |
| `groupingTable/` | 分组表格（父子行）示例 (see `groupingTable/AGENTS.md`) |
| `lockTable/` | 列固定（fixed left/right）示例 (see `lockTable/AGENTS.md`) |
| `mergeTable/` | 单元格合并 customCell 示例 (see `mergeTable/AGENTS.md`) |
| `postilTable/` | 表格行批注 timeline 示例 (see `postilTable/AGENTS.md`) |
| `printTable/` | 表格打印 print-js 示例 (see `printTable/AGENTS.md`) |
| `redactTable/` | 行内编辑（点击行可编辑）示例 (see `redactTable/AGENTS.md`) |
| `signTable/` | 项目标记（彩色 dot 标签）示例 (see `signTable/AGENTS.md`) |
| `statisticsTable/` | 表尾合计 a-table-summary 示例 (see `statisticsTable/AGENTS.md`) |
| `tableTree/` | 字典树形数据表格示例 (see `tableTree/AGENTS.md`) |
| `treeTable/` | 左侧树 + 右侧表的组合示例 (see `treeTable/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 这些是**示例/Demo**，请勿在 lab/system 等业务模块直接引用 `../extend/...`，应复制对应代码到业务目录后改造。
- 所有示例共用 `commonForm/index.vue` 作为新增/编辑表单与 `/@/api/extend/table` 假接口；改这俩文件会同时影响多个 Demo。
- 命名遵循 `defineOptions({ name: 'extend-tableDemo-<dirName>' })`，新增 Demo 时保持同款 keep-alive 名称。

### Common patterns
- `BasicTable` + `useTable({ api, columns, useSearchForm, formConfig })` 注册器
- 字典数据通过 `useBaseStore().getDictionaryData('IndustryType')` 拉取并缓存
- 操作列由 `TableAction` 渲染，行内显隐通过 `getTableActions(record)` 计算

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
