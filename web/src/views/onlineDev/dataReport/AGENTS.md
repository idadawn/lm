<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# dataReport

## Purpose
在线开发 - 数据报表 (Data Report) 管理页面：展示已配置的报表列表，支持新建/编辑、复制、导入 (.json)、预览（弹窗+全屏 iframe）。报表设计器本身托管在独立子应用 (`reportServer`)，本目录通过 iframe 嵌入。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 报表列表页：`BasicTable` + `getDataReportList`，提供 CRUD/Copy/Import/Preview 操作 |
| `Form.vue` | 全屏 `BasicModal`，通过 `iframe` 加载 `reportServer/index.html?token=...` 进入报表设计器 |
| `PreviewPopup.vue` | 全屏 `BasicPopup`，通过 `iframe` 加载 `reportServer/preview.html?id=...` 预览 |

## For AI Agents

### Working in this directory
- 报表设计器是独立前端项目，通过 `useGlobSetting().reportServer` / `report` 拼接 URL 嵌入。
- iframe 与父页面通过 `window.postMessage('closeDialog', ...)` 通信关闭弹窗。
- 上传组件使用 `<jnpf-upload-btn>`，`accept=".json"` 走后端 `/api/datareport/Data/Action/Import`。

### Common patterns
- `BasicTable` + `useTable` + `TableAction` 通用列表三件套。
- 弹窗体系混用 `useModal` (Form/Preview) 与 `usePopup` (PreviewPopup)。

## Dependencies
### Internal
- `/@/api/onlineDev/dataReport`、`/@/components/Table`、`/@/components/CommonModal`
- `/@/hooks/setting`（取 `reportServer`/`report` 域名）、`/@/utils/auth`（token）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
