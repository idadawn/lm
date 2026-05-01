<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Options

## Purpose
Strongly-typed configuration option classes implementing `IConfigurableOptions` — bound to sections of `AppSetting.json` / `appsettings.json` at startup. Centralizes the platform's app-wide settings (file upload, OAuth/SSO, OSS, message link domains, social login config).

## Key Files
| File | Description |
|------|-------------|
| `AppOptions.cs` | Core platform settings — `CodeAreasName`、`SystemPath`、upload allow-lists (image/file/wechat/MP)、`SpecialString`、`PreviewType`、kkfile `Domain`、`YOZO` (永中) sub-config、`ErrorReport`/`ErrorReportTo`. |
| `OauthOptions.cs` | SSO config — `Enabled`、`LoginPath`、`SucessFrontUrl`、`DefaultSSO`、`TicketTimeout`、nested `SSO` (`Auth2`、`Cas`) and `Pull` (user provisioning REST endpoints). |
| `OssOptions.cs` | Object-storage provider config (Aliyun / Tencent / Minio / Qiniu / Huawei). |
| `SocialsOptions.cs` | Social-login provider config (WeChat、DingTalk、…). |
| `MessageOptions.cs` | Message link domains — `DoMainPc`、`DoMainApp`、`AppPushUrl`. |

## For AI Agents

### Working in this directory
- Namespace `Poxiao.Infrastructure.Options`.
- Every class is `sealed` and implements `IConfigurableOptions` (from `Poxiao.ConfigurableOptions`) — keep the interface so `App.GetConfig<T>()` can resolve it.
- Names map by convention to JSON sections (e.g., `AppOptions` ↔ `"App"` section; `OauthOptions` ↔ `"Oauth"`).
- Nested config classes (`SSO`, `Auth2`, `Cas`, `Pull`, `YOZO`) are kept in the same file as the parent options class — match this pattern.
- Property names use PascalCase to match `IConfiguration` binding semantics — do **not** switch to camelCase (would break binding from `appsettings.json`).

### Common patterns
- One options class per concern; nested DTOs co-located.
- Lists for allow-lists (`AllowUploadImageType`, `MPUploadFileType`) — empty means "block all".

## Dependencies
### Internal
- `Poxiao.ConfigurableOptions` — `IConfigurableOptions` marker.
- `Poxiao.Infrastructure.Enums.PreviewType`.
### External
- (Indirectly) Aspose.Cells / NPOI in `OauthOptions.cs` `using` (probably accidental — remove on next refactor).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
