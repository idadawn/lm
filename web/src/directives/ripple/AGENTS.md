<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ripple

## Purpose
Material Design 风格的点击波纹指令。鼠标按下时基于点击坐标动态生成 `.ripple-container > .ripple` DOM，使用 cubic-bezier 过渡半径扩散，松开/拖拽/离开时淡出并清理。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `RippleDirective` 实现：`beforeMount` 监听 `mousedown`，`updated` 同步 `ripple-background` 与 disable 状态；附加 `clearRipple`/`setBackground` 至宿主元素 |
| `index.less` | 波纹容器与扩散动画的样式声明 |

## For AI Agents

### Working in this directory
- 通过 `el.setAttribute('ripple-background', '...')` 或 directive 修饰符（如 `v-ripple.touchstart.300`）覆盖事件类型与 transition 时长。
- 指令会临时把宿主 `position` 改为 `relative`（若原值非 `relative`），清理时尝试还原；写新组件时避免在父级 transform 中破坏定位上下文。
- 不要把状态挂在模块作用域：每次 `rippler()` 调用内部独立创建 DOM 与监听，跨实例无共享。

### Common patterns
- 使用 `setProps(modifiers, options)` 将 v-modifier 解析为 `event` (字符串) 或 `transition` (毫秒数)。
- DOM 清理通过 `setTimeout` 串行：先褪色 → 移除容器 → 还原 position，避免动画截断。

## Dependencies
### External
- `vue` (`Directive` 类型)。本指令在 `directives/index.ts` 之外按需引入，使用前需要在组件级 `directives: { ripple: RippleDirective }`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
