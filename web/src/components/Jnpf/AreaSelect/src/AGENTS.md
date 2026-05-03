<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfAreaSelect` 实现层。`AreaSelect.vue` 是主体（`<a-select>` + `<a-modal>` 选择面板），`props.ts` 集中暴露 props 类型供父级 `index.ts` re-export。

## Key Files
| File | Description |
|------|-------------|
| `AreaSelect.vue` | 行政区选择主体：内层维护 `treeData`/`selectedData`/`visible`/`loading`，提供 `handleSubmit`/`handleSelect`/`removeData`/`removeAll`/`onLoadData` 等交互方法 |
| `props.ts` | props 定义（v-model value、placeholder、disabled、onlyLast 等） |

## For AI Agents

### Working in this directory
- 树懒加载：`onLoadData(treeNode)` 应根据 `treeNode.dataRef.id` 调用接口拿子级；切勿在 `onMounted` 一次拉全国数据。
- 已选列表是用户提交前的"暂存区"，仅在 `handleSubmit` 时 emit `update:value`，符合"模态选择确认提交"模式。
- `props.ts` 中 prop 默认值统一中文 placeholder，保持与表单术语一致。

### Common patterns
- `<a-select>` 作为只读触发器（`disabled` + `@click="openSelectModal"`），点击打开模态。
- 通过 `selectedData.fullName` 显示文本，`treeData` 仅传给左侧树。

## Dependencies
### Internal
- `/@/components/Tree`、`/@/components/Modal`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
