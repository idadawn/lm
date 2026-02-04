namespace Poxiao.system.Entitys.Dto.Item;

/// <summary>
/// 物料信息输入参数.
/// </summary>
public class ItemListOutput
{
    /// <summary>
    /// 物料编码.
    /// </summary>
    public string? id { get; set; }

    /// <summary>
    /// 物料编码.
    /// </summary>
    public string? itemCode { get; set; }

    /// <summary>
    /// 物料描述.
    /// </summary>
    public string? itemDesc { get; set; }

    /// <summary>
    /// 物料类型.
    /// </summary>
    public string? itemCategory { get; set; }

}