<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# signet

## Purpose
扩展示例：电子签章/盖章演示页面。展示如何在打印页面上使用 `vue-drag-resize` 拖拽放置印章图片，并配合 `print-js`/打印样式输出报价单。属于 `views/extend/` 演示套件，仅作前端能力示范，不接入真实业务接口。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 报价单 + 盖章页面：表格、联系人信息、可拖拽 base64 印章 PNG，点击「打印」走 `printStyle` 模板。 |

## For AI Agents

### Working in this directory
- 仅修改演示展示文案/样式，不要把硬编码的「引迈信息技术有限公司」联系人改成业务真实数据。
- 印章使用内联 base64 GIF；如替换图片，注意保留 `VueDragResize` 的 `:isDraggable="showBtn"` 切换逻辑。
- 打印样式从 `../printData/components/printStyle` 引入，避免在本目录复制一份。

### Common patterns
- `defineOptions({ name: 'extend-signet' })` 设置 keep-alive 名称。
- 使用 `reactive` + `toRefs` 暴露状态，符合本仓库 Vue 3 SFC 风格。
- 通过 `ref` + `printJS`/自定义 `handlePrint` 触发浏览器打印。

## Dependencies
### Internal
- `/@/views/extend/printData/components/printStyle` 打印样式
### External
- `ant-design-vue`（Button/Table）、`vue-drag-resize`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
