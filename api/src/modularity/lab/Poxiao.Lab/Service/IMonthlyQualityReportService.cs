using Poxiao.Lab.Entity.Dto.MonthlyQualityReport;

namespace Poxiao.Lab.Service;

/// <summary>
/// 月度质量报表服务接口.
/// </summary>
public interface IMonthlyQualityReportService
{
    /// <summary>
    /// 获取完整的月度质量报表数据.
    /// </summary>
    /// <param name="query">查询条件</param>
    /// <returns>完整报表数据</returns>
    Task<MonthlyQualityReportResponseDto> GetReportAsync(MonthlyQualityReportQueryDto query);

    /// <summary>
    /// 获取顶部汇总指标.
    /// </summary>
    /// <param name="query">查询条件</param>
    /// <returns>汇总指标</returns>
    Task<MonthlyQualityReportSummaryDto> GetSummaryAsync(MonthlyQualityReportQueryDto query);

    /// <summary>
    /// 获取明细表格数据.
    /// </summary>
    /// <param name="query">查询条件</param>
    /// <returns>明细列表</returns>
    Task<List<MonthlyQualityReportDetailDto>> GetDetailsAsync(MonthlyQualityReportQueryDto query);

    /// <summary>
    /// 获取班组统计数据.
    /// </summary>
    /// <param name="query">查询条件</param>
    /// <returns>班组统计列表</returns>
    Task<List<MonthlyQualityReportShiftGroupDto>> GetShiftGroupsAsync(MonthlyQualityReportQueryDto query);

    /// <summary>
    /// 获取质量趋势图表数据.
    /// </summary>
    /// <param name="query">查询条件</param>
    /// <returns>趋势数据列表</returns>
    Task<List<QualityTrendDto>> GetQualityTrendAsync(MonthlyQualityReportQueryDto query);

    /// <summary>
    /// 获取不合格分类统计.
    /// </summary>
    /// <param name="query">查询条件</param>
    /// <returns>不合格分类列表</returns>
    Task<List<UnqualifiedCategoryDto>> GetUnqualifiedCategoriesAsync(MonthlyQualityReportQueryDto query);

    /// <summary>
    /// 获取班次对比数据.
    /// </summary>
    /// <param name="query">查询条件</param>
    /// <returns>班次对比列表</returns>
    Task<List<ShiftComparisonDto>> GetShiftComparisonAsync(MonthlyQualityReportQueryDto query);
}
