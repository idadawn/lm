namespace Poxiao.Lab.Entity.Dto.Dashboard;

/// <summary>
/// 生产热力图数据
/// </summary>
public class ProductionHeatmapDto
{
    /// <summary>
    /// 星期（0=周一，6=周日）
    /// </summary>
    public int DayOfWeek { get; set; }

    /// <summary>
    /// 小时（0-23）
    /// </summary>
    public int Hour { get; set; }

    /// <summary>
    /// 合格率或叠片系数
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// 样本数量
    /// </summary>
    public int Count { get; set; }
}