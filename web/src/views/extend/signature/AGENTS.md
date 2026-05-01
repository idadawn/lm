<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# signature

## Purpose
电子签名（在线签字）演示页面：在报价单模板中嵌入手写签名画板，签名后转图片保存并支持打印。属于 `extend` 模块下交互式打印场景。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 主页面：`vue-esign` 画板 + “清空 / 确定签名 / 打印”操作；签名提交后 `resultImg` 显示图片，画板隐藏；打印复用 `../printData/components/printStyle`。 |

## For AI Agents

### Working in this directory
- 签名组件来自项目内 `/@/components/Jnpf/Sign/src/esign.vue`（封装版），不要直接 `npm install vue-esign`，避免重复依赖。
- 打印样式与 `printData` 共享，通过 `import printStyle from '../printData/components/printStyle'`；如改样式需考虑那边影响。
- `lineWidth/lineColor/bgColor/isCrop` 通过 `reactive` 暴露；提交时调用 `esignRef.value.generate()` 拿到 dataURL。

### Common patterns
- 通过 `showSig` 在画板与结果图片之间切换；打印通过临时 iframe + `printStyle` 注入。

## Dependencies
### Internal
- `/@/components/Jnpf/Sign/src/esign.vue`、`/@/hooks/web/useMessage`、`../printData/components/printStyle`
### External
- `ant-design-vue` Table / Button、`vue`
