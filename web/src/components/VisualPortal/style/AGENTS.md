<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# style

## Purpose
VisualPortal 全局 Less 样式入口。定义 `@{namespace}-basic-portal` BEM 前缀下的卡片头/操作栏/组件菜单/空数据等通用视觉规范,被 Design 与 Portal 两套场景共享。

## Key Files
| File | Description |
|------|-------------|
| `index.less` | `.portal-card-box` 卡片骨架、`.portal-common-title`、`.options-box` 工具栏、菜单 hover 样式等 |

## For AI Agents

### Working in this directory
- 必须使用 `@{namespace}` 与 `@prefix-cls` 变量,不要硬编码 `jnpf-basic-portal`,以兼容多主题命名空间。
- 选择器一律落在 `.@{prefix-cls}` 内部,避免污染全局;对 `ant-card` 等三方类使用 `:deep()`。
- 不在此处放卡片自身样式,卡片样式归各 `Portal/H*` 组件自管。

### Common patterns
- 单位偏好 `px` + flex 布局;卡片头固定 55px,空数据居中 + nodata 图。

## Dependencies
### Internal
- `/@/hooks/web/useDesign` 提供的 namespace 变量
### External
- Less,Ant Design Vue 类名(`ant-card-head*`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
