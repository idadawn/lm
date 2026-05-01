<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# printData

## Purpose
打印数据演示页面，集成 4 套不同格式的可打印模板（报价单、水电费用、员工档案、入库通知），通过 iframe 打印 + 内联 CSS 实现浏览器原生打印。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 容器页：`<a-tabs>` 切换四个模板（Offer/Bill/Record/Storage），点击“打印”按钮时取当前 tab `printRef.innerHTML` 拼接 `printStyle` 写入新建 iframe，调 `iframe.contentWindow.print()`。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 四个打印模板组件 + 全量打印样式 (`printStyle.ts`) (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 打印通过临时 iframe 实现，不依赖第三方库；调整样式请改 `components/printStyle.ts` 而非这里。
- 每个模板组件必须暴露 `printRef`（`defineExpose({ printRef })` 或顶层 ref），否则 `getRef().printRef` 取不到 DOM。
- `destroyInactiveTabPane` 是必要的，避免 `printRef` 引用旧 DOM。

### Common patterns
- 父子约定：父调子的 `printRef` 拿到要打印的 HTML 片段；样式由父统一注入。

## Dependencies
### Internal
- `./components/Offer.vue` / `Bill.vue` / `Record.vue` / `Storage.vue` / `printStyle.ts`
### External
- `ant-design-vue` Tabs / Button、`vue`
