using Poxiao.Lab.Entity.Enum;

namespace Poxiao.Lab.Entity.Dto.MonthlyQualityReport;

/// <summary>
/// 月度质量报表查询条件 DTO.
/// </summary>
public class MonthlyQualityReportQueryDto
{
    /// <summary>
    /// 开始日期.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// 结束日期.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// 班次筛选 (甲/乙/丙).
    /// </summary>
    public string Shift { get; set; }

    /// <summary>
    /// 炉号筛选.
    /// </summary>
    public string ShiftNo { get; set; }

    /// <summary>
    /// 产品规格/带宽筛选.
    /// </summary>
    public string ProductSpecCode { get; set; }

    /// <summary>
    /// 产线筛选.
    /// </summary>
    public int? LineNo { get; set; }
}

/// <summary>
/// 质量趋势图表数据 DTO.
/// </summary>
public class QualityTrendDto
{
    /// <summary>
    /// 日期.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// 合格率 (%).
    /// </summary>
    public decimal QualifiedRate { get; set; }

    /// <summary>
    /// A类占比 (%).
    /// </summary>
    public decimal ClassARate { get; set; }

    /// <summary>
    /// B类占比 (%).
    /// </summary>
    public decimal ClassBRate { get; set; }
}

/// <summary>
/// 不合格分类统计 DTO.
/// </summary>
public class UnqualifiedCategoryDto
{
    /// <summary>
    /// 分类名称 (判定等级名称).
    /// </summary>
    public string CategoryName { get; set; }

    /// <summary>
    /// 重量 (kg).
    /// </summary>
    public decimal Weight { get; set; }

    /// <summary>
    /// 占比 (%).
    /// </summary>
    public decimal Rate { get; set; }
}

/// <summary>
/// 班次对比统计 DTO.
/// </summary>
public class ShiftComparisonDto
{
    /// <summary>
    /// 班次名称 (甲/乙/丙).
    /// </summary>
    public string Shift { get; set; }

    /// <summary>
    /// 总产量 (kg).
    /// </summary>
    public decimal TotalWeight { get; set; }

    /// <summary>
    /// 合格率 (%).
    /// </summary>
    public decimal QualifiedRate { get; set; }

    /// <summary>
    /// A类占比 (%).
    /// </summary>
    public decimal ClassARate { get; set; }

    /// <summary>
    /// B类占比 (%).
    /// </summary>
    public decimal ClassBRate { get; set; }
}

/// <summary>
/// 判定等级列配置 DTO.
/// </summary>
public class MonthlyQualityReportColumnsDto
{
    /// <summary>
    /// 合格等级列 (A级, B级等).
    /// </summary>
    public List<JudgmentLevelColumnDto> QualifiedColumns { get; set; } = new();

    /// <summary>
    /// 不合格等级列.
    /// </summary>
    public List<JudgmentLevelColumnDto> UnqualifiedColumns { get; set; } = new();
}
