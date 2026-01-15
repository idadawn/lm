namespace Poxiao.Lab.Entity.Dto.Unit;

/// <summary>
/// 单位维度输入 DTO.
/// </summary>
public class UnitCategoryInput
{
    /// <summary>
    /// 维度名称.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 唯一编码.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    public long? SortCode { get; set; }
}
