<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# data

## Purpose
`matchingRate` 页面的静态演示数据。每个 JS 文件导出一个分组数组（订单/工程/合同/MRP 系统），元素含 id、name、items（含 name + color 用于按钮渲染）。

## Key Files
| File | Description |
|------|-------------|
| `dingdan_data.js` | 订单维度演示数据（订单号 + 子项颜色）|
| `gongcheng_data.js` | 工程维度演示数据 |
| `hetong_data.js` | 合同维度演示数据 |
| `xitong_data.js` | 对接 MRP 系统维度演示数据 |

## For AI Agents

### Working in this directory
- 这些是 mock 数据，不要在生产逻辑中依赖；接口落地后整目录可移除。
- 颜色字段使用 CSS 颜色字符串，直接绑到 `:style="{ background }"`。

### Common patterns
- `export const xxx_data = [...]`，被 `../index.vue` 静态 import。

## Dependencies
### Internal
- 仅被 `../index.vue` 引用
### External
- 无

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
