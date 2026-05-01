<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# footer

## Purpose
默认布局的页脚区域。展示版权、备案号、链接等公司信息，根据 `useRootSetting` 的 `getShowFooter` 开关显隐。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `LayoutFooter`：渲染版权与多列链接，z-index 走 `@page-footer-z-index`，文本来自 `appStore.getSysConfigInfo` |

## For AI Agents

### Working in this directory
- 文案应来自后端 `sysConfig`（`copyright`/`recordNumber` 等），避免硬编码企业名。
- 显隐由 `getShowFooter` 控制，新加链接时考虑紧凑模式 / 响应式断点。

### Common patterns
- 使用 `useDesign('layout-footer')` 生成 BEM 前缀；样式与 `design/var/index.less` 中的 `@page-footer-z-index` 协作。

## Dependencies
### Internal
- `/@/hooks/setting/useRootSetting`、`/@/store/modules/app`、`/@/hooks/web/useDesign`。
### External
- `ant-design-vue` (`Layout.Footer`)、`vue`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
