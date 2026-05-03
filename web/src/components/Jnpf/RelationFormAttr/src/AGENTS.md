<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfRelationFormAttr` SFC 实现：与 `PopupAttr` 同构。`isStorage=false` 时 watch `generatorStore.getRelationData` 派生显示；`isStorage=true` 时通过 `emitter.on('setRelationData', ...)` 接收 `JnpfRelationForm` 选中事件并 emit 入库。

## Key Files
| File | Description |
|------|-------------|
| `RelationFormAttr.vue` | inline props（`value`/`showField`/`relationField`/`detailed`/`isStorage`/`disabled`），双数据通道 |

## For AI Agents

### Working in this directory
- `relationField` 必须等于来源 `JnpfRelationForm` 的 `field` 字段，否则 emitter 比对失败。
- `isStorage=true` 调用 `formItemContext.onFieldChange()` 触发表单校验；`false` 仅展示。
- 占位文案差异点（"用于展示关联表单的属性…"）与 `PopupAttr` 不同——不要相互复用 placeholder 字符串。

### Common patterns
- inject `'emitter'`（mitt）订阅 `setRelationData`——provider 在表单设计器顶层。

## Dependencies
### Internal
- `/@/store/modules/generator`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
