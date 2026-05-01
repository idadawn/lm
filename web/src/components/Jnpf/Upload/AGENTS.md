<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Upload

## Purpose
Jnpf 表单的文件 / 图片上传组件族。对外提供四个变体：`JnpfUploadBtn`、`JnpfUploadFile`（大文件分片）、`JnpfUploadImg`（多图）、`JnpfUploadImgSingle`（单图），统一接入后端 `/api/file/chunk` 与 `getDownloadUrl` 等接口。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 通过 `withInstall` 一次性导出四个上传组件实例。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 上传组件实现、props、SimpleUploader 子组件 (see `src/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 任何新增上传变体需在 `index.ts` 增加 `withInstall` 导出，命名遵循 `JnpfUpload*`。
- 鉴权 / 上传地址 / 文件类型映射来自 `useGlobSetting()`（`apiUrl`、`uploadUrl`），不要硬编码 host。

### Common patterns
- 使用 `Form.useInjectFormItemContext()` 触发表单 item 的 `onFieldChange`，保证校验联动。

## Dependencies
### Internal
- `/@/utils`、`/@/hooks/setting`、`/@/api/basic/common`
### External
- `ant-design-vue`、`vue-simple-uploader`、`spark-md5`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
