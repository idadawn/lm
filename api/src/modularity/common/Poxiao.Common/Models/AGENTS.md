<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Models

## Purpose
Cross-module shared domain models — non-entity, non-DTO POCOs reused across feature modules. Covers attachments, chunks, file controls, import field metadata, schedule tasks, social-login config, static data, super-query rules and Oracle parameter shapes.

## Key Files
| File | Description |
|------|-------------|
| `AnnexModel.cs` | 附件 model (`FileId`、`FileName`、`FileSize`、`FileTime`、`FileState`、`FileType`). |
| `ChunkModel.cs` | Resumable upload chunk metadata. |
| `ImportFieldsModel.cs` | Field mapping metadata for batch imports (~8 KB). |
| `SuperQueryModel.cs` | High-level dynamic query — `matchLogic` + `conditionJson[]`; `ConvertSuperQuery` is the SqlSugar-bound translation. |
| `DataModelFormat.cs` | Field formatting hints. |
| `FileControlsModel.cs` | File-control bindings. |
| `SocialsLoginConfigModel.cs` | Per-provider social login config (WeChat, DingTalk, …). |
| `OracleParamModel.cs` | Oracle bind-parameter shape. |
| `ScheduleTaskModel.cs`, `SelectorModel.cs`, `StaticDataModel.cs`, `ControlLinkageParameterModel.cs` | Smaller domain helpers. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Authorize/` | 数据权限/资源条件 models (see `Authorize/AGENTS.md`). |
| `Machine/` | CPU/Memory/Disk/System info models (see `Machine/AGENTS.md`). |
| `NPOI/` | Excel export config + column models (see `NPOI/AGENTS.md`). |
| `User/` | 登录用户/在线用户/数据范围 models (see `User/AGENTS.md`). |
| `VisualDev/` | 可视化开发 query models (see `VisualDev/AGENTS.md`). |
| `WorkFlow/` | 工作流表单/JSON/任务模型 (see `WorkFlow/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Namespace `Poxiao.Infrastructure.Models` (note plural).
- All classes use `[SuppressSniffer]`. Mix of PascalCase (legacy `AnnexModel`) and camelCase (newer ones like `SuperQueryModel`) properties — match the file you're editing.
- These are reusable runtime POCOs; do not add SqlSugar `[SugarColumn]` attributes here (those belong on entities under `Entity/` of feature modules).
- `SuperQueryModel` ↔ `ConvertSuperQuery` pairs map a UI-built query to a SqlSugar `IConditionalModel`; modify both halves together.

### Common patterns
- Composition via nested classes (`Conditionjson`, `ConvertSuperQuery` defined alongside `SuperQueryModel`).
- Chinese XML doc comments and version banners on legacy models (`版 本：V3.3.3`).

## Dependencies
### Internal
- `Poxiao.DependencyInjection` — `[SuppressSniffer]`.
- `Const/` for shared `WhereType`/`ConditionalType` enums.
### External
- SqlSugar conditional models (interface only).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
