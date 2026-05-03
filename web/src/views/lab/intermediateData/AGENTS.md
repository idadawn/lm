<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# intermediateData

## Purpose
"中间数据"页面：以产品规格 Tabs 形式展示按公式自动计算的中间数据表，支持自定义排序、填充颜色、批量保存颜色配置。每行可触达层压/厚度子表与可编辑测量单元格。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 主页：BasicTable + 自定义排序控件 + 颜色填充工具栏 + 产品规格 Tabs |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 计算进度条、颜色选择、可编辑单元格、子表 (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 排序使用本地 `sortRules` 状态 + 自定义 `CustomSortControl`，不要走 BasicTable 默认排序。
- 颜色填充以 `selectedColor` 为绘制色或 `isClearMode` 清除；`saveColorsBatch` 批量提交。
- 权限按钮使用 `hasBtnP(PERM_FILL_COLOR)` 控制可见。

### Common patterns
- `selectedProductSpecId` 切换 Tab 后 reload 数据。
- 单元格支持原地编辑（见 `components/cells/`）。

## Dependencies
### Internal
- `/@/api/lab/intermediateData`, `/@/components/Table`
- `usePermission`/`hasBtnP`
### External
- `ant-design-vue`, `@ant-design/icons-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
