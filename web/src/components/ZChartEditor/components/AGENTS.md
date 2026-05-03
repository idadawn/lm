<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
ZChartEditor 的三大子区域组件聚合层。将 `editorTree`(指标维度树)、`editorDashboard`(可拖拽画布 + ECharts)、`editorElements`(右侧属性/筛选编辑面板)以 `withInstall` 形式导出,供 `src/index.vue` 装配。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 仅导出 `ZEditorForm` / `ZEditorTree`(`withInstall` 包装,与 ZEditor 命名同名,注意区分) |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `editorDashboard/` | 中央拖拽画布,集成 ECharts 与多 chart/filter 节点 (see `editorDashboard/AGENTS.md`) |
| `editorElements/` | 右侧属性面板与筛选条件配置 (see `editorElements/AGENTS.md`) |
| `editorTree/` | 左侧指标/维度二级 Tab 拖拽源 (see `editorTree/AGENTS.md`) |

## For AI Agents

### Working in this directory
- `index.ts` 当前仅导出 form/tree 两类公共组件;若新增 dashboard 公共出口,需评估是否会与外部 ZEditor 同名冲突。
- 子组件内部 `defineOptions({ name: 'ZEditor*' })` 名称易混淆,新增组件时统一以 `ZChart*` 命名以便 devtools 区分。

### Common patterns
- 三个目录均有 `props.ts` 单独抽离 prop 声明,主文件以 `defineProps(_props)` 引用。

## Dependencies
### Internal
- `../hooks/useEditor`、`/@/store/modules/chart`
### External
- Ant Design Vue,`@ant-design/icons-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
