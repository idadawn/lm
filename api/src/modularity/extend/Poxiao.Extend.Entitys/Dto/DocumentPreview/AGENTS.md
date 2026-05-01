<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# DocumentPreview

## Purpose
文档预览的 DTO。和 `Document` 区别在于：这里不管理文件本身，只负责"在线预览"端点的入参/出参/列表。

## Key Files
| File | Description |
|------|-------------|
| `DocumentPreviewListOutput.cs` | 可预览文件列表 |
| `DocumentPreviewPreviewInput.cs` | 预览请求入参（文件 id / token） |
| `DocumentPreviewPreviewOutput.cs` | 预览返回（预览 URL / 类型 / 过期时间） |

## For AI Agents

### Working in this directory
- 与 `DocumentService` 共用底层 `DocumentEntity`，但 service 层在 `Poxiao.Extend/DocumentPreview.cs`，不要混淆。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
