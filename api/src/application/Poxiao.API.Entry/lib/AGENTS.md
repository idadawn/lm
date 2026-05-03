<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# lib

## Purpose
Native interop binaries shipped with the API host. Currently contains a single vendored snowflake / worker-id library (`yitidgengo` v1.3.1) used to derive distributed worker IDs.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `regworkerid_lib_v1.3.1/` | Vendored Yitter / yitidgengo snowflake worker-id library binaries. See `regworkerid_lib_v1.3.1/AGENTS.md`. |

## For AI Agents

### Working in this directory
- These DLL/SO files are referenced as `<EmbeddedResource>` in `Poxiao.API.Entry.csproj` — when bumping the version, update the csproj `<None Remove>` and `<EmbeddedResource Include>` entries to match the new file paths.
- Do not check binaries into a different location; build expects them under `lib/regworkerid_lib_v1.3.1/`.
- This is the only directory in the host project containing native artefacts; keep other `.dll`/`.so` files out of the source tree.

## Dependencies
### Internal
- Loaded at runtime by Yitter / `IdGen` workers used inside the framework for distributed ID generation.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
