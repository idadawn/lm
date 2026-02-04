namespace Poxiao.Lab.Entity.Dto.Dashboard;

/// <summary>
/// 质量分布数据.
/// </summary>
public class QualityDistributionDto
{
    /// <summary>
    /// 质量类别（如 A级、B级、性能不合、其他不合）.
    /// </summary>
    public string Category { get; set; } = "";

    /// <summary>
    /// 数量.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// 重量.
    /// </summary>
    public decimal Weight { get; set; }

    /// <summary>
    /// 占比(%).
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// 显示颜色.
    /// </summary>
    public string? Color { get; set; }
}
