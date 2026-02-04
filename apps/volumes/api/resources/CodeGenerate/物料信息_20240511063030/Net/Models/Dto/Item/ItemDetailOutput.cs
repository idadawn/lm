namespace Poxiao.Kpi.Entitys.Dto.Item;

/// <summary>
/// 物料信息详情输出参数.
/// </summary>
public class ItemDetailOutput
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