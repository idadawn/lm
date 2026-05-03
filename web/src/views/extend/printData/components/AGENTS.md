<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
`extend/printData` 页面的可打印模板组件集合，每个 `.vue` 是一种业务单据样式；`printStyle.ts` 是注入到打印 iframe 的全量内联样式。

## Key Files
| File | Description |
|------|-------------|
| `Offer.vue` | 报价单模板：表头、客户信息、产品价格表、合计与签字栏。 |
| `Bill.vue` | 水电费用清单：使用 `a-table` 多级表头（年份 → 电费/水费），按月份汇总。 |
| `Record.vue` | 员工档案表：基础信息 + 工作经历，固定栏位布局。 |
| `Storage.vue` | 入库通知模板：商品行 + 数量 / 单位 / 备注。 |
| `printStyle.ts` | 长字符串导出（~32KB），包含 reset、表格、字体、@page 打印样式；由 `printData/index.vue` 在打印时注入 iframe。 |

## For AI Agents

### Working in this directory
- 每个模板必须导出 `printRef` —— 父组件以 `ref.printRef.innerHTML` 抓 DOM 写入打印 iframe。
- 不要在模板里写 `<style scoped>` 期望它影响打印；scoped 样式不会带进 iframe。所有打印相关样式必须放 `printStyle.ts`。
- 表格优先 `pagination: false` + `bordered`，并用 `size="small"` 控制密度。

### Common patterns
- 顶层 `<div ref="printRef">` 包裹整页；标题使用 `<h1>/<h2>`，列定义内联在 `<script setup>` 中。

## Dependencies
### Internal
- 父：`../index.vue`；样式：`./printStyle.ts`
### External
- `ant-design-vue` Table、`vue`
