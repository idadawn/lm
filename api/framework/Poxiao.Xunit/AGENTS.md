<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Xunit

## Purpose
xUnit testing scaffolding for the Poxiao framework. Implements an *assembly fixture* pattern (`[AssemblyFixture(typeof(...))]`) so a single fixture instance can be shared across every test class in an assembly — useful for booting the host once per test run.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `XunitExtensions/` | Custom `XunitTestFrameworkExecutor` / assembly+collection runners (see `XunitExtensions/AGENTS.md`). |

## Key Files
| File | Description |
|------|-------------|
| `AssemblyFixtureAttribute.cs` | Assembly-targeted attribute that registers a fixture type to be instantiated once and injected into tests. |
| `TestStartup.cs` | `XunitTestFramework` subclass that returns the custom executor — referenced via `[assembly: TestFramework(...)]` in test projects. |
| `Poxiao.Xunit.csproj` | net10.0 library referencing `xunit.extensibility.execution` 2.9.3 and `Poxiao.csproj`. |

## For AI Agents

### Working in this directory
- This is xUnit v2 (`xunit.extensibility.execution` 2.9.3) — do not migrate APIs to v3 patterns without coordinating across all test projects.
- The fixture model relies on constructor injection by xUnit; instances are stored in `Dictionary<Type, object>` and disposed at end-of-assembly.
- New test projects must declare `[assembly: TestFramework("Poxiao.Xunit.TestStartup", "Poxiao.Xunit")]` to activate this scaffolding.

### Common patterns
- Reflection-based fixture lookup via `[AssemblyFixture]` attributes scanned from each test assembly.

## Dependencies
### Internal
- `Poxiao.csproj`.
### External
- `xunit.extensibility.execution` 2.9.3.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
