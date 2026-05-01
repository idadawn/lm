<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Monitor

## Purpose
DTO(s) for the "系统监控" admin page — surfaces server/system, CPU, memory, and disk metrics together with a server-side timestamp.

## Key Files
| File | Description |
|------|-------------|
| `MonitorOutput.cs` | Aggregate output — `system: SystemInfoModel`, `cpu: CpuInfoModel`, `memory: MemoryInfoModel`, `disk: DiskInfoModel`, `time: DateTime?`. The four nested models come from `Poxiao.Infrastructure.Model.Machine`. |

## For AI Agents

### Working in this directory
- The actual machine inspection lives in `Poxiao.Infrastructure.Model.Machine.*`; this DTO is purely a transport wrapper. Don't move metric collection here.
- Single-snapshot shape — extending to time-series would require a new DTO, not adding lists to `MonitorOutput`.
- Namespace `Poxiao.Systems.Entitys.Dto.Monitor`. `[SuppressSniffer]` is applied.
- `time` is nullable to allow controllers to omit the timestamp on partial errors.

### Common patterns
- Single output DTO per directory when the feature is read-only.

## Dependencies
### Internal
- `Poxiao.Infrastructure.Model.Machine` (system/cpu/memory/disk models).

### External
- `Poxiao.DependencyInjection` (`[SuppressSniffer]`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
