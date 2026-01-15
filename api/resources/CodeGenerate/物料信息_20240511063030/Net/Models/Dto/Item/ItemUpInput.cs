namespace Poxiao.Kpi.Entitys.Dto.Item;

/// <summary>
/// 物料信息更新输入.
/// </summary>
public class ItemUpInput : ItemCrInput
{
    /// <summary>
    /// 物料编码.
    /// </summary>
    public string? id { get; set; }
}