<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfRelationForm` SFC：折叠态 `a-select` + dialog/drawer 表格弹窗，从 `getFieldDataSelect`/`getDataChange`/`getConfigData`（`/@/api/onlineDev/visualDev`）获取动态模型记录与表单配置。`disabled=true` 且有值时点击折叠态调用 `Detail` 组件渲染只读详情。

## Key Files
| File | Description |
|------|-------------|
| `RelationForm.vue` | ~355 行；单选 radio rowSelection；`searchInfo` 含 `modelId`/`relationField`/`columnOptions`；`setValue` 用 `getDataChange` 反查记录详情 |
| `props.ts` | `relationFormProps`：`modelId`/`relationField`/`field`/`columnOptions`/`popupType`/`popupTitle`/`popupWidth`/`hasPage`/`pageSize`/`multiple` |

## For AI Agents

### Working in this directory
- 单选语义固定（`getRowSelection` 写死 `type: 'radio'`），即便 `multiple=true` 也不会切多选——多选场景请使用 `PopupSelect`。
- 选中后通过 `emitter.emit('setRelationData', { jnpfRelationField: props.field, ...data, id })` 通知 attr 组件，payload 必须含 `id`。
- 只读 + 已有值场景：`getConfigData(modelId)` 解析 `formData` 后强制 `popupType='general'` 再传给 `Detail.init`。
- `setValue` 多次校验 `props.value`（异步过程中可能被清空），与 Organize/UserSelect 同模式。

### Common patterns
- 共享 ant 表格搜索 + 序号列 + 滚动高度 `getScrollY` 计算逻辑（与 `PopupSelect` 几近同源，注意维护时同步）。

## Dependencies
### Internal
- `/@/api/onlineDev/visualDev`、`/@/store/modules/generator`、`/@/views/common/dynamicModel/list/detail`、`/@/components/Modal/src/components/ModalClose.vue`、`/@/hooks/web/useDesign`、`/@/hooks/web/useI18n`
### External
- `ant-design-vue`、`@ant-design/icons-vue`、`lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
