<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Extras.DependencyModel.CodeAnalysis

## Purpose
Pluggable assembly-scanning and Roslyn-based code-analysis adapter for the Poxiao framework. Carries the heavy compile/analysis dependencies (Roslyn, Razor language, Demystifier, DependencyModel) so they can be opted into without bloating the core `Poxiao.csproj` shipped to all hosts.

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.Extras.DependencyModel.CodeAnalysis.csproj` | net10.0 library that pulls in `Microsoft.CodeAnalysis.CSharp` 4.12.0, `Microsoft.AspNetCore.Razor.Language` 6.0.36, `Microsoft.Extensions.DependencyModel` 10.0.0, `Microsoft.AspNetCore.Mvc.NewtonsoftJson` 10.0.0, `System.Text.Json` 10.0.0, and `Ben.Demystifier` 0.4.1. |

## For AI Agents

### Working in this directory
- This package is currently dependencies-only — actual analyzer/scanning logic lives in the host `Poxiao` framework which loads these packages reflectively. Adding code here requires updating the host's reflective lookup paths.
- Keep package versions aligned with the rest of the framework (Roslyn 4.12, Razor 6.0.36, DependencyModel 10.0).
- Don't add NuGet entries here unless they are genuinely "compile-time / code-analysis" concerns (Roslyn, Razor, DependencyModel, Demystifier).

### Common patterns
- Roles strictly as a *transitive dependency carrier* — empty source set is intentional.

## Dependencies
### External
- `Microsoft.CodeAnalysis.CSharp`, `Microsoft.AspNetCore.Razor.Language`, `Microsoft.Extensions.DependencyModel`, `Microsoft.AspNetCore.Mvc.NewtonsoftJson`, `System.Text.Json`, `Ben.Demystifier`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
