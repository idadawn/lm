<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# commonForm

## Purpose
`tableDemo` 中所有列表 Demo 共用的「项目」新增/编辑弹窗（BasicModal + BasicForm）。包含项目名称、编码、负责人、立项人、项目类型 Select、阶段、客户、费用等字段以及必填校验，对接 `/@/api/extend/table` 中的 `getInfo / createTable / updateTable`。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | BasicModal 弹窗组件，schemas + useForm + useModalInner，对外 emit `register`/`reload`。 |

## For AI Agents

### Working in this directory
- 该 Form 是 commonTable / lockTable / mergeTable / signTable 等多个 Demo 的共享依赖；改 schema 字段会影响所有引用方。
- 标题通过传入 data 中的 id 计算 `getTitle`（新增 vs 编辑）；保持调用方 `addOrUpdateHandle(id?)` 约定。
- 字典 `industryTypeList` 由调用方通过 `init()` 透传 `componentProps.options`，本组件不直接拉字典。

### Common patterns
- `BasicForm` 的 `FormSchema[]` 写法 + `rules` 必填提示
- `useModalInner(init)` 注册回调，`changeOkLoading` 控制提交态
- 提交后 `emit('reload')` 让父表格刷新

## Dependencies
### Internal
- `/@/components/Modal`, `/@/components/Form`, `/@/api/extend/table`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
