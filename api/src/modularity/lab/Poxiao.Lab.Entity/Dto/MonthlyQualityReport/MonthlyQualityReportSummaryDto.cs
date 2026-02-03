namespace Poxiao.Lab.Entity.Dto.MonthlyQualityReport;

/// <summary>
/// 月度质量报表汇总
/// </summary>
public class MonthlyQualityReportSummaryDto
{
    /// <summary>
    /// 检测总重（kg）
    /// </summary>
    public decimal TotalWeight { get; set; }

    /// <summary>
    /// 合格率（%）
    /// </summary>
    public decimal QualifiedRate { get; set; }

    /// <summary>
    /// 合格分类统计（动态）
    /// </summary>
    public Dictionary<string, LevelStatDto> QualifiedCategories { get; set; } = new();

    /// <summary>
    /// 合格合计重量（kg）
    /// </summary>
    public decimal QualifiedWeight { get; set; }

    /// <summary>
    /// 不合格分类统计（动态）
    /// </summary>
    public Dictionary<string, decimal> UnqualifiedCategories { get; set; } = new();

    /// <summary>
    /// 不合格合计重量（kg）
    /// </summary>
    public decimal UnqualifiedWeight { get; set; }

    /// <summary>
    /// 不合格率（%）
    /// </summary>
    public decimal UnqualifiedRate { get; set; }
}
