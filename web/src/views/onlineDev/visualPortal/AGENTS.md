<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# visualPortal

## Purpose
在线开发 - 可视化门户 (Visual Portal) 管理：维护门户元数据，调用门户设计器 (`/@/components/VisualPortal/Design`) 进行拖拽设计，发布到 Web/APP，并支持导入/导出 `.vp` 文件。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 门户列表：`getPortalList` 渲染表格，调度 `Form` / `PortalDesign` / `ReleaseModal` / `PreviewModal` 等弹窗 |
| `Form.vue` | 新建/编辑门户基础信息：`type=0` 设计型、`type=1` 配置路径型；支持"确定并设计"直跳设计器 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 门户专用 modal（发布弹窗）(see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 设计器是共用组件 `/@/components/VisualPortal/Design`，本目录只负责列表与元数据表单。
- `linkType=1` 时 `Form.vue` 内置 `validateUrl` 校验外链。
- 默认运行名 `defineOptions({ name: 'onlineDev-webDesign' })`（与 webDesign 复用，注意区分）。

### Common patterns
- 列表页统一布局 `page-content-wrapper-center`。
- `enabledMark` 字段用 `a-tag` 渲染启用/禁用状态。

## Dependencies
### Internal
- `/@/api/onlineDev/portal`、`/@/components/VisualPortal/Design`、`/@/components/CommonModal`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
