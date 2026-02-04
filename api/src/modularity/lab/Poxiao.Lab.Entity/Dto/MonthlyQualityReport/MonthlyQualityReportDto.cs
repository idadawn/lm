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
/// 月度质量报表顶部汇总指标 DTO.
/// </summary>
public class MonthlyQualityReportSummaryDto
{
    /// <summary>
    /// 检验总重 (kg).
    /// </summary>
    public decimal TotalWeight { get; set; }

    /// <summary>
    /// 合格率 (%).
    /// </summary>
    public decimal QualifiedRate { get; set; }

    /// <summary>
    /// A类总计 (kg).
    /// </summary>
    public decimal ClassAWeight { get; set; }

    /// <summary>
    /// A类占比 (%).
    /// </summary>
    public decimal ClassARate { get; set; }

    /// <summary>
    /// B类总计 (kg).
    /// </summary>
    public decimal ClassBWeight { get; set; }

    /// <summary>
    /// B类占比 (%).
    /// </summary>
    public decimal ClassBRate { get; set; }

    /// <summary>
    /// 不合格总计 (kg).
    /// </summary>
    public decimal UnqualifiedWeight { get; set; }

    /// <summary>
    /// 不合格占比 (%).
    /// </summary>
    public decimal UnqualifiedRate { get; set; }
}

/// <summary>
/// 月度质量报表明细行 DTO (左侧表格).
/// </summary>
public class MonthlyQualityReportDetailDto
{
    /// <summary>
    /// 生产日期.
    /// </summary>
    public DateTime? ProdDate { get; set; }

    /// <summary>
    /// 班次 (甲/乙/丙).
    /// </summary>
    public string Shift { get; set; }

    /// <summary>
    /// 炉号.
    /// </summary>
    public string ShiftNo { get; set; }

    /// <summary>
    /// 带宽/产品规格.
    /// </summary>
    public string ProductSpecCode { get; set; }

    /// <summary>
    /// 检测量 (kg).
    /// </summary>
    public decimal DetectionWeight { get; set; }

    /// <summary>
    /// A类重量 (kg).
    /// </summary>
    public decimal ClassAWeight { get; set; }

    /// <summary>
    /// A类占比 (%).
    /// </summary>
    public decimal ClassARate { get; set; }

    /// <summary>
    /// B类重量 (kg).
    /// </summary>
    public decimal ClassBWeight { get; set; }

    /// <summary>
    /// B类占比 (%).
    /// </summary>
    public decimal ClassBRate { get; set; }

    /// <summary>
    /// 不合格重量 (kg).
    /// </summary>
    public decimal UnqualifiedWeight { get; set; }

    /// <summary>
    /// 合格率 (%).
    /// </summary>
    public decimal QualifiedRate { get; set; }

    /// <summary>
    /// 不合格分类明细 (等级名称 -> 重量).
    /// </summary>
    public Dictionary<string, decimal> UnqualifiedCategories { get; set; } = new();

    /// <summary>
    /// 是否为汇总行 (班次小计/月度合计).
    /// </summary>
    public bool IsSummaryRow { get; set; } = false;

    /// <summary>
    /// 汇总行类型 (ShiftSubtotal=班次小计, MonthlyTotal=月度合计).
    /// </summary>
    public string SummaryType { get; set; }
}

/// <summary>
/// 月度质量报表班组统计 DTO (右侧表格).
/// </summary>
public class MonthlyQualityReportShiftGroupDto
{
    /// <summary>
    /// 班次 (甲/乙/丙).
    /// </summary>
    public string Shift { get; set; }

    /// <summary>
    /// 带宽/产品规格.
    /// </summary>
    public string ProductSpecCode { get; set; }

    /// <summary>
    /// 检测量 (kg).
    /// </summary>
    public decimal DetectionWeight { get; set; }

    /// <summary>
    /// A类重量 (kg).
    /// </summary>
    public decimal ClassAWeight { get; set; }

    /// <summary>
    /// A类占比 (%).
    /// </summary>
    public decimal ClassARate { get; set; }

    /// <summary>
    /// B类重量 (kg).
    /// </summary>
    public decimal ClassBWeight { get; set; }

    /// <summary>
    /// B类占比 (%).
    /// </summary>
    public decimal ClassBRate { get; set; }

    /// <summary>
    /// 不合格重量 (kg).
    /// </summary>
    public decimal UnqualifiedWeight { get; set; }

    /// <summary>
    /// 合格率 (%).
    /// </summary>
    public decimal QualifiedRate { get; set; }

    /// <summary>
    /// 是否为汇总行 (班次小计/月度合计).
    /// </summary>
    public bool IsSummaryRow { get; set; } = false;

    /// <summary>
    /// 汇总行类型 (ShiftSubtotal=班次小计, MonthlyTotal=月度合计).
    /// </summary>
    public string SummaryType { get; set; }
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
}

/// <summary>
/// 判定等级统计列信息 DTO.
/// </summary>
public class JudgmentLevelColumnDto
{
    /// <summary>
    /// 等级ID.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 等级代码.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 等级名称.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 质量状态.
    /// </summary>
    public QualityStatusEnum QualityStatus { get; set; }

    /// <summary>
    /// 展示颜色.
    /// </summary>
    public string Color { get; set; }

    /// <summary>
    /// 判定权重 (优先级).
    /// </summary>
    public int Priority { get; set; }
}

/// <summary>
/// 月度质量报表完整响应 DTO.
/// </summary>
public class MonthlyQualityReportResponseDto
{
    /// <summary>
    /// 顶部汇总指标.
    /// </summary>
    public MonthlyQualityReportSummaryDto Summary { get; set; }

    /// <summary>
    /// 明细表格数据.
    /// </summary>
    public List<MonthlyQualityReportDetailDto> Details { get; set; } = new();

    /// <summary>
    /// 班组统计数据.
    /// </summary>
    public List<MonthlyQualityReportShiftGroupDto> ShiftGroups { get; set; } = new();

    /// <summary>
    /// 质量趋势数据.
    /// </summary>
    public List<QualityTrendDto> QualityTrends { get; set; } = new();

    /// <summary>
    /// 不合格分类统计.
    /// </summary>
    public List<UnqualifiedCategoryDto> UnqualifiedCategories { get; set; } = new();

    /// <summary>
    /// 班次对比数据.
    /// </summary>
    public List<ShiftComparisonDto> ShiftComparisons { get; set; } = new();

    /// <summary>
    /// 需要展示的不合格分类列 (IsStatistic=true 且 QualityStatus=Unqualified 的判定等级).
    /// </summary>
    public List<JudgmentLevelColumnDto> UnqualifiedColumns { get; set; } = new();
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
