<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# barCode

## Purpose
二维码 / 条形码生成示例页。提供两个 Tab：分别用 `qrcode` 库在 `<canvas>` 上绘制二维码，用 `jsbarcode` 库绘制条形码，输入字符串即可在前端实时生成图像。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 单文件页：`a-tabs` 切换二维码/条形码生成；表单输入 + canvas 渲染 |

## For AI Agents

### Working in this directory
- 纯前端绘制，无后端调用；不要引入接口或鉴权逻辑。
- `qrcode.toCanvas(element, value, options)` 与 `JsBarcode(element, value, options)` 都直接接受 DOM 节点；不要替换为 v-if 切换组件，否则 ref 会丢失。
- `defineOptions({ name: 'extend-barCode' })` 用于 keep-alive 缓存。

### Common patterns
- `reactive` + `toRefs` 暴露状态；`ref` 拿 canvas DOM。
- 条码内容仅支持数字时建议保留 `JsBarcode` 默认 `format: 'CODE128'`。

## Dependencies
### External
- `jsbarcode`, `qrcode`, `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
