<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# style

## Purpose
`FlowProcess` 流程设计器组件的 Less 样式资源目录。定义流程画布、节点卡片、连接线、左侧组件库以及右侧 `propPanel` 抽屉的视觉风格。样式通过命名空间前缀 `@{namespace}-basic-process` 与全局主题变量集成。

## Key Files
| File | Description |
|------|-------------|
| `index.less` | 主样式：流程容器、左侧组件列表、中间画布节点（开始/审批/条件/子流程）、连接线、缩放控制 |
| `propPanel.less` | 右侧节点属性抽屉样式：标签页、配置表单分组、`common-pane` 通用面板布局 |

## For AI Agents

### Working in this directory
- 顶层样式必须使用 `@prefix-cls: ~'@{namespace}-basic-process'`，避免污染全局。
- 颜色变量优先使用 `@component-background`、`@text-color` 等 Ant Design Vue 主题变量，硬编码色值仅限流程线条专用 (`@line-color: #a9b4cd`)。
- 不要在此处引入新的全局选择器；新增节点样式应嵌套在 `.@{prefix-cls}` 之下。
- 不要内联 JS 逻辑；样式由 `FormGenerator/index.ts` 与流程入口通过 `import` 注入。

### Common patterns
- `.flex-center()` mixin 用于节点内容居中对齐。
- 使用相对单位 (`px` for borders, `%` for layout) 保证多分辨率下流程图可读性。

## Dependencies
### Internal
- 全局 Less 变量：`/@/design/var.less`（间接通过 `@{namespace}` 注入）
### External
- 无 (纯 Less 文件)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
