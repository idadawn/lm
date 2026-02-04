namespace Poxiao.Lab.Entity.Dto.MonthlyQualityReport;

/// <summary>
/// 班组统计行
/// </summary>
public class MonthlyQualityReportShiftGroupDto
{
    public string Shift { get; set; }
    public string ProductSpecCode { get; set; }
    public decimal DetectionWeight { get; set; }

    /// <summary>
    /// 合格分类统计（动态）
    /// </summary>
    public Dictionary<string, LevelStatDto> QualifiedCategories { get; set; } = new();

    /// <summary>
    /// 合格合计重量
    /// </summary>
    public decimal QualifiedWeight { get; set; }

    /// <summary>
    /// 合格率
    /// </summary>
    public decimal QualifiedRate { get; set; }

    public bool IsSummaryRow { get; set; }
    public string SummaryType { get; set; }

    /// <summary>
    /// 不合格重量（kg）
    /// </summary>
    public decimal UnqualifiedWeight { get; set; }

    /// <summary>
    /// A类重量（kg）
    /// </summary>
    public decimal ClassAWeight { get; set; }

    /// <summary>
    /// A类占比（%）
    /// </summary>
    public decimal ClassARate { get; set; }

    /// <summary>
    /// B类重量（kg）
    /// </summary>
    public decimal ClassBWeight { get; set; }

    /// <summary>
    /// B类占比（%）
    /// </summary>
    public decimal ClassBRate { get; set; }
}
