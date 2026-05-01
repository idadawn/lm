<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Json

## Purpose
Custom Newtonsoft.Json converters for the platform. Currently provides an enum-by-name converter that respects `[EnumMember]` aliases on enums tagged with the marker `JsonUseEnumNameAttribute`.

## Key Files
| File | Description |
|------|-------------|
| `EnumUseNameConverter.cs` | `EnumUseNameConverter<TEnum>` (extends `StringEnumConverter`) + marker `JsonUseEnumNameAttribute` — serializes the enum value's name as a string and on read maps `[EnumMember(Value=…)]` aliases back to the enum, falling back to `Enum.TryParse` and finally `default(TEnum)`. |

## For AI Agents

### Working in this directory
- Mark a target enum with `[JsonUseEnumName]` to enable alias-aware deserialization; otherwise the converter still serializes by name but the alias dict stays empty.
- Static dict `EnumMembers` is populated once via `static` ctor — adding new aliases requires AppDomain restart.
- This converter coexists with `Enums/` `[Description]` attributes; `[Description]` is for UI labels, `[EnumMember]` is for wire format. Don't conflate them.
- Place additional converters here following the same single-file-per-converter convention.

### Common patterns
- Lives at the assembly root namespace (no `namespace` declaration in the file). Keep this only if all consumers reference it without using-statements.
- Failure mode is silent fallback to `default(TEnum)`.

## Dependencies
### External
- Newtonsoft.Json (`StringEnumConverter`, `JsonReader`/`JsonWriter`).
- `System.Runtime.Serialization` (`EnumMemberAttribute`).
### Internal
- Consumed by enums in `Enums/` and feature modules that opt in via `[JsonUseEnumName]`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
