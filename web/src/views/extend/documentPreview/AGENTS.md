<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# documentPreview

## Purpose
文档在线预览扩展页。两 Tab 演示「本地预览」（doc/docx/xls/xlsx/ppt/pptx/pdf 等）与「永中云预览」两种集成方式，搜索文件后点击文件名调起全屏 iframe 预览。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 主页：tab 切换预览方式 + `BasicTable` 文件列表 + 顶部免责说明 |
| `Preview.vue` | 全屏预览弹层：基于 `a-modal` 嵌入 `<iframe>`，URL 由 `previewFile` 接口拼接并附加 token |

## For AI Agents

### Working in this directory
- 永中云预览仅作演示用途（免责声明已写明），不要把它默认成生产预览方式。
- 预览 URL 经 `encryptByBase64` 编码 token 后追加查询参数；token 失效时需提示重新登录。
- 弹层使用 `jnpf-full-modal` 全屏样式，关闭时调 `handleCancel` 卸载 iframe，避免内存泄漏。

### Common patterns
- `getSearchInfo` computed 把 tab 类型注入查询参数，让同一表格切换数据源。
- 列表点击文件名 `handleView(fileId, fileName)` -> 子组件 `filePreviewRef.open()`。

## Dependencies
### Internal
- `/@/api/extend/documentPreview`
- `/@/components/{Form,Table,Modal}`
- `/@/utils/{auth,cipher}`, `/@/hooks/setting`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
