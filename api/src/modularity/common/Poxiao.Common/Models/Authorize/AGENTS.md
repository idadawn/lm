<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Authorize

## Purpose
数据权限 (data-permission) condition models — describe row-level filters attached to authorization rules. Used by the data-rule engine and code-gen authorize helper to translate a user/role's "data scope" definition into SqlSugar `IConditionalModel` filters at query time.

## Key Files
| File | Description |
|------|-------------|
| `AuthorizeModuleResourceConditionItemModel.cs` | One condition item — `Field`、`Type`、`Op`、`Value`、`BindTable`、`FieldRule` (0=主表 / 1=副表). PascalCase variant for backend; the file also defines `…ConditionItemModelInput` camelCase variant for incoming JSON. |
| `AuthorizeModuleResourceConditionModel.cs` | Group container holding multiple condition items + match logic. |
| `CodeGenAuthorizeModuleResourceModel.cs` | Code-gen-side resource: `FieldRule`、`TableName`、`conditionalModel` (`List<IConditionalModel>`). Also defines `CodeGenAuthorizeModuleResource` (raw `List<object>` form) and `CodeGenDataRuleModuleResourceModel` (adds `UserOrigin` pc/app + serialized JSON). |

## For AI Agents

### Working in this directory
- Namespace `Poxiao.Infrastructure.Models.Authorize`.
- Two casings on purpose: PascalCase classes feed back-end logic; the `*Input` companions are camelCase for JSON wire compatibility — keep them in sync when fields change.
- `FieldRule` legend: `0` = 主表 (main entity), `1` = 副表 (joined), `2` = 子表 (child).
- `IConditionalModel` is from SqlSugar; the "raw" `List<object>` shape exists because some incoming JSON cannot be directly deserialized into the interface — preserve both shapes.

### Common patterns
- Pair of "domain" + "input" model per concept (PascalCase / camelCase).
- `[SuppressSniffer]` not always present here — if adding new types, add it for consistency.

## Dependencies
### Internal
- Consumed by `Security/CodeGenAuthorizeHelper.cs` and feature-module data-rule services.
### External
- SqlSugar (`IConditionalModel`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
