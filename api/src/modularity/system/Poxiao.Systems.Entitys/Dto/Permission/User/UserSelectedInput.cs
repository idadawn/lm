using Poxiao.Infrastructure.Filter;
using Poxiao.DependencyInjection;

namespace Poxiao.Systems.Entitys.Dto.User;

/// <summary>
/// 选择用户输入.
/// </summary>
[SuppressSniffer]
public class UserSelectedInput
{
    /// <summary>
    /// 选择的组织id、岗位id、角色id、用户id.
    /// </summary>
    public List<string> userId { get; set; }

    /// <summary>
    /// 分页输入参数.
    /// </summary>
    public PageInputBase pagination { get; set; }
}