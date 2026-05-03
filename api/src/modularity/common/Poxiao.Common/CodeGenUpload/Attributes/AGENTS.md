<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Attributes

## Purpose
Defines `[CodeGenUpload(...)]` — a property-level attribute placed on generated DTOs to record which visual-form control (`poxiaoKey`) produced the value, plus control-specific metadata (props, options, multiple, ableDepIds, modelId, interfaceId, …). The Excel-import / data-import pipeline uses these attributes to coerce uploaded cells back into the runtime shape the front-end expects.

## Key Files
| File | Description |
|------|-------------|
| `CodeGenUploadAttribute.cs` | Multi-overload `[AttributeUsage(AttributeTargets.Property)]` attribute. Each constructor maps to a control kind: `comInput`/`textarea`/`switch`/`radio`/`checkbox`/`popupSelect`/`relationForm`/`usersSelect`/`userSelect`/`address`/`treeSelect`/`depSelect`/`select`/`cascader`/`posSelect`/`popupTableSelect`. Stores parsed `CodeGenConfigModel` and selectable id whitelists (`ableDepIds`, `ablePosIds`, `ableUserIds`, `ableRoleIds`, `ableGroupIds`). |

## For AI Agents

### Working in this directory
- When adding a new control kind, add a new constructor whose first parameter is `Model` and whose `Config` JSON last argument deserialises into `CodeGenConfigModel`. Switch on `this.Config.poxiaoKey` to assign control-specific fields.
- Do not change existing constructor parameter orders — generated code already targets specific overloads.
- Property names use lowerCamelCase (`multiple`, `props`, `options`, `selectType`) so they round-trip with the Vue-side schema.

### Common patterns
- JSON arguments are decoded via the `string.ToObject<T>()` extension (Newtonsoft.Json wrapper).
- Control kinds are referenced via `Poxiao.Infrastructure.Const.PoxiaoKeyConst` constants — never hard-code their string values.

## Dependencies
### Internal
- `Poxiao.Infrastructure.Models` (CodeGenConfigModel, CodeGenPropsModel), `Poxiao.Infrastructure.Const.PoxiaoKeyConst`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
