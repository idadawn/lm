<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
Host-builder extension that wires Serilog into the Poxiao API. Reads the `Serilog` configuration section, enriches with `LogContext`, and supplies a console + rolling-file fallback when no sinks are configured.

## Key Files
| File | Description |
|------|-------------|
| `SerilogHostingExtensions.cs` | `UseSerilogDefault(IHostBuilder, Action<LoggerConfiguration>?)` plus an `[Obsolete]` `IWebHostBuilder` shim that returns the builder unchanged under .NET 10. |

## For AI Agents

### Working in this directory
- Live in the `Microsoft.Extensions.Hosting` namespace so consumers get the extension via existing `using` statements.
- Console output template is `[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}` — keep it stable so log shippers can parse it.
- Rolling-file path uses `application..log` (the double dot is intentional; Serilog's daily roller substitutes the date between the dots).

### Common patterns
- Detect single-file environment via empty `Assembly.GetEntryAssembly().Location` and switch to `AppContext.BaseDirectory`.

## Dependencies
### External
- `Serilog`, `Serilog.Events`, `Microsoft.AspNetCore.Builder`, `Microsoft.AspNetCore.Hosting`, `Microsoft.Extensions.Hosting`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
