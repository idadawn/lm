<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# api

## Purpose
.NET 10 backend for the Laboratory Data Analysis System (检测室数据分析系统). Hosts a modular monolith built on the in-house Poxiao framework (Furion/Sukt-style) with SqlSugar ORM, JWT auth, RabbitMQ EventBus and a sidecar calculation Worker. The API is the primary code path for lab data import, formula calculation, judgement and AI-powered features.

## Key Files
| File | Description |
|------|-------------|
| `Directory.Build.props` | Solution-wide MSBuild props; pins `Microsoft.CodeAnalysis.CSharp` 4.12.0 to resolve transitive version conflicts. |
| `Dockerfile.build` | Two-stage Docker build (mcr `aspnet:10.0` base) with Aliyun apt mirrors, `Asia/Shanghai` TZ, listens on `:9530`, `/health` healthcheck, ENTRYPOINT `Poxiao.API.Entry.dll`. |
| `Dockerbuild.bat` | Windows convenience launcher for `Dockerfile.build`. |
| `NuGet.config` | NuGet feeds (typically including domestic mirrors). |
| `Version.txt` | Single-source-of-truth version string consumed by build scripts. |
| `delete-bin-obj.ps1` | Cleans `bin/` and `obj/` recursively. |
| `CHANGELOG.md` | High-level backend changelog. |
| `README.md` | Minimal env requirements (legacy: SDK 6.0.x note is outdated — actual target is net10.0). |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `framework/` | Poxiao core framework + extras (DI, JWT, SqlSugar/Dapper, Serilog, Mapster, xUnit) (see `framework/AGENTS.md`). |
| `src/` | Application + infrastructure + feature modules; contains the main `Poxiao.sln` (see `src/AGENTS.md`). |
| `tests/` | xUnit unit + integration test projects (see `tests/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Target framework is `net10.0` everywhere — do not downgrade. The repo `README.md` references SDK 6.0.x but that is stale.
- The Docker build expects `./scripts/build-api.sh` to have produced `publish/api/` first; do not edit `Dockerfile.build` to compile in-image without coordination.
- All entity work must follow `.cursorrules` (CLDEntityBase field-naming rules — see project root `.cursorrules`).
- Logs are written under `/app/logs` in the container and `logs/` in dev runs.

### Common patterns
- Modular monolith: `framework/` is reusable infra; `src/modularity/<module>/` are feature modules referenced by the API host project.
- The `Poxiao.API.Entry` host project is the single web entrypoint; `Poxiao.Lab.CalcWorker` is a sibling background-only Worker.

## Dependencies
### Internal
- Root project metadata in `/data/project/lm/CLAUDE.md` and `/data/project/lm/.cursorrules`.

### External
- .NET 10 SDK, RabbitMQ, MySQL/SQL Server/Oracle, Redis, optional Qdrant/TEI/vLLM (see root `docker-compose.yml`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
