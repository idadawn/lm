<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`Icon` 组件的实现层。三个 `.vue` 各负其责：`Icon.vue` 是聪明组件（自动判定 SVG sprite vs Iconify），`SvgIcon.vue` 是基于 `<svg><use xlink:href>` 的本地 sprite 渲染器，`IconPicker.vue` 是图标网格选择器（搜索 + 分页）。

## Key Files
| File | Description |
|------|-------------|
| `Icon.vue` | 通用图标：基于 `icon` prop 后缀 `\|svg` 决定走 `SvgIcon` 还是 Iconify (`@purge-icons/generated`)；支持 `size`/`color`/`spin`/`prefix` |
| `SvgIcon.vue` | SVG sprite 渲染：`<svg><use :xlink:href="#icon-{name}" /></svg>`，依赖 `vite-plugin-svg-icons` 打包到 sprite |
| `IconPicker.vue` | 图标选择器：`<a-input>` + `<a-popover>` 弹层，分页展示 `data/icons.data.ts`，支持搜索（debounce） |

## For AI Agents

### Working in this directory
- `Icon.vue` 中 `SVG_END_WITH_FLAG = '|svg'` 是 SVG vs Iconify 路由约定；其它代码不要重复使用该字符串。
- 修改 `SvgIcon` 的 `prefix` 默认值 (`'icon'`) 需同步 vite svg-icon 插件配置；否则 sprite ID 不匹配。
- `IconPicker` 的图标数据来自相对路径 `../data/icons.data.ts`；如需多套图标集，建议改为 prop 注入而非 hard-coded import。

### Common patterns
- `useDesign('svg-icon')` 生成命名空间 class，配合 Less `@{prefix-cls}` 主题变量。
- `useI18n` 提供 `t('component.icon.placeholder')` 等 i18n 文案。

## Dependencies
### Internal
- `../data/icons.data.ts`、`/@/hooks/web/useDesign`、`/@/hooks/web/useI18n`、`/@/utils/is`、`/@/utils/propTypes`
### External
- `@purge-icons/generated`、`ant-design-vue`、`vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
