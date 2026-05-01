<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
ZEditor 主组件实现。挂载 `MindMap` 思维导图容器、点击节点弹出右侧 `a-drawer` 装载 `editorForm`,并接入 `useMindMapCallback` / `useMindMapResult` 生命周期获取当前选中节点。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 画布 + 抽屉布局;`deleteItem` 通过 `Modal.confirm` 调 `deleteIndicatorValueChain` 删除节点 |
| `props.ts` | 顶层 props:`source / statusOptions` 等 |

## For AI Agents

### Working in this directory
- `useMindMapCallback({ nodeClick })` 由父消费 MindMap 内部事件,不要在画布组件内重复绑定。
- 删除接口失败时 toast `message.error`,成功后调用 `graph.value.removeChild(id)`,顺序不可颠倒。
- 注释中保留了 `useMindMapMore` 的旧引用,新增需求请确认走当前 `useMindMap` 流程。

### Common patterns
- 文案统一通过 `useI18n()`,不要硬编码中文确认弹窗文本。

## Dependencies
### Internal
- `/@/components/MindMap` 及其 hooks
- `/@/api/createModel/model` (`deleteIndicatorValueChain`)
- `../components/editorForm/index.vue`
### External
- `ant-design-vue` (Modal/Drawer/message),`@ant-design/icons-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
