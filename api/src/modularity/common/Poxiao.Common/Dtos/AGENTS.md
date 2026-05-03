<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Dtos

## Purpose
Cross-module shared transport DTOs used by the entire `Poxiao.Common` infrastructure layer — generic data import I/O, dynamic interface request parameter descriptors, and shared third-party (DingTalk、WeChat) connection-parameter shapes. Anything reused by multiple feature modules lives here rather than in module-specific Dto folders.

## Key Files
| File | Description |
|------|-------------|
| `DataImportInput.cs` | Generic import payload — `List<Dictionary<string,object>> list`. |
| `DataImportOutput.cs` | Import result — success/fail counts、`failResult` rows、`resultType`. |
| `DataInterfaceReqParameterInfo.cs` | Dynamic data-interface field descriptor (`field`、`dataType`、`required`、`defaultValue`、`fieldName`). Reused by VisualDev and DataInterface. |
| `DingParameterInfo.cs` | DingTalk connection params (`dingAgentId`、`dingSynAppKey`、`dingSynAppSecret`). |
| `WeChatParameterInfo.cs` | 微信开放平台 connection params. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `DataBase/` | DB schema introspection DTOs (see `DataBase/AGENTS.md`). |
| `Message/` | Message template send-model DTOs (see `Message/AGENTS.md`). |
| `OAuth/` | Multi-tenant OAuth DTOs (see `OAuth/AGENTS.md`). |
| `VisualDev/` | Visual-development form/dialog DTOs (see `VisualDev/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Namespace is `Poxiao.Infrastructure.Dtos` (legacy "Infrastructure" name retained even though the assembly is `Poxiao.Common`). Don't rename.
- Decorate with `[SuppressSniffer]` from `Poxiao.DependencyInjection` to opt out of DI scanning.
- camelCase property names — these objects flow directly to JSON.
- Keep these DTOs feature-agnostic; module-specific shapes belong in their own module's `Dto/`.

### Common patterns
- Dictionary-of-string-to-object for dynamic row payloads (`DataImportInput`, `failResult`).
- One concept per file.

## Dependencies
### Internal
- `Poxiao.DependencyInjection` — `[SuppressSniffer]`.
- Sibling `Filter/` (`PageInputBase`) is consumed by paged sub-DTOs.
### External
- None.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
