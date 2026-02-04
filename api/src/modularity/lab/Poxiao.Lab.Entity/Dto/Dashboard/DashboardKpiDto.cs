namespace Poxiao.Lab.Entity.Dto.Dashboard;

/// <summary>
/// 驾驶舱KPI数据.
/// </summary>
public class DashboardKpiDto
{
    /// <summary>
    /// 总重量.
    /// </summary>
    public decimal TotalWeight { get; set; }

    /// <summary>
    /// 合格率(%).
    /// </summary>
    public decimal QualifiedRate { get; set; }

    /// <summary>
    /// 平均叠片系数(%).
    /// </summary>
    public decimal LaminationFactorAvg { get; set; }

    /// <summary>
    /// 叠片系数趋势数据.
    /// </summary>
    public List<decimal> LaminationFactorTrend { get; set; } = new();

    /// <summary>
    /// 警告信息列表.
    /// </summary>
    public List<DashboardWarningDto> Warnings { get; set; } = new();
}

/// <summary>
/// 驾驶舱警告信息.
/// </summary>
public class DashboardWarningDto
{
    /// <summary>
    /// 类型: quality-质量, process-工艺, device-设备.
    /// </summary>
    public string Type { get; set; } = "";

    /// <summary>
    /// 警告消息.
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// 级别: info-信息, warning-警告, error-错误.
    /// </summary>
    public string Level { get; set; } = "";
}
