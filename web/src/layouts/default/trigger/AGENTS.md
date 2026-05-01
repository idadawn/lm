<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# trigger

## Purpose
菜单折叠触发器。两种形态：放在侧栏底部 (`SiderTrigger`) 或顶部栏左侧 (`HeaderTrigger`)，通过 `useMenuSetting().toggleCollapsed()` 切换菜单折叠态。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `LayoutTrigger`：props `sider`(默认 true) 决定渲染 SiderTrigger，否则 HeaderTrigger；`theme` 控制 light/dark 配色 |
| `SiderTrigger.vue` | 侧栏底部样式触发器（图标 + 文字，受 `getCollapsed` 影响显隐） |
| `HeaderTrigger.vue` | 顶栏样式折叠图标按钮，配合 `getShowHeaderTrigger` 显隐 |

## For AI Agents

### Working in this directory
- 触发器具体显示位置由 `useMenuSetting().getTrigger` (`TriggerEnum`) 控制；layout 各处会根据 enum 选择性渲染 `LayoutTrigger`。
- 不要在两个子组件里自行调 store，统一走 `useMenuSetting()` 暴露的 `toggleCollapsed`。
- 主题颜色由 `theme` prop 决定，与 header / sider 主题强相关，保持透传。

### Common patterns
- `<script setup>` + `defineOptions({ name: 'LayoutTrigger' })`；props 用 `propTypes`。
- 图标使用 iconfont (`icon-ym-*`) 与 antd icons 混合。

## Dependencies
### Internal
- `/@/utils/propTypes`、`/@/hooks/setting/useMenuSetting`、`/@/hooks/web/useDesign`。
### External
- `vue`、`ant-design-vue`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
