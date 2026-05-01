<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# component

## Purpose
组件协作层钩子，处理表单元素双向绑定与页面上下文注入等场景，被 `BasicForm`、自定义控件、页面容器复用。

## Key Files
| File | Description |
|------|-------------|
| `useFormItem.ts` | `useRuleFormItem(props, key, changeEvent, emitData)`：通过 `reactive` + `watchEffect` 镜像 props，配合 `getCurrentInstance().emit` 实现 v-model 双向绑定，自动 diff 后异步触发事件 |
| `usePageContext.ts` | 页面级 provide/inject 上下文，封装 `createPageContext` / `usePageContext` 用于 layouts ↔ 子页面共享状态 |

## For AI Agents

### Working in this directory
- `useRuleFormItem` 默认 `key='value'`、`changeEvent='change'`，可与 antd `<a-form-item>` 校验链路无缝对接；自定义事件名时记得在父组件 `v-model:xxx` 同步。
- 不要在此处写视图或样式，仅提供组合函数；UI 组件请放 `web/src/components/`。

### Common patterns
- 通过 `getCurrentInstance()?.emit` 取 emit，不强依赖 setup 第二参，便于在嵌套 hook 内复用。
- 用 `lodash-es/isEqual` 阻止重复 emit，结合 `setTimeout` 让父组件先消化变更。

## Dependencies
### External
- `vue` (`reactive`/`computed`/`watchEffect`/`getCurrentInstance` 等)、`lodash-es`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
