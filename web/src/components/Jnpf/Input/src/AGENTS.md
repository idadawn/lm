<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfInput` / `JnpfTextarea` 的实现。在 antd 基础上增加 `prefixIcon` / `suffixIcon` 字符串（字体类名）、`showPassword` 切换为 `Input.Password`。

## Key Files
| File | Description |
|------|-------------|
| `Input.vue` | 文本输入：`Comp = props.showPassword ? Input.Password : Input`；通过 `<i :class="prefixIcon">` 注入图标 |
| `Textarea.vue` | 多行输入：固定 `rows`，包装 `Input.TextArea` |
| `props.ts` | `inputProps`（`showPassword`、`prefixIcon`、`suffixIcon`、`value`）、`textareaProps`（`rows`、`value`） |

## For AI Agents

### Working in this directory
- `prefixIcon` / `suffixIcon` 是字体图标类名字符串（如 `icon-ym-xxx`），不是 antd 图标组件——保持渲染为 `<i :class>`。
- 切换 `showPassword` 必须重渲染（`Comp` 在 setup 阶段决定）；切换为 prop 响应式时需重写为 computed。
- 透传 `$slots` 时使用 `Object.keys($slots)` 动态遍历。

### Common patterns
- `useDesign('input')` 命名空间。
- `defineOptions({ inheritAttrs: false })` + `useAttrs` 模式。

## Dependencies
### Internal
- `/@/hooks/core/useAttrs`、`/@/hooks/web/useDesign`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
