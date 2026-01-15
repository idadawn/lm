using Poxiao.Infrastructure.Filter;
using Poxiao.DependencyInjection;

namespace Poxiao.Systems.Entitys.Dto.Position;

/// <summary>
/// 获取岗位列表输入.
/// </summary>
[SuppressSniffer]
public class PositionConditionInput : KeywordInput
{
    /// <summary>
    /// 部门id.
    /// </summary>
    public List<string> departIds { get; set; } = new List<string>();

    /// <summary>
    /// 岗位id.
    /// </summary>
    public List<string> positionIds { get; set; } = new List<string>();
}
