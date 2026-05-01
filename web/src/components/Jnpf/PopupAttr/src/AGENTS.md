<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfPopupAttr` SFC 实现。两条数据流：`isStorage=false` 时 watch `generatorStore.getRelationData` 派生显示；`isStorage=true` 时通过 `emitter.on('setRelationData', ...)` 监听 `JnpfPopupSelect` 选中事件，并 `emit('update:value')` 入库。

## Key Files
| File | Description |
|------|-------------|
| `PopupAttr.vue` | inline props（`value`/`showField`/`relationField`/`detailed`/`isStorage`/`disabled`），双数据通道 setValue + emitter 订阅 |

## For AI Agents

### Working in this directory
- `relationField` 必须等于来源 `JnpfPopupSelect` 的 `field` 字段，否则 emitter 比对失败不会回填。
- `isStorage=true` 走 emitter 路径并触发 `formItemContext.onFieldChange()` 进行表单校验；`false` 仅作展示，不写入模型。
- 占位文案（"用于展示关联弹窗的属性…"）由 `isStorage` 决定，勿误改造成动态 placeholder 配置项。

### Common patterns
- inject `'emitter'`（mitt 实例）订阅 `setRelationData` 事件——表单设计器层级已 provide。

## Dependencies
### Internal
- `/@/store/modules/generator`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
