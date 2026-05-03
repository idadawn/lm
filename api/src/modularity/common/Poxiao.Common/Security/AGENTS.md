<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Security

## Purpose
Helper toolkit for security, codegen, file/Excel I/O, IDs, JSON, machine info, networking and high-level dynamic queries. Despite the folder name, this is the catch-all `*Helper.cs` library — every module reaches in for these primitives.

## Key Files
| File | Description |
|------|-------------|
| `HashHelper.cs` | MD5/SHA1/SHA256/SHA512 string + byte[] hashing (UTF-8 default). |
| `SnowflakeIdHelper.cs` | `NextId()` — Yitter `IdGenerator` wrapper with optional Redis-backed `WorkerId` registration via P/Invoke into `lib/regworkerid_lib_v1.3.1`. Lazy `App.GetConfig<CacheOptions>` to survive Worker hosts where DI isn't fully booted. |
| `JsonHelper.cs` | `ToJsonString` / `ToObject<T>` / `ToList<T>` extension helpers backed by `IJsonSerializerProvider` (Newtonsoft) with `PraseToJson` formatter. |
| `FileHelper.cs` (~26 KB) | File system, MIME and stream utilities. |
| `ExcelExportHelper.cs` (~43 KB) / `ExcelImportHelper.cs` | NPOI/Aspose.Cells-driven export/import driven by `Models/NPOI/`. |
| `MachineHelper.cs` | Populates `Models/Machine/` from host metrics. |
| `SuperQueryHelper.cs` (~26 KB) | Translates `SuperQueryModel` → SqlSugar `IConditionalModel`. |
| `CodeGenHelper.cs`, `CodeGenExportDataHelper.cs`, `CodeGenAuthorizeHelper.cs` | Code-gen runtime — entity/field/auth resolution. |
| `PinyinHelper.cs` (~37 KB) | Chinese→pinyin conversion for search/matching. |
| `NetHelper.cs`, `PDFHelper.cs`, `XmlHelper.cs`, `TreeHelper.cs`, `QueryTreeHelper.cs`, `ComparisonHelper.cs`, `EqualityHelper.cs`, `ReflectionHelper.cs` | Smaller domain helpers. |

## For AI Agents

### Working in this directory
- Namespace `Poxiao.Infrastructure.Security`.
- All classes are `static` — do not switch to instance/DI without coordinating call sites across modules.
- `SnowflakeIdHelper` swallows registration exceptions (`Log.Error("RegisterOne failed", ex)`) and falls back to `WorkerId=1`. Don't change this fallback path; many non-web hosts depend on it.
- `JsonHelper._jsonSerializer` is initialized at field-init time via `App.GetService` — order of initialization matters; avoid moving it.
- The native `regworkerid` lib path is hardcoded — keep matching native binary in `lib/` when bumping versions.
- Don't add business-logic helpers here; this folder is reserved for cross-cutting infra.

### Common patterns
- `static class XxxHelper` with extension methods.
- Optional `Encoding` defaults to UTF-8 throughout.
- Chinese XML doc + version banner header on legacy helpers.

## Dependencies
### Internal
- `Poxiao.Cache` (`CacheOptions`).
- `Poxiao.JsonSerialization` (`IJsonSerializerProvider`, `NewtonsoftJsonSerializerProvider`).
- `Poxiao.Logging` (`Log`).
- `Models/`, `Enums/`, `Net/` of this assembly.
### External
- `Yitter.IdGenerator` (snowflake).
- `Newtonsoft.Json`.
- `NPOI`, `Aspose.Cells`.
- Native `regworkerid_lib` (P/Invoke).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
