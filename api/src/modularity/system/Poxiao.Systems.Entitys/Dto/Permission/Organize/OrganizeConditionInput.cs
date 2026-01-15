using Poxiao.Infrastructure.Filter;
using Poxiao.DependencyInjection;

namespace Poxiao.Systems.Entitys.Dto.Organize;

/// <summary>
/// 获取部门列表输入.
/// </summary>
[SuppressSniffer]
public class OrganizeConditionInput : KeywordInput
{
    /// <summary>
    /// 部门id.
    /// </summary>
    public List<string> departIds;
}
