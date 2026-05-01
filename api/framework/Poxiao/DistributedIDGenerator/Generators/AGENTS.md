<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Generators

## Purpose
The pluggable ID-generator contract and its default implementation. `IDistributedIDGenerator.Create(options)` is the single primitive every concrete generator implements; `SequentialGuidIDGenerator` ships with the framework and produces RFC 4122 v4 GUIDs whose first 6 bytes encode the current ticks for monotonic insert order.

## Key Files
| File | Description |
|------|-------------|
| `IDistributedIDGenerator.cs` | Single-method interface `object Create(object idGeneratorOptions = default)`. |
| `SequentialGuidIDGenerator.cs` | Default `ISingleton` implementation using `RandomNumberGenerator` plus `DateTimeOffset.UtcNow.Ticks`. |

## For AI Agents

### Working in this directory
- Implementing a new generator: implement `IDistributedIDGenerator` plus a lifetime marker (typically `ISingleton`) and resolve via `App.GetService<IDistributedIDGenerator>()` or by named provider if multiple are registered.
- The `SequentialGuidIDGenerator` algorithm is taken from Pomelo.EFCore.MySql — preserve the inline citation comment when refactoring.
- Do not allocate per call: keep `RandomNumberGenerator` static.

### Common patterns
- `idGeneratorOptions` is `object` to keep the interface neutral; downcast to `SequentialGuidSettings` (see `../Settings/`) when relevant.

## Dependencies
### Internal
- `Poxiao.DependencyInjection` (`ISingleton`), `../Settings/SequentialGuidSettings`.
### External
- `System.Security.Cryptography`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
