<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# PrintDev

## Purpose
DTOs for "打印模板" — print-template definition / preview / data-fetch flow. A print template binds a SQL data-source (`dbLinkId` + `sqlTemplate`) to an HTML/Hiprint template, with optional left-side label fields and paper parameters; data can then be retrieved by record id(s) for actual printing.

## Key Files
| File | Description |
|------|-------------|
| `PrintDevCrInput.cs` | Create — `fullName`, `enCode`, `category`, `enabledMark`, `type` (1:流程表单 / 2:功能表单), `description`, `sortCode`, `dbLinkId`, `sqlTemplate`, `leftFields`, `printTemplate`, `pageParam`. |
| `PrintDevUpInput.cs` | Update payload (adds `id`). |
| `PrintDevInfoOutput.cs` | Detail / edit-form output. |
| `PrintDevListInput.cs` / `PrintDevListOutput.cs` | Grid query input + projection. |
| `PrintDevSelectorOutput.cs` | Lightweight `{id, fullName, enCode}` projection for dropdown selectors. |
| `PrintDevDataOutput.cs` | Wrapper for the resolved data rows used during actual print render. |
| `PrintDevFieldsQuery.cs` / `PrintDevFieldsOutput.cs` | "Fields" auxiliary endpoint — query & output of available column metadata for a template. |
| `PrintDevSqlDataQuery.cs` | Data-fetch input — `id` (template), `formId` (single record), `ids` (batch). |

## For AI Agents

### Working in this directory
- `type` is an integer (1/2) for "流程表单" vs "功能表单"; preserve as `int?` to allow null on partial updates.
- `sqlTemplate` is a stored SQL string with `{paramName}` style placeholders resolved at render time — be cautious about validating / encoding.
- `dbLinkId` references a row in the data-source-link module; do not embed connection details here.
- Namespace `Poxiao.Systems.Entitys.Dto.PrintDev` (no `.System.` segment).
- `[SuppressSniffer]` applied to all DTOs.

### Common patterns
- Cr/Up/Info/List/Selector/Output suffix; auxiliary "fields" / "data" / "sql-data" use Query/Output suffix.
- Heavy template strings (`sqlTemplate`, `printTemplate`, `pageParam`) carried as plain strings.

## Dependencies
### Internal
- Consumed by `modularity/system/Poxiao.Systems` print-template service & controllers; integrates with `PrintLog` DTOs (sibling directory).

### External
- `Poxiao.DependencyInjection` (`[SuppressSniffer]`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
