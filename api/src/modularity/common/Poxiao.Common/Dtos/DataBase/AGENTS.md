<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# DataBase

## Purpose
Database introspection DTOs — return shapes describing tables and their fields, used by the dynamic data-interface, code-generation and visual-development tools when previewing or binding to arbitrary database schemas (SQL Server / MySQL / Oracle).

## Key Files
| File | Description |
|------|-------------|
| `TableInfoOutput.cs` | Single-table metadata (name, comment, table type, etc.). |
| `TableFieldOutput.cs` | Field/column metadata — `field`、`fieldName`、`dataType`、`dataLength`、`primaryKey` (0/1)、`allowNull`. |
| `DatabaseTableInfoOutput.cs` | Aggregate: `tableInfo` + `tableFieldList` + `hasTableData` flag. |

## For AI Agents

### Working in this directory
- Namespace `Poxiao.Infrastructure.Dtos.DataBase` (note casing: 'B' capital).
- `primaryKey` and `allowNull` are `int` 0/1 to stay consistent with the rest of the platform's frontend contract — do not change to `bool`.
- Field names mirror SqlSugar `DbColumnInfo` shape so service code can `Adapt<>` from it.
- `[SuppressSniffer]` required on all classes.

### Common patterns
- Composition over inheritance: `DatabaseTableInfoOutput` aggregates the other two.
- Multi-DB compatibility: descriptions stay generic enough to fit MySQL/SQL Server/Oracle column metadata.

## Dependencies
### Internal
- `Poxiao.DependencyInjection` — `[SuppressSniffer]`.
- Consumed by `Manager/DataBase/` services and the VisualDev/CodeGen modules.
### External
- Implicit alignment with SqlSugar DBO models.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
