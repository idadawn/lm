<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
四类上传控件的实现层。`UploadFile.vue` 负责普通文件 + 大文件分片上传（依赖 `SimpleUploader`），`UploadImg.vue` / `UploadImgSingle.vue` 处理图片墙，`UploadBtn.vue` 提供按钮形态，`Preview.vue` 用于在线预览，配合 `helper.ts` 做扩展名校验。

## Key Files
| File | Description |
|------|-------------|
| `UploadFile.vue` | 文件列表 + 预览 + 批量下载，分片上传委派给 `SimpleUploader/FileUploader.vue`。 |
| `UploadImg.vue` | 多图上传，使用 `a-upload` + `picture-card`，`beforeUpload` 校验大小与 MIME。 |
| `UploadImgSingle.vue` | 单图上传变体，复用 `uploadImgProps`。 |
| `UploadBtn.vue` | 仅按钮触发的上传器，外层场景常用于附件入口。 |
| `Preview.vue` | 通过 `getOnlinePreviewUrl` 与对话框预览 office / pdf 文件。 |
| `helper.ts` | `checkFileType` / `checkImgType` / `getBase64WithFile` 工具函数。 |
| `props.ts` | 共享 `uploadBaseProps`、`uploadImgProps`、`uploadFileProps`、`units` (`KB/MB/GB`)。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `SimpleUploader/` | 分片上传实现（vue-simple-uploader 封装） (see `SimpleUploader/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 文件大小校验统一使用 `units[sizeUnit]`，不要在组件里手写 `1024 * 1024`。
- 修改 prop 时同步更新 `props.ts`，并保持 `uploadFileProps` / `uploadImgProps` 都基于 `uploadBaseProps`。
- 预览路径需要拼接 `globSetting.apiUrl + file.url`，不要直接用 `file.url`。

### Common patterns
- `accept` 字符串会被解析为可接受类型集合并展示在错误提示中。
- 通过 `defineExpose({ uploadFile })` 让父级表单可命令式触发上传。

## Dependencies
### Internal
- `/@/api/basic/common`、`/@/hooks/setting`、`/@/utils/file/download`、`/@/components/Preview`
### External
- `ant-design-vue`、`@ant-design/icons-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
