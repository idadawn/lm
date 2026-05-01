<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.CodeGen

## Purpose
Single-project implementation of the 代码生成器 (server-side code generator). Exposes a dynamic-API controller under `api/visualdev/[controller]` (Tag `CodeGenerater`) that reads `VisualDevEntity` metadata, runs templates through `IViewEngine`, and emits zipped C# / Vue / SQL artifacts to the caller. The single 100KB `CodeGenService.cs` file is the entire module — no separate Controller/Dto folders.

## Key Files
| File | Description |
|------|-------------|
| `CodeGenService.cs` | The whole module: ~100KB class implementing `IDynamicApiController, ITransient`. Endpoints for namespace/areas listing, template preview, multi-file zip generation, etc. Uses `ISqlSugarRepository<VisualDevEntity>`, `IViewEngine`, `IDbLinkService`, `IDictionaryDataService`, `IFileService`, `IUserManager`, `IDataBaseManager`, `ICacheManager`. |
| `Poxiao.CodeGen.csproj` | References `Poxiao.VisualDev.Engine` and `Poxiao.VisualDev.Interfaces`; pulls StyleCop analyzers. |

## For AI Agents

### Working in this directory
- `CodeGenService` is intentionally a single fat service. New endpoints should be **added as methods inside it**, grouped under existing `#region Get / Post / Put / Delete` blocks rather than split into multiple files (matches the established convention for this module).
- Controller route is fixed: `[Route("api/visualdev/[controller]")]` and `Order = 175`. Don't reroute.
- Generation logic delegates to `Poxiao.VisualDev.Engine.CodeGen` — keep template authoring there, only orchestration here.
- Output streams use `System.IO.Compression.ZipArchive`; preserve UTF-8 encoding when adding new file types.
- This service is registered as `ITransient` (transient lifetime) — don't cache state on the class.

### Common patterns
- All dependencies injected via constructor with Chinese-XML-doc fields.
- Calls into `KeyVariable.AreasName` (a static cache populated by VisualDev startup) for namespace lookup.
- `[ApiDescriptionSettings(Tag = "CodeGenerater", Name = "Generater", Order = 175)]` for Swagger grouping.

## Dependencies
### Internal
- `Poxiao.VisualDev.Engine` (engine + `Engine.CodeGen` + `Engine.Model.CodeGen` + `Engine.Security`).
- `Poxiao.VisualDev.Entitys` (`VisualDevEntity`, `Dto.CodeGen`, `Enum`).
- `Poxiao.Systems.Interfaces` (`Common.IDictionaryDataService`, `IFileService`, `IDataBaseManager`, etc.).
- `Poxiao.Infrastructure` (`ICacheManager`, `IUserManager`, `KeyVariable`).
- `Poxiao.ViewEngine` — template rendering.
### External
- `Microsoft.AspNetCore.Mvc`, `SqlSugar`, `NPOI.Util`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
