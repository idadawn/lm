<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Common

## Purpose
Foundation library for the modular monolith — assembly name `Poxiao.Infrastructure`, csproj `Poxiao.Infrastructure.csproj`. Provides everything that downstream modules can take a hard reference on without pulling in feature-module entities: SqlSugar entity base classes, DTOs, enums, options, models, NPOI/Excel helpers, helpers for Excel/PDF/Snowflake/Pinyin/Hash/JSON, IP/UserAgent utilities, the standard `PageInputBase`/`PageInfo` paging contract, and the `SqlSugarUnitOfWork` filter implementation.

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.Infrastructure.csproj` | .NET project; bundles Aspose.Cells/Words, NPOI, SkiaSharp, UAParser, Yitter.IdGenerator. Excludes redundant `Contracts/OContracts/{EntityBase,ICreatorTime,IDeleteTime,IEntity}.cs` from compilation. |
| `GlobalUsings.cs` | Global usings: Mapster, Newtonsoft.Json, SqlSugar, `Poxiao.Infrastructure.Const/Contracts/Security`, `Poxiao.Extras.DatabaseAccessor.SqlSugar.Models`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Captcha/` | SkiaSharp-based image captcha generator (see `Captcha/AGENTS.md`) |
| `CodeGenUpload/` | Attribute carrying online-form control config metadata (see `CodeGenUpload/AGENTS.md`) |
| `Configuration/` | `FileVariable` / `KeyVariable` static config readers (see `Configuration/AGENTS.md`) |
| `Const/` | `ClaimConst`, `CommonConst`, `PoxiaoKeyConst` cache key + form-control constants (see `Const/AGENTS.md`) |
| `Contracts/` | SqlSugar entity base classes (CLDEntityBase, EntityBase…) (see `Contracts/AGENTS.md`) |
| `Dtos/` | Cross-module DTOs (database, message, OAuth, VisualDev, import/export) (see `Dtos/AGENTS.md`) |
| `Enums/` | Domain enums (account type, OSS provider, error codes, query type, login method) (see `Enums/AGENTS.md`) |
| `Extension/` | `string`/`enum`/`DateTime`/dictionary extension helpers (see `Extension/AGENTS.md`) |
| `Filter/` | Pagination contracts (`PageInputBase`, `PageInfo`, `KeywordInput`) (see `Filter/AGENTS.md`) |
| `Json/` | `EnumUseNameConverter` for Newtonsoft.Json enum-by-name (see `Json/AGENTS.md`) |
| `Models/` | Domain models grouped by sub-area (see `Models/AGENTS.md`) |
| `Net/` | IP locator (CZ88 dat) and UserAgent parsing (see `Net/AGENTS.md`) |
| `Options/` | `IConfigurableOptions`-bound `AppOptions`/`OssOptions`/etc. (see `Options/AGENTS.md`) |
| `Security/` | Helpers (`SnowflakeIdHelper`, hash, Excel/PDF, codegen, query tree) (see `Security/AGENTS.md`) |
| `UnitOfWork/` | SqlSugar transactional `IUnitOfWork` implementation (see `UnitOfWork/AGENTS.md`) |

## For AI Agents

### Working in this directory
- Namespaces all start with `Poxiao.Infrastructure.*` (matching the assembly name), not `Poxiao.Common`. Match this when adding files.
- Don't reference `Poxiao.*.Entitys` modules from here — keep this layer dependency-free except for framework projects (otherwise rebuild graph cycles appear).
- Removing any file from `Contracts/OContracts` requires also editing the `<Compile Remove>` list in the csproj.

### Common patterns
- Public types decorated with `[SuppressSniffer]` to silence the in-house sniffer.
- Camel-case property names on DTOs (`userId`, `pageSize`, `vModel`) — they serialise directly to the Vue front-end.
- `App.GetConfig<TOptions>("Section", true)` is the standard way to read configuration; see `Configuration/KeyVariable.cs`.

## Dependencies
### Internal
- `framework/Poxiao.Extras.Authentication.JwtBearer`, `framework/Poxiao.Extras.DatabaseAccessor.SqlSugar`, `framework/Poxiao.Extras.ObjectMapper.Mapster`.

### External
- Aspose.Cells / Aspose.Words, FreeSpire.Office, NPOI, SkiaSharp, UAParser, Yitter.IdGenerator, System.Management, Newtonsoft.Json, Mapster.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
