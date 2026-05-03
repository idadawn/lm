<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfCalculate` 的 SFC 实现。读取 `expression` 配置，结合 `formData` 与表格行（`rowIndex`/`tableVModel`）求值，输出存储用 (`isStorage=1`) 或仅展示用结果，并可附带金额大写。

## Key Files
| File | Description |
|------|-------------|
| `Calculate.vue` | 组件实现：`toRPN` + `calcRPN` 求值、`getAmountChinese`/`thousandsFormat` 格式化、`useDebounceFn` 防抖 |

## For AI Agents

### Working in this directory
- 求值依赖 `mergeNumberOfExps -> toRPN`，结果用 `calcRPN(rpn, formData, rowIndex)` 计算，**不要**在组件内手写解析器。
- `isStorage` 使用 `Number` (0/1)，不是布尔，保留与 FormGenerator 配置一致。
- 当 `detailed` 为 `true` 时切换为纯文本展示，应避免渲染输入框。
- 通过 `Form.useInjectFormItemContext().onFieldChange()` 同步外层校验。

### Common patterns
- 通过 `useDesign('calculate')` 生成命名空间类。
- `getValue` / `getChineseName` 都是 `computed`，副作用集中在 `watch`。

## Dependencies
### Internal
- `/@/components/FormGenerator/src/helper/utils`、`/@/utils/jnpf`、`/@/hooks/web/useDesign`
### External
- `ant-design-vue`、`@vueuse/core`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
