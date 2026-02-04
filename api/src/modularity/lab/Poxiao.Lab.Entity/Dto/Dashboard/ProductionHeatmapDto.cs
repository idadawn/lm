namespace Poxiao.Lab.Entity.Dto.Dashboard;

/// <summary>
/// 生产热力图数据.
/// </summary>
public class ProductionHeatmapDto
{
    /// <summary>
    /// 星期（0-6，周一到周日）.
    /// </summary>
    public int DayOfWeek { get; set; }

    /// <summary>
    /// 小时（0-23）.
    /// </summary>
    public int Hour { get; set; }

    /// <summary>
    /// 合格率(%).
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// 样本数量.
    /// </summary>
    public int Count { get; set; }
}
