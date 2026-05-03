<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`PageWrapper` 与 `PageFooter` 的实现层。`PageWrapper` 通过 `useContentHeight` 监听父容器高度，向下 provide `PageWrapperFixedHeightKey`，让 `BasicTable` 等组件实现自适应；`PageFooter` 是 fixed 在底部的双侧操作槽。

## Key Files
| File | Description |
|------|-------------|
| `PageWrapper.vue` | 头部（`PageHeader` + `headerContent` 插槽）、内容（默认插槽）、底部（`leftFooter` / `rightFooter`）；支持 `dense`、`ghost`、`contentBackground`、`contentFullHeight`、`fixedHeight`。 |
| `PageFooter.vue` | 通过 `useMenuSetting().getCalcContentWidth` 动态计算右下宽度，避开折叠侧栏。 |

## For AI Agents

### Working in this directory
- `fixedHeight=true` 会向下 inject 一个布尔值，业务组件（如 `BasicTable`）据此切换为 flex 布局，不要随意修改 InjectionKey。
- `setCompensation({ useLayoutFooter: true, elements: [footerRef] })` 会把 `PageFooter` 的高度纳入内容区计算，新增固定底部组件时也应使用 `useContentHeight` 的补偿能力。
- `getHeaderSlots` 排除 `default/leftFooter/rightFooter/headerContent`，新增插槽不要冲突这些保留名。

### Common patterns
- 使用 `propTypes` 自定义校验器（来自 `/@/utils/propTypes`）。

## Dependencies
### Internal
- `/@/hooks/web/useDesign`、`/@/hooks/web/useContentHeight`、`/@/hooks/setting/useMenuSetting`、`/@/enums/pageEnum`、`/@/utils/propTypes`
### External
- `ant-design-vue`（`PageHeader`）、`lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
