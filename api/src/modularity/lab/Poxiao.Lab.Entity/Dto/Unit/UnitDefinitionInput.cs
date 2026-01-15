namespace Poxiao.Lab.Entity.Dto.Unit;

/// <summary>
/// 单位定义输入 DTO.
/// </summary>
public class UnitDefinitionInput
{
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
    /// 排序码.
    /// </summary>
    public long? SortCode { get; set; }

    /// <summary>
    /// 换算因子（仅在设置为基准单位时使用，用于指定当前单位相对于原基准单位的比例）.
    /// <para>如果提供了此值，将忽略 ScaleToBase 和数据库中的原值，强制使用此值进行重算</para>
    /// </summary>
    public decimal? ConversionFactor { get; set; }
}
