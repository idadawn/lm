<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# SynThirdInfo

## Purpose
DTO holding statistics returned by the 第三方同步信息 endpoint — summarizes results of syncing organization/user data from external IdP/IM providers (DingTalk、企业微信). Single output DTO consumed by the system management UI's sync status card.

## Key Files
| File | Description |
|------|-------------|
| `SynThirdInfoOutput.cs` | Aggregate counters: `recordTotal`、`synDate`、`synSuccessCount`、`synFailCount`、`unSynCount`、`synType`. |

## For AI Agents

### Working in this directory
- This directory currently contains only an output DTO; if a query input is later required, add `SynThirdInfoInput.cs` mirroring sibling DTO conventions.
- camelCase property names; `[SuppressSniffer]` attribute required.
- `synDate` is nullable `DateTime?` — preserve nullability when extending.

### Common patterns
- Single-class folder (one DTO per concept).
- Lives under `Dto/System/` and uses `Poxiao.Systems.Entitys.Dto.SynThirdInfo` namespace (folder-name-as-namespace).

## Dependencies
### Internal
- `Poxiao.DependencyInjection` — `[SuppressSniffer]`.
- Consumed by sync orchestration services in `Poxiao.Systems` (DingTalk / 企业微信 sync).
### External
- None.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
