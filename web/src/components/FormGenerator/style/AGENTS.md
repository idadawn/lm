<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# style

## Purpose
`FormGenerator` 设计器的 Less 样式资源。`index.less` 处理设计器主体三栏布局（左侧组件库、中间画布、右侧面板）的网格、可拖拽视觉态与选中态；`rightPanel.less` 专属右侧属性面板的折叠/分组样式。

## Key Files
| File | Description |
|------|-------------|
| `index.less` | 设计器全局样式：`@prefix-cls: ~'@{namespace}-basic-generator'`，三栏 flex 布局、组件列表、画布字段卡片、动作栏 |
| `rightPanel.less` | 右侧字段属性面板样式：`a-form-item` 紧凑布局、属性分类标签页、滚动容器 |

## For AI Agents

### Working in this directory
- 修改时遵循 `@{prefix-cls}` 命名空间隔离原则；勿引入全局选择器。
- 拖拽视觉态（`.components-item:hover`、`.disabled`、`.active-item`）是设计器交互的关键 — 修改前先核对 `vuedraggable` 类名约定。
- 不要在此处放业务模块样式；与 `FlowProcess/style` 一样仅服务于本组件。

### Common patterns
- 使用 Ant Design Vue Less 主题变量保持深色/浅色模式一致。
- 通过 `box-sizing: border-box` 与 `flex-shrink: 0` 控制三栏宽度稳定。

## Dependencies
### Internal
- 全局 Less 变量 (`/@/design/`)
### External
- 无

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
