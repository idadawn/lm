<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Configuration

## Purpose
Static façades over `appsettings`-bound options (`AppOptions`, `OssOptions`, `TenantOptions`). Centralises file-system layout (`UserAvatar`, `IMContentFile`, `DataBackupFile`, `DocumentFile`, `EmailFile`, …) and exposes single-call accessors so callers don't have to inject `IOptions<T>` everywhere.

## Key Files
| File | Description |
|------|-------------|
| `KeyVariable.cs` | Static config façade. Reads `Tenant`, `Poxiao_App`, `OSS` sections via `App.GetConfig<T>(section, true)` and exposes `MultiTenancy`, `MultiTenancyType`, `SystemPath`, `AreasName`, `AllowImageType`, `AllowUploadFileType`, `WeChatUploadFileType`, `SpecialString`, `BucketName`, `FileStoreType`, `AppVersion`, `AppUpdateContent`. |
| `FileVariable.cs` | Static derived absolute paths under `KeyVariable.SystemPath`: `UserAvatarFilePath`, `TemporaryFilePath`, `DataBackupFilePath`, `IMContentFilePath`, `SystemFilePath`, `MPMaterialFilePath`, `DocumentFilePath`, `GenerateCodePath`, `DocumentPreviewFilePath`, `EmailFilePath`, `BiVisualPath`, `TemplateFilePath`. |

## For AI Agents

### Working in this directory
- `SystemPath` returns `string.Empty` when an OSS provider is configured (anything other than `OSSProviderType.Invalid`); local-disk callers must guard against that and route through `IFileManager` instead.
- `FileVariable` paths are evaluated at field-initialisation time relative to `KeyVariable.SystemPath`. Don't add mutable setters or runtime path mutation — downstream code captures these into local variables.
- Add new physical-file roots here (don't sprinkle `Path.Combine` everywhere).

### Common patterns
- All access flows through `App.GetConfig<TOptions>("Section", true)` — the `true` flag enables `IConfigurableOptions` validation.

## Dependencies
### Internal
- `Poxiao.Infrastructure.Options.AppOptions`, `OssOptions`; `TenantOptions` (configurable options), `Poxiao.Infrastructure.Enums.OSSProviderType`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
