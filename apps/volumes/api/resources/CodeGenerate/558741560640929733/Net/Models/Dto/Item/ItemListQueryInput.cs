using Poxiao.Infrastructure.Filter;

namespace Poxiao.system.Entitys.Dto.Item;

/// <summary>
/// 物料信息列表查询输入.
/// </summary>
public class ItemListQueryInput : PageInputBase
{
    /// <summary>
    /// 高级查询.
    /// </summary>
    public string superQueryJson { get; set; }

    /// <summary>
    /// 选择导出数据key.
    /// </summary>
    public string selectKey { get; set; }

    /// <summary>
    /// 导出类型.
    /// </summary>
    public int dataType { get; set; }

    /// <summary>
    /// 物料编码.
    /// </summary>
    public string itemCode { get; set; }

    /// <summary>
    /// 物料描述.
    /// </summary>
    public string itemDesc { get; set; }

    /// <summary>
    /// 物料类型.
    /// </summary>
    public List<string> itemCategory { get; set; }

}