<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# fieldForm4

## Purpose
上传类字段示例：演示 `jnpf-upload-file`（单/多附件）和 `jnpf-upload-img`（图片上传）的初始值绑定、提示信息、查看/下载按钮控制等用法。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 附件上传 + 图片上传组件示例（含初始 fileList / imgList） |

## For AI Agents

### Working in this directory
- 初始 fileList 为 `{ name, url }` 数组；业务化时改为后端返回的附件清单。
- `:showView` / `:showDownload` 控制查看与下载按钮显示；演示中关闭以突出上传交互。
- 不要在此处实现存储签名逻辑，统一走项目级上传接口。

### Common patterns
- `jnpf-upload-img` 默认带预览 + 排序，避免再叠加 `a-upload` 包装。
- `showTip` 启用文件大小 / 类型提示，符合项目 UX 规范。

## Dependencies
### Internal
- `/@/components/Container`（`ScrollContainer`）
### External
- `ant-design-vue`（`jnpf-upload-*` 为项目全局组件）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
