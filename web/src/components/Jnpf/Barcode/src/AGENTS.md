<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfBarcode` 的 SFC 实现。基于 `jsbarcode` 在 `<canvas>` 上渲染条码，支持 `static`（直接展示 `staticText`）与 `relation`（从 `formData[relationField]` 动态取值）两种数据来源。

## Key Files
| File | Description |
|------|-------------|
| `Barcode.vue` | 组件实现：使用 `useDesign('barcode')` 生成前缀类名，`buildShortUUID` 生成 canvas id；watch `formData` / 渲染参数变化重绘 |

## For AI Agents

### Working in this directory
- 渲染必须放在 `nextTick` 之后再调用 `JsBarcode`，避免 canvas 尚未挂载。
- 对 `format`、`lineColor`、`background`、`width`、`height`、`barcode` 这些会影响图形输出的字段，整体放进同一个 `watch` 数组，确保任一变化都触发重绘。
- 不要把样式从 `useDesign` 命名空间剥离，全局主题切换依赖该前缀。

### Common patterns
- `dataType` 控制取值来源；`relationField` 通过深度 watch `formData` 同步值。
- 动态生成 canvas id 防止同页面多个条码冲突。

## Dependencies
### Internal
- `/@/hooks/web/useDesign`、`/@/utils/uuid`
### External
- `jsbarcode`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
