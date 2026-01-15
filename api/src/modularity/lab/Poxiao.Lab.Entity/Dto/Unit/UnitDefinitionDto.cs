namespace Poxiao.Lab.Entity.Dto.Unit;

/// <summary>
/// 单位定义 DTO.
/// </summary>
public class UnitDefinitionDto
{
    /// <summary>
    /// 主键 ID.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 关联维度 ID.
    /// </summary>
    public string CategoryId { get; set; }

    /// <summary>
    /// 单位全称.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 单位符号.
    /// </summary>
    public string Symbol { get; set; }

    /// <summary>
    /// 是否为基准单位.
    /// </summary>
    public bool IsBase { get; set; }

    /// <summary>
    /// 换算至基准单位的比例系数.
    /// </summary>
    public decimal ScaleToBase { get; set; }

    /// <summary>
    /// 换算偏移量.
    /// </summary>
    public decimal Offset { get; set; }

    /// <summary>
    /// 显示精度（小数位数）.
    /// </summary>
    public int Precision { get; set; }

    /// <summary>
    /// 显示名称（格式：单位全称 (单位符号)）.
    /// </summary>
    public string DisplayName => $"{Name} ({Symbol})";
}
