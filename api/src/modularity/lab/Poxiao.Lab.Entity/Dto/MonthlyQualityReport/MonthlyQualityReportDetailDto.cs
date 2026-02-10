namespace Poxiao.Lab.Entity.Dto.MonthlyQualityReport;

/// <summary>
/// 月度质量报表明细行
/// </summary>
public class MonthlyQualityReportDetailDto
{
    /// <summary>
    /// 生产日期
    /// </summary>
    public DateTime? ProdDate { get; set; }

    /// <summary>
    /// 班次
    /// </summary>
    public string Shift { get; set; }

    /// <summary>
    /// 炉号
    /// </summary>
    public string ShiftNo { get; set; }

    /// <summary>
    /// 产品规格/带宽
    /// </summary>
    public string ProductSpecCode { get; set; }

    /// <summary>
    /// 检测量（kg）
    /// </summary>
    public decimal DetectionWeight { get; set; }

    /// <summary>
    /// 合格分类统计（动态）
    /// Key: 等级名称（如 "A", "B"）
    /// Value: { Weight: 重量, Rate: 占比 }
    /// </summary>
    public Dictionary<string, LevelStatDto> QualifiedCategories { get; set; } = new();

    /// <summary>
    /// 合格合计重量（kg）
    /// </summary>
    public decimal QualifiedWeight { get; set; }

    /// <summary>
    /// 合格率（%）
    /// </summary>
    public decimal QualifiedRate { get; set; }

    /// <summary>
    /// 不合格分类统计（动态）
    /// Key: 等级名称（如 "性能不合", "极差不合"）
    /// Value: 重量（kg）
    /// </summary>
    public Dictionary<string, decimal> UnqualifiedCategories { get; set; } = new();

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

    /// <summary>
    /// 是否为汇总行
    /// </summary>
    public bool IsSummaryRow { get; set; }

    /// <summary>
    /// 汇总类型（ShiftSubtotal=班次小计, MonthlyTotal=月度合计）
    /// </summary>
    public string SummaryType { get; set; }

    /// <summary>
    /// 动态统计列
    /// Key: 统计配置ID
    /// Value: { Weight: 重量, Rate: 占比 }
    /// </summary>
    public Dictionary<string, LevelStatDto> DynamicStats { get; set; } = new();
}
