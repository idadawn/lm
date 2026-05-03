<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Loading 组件的实现与命令式调用层。包含 SFC、运行时挂载工厂以及 Hook 包装，支持嵌入到任意 DOM 元素中作为局部遮罩。

## Key Files
| File | Description |
|------|-------------|
| `Loading.vue` | `Spin` 包装；支持 `tip` / `size` / `absolute` / `loading` / `background` / `theme('dark'\|'light')`，在 `data-theme=dark` 下自动套用 `@modal-mask-bg`。 |
| `createLoading.ts` | 通过 `createVNode` + `render` 创建独立 vnode，返回 `{vm, open, close, setTip, setLoading, $el}` 命令式句柄。 |
| `useLoading.ts` | Hook 包装；接收 `LoadingProps` 或 `{target, props}`，返回 `[open, close, setTip]`。 |
| `typing.ts` | `LoadingProps` 接口（`tip/size/absolute/loading/background/theme`）。 |

## For AI Agents

### Working in this directory
- `createLoading` 中的 `wait` 选项使用 `setTimeout(fn, 0)` 解决 vben-admin issue #438，谨慎修改。
- `useLoading` 通过 `Reflect.has(opt, 'target')` 区分两种重载，新增字段不要破坏该判定。
- 销毁时必须 `vm.el.parentNode.removeChild(vm.el)` 清理 DOM，否则会泄漏。

### Common patterns
- `defineComponent` 风格 + Options API（与项目其它 setup-script 不同），保持现有写法以维持类型推断。

## Dependencies
### Internal
- `/@/enums/sizeEnum`
### External
- `ant-design-vue`、`vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
