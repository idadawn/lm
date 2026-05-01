<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# SimpleUploader

## Purpose
基于 `vue-simple-uploader` 的分片上传封装。提供前端 MD5 计算（`spark-md5`）、断点续传、秒传、文件类型 / 大小校验，并在合并阶段调用 `chunkMerge` 接口落库。

## Key Files
| File | Description |
|------|-------------|
| `FileUploader.vue` | 上传器主组件；配置 `target=/api/file/chunk`、`chunkSize=5MB`，处理 `onFileAdded` → `computeMD5` → `resume` 流程，合并成功后 emit `fileSuccess`。 |
| `FileItem.vue` | 单文件行 UI；展示进度条、状态文本、暂停/恢复/重试/删除按钮，监听 uploader 全局事件同步状态。 |

## For AI Agents

### Working in this directory
- 文件名上传前需 `replaceAll('#', '')` 防止后端路径解析异常，沿用现有写法。
- MD5 计算使用 10MB 切片读取，不要与上传分片 `chunkSize`（5MB）混淆。
- 合并接口失败时必须 `file.cancel()` 避免列表残留。

### Common patterns
- `customStatus`（`check` / `uploading`）覆盖 vue-simple-uploader 默认状态字典，实现「文件校验中」提示。
- `checkChunkUploadedByResponse` 解析后端 `chunkNumbers` 实现秒传判定。

## Dependencies
### Internal
- `/@/api/basic/common`（`chunkMerge`）、`/@/hooks/setting`、`/@/utils/auth`
### External
- `vue-simple-uploader`、`spark-md5`、`ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
