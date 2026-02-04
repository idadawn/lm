namespace Poxiao.Lab.Entity.Dto.Dashboard;

/// <summary>
/// 缺陷Top5数据
/// </summary>
public class DefectTopDto
{
    /// <summary>
    /// 缺陷类别（如：麻点、划痕、毛边、亮线、网眼等）
    /// </summary>
    public string Category { get; set; } = "";

    /// <summary>
    /// 出现次数
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// 重量
    /// </summary>
    public decimal Weight { get; set; }
}
