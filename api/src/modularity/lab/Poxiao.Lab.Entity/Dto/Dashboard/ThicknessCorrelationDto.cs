namespace Poxiao.Lab.Entity.Dto.Dashboard;

/// <summary>
/// 厚度-叠片系数关联数据
/// </summary>
public class ThicknessCorrelationDto
{
    /// <summary>
    /// 平均厚度
    /// </summary>
    public decimal Thickness { get; set; }

    /// <summary>
    /// 叠片系数
    /// </summary>
    public decimal LaminationFactor { get; set; }

    /// <summary>
    /// 质量等级
    /// </summary>
    public string QualityLevel { get; set; } = "";

    /// <summary>
    /// 记录ID
    /// </summary>
    public string Id { get; set; } = "";
}
