namespace Poxiao.Lab.Entity.Dto.Dashboard;

/// <summary>
/// 叠片系数趋势数据.
/// </summary>
public class LaminationTrendDto
{
    /// <summary>
    /// 日期.
    /// </summary>
    public string Date { get; set; } = "";

    /// <summary>
    /// 平均值.
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// 最小值.
    /// </summary>
    public decimal Min { get; set; }

    /// <summary>
    /// 最大值.
    /// </summary>
    public decimal Max { get; set; }
}
