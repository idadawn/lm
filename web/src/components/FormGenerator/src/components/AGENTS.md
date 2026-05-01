<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
`FormGenerator` 设计器内部使用的对话框、面板、运行时解析器组件集合。`Parser.vue` 是把设计 JSON 渲染为运行时表单的关键组件 — 业务列表/表单页通过它消费设计器产物。

## Key Files
| File | Description |
|------|-------------|
| `Parser.vue` | 大型 TSX 组件 (~36KB)，运行时解析 `formData/formRules`，处理 dyOptions、字典数据、远端接口、关联表/弹窗/计算字段、表单脚本钩子 |
| `FieldModal.vue` | 字段重命名/V-model 配置弹窗 |
| `FormAttrPane.vue` | 表单全局属性面板（form size、labelWidth、popupType、按钮、主键策略等） |
| `FormScript.vue` | 表单脚本编辑（onLoad/onSubmit 等钩子的 JS 片段） |
| `PreviewModal.vue` | 设计器实时预览模态 |
| `StylePane.vue` / `StyleScript.vue` | 字段/表单样式与自定义 CSS 编辑 |

## For AI Agents

### Working in this directory
- `Parser.vue` 不要拆分为多个文件 — 它依赖大量组件内部 ref（`tableRefs`、`relations`、`options`），保持单文件以减少状态隔离风险。
- 当 `Parser` 加载远端选项失败时静默返回空 options，业务侧不能假设 options 总有值。
- 表单脚本通过 `getScriptFunc` (`/@/utils/jnpf`) 编译为运行时 `Function`；改动该执行机制是 breaking change。

### Common patterns
- `provide/inject` 注入 `Parser` 实例 ref，让 RelationForm/PopupSelect 等子表单跨组件读取数据。
- 字典/接口数据通过 `getDictionaryDataSelector` / `getDataInterfaceRes` 拉取，结果写入响应式 `state.options`。

## Dependencies
### Internal
- `/@/store/modules/generator`、`/@/api/systemData/dictionary`、`/@/api/systemData/dataInterface`、`/@/utils/jnpf`、`/@/utils/uuid`、`/@/utils/http/axios`
- `../helper/config`、`../helper/render`
### External
- `ant-design-vue`、`dayjs`、`lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
