<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Files

## Purpose
File I/O abstraction. Hides whether a file is on local disk or one of the OSS providers (Minio / Aliyun / QCloud / Qiniu / HuaweiCloud) selected via `OssOptions.Provider`. Supports JSON template export/import, large-file chunked upload + merge, and folder listing.

## Key Files
| File | Description |
|------|-------------|
| `IFileManager.cs` | Interface with three concern groups: JSON export/import (`Export`, `Import`), OSS-aware ops (`UploadFileByType`, `DownloadFileByType`, `GetObjList`, `DeleteFile`, `ExistsFile`, `GetFileStream`, `MoveFile`, `CopyFile`), and chunked-upload (`UploadChunk(ChunkModel)`, `Merge(ChunkModel)`). Plus utility `GetPathByType(type)`, `GetFileSize(long)`, `GetChunkModel(input, saveFileName)`. |
| `FileManager.cs` | Implementation; switches on `KeyVariable.FileStoreType`. |

## For AI Agents

### Working in this directory
- Never assume a local path — always go through `UploadFileByType` / `DownloadFileByType` / `GetFileStream`. Local-disk fall-back is only when `OSSProviderType.Invalid`.
- `GetPathByType(string type)` resolves to one of the `FileVariable` static paths (UserAvatar, IMContentFile, …). Add new logical buckets there, then surface them here.
- `Export(jsonStr, name, ExportFileType)` produces JSON template files used by visualdev for cross-tenant migration; the front-end imports them via `Import(IFormFile)`.

### Common patterns
- All async; file streams disposed by callers.
- `ChunkModel` carries the upload session info between `UploadChunk` (each part) and `Merge` (final assembly).

## Dependencies
### Internal
- `Poxiao.Infrastructure.Models` (AnnexModel, ChunkModel, FileControlsModel), `Poxiao.Infrastructure.Enums.ExportFileType`, `Poxiao.Infrastructure.Configuration.FileVariable`/`KeyVariable`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
