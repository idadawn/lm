<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Extras.Logging.Serilog

## Purpose
Serilog logging adapter for the Poxiao framework. Wraps `Serilog.AspNetCore` 9.0 and exposes `UseSerilogDefault` host-builder extensions that read configuration from `Serilog:*` keys, fall back to console + rolling-file output, and handle single-file deployments via `AppContext.BaseDirectory`.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Extensions/` | `UseSerilogDefault` host-builder extension (see `Extensions/AGENTS.md`). |

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.Extras.Logging.Serilog.csproj` | net10.0 library; `<NoWarn>0618</NoWarn>` because the legacy `IWebHostBuilder` overload is intentionally kept as `[Obsolete]` for back-compat. |

## For AI Agents

### Working in this directory
- Two `UseSerilogDefault` overloads exist; the `IWebHostBuilder` one is **obsolete** under .NET 10 (returns the original builder unchanged) — keep callers migrating to `IHostBuilder`.
- Default fallback when `Serilog:WriteTo:0:Name` is not configured is console + daily-rolling file at `BaseDirectory/logs/application..log` with UTF-8 encoding; preserve that contract for ops parity.

### Common patterns
- `Assembly.GetEntryAssembly().Location` is empty in single-file publish — code branches on that to pick `AppContext.BaseDirectory`.

## Dependencies
### External
- `Serilog.AspNetCore` 9.0.0, `Microsoft.Extensions.DependencyInjection.Abstractions` 10.0.0.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
