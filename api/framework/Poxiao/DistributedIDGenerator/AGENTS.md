<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# DistributedIDGenerator

## Purpose
Distributed-friendly ID generation for Poxiao. Provides the static `IDGen` facade that resolves an `IDistributedIDGenerator` from DI (default: `SequentialGuidIDGenerator`, RFC 4122 v4 with monotonic timestamp prefix) and a separate short-ID utility under `ShortID/` for human-friendly identifiers (8–14 chars).

## Key Files
| File | Description |
|------|-------------|
| `IDGen.cs` | Static facade: `IDGen.NextID()` (Guid) and `IDGen.NextID(options, sp)` (object) for entity primary keys. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Enums/` | `SequentialGuidType` enum (see `Enums/AGENTS.md`). |
| `Generators/` | `IDistributedIDGenerator` and `SequentialGuidIDGenerator` (see `Generators/AGENTS.md`). |
| `Settings/` | `SequentialGuidSettings` (TimeNow override, endianness) (see `Settings/AGENTS.md`). |
| `ShortID/` | nanoid-style short ID generator (see `ShortID/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Entity PKs in `api/src/modularity/**` typically use `Guid` (see `.cursorrules`); call `IDGen.NextID()` rather than `Guid.NewGuid()` to keep MySQL clustered-index ordering favourable.
- `SequentialGuidIDGenerator` is registered as `ISingleton` — do not introduce per-request state.
- Use `ShortID.ShortIDGen` only for user-visible codes (invitation codes, share links), never as DB PK.

### Common patterns
- Code referenced from Pomelo EFCore MySql and `bolorundurowb/shortid` — keep attributions in `<para>` comments when editing.

## Dependencies
### Internal
- `Poxiao` core (`App`, `App.GetService`), `Poxiao.DependencyInjection` (`ISingleton`).
### External
- `System.Security.Cryptography` (`RandomNumberGenerator`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
