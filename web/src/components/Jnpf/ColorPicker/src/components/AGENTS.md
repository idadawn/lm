<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
`JnpfColorPicker` 的内部子面板。每个 SFC 接收外部传入的 `Color` 实例（来自 `lib/color.ts`），通过 `lib/draggable` 监听鼠标事件，写回 `color.value/saturation/hue/alpha`。

## Key Files
| File | Description |
|------|-------------|
| `svPanel.vue` | 饱和度/明度二维拾取面板，渲染 `cursor` 并响应拖拽 |
| `hueSlider.vue` | 色相滑条，支持 `vertical` 方向 |
| `alphaSlider.vue` | 透明度滑条 |
| `preDefine.vue` | 预定义颜色块列表，点击切换 `color` |

## For AI Agents

### Working in this directory
- 所有滑块 `mounted` 后调用 `lib/draggable.ts` 注册 drag handler；新增子面板请保持同样模式以共享行为。
- 子组件不要直接修改 `props.color` 引用本身，只能调用 `Color` 实例上的 setter。
- `vertical` 等模式由 prop 控制，不要硬编码方向。

### Common patterns
- 使用 `defineComponent` + `setup` （非 `<script setup>`）以暴露 `name` 给 ColorPicker 主体的 ref 引用。

## Dependencies
### Internal
- `../lib/color`、`../lib/draggable`、`../type/types`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
