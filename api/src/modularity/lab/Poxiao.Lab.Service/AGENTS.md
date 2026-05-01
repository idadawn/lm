<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Lab.Service

## Purpose
独立放置的服务接口"零碎"目录，目前仅承载 **驾驶舱（Dashboard）服务接口**。它与 `Poxiao.Lab.Interfaces` 是平行存在的历史结构——驾驶舱接口因为引入时机较晚，被单独建在这里而未合并回 `Poxiao.Lab.Interfaces`。

## Key Files
| File | Description |
|------|-------------|
| `IDashboardService.cs` | `IDashboardService`：`GetKpiAsync/GetQualityDistributionAsync/GetLaminationTrendAsync/GetDefectTop5Async/GetProductionHeatmapAsync/GetThicknessCorrelationAsync/GetDailyProductionAsync`，全部接受 `DashboardQueryDto`。 |

## For AI Agents

### Working in this directory
- **小心重复定义**：`Poxiao.Lab/Service/IDashboardService.cs` 中存在另一份**几乎一致**的接口副本（命名空间同为 `Poxiao.Lab.Service`）。两者都被 `DashboardService.cs` 实现引用——任何方法变更必须同步修改两份文件，否则编译会断裂或 DI 解析到错误的契约。
- 该项目除接口外不放实现；DTO 已位于 `../Poxiao.Lab.Entity/Dto/Dashboard/`。
- 新增驾驶舱 API 时按现有模式：`(DashboardQueryDto query) → Task<TDto>` 或 `Task<List<TDto>>`，避免无参方法（`GetDailyProductionAsync` 是固定"今日 vs 昨日"语义所以无参）。

### Common patterns
- 全部异步、全部返回 DTO（不返回实体），保证 Controller 端可直接序列化。

## Dependencies
### Internal
- `../Poxiao.Lab.Entity/Dto/Dashboard/`。

### External
- 无（纯接口项目；csproj 暂未列出，构建系统通过解决方案配置整体编译）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
