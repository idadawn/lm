<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# data

## Purpose
`Icon` 组件的图标元数据目录。`icons.data.ts` 内置 Ant Design 图标集的全部图标名列表（`prefix: 'ant-design'`），供 `IconPicker.vue` 渲染图标网格、分页、搜索使用。

## Key Files
| File | Description |
|------|-------------|
| `icons.data.ts` | 默认导出 `{ prefix: 'ant-design', icons: string[] }`，包含 ~2000 条 antd icon 名（如 `'account-book-filled'`、`'aim-outlined'`、`'alipay-circle-filled'`） |

## For AI Agents

### Working in this directory
- 此文件是生成数据，避免手动维护单条图标名；如要切换图标集应整体替换该文件，并同步 `IconPicker.vue` 的 `prefix` 取值。
- 图标命名遵守 antd 规范：`<name>-<filled|outlined|twotone>`；自定义图标走 SvgIcon sprite 而非添加到此文件。
- 不要 import 此文件之外的脚本/数据；保持纯数据导出。

### Common patterns
- 单文件 default export，`prefix + icons[]` 二字段结构。

## Dependencies
### Internal
- 无
### External
- 无（纯数据）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
