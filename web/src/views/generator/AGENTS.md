<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# generator

## Purpose
代码生成器（在线开发）功能的视图根目录。围绕 `/@/api/onlineDev/visualDev` 的 `getVisualDevList / create / update / copy / downloadCode / codePreview` 提供两条业务线（webForm = 普通/流程表单、flowForm = 工作流表单），并提供两个共享弹窗：下载产物配置与代码预览。

## Key Files
| File | Description |
|------|-------------|
| `DownloadModal.vue` | 「输出设置」下载弹窗：选择模块、模块包名、功能描述、类名，调用 `downloadCode` 并 `downloadByUrl` 触发下载。 |
| `PreviewModal.vue` | 「代码预览」全屏弹窗：左侧文件树 + 右侧 `MonacoEditor` 多语言只读预览。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `flowForm/` | 流程表单生成器列表 + 设计向导（基础设置 / 表单设计两步） (see `flowForm/AGENTS.md`) |
| `webForm/` | 普通/流程表单生成器列表 + 三步向导（基础设置 / 表单设计 / 列表设计） (see `webForm/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 列表页通过 `searchInfo.type` 区分 web 表单（type=2）vs 流程表单（type=3）；新增类型时同步后端枚举。
- 共享弹窗 `DownloadModal` / `PreviewModal` 通过相对路径 `../DownloadModal.vue` 在子目录引用，请勿就地复制。
- 资源 logo `../../assets/images/zhichang.png`（来自 webForm 子目录视角）；调整层级注意路径。

### Common patterns
- `BasicModal` `defaultFullscreen` + 自定义 header（步骤条 + 操作按钮）
- `useModal` / `useModalInner` 注册器
- 列表与弹窗之间通过 `register/openModal(record, mode)` 解耦

## Dependencies
### Internal
- `/@/api/onlineDev/visualDev`, `/@/components/Modal`, `/@/components/Tree`, `/@/components/CodeEditor`, `/@/utils/file/download`
### External
- `ant-design-vue`, `monaco-editor`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
