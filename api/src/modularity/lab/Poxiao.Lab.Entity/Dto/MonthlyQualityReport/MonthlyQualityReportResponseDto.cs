namespace Poxiao.Lab.Entity.Dto.MonthlyQualityReport;

/// <summary>
/// 月度质量报表响应
/// </summary>
public class MonthlyQualityReportResponseDto
{
    /// <summary>
    /// 汇总数据
    /// </summary>
    public MonthlyQualityReportSummaryDto Summary { get; set; }

    /// <summary>
    /// 明细数据
    /// </summary>
    public List<MonthlyQualityReportDetailDto> Details { get; set; }

    /// <summary>
    /// 班组统计
    /// </summary>
    public List<MonthlyQualityReportShiftGroupDto> ShiftGroups { get; set; }

    /// <summary>
    /// 质量趋势
    /// </summary>
    public List<QualityTrendDto> QualityTrends { get; set; }

    /// <summary>
    /// 不合格分类统计（图表用）
    /// </summary>
    public List<UnqualifiedCategoryDto> UnqualifiedCategoryStats { get; set; }

    /// <summary>
    /// 不合格分类统计（别名，与UnqualifiedCategoryStats相同）
    /// </summary>
    public List<UnqualifiedCategoryDto> UnqualifiedCategories { get; set; }

    /// <summary>
    /// 班次对比
    /// </summary>
    public List<ShiftComparisonDto> ShiftComparisons { get; set; }

    /// <summary>
    /// 合格等级列定义（A、B等，需要显示占比）
    /// </summary>
    public List<JudgmentLevelColumnDto> QualifiedColumns { get; set; }

    /// <summary>
    /// 不合格等级列定义（性能不合等，只显示重量）
    /// </summary>
    public List<JudgmentLevelColumnDto> UnqualifiedColumns { get; set; }

    /// <summary>
    /// 报表统计配置
    /// </summary>
    public List<ReportConfig.ReportConfigDto> ReportConfigs { get; set; }
}
