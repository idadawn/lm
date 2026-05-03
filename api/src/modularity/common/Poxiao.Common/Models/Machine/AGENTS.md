<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Machine

## Purpose
Host/server hardware diagnostic models — CPU、内存、磁盘、操作系统信息 read by `Security/MachineHelper.cs` and surfaced through the system-status admin page.

## Key Files
| File | Description |
|------|-------------|
| `CpuInfoModel.cs` | CPU info — `name`、`package` (物理CPU个数)、`core`/`coreNumber`、`logic`、`used`、`idle`. |
| `MemoryInfoModel.cs` | Memory info — `total`、`available`、`used`、`usageRate`. |
| `DiskInfoModel.cs` | Disk volume info. |
| `SystemInfoModel.cs` | OS-level info (name、version、uptime). |

## For AI Agents

### Working in this directory
- Namespace `Poxiao.Infrastructure.Model.Machine` (note **singular** `Model` — different from the parent `Models/` folder; this is intentional and must be kept).
- All numeric "used"/"available"/"total" fields are `string` (already formatted with units like `"8 GB"`) rather than `long` bytes — keep the type to preserve the existing UI contract.
- `[SuppressSniffer]` on every class.
- These are pure read-only POCOs populated via `MachineHelper`/`Hardware.Info` library — don't put parsing logic here.

### Common patterns
- camelCase props with Chinese XML comments.
- Each metric is a flat record; no nesting between CPU/Memory/Disk.

## Dependencies
### Internal
- Populated by `Security/MachineHelper.cs`.
### External
- (Indirect) `Hardware.Info` / `System.Management` via the helper.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
