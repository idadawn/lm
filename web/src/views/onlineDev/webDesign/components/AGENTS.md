<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
`webDesign` 列表页配套的弹窗组件：新建类型选择、功能发布、外链表单、数据表选择。仅服务在线开发列表页流程。

## Key Files
| File | Description |
|------|-------------|
| `AddModal.vue` | 新建功能类型选择：表单 (`type=2`) / 视图 (`type=4`)，emit `select` |
| `ReleaseModal.vue` | 同步功能至应用：勾选 Web/APP 端发布目标 |
| `ShortLinkModal.vue` | 表单外链配置：开关 `formUse`/`formPassUse`，生成可复制链接 + QR 码 |
| `TableModal.vue` | 数据表选择：通过 `getDataModelList` 拉取 `dataModel` 列表供主/子表绑定 |

## For AI Agents

### Working in this directory
- 这些弹窗都通过 `useModal` 在父 `index.vue` 中注册并 `openModal(record)` 调起。
- `TableModal` 调用 `systemData/dataModel` 接口；新增数据源时优先复用现有 `linkId` 默认 `'0'`。
- 外链下载二维码使用 `QrCode` 组件 + `handleDownload(1)`。

### Common patterns
- `BasicModal` + `useModalInner(init)` 接收父级数据。
- `emit('reload')` 通知父表刷新。

## Dependencies
### Internal
- `/@/api/systemData/dataModel`、`/@/components/Modal`、`/@/components/Table`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
