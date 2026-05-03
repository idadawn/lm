<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# XunitExtensions

## Purpose
Custom xUnit runners that implement the assembly-fixture pattern. The executor wires up a fixture-aware assembly runner, which in turn instantiates `[AssemblyFixture]`-marked types once and passes them to a fixture-aware collection runner so tests can receive the shared instance via constructor injection.

## Key Files
| File | Description |
|------|-------------|
| `XunitTestFrameworkExecutorWithAssemblyFixture.cs` | Custom `XunitTestFrameworkExecutor` that hands off to the assembly-fixture-aware runner. |
| `XunitTestAssemblyRunnerWithAssemblyFixture.cs` | Discovers `[AssemblyFixture]` attributes, instantiates fixtures once, stores them in `assemblyFixtureMappings`, and passes them to the collection runner. |
| `XunitTestCollectionRunnerWithAssemblyFixture.cs` | Collection runner that receives the fixture map and feeds matching instances into test class constructors. |

## For AI Agents

### Working in this directory
- These are tightly coupled to xUnit v2 internals; touching one runner usually means updating the other two.
- Fixture lifecycle: created in `AfterTestAssemblyStartingAsync`, disposed implicitly at assembly teardown — preserve `Aggregator.Run` error capture.
- Keep these classes `public` (they are activated by xUnit attribute reference from test projects).

### Common patterns
- Pass-through constructors mirroring base xUnit runners; only override the hooks that need fixture awareness.

## Dependencies
### External
- `Xunit.Sdk`, `Xunit.Abstractions` (xUnit v2).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
