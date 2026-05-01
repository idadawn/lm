<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Common (Interfaces)

## Purpose
通用基础服务接口。当前唯一接口为文件管理 `IFileService`，被 `FileService` 实现。

## Key Files
| File | Description |
|------|-------------|
| `IFileService.cs` | 文件上传/下载/缩略图/压缩/验证码图等通用入口契约 |

## For AI Agents

### Working in this directory
- 新增公共能力（如二维码、签名）若不属于 Permission/System，可加在此目录；命名 `IXxxService`。
- 任何方法新增/签名调整必须同步 `Poxiao.Systems/Common/FileService.cs`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
