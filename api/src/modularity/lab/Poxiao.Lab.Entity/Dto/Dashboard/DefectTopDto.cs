namespace Poxiao.Lab.Entity.Dto.Dashboard;

/// <summary>
/// 缺陷Top5数据.
/// </summary>
public class DefectTopDto
{
    /// <summary>
    /// 缺陷类别.
    /// </summary>
    public string Category { get; set; } = "";

    /// <summary>
    /// 出现次数.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// 影响重量.
    /// </summary>
    public decimal Weight { get; set; }
}
