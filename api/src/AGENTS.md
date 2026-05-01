<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Source root for the API — holds the main Visual Studio solution (`Poxiao.sln`), application host projects, infrastructure adapters, and all feature modules of the modular monolith. Build of this directory produces both the web API and the calculation Worker.

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.sln` | Main solution; references `application/`, `infrastructure/`, every `modularity/<module>/*` project, and the `framework/` projects. |
| `Poxiao.sln.DotSettings` | Rider/ReSharper team-shared settings. |
| `Directory.Build.props` | Project-wide MSBuild props (target framework, analyzers, language version). |
| `Directory.Build.targets` | Project-wide MSBuild targets. |
| `Dockerfile` | Multi-stage Dockerfile for the API host (paired with `../Dockerfile.build`). |
| `.dockerignore` | Files excluded from Docker build context. |
| `dotnet.ruleset` / `stylecop.json` / `.editorconfig` | StyleCop + analyzer config shared across all `src/` projects. |
| `global.json` | Pins the .NET SDK channel for this folder. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `application/` | Host projects (`Poxiao.API.Entry` web API + `Poxiao.Lab.CalcWorker` background calc Worker) (see `application/AGENTS.md`). |
| `infrastructure/` | Cross-cutting infrastructure adapters: WebSockets, RabbitMQ EventBus, third-party (Email/SMS/WeChat/DingDing/JSEngine), CollectiveOAuth (see `infrastructure/AGENTS.md`). |
| `modularity/` | Feature modules (lab, system, workflow, kpi, ai, etc.) — each module owns its Entity/Service/Controller/Dto folders (see `modularity/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Always work via `Poxiao.sln` so all module references resolve. Adding a new project requires registering it both in this `.sln` and in `application/Poxiao.API.Entry/Poxiao.API.Entry.csproj` (which lists every module via `<ProjectReference>`).
- StyleCop is enabled solution-wide via `Directory.Build.props` + `dotnet.ruleset`; do not silence rules without justification.
- Database conventions in `/data/project/lm/.cursorrules` apply — `F_Id`, `F_TenantId`, `F_CREATORTIME`/`F_CREATORUSERID`/`F_ENABLEDMARK`, `F_LastModifyTime`/`F_LastModifyUserId`/`F_DeleteMark`/`F_DeleteTime`/`F_DeleteUserId`.

### Common patterns
- Projects are layered: `application/*` → `modularity/*` → `infrastructure/*` and `framework/*`.
- Feature module project naming: `Poxiao.<Module>` (entities/services) + `Poxiao.<Module>.Web.Core` (controllers).

## Dependencies
### Internal
- `../framework/` (vendored Poxiao framework, project-referenced).

### External
- All transitive .NET 10 packages from individual `.csproj` files; primary ORM is SqlSugar.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
