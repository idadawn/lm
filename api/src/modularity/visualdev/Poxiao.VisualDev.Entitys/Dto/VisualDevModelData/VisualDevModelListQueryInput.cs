using Poxiao.Infrastructure.Filter;

namespace Poxiao.VisualDev.Entitys.Dto.VisualDevModelData;

/// <summary>
/// 在线开发功能模块列表查询输入.
/// </summary>
public class VisualDevModelListQueryInput : PageInputBase
{
    /// <summary>
    /// 选择导出数据key.
    /// </summary>
    public List<string> selectKey { get; set; }

    /// <summary>
    /// 导出类型.
    /// </summary>
    public string dataType { get; set; } = "0";

    /// <summary>
    /// 数据过滤.
    /// </summary>
    public string dataRuleJson { get; set; }

    /// <summary>
    /// 高级查询.
    /// </summary>
    public virtual string superQueryJson { get; set; }
}