<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Enums

## Purpose
Cross-module shared enums for `Poxiao.Common` — covers account types, login methods, gender, query/sort operators, OSS providers, file location and preview, notice statuses, HTTP/error codes. The largest is `ErrorCode.cs` (≈58 KB) which centralizes platform-wide business error codes.

## Key Files
| File | Description |
|------|-------------|
| `ErrorCode.cs` | Master business error-code enum (≈58 KB) — referenced by `throw Oops.Oh(ErrorCode.xxx)` patterns. |
| `HttpStatusCode.cs` | Wrapped HTTP status enum with Chinese descriptions. |
| `QueryType.cs` | Filter operators (Equal、Contains、GreaterThan、In、Included、…) used in dynamic queries / SuperQuery. |
| `DBSortByType.cs` | Sort directions for SqlSugar query builders. |
| `OSSProviderType.cs` | OSS providers — Invalid(local)、Minio、Aliyun、QCloud、Qiniu、HuaweiCloud. |
| `AccountType.cs` | None / Administrator. |
| `LoginMethod.cs`, `Gender.cs`, `FileLocation.cs`, `PreviewType.cs`, `RequestType.cs`, `ExportFileType.cs`, `NoticeStatus.cs`, `NoticeUserStatus.cs` | Smaller domain enums for system identity, files, notices and exports. |

## For AI Agents

### Working in this directory
- Namespace `Poxiao.Infrastructure.Enums`.
- Every enum value carries a `[Description("中文")]` attribute — required by `EnumExtensions.GetEnumDictionary` (sibling `Extension/EnumExtensions.cs`) which the frontend dropdowns rely on. Always include it.
- `[SuppressSniffer]` on the enum type itself.
- Add new error codes only at the **end** of `ErrorCode.cs` to preserve numeric stability across releases.
- Don't import enums into module-specific feature folders — keep them centralized here.

### Common patterns
- Underlying type is implicit `int`; explicit values used only when stability matters (`AccountType`).
- Chinese `[Description]` text doubles as user-facing labels.

## Dependencies
### Internal
- Consumed across every module; tightly coupled to `Extension/EnumExtensions.cs` for runtime label/dictionary lookup.
### External
- `System.ComponentModel` for `[Description]`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
