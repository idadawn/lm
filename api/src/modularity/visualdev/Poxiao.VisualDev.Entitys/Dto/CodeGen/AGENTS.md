<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# CodeGen

## Purpose
代码生成下载表单的入参 DTO。配合 `VisualDevService` 的下载/导出接口（基于功能模板生成 .NET/前端代码包）。

## Key Files
| File | Description |
|------|-------------|
| `DownloadCodeFormInput.cs` | module / className / subClassName / description / modulePackageName |

## For AI Agents

### Working in this directory
- `subClassName` 是逗号分隔的子表名集合；解析在 Engine 层完成，不要在控制器拆分。
- `modulePackageName` 仅 Java 后端使用，.NET 后端忽略（`hasPackage = false`）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
