<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Enums

## Purpose
Enum types that configure sequential-GUID layout for `SequentialGuidIDGenerator`.

## Key Files
| File | Description |
|------|-------------|
| `SequentialGuidType.cs` | `SequentialAsString` / `SequentialAsBinary` / `SequentialAtEnd` — controls whether timestamp bytes go at the start or end and whether the byte layout matches binary or string sort order. |

## For AI Agents

### Working in this directory
- Database indexes assume a particular ordering; changing the default value would change cluster-key behaviour for existing tables — do not flip without a coordinated migration.
- Each enum value carries a Chinese `[Description]` attribute that surfaces in admin UIs.

### Common patterns
- Decorated `[SuppressSniffer]`.

## Dependencies
### External
- `System.ComponentModel`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
