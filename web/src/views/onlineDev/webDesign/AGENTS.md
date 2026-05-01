<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# webDesign

## Purpose
在线开发 - Web 端可视化开发 (VisualDev) 主入口：以表格形式管理"表单"和"视图"型功能，提供分步骤的"基础设置 → 表单设计 → 列表设计"全屏向导，并集成工作流 (`workFlow/flowEngine`)、版本管理、外链发布等功能。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 列表页：`getVisualDevList` 渲染，集成 7 个弹窗（Form/ViewForm/Add/Release/ShortLink/Engine/VersionManage/Preview） |
| `Form.vue` | 表单型功能全屏向导：3 步 `a-steps`（基础设置/表单设计/列表设计），主表+子表 `tables` 支持 `webType` 切换 |
| `ViewForm.vue` | 视图型功能全屏向导：2 步（基础设置/列表设计），支持接口数据源与 `interfaceParam` 参数映射 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 类型选择、发布、外链、数据表选择等专用弹窗 (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- "表单" (`type=2`) 与"视图" (`type=4`) 由 `AddModal` 选择后分别走 `Form.vue` / `ViewForm.vue`。
- 全屏弹窗使用 `defaultFullscreen` + `jnpf-full-modal full-modal` 类。
- 发布逻辑通过 `ReleaseModal` 调用，外链 (无需登录填写) 通过 `ShortLinkModal`。

### Common patterns
- `useModal` 注册多个独立弹窗、`registerXxx`/`openXxx` 命名模式。
- 步骤校验：`activeStep` + `maxStep` 守卫"下一步"按钮。

## Dependencies
### Internal
- `/@/api/onlineDev/visualDev`、`/@/api/workFlow/formDesign`、`/@/views/workFlow/flowEngine/*`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
