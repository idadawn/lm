<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# indicatorDefine

## Purpose
KPI 模块"指标定义"页面：维护基础/派生/复合三类指标，支持目录树、关键字/类型/标签筛选、批量上下线、克隆、导入导出。指标点击后可跳转图表分析。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 主页面：左侧 BasicLeftTree 指标目录 + 中间 BasicTable 列表 + 顶部筛选栏；调度新增/编辑/删除/克隆/上下线/导入导出 |
| `Form.vue` | 通用指标表单容器，按 type 切换三类子表单 |
| `FormAtomic.vue` | 基础指标（Basic）表单 |
| `FormDerive.vue` | 派生指标（Derive）表单 |
| `FormRecombination.vue` | 复合指标（Composite）表单 |
| `OrgTree.vue` | 部门/组织选择树 |
| `chartsTree.vue` | 跳转图表分析视图 |
| `ImportModal.vue` / `ExportModal.vue` | 指标导入/导出弹窗 |
| `datamodels.ts` | 指标 schema/枚举常量 |
| `index copy.vue` | 旧版备份，勿引用 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 归因分析、维度贡献、目标新增等子组件 (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 新建表单仅扩展 `Form*.vue`；不要把通用逻辑塞进 `index.vue`。
- API 走 `/@/api/indicators/*`、`/@/api/labelManagement`、`/@/api/chart`。
- 不要修改 `index copy.vue`（历史版本），新功能在 `index.vue`。

### Common patterns
- `BasicTable` + `useTable` + `BasicLeftTree` 三段式布局（`page-content-wrapper-left/center`）。
- `useModal` 注册 Form/DepForm/Import/Export 子弹窗。
- `searchInfo` 响应式对象传给 BasicTable，`searchFun/resetFun` 触发 reload。

## Dependencies
### Internal
- `/@/components/Table`, `/@/components/Modal`, `/@/components/Popup`, `BasicLeftTree`
- `/@/api/indicators`, `/@/api/labelManagement`, `/@/api/chart`
### External
- `ant-design-vue`, `@ant-design/icons-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
