<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
`BasicForm` 内部的两个子组件：单字段渲染器与底部按钮区。是 `BasicForm.vue` 的实现细节，不直接对外导出。

## Key Files
| File | Description |
|------|-------------|
| `FormItem.vue` | TSX 编写的 schema 单字段渲染器：依据 `schema.component` 从 `componentMap` 取组件、合并 props/rules、支持 `dynamicDisabled`、`renderComponentContent`、`ifShow/show` 条件渲染、占位符自动生成 |
| `FormAction.vue` | 底部按钮组：提交/重置/折叠（高级搜索）按钮 + 三个 named slots `submitBefore`/`resetBefore`/`advanceBefore`/`advanceAfter` |

## For AI Agents

### Working in this directory
- `FormItem` 使用 `inheritAttrs: false`；新加 prop 必须显式声明（见 `props.schema/formProps/allDefaultValues/formModel/setFormModel` 等）。
- 在 `FormItem` 内调用 `componentMap` 时务必经 `upperFirst` 规范化 key；`noFieldComponents`（如 `Divider`、`GroupTitle`）不绑 `formModel.field`。
- `FormAction` 的 `actionColOpt`、`getSubmitBtnOptions` 等响应式属性来自 `useFormContext`，新增按钮请走 slot 而非硬编码。

### Common patterns
- 通过 `usePermission().hasPermission()` 控制按钮/字段可见性。
- i18n key：`component.form.fold` / `component.form.unfold`。

## Dependencies
### Internal
- `../componentMap`、`../helper`、`../hooks/useFormContext`、`../hooks/useLabelWidth`
- `/@/components/Table`、`/@/components/Jnpf`（`JnpfDivider`）
- `/@/utils/helper/tsxHelper` (`getSlot`)
### External
- `ant-design-vue`、`@vueuse/core`、`lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
