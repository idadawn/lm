using Poxiao.DependencyInjection;
using Poxiao.WorkFlow.Entitys.Model.Item;

namespace Poxiao.WorkFlow.Entitys.Model.Properties;

[SuppressSniffer]
public class ConditionProperties
{
    /// <summary>
    /// 标题.
    /// </summary>
    public string? title { get; set; }

    /// <summary>
    /// 条件明细.
    /// </summary>
    public List<ConditionsItem>? conditions { get; set; }

    /// <summary>
    /// 是否默认.
    /// </summary>
    public bool isDefault { get; set; }
}
