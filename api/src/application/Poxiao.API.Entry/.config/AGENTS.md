<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# .config

## Purpose
Local dotnet tool manifest for the API host project. Pins `dotnet-ef` 10.0.6 so EF Core migrations / scaffolding can be invoked from this folder without a global install.

## Key Files
| File | Description |
|------|-------------|
| `dotnet-tools.json` | `isRoot=true` manifest declaring `dotnet-ef` (`rollForward: false`). |

## For AI Agents

### Working in this directory
- Run `dotnet tool restore` from `Poxiao.API.Entry/` before invoking `dotnet ef ...` if the build host is fresh.
- The codebase primarily uses SqlSugar (not EF Core) — `dotnet-ef` is kept here for ad-hoc tooling/inspection only. Do not introduce EF migrations as a substitute for SqlSugar `CodeFirst.InitTables` (see `Extensions/DatabaseInitExtension.cs`).

### Common patterns
- Standard .NET local tool manifest layout.

## Dependencies
### External
- `dotnet-ef` 10.0.6.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
