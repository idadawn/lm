<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# common

## Purpose
Shared infrastructure module referenced by every other feature module in the LIMS backend. Hosts the `Poxiao.Infrastructure` namespace tree: entity base classes, DTO/model contracts, helpers, OSS/file/DB/InfluxDB managers, captcha, EventBus subscribers, dynamic Job runtime, and global filters. Splitting it into three projects keeps domain code (`Poxiao.Common`) free of cross-module dependencies that the manager-rich `Poxiao.Common.Core` requires.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Poxiao.Common/` | Plain types: contracts, DTOs, enums, models, options, helpers (no cross-module entity refs) (see `Poxiao.Common/AGENTS.md`) |
| `Poxiao.Common.Core/` | Runtime services: EventBus, MVC filters, IM handler, dynamic Quartz-style Jobs, Managers (DB / Files / InfluxDB / User), Mapster registrations (see `Poxiao.Common.Core/AGENTS.md`) |
| `Poxiao.Common.CodeGen/` | Code-generator runtime helpers — control parsing for online forms and `ExportImportDataHelper` (see `Poxiao.Common.CodeGen/AGENTS.md`) |

## For AI Agents

### Working in this directory
- Treat the three projects as a layered split: `Poxiao.Common` -> `Poxiao.Common.Core` (depends on system/message/visualdev/taskschedule entities) -> `Poxiao.Common.CodeGen` (depends on visualdev). Never introduce a back-reference from `Poxiao.Common` to the others.
- Almost everything is exposed via the `Poxiao.Infrastructure.*` namespace despite the project being named `Poxiao.Common`. Don't rename without checking thousands of `using Poxiao.Infrastructure.*` consumers.
- Before adding new entity base types, re-read `.cursorrules` — naming conventions for `F_*` columns differ between `EntityBase`/`CDEntityBase` (snake_case) and `OEntityBase`/`CLDEntityBase` (legacy F_ casing).

### Common patterns
- `[SuppressSniffer]` is applied to nearly every public class/enum to suppress framework code-sniffer warnings — keep it on new public types here.
- Static config readers use `App.GetConfig<TOptions>("Section", true)` (e.g. `KeyVariable`, `IMHandler._messageOptions`).
- DI lifetimes come from interfaces in `Poxiao.DependencyInjection` (`ITransient`, `ISingleton`).

## Dependencies
### Internal
- Framework projects: `Poxiao.Extras.Authentication.JwtBearer`, `Poxiao.Extras.DatabaseAccessor.SqlSugar`, `Poxiao.Extras.ObjectMapper.Mapster`.
- Infrastructure: `Poxiao.Extras.EventBus.RabbitMQ`, `Poxiao.Extras.WebSockets`.
- Module entity packages: `Poxiao.Systems.Entitys`, `Poxiao.Message.Entitys`, `Poxiao.TaskScheduler.Entitys`, `Poxiao.VisualDev.Entitys`.

### External
- SqlSugar, Mapster, Newtonsoft.Json, NPOI, Aspose.Cells/Words, FreeSpire.Office, SkiaSharp, UAParser, Yitter.IdGenerator, InfluxData.Net, RabbitMQ.Client, DotNetCore.Natasha.CSharp.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
