namespace Poxiao.Lab.Entity.Dto.Dashboard;

/// <summary>
/// 驾驶舱KPI数据
/// </summary>
public class DashboardKpiDto
{
    /// <summary>
    /// 检验总重
    /// </summary>
    public decimal TotalWeight { get; set; }

    /// <summary>
    /// 合格率
    /// </summary>
    public decimal QualifiedRate { get; set; }

    /// <summary>
    /// 叠片系数均值
    /// </summary>
    public decimal LaminationFactorAvg { get; set; }

    /// <summary>
    /// 叠片系数趋势（近7天）
    /// </summary>
    public List<decimal> LaminationFactorTrend { get; set; } = new();

    /// <summary>
    /// 预警信息
    /// </summary>
    public List<DashboardWarningDto> Warnings { get; set; } = new();
}

/// <summary>
/// 预警信息
/// </summary>
public class DashboardWarningDto
{
    /// <summary>
    /// 预警类型
    /// </summary>
    public string Type { get; set; } = "info";

    /// <summary>
    /// 预警消息
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// 预警级别
    /// </summary>
    public string Level { get; set; } = "info";
}
