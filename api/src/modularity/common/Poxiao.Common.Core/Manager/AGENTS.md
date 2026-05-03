<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Manager

## Purpose
Service abstractions that wrap heavyweight infrastructure (multi-tenant SqlSugar connections, OSS file storage, InfluxDB time-series, JWT-derived user context) behind clean interfaces. Each `I*Manager` lives next to its implementation; consumers in feature modules inject the interface only.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `DataBase/` | `IDataBaseManager` — DB-link aware SqlSugar operations across tenants/external sources (see `DataBase/AGENTS.md`) |
| `Files/` | `IFileManager` — OSS-aware upload/download/move/copy plus chunked uploads (see `Files/AGENTS.md`) |
| `InfluxDB/` | `IInfluxDBManager` — measurement enumeration + time-range queries (see `InfluxDB/AGENTS.md`) |
| `User/` | `IUserManager` — request-scoped user/tenant context + data-permission expressions (see `User/AGENTS.md`) |

## For AI Agents

### Working in this directory
- Always inject the interface, never the implementation. Implementations are registered automatically via `ITransient` / `IScoped` / `ISingleton`.
- New managers should follow the same naming: `I{Name}Manager` interface + `{Name}Manager` class in the matching subfolder.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
