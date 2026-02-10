using Poxiao.Lab.Entity.Dto.Dashboard;

namespace Poxiao.Lab.Service;

/// <summary>
/// 驾驶舱服务接口.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// 获取KPI数据.
    /// </summary>
    Task<DashboardKpiDto> GetKpiAsync(DashboardQueryDto query);

    /// <summary>
    /// 获取质量分布数据.
    /// </summary>
    Task<List<QualityDistributionDto>> GetQualityDistributionAsync(DashboardQueryDto query);

    /// <summary>
    /// 获取叠片系数趋势数据.
    /// </summary>
    Task<List<LaminationTrendDto>> GetLaminationTrendAsync(DashboardQueryDto query);

    /// <summary>
    /// 获取缺陷Top5数据.
    /// </summary>
    Task<List<DefectTopDto>> GetDefectTop5Async(DashboardQueryDto query);

    /// <summary>
    /// 获取生产热力图数据.
    /// </summary>
    Task<List<ProductionHeatmapDto>> GetProductionHeatmapAsync(DashboardQueryDto query);

    /// <summary>
    /// 获取厚度-叠片系数关联数据.
    /// </summary>
    Task<List<ThicknessCorrelationDto>> GetThicknessCorrelationAsync(DashboardQueryDto query);

    /// <summary>
    /// 获取今日产量数据（与昨日对比）.
    /// </summary>
    Task<DailyProductionDto> GetDailyProductionAsync();
}
