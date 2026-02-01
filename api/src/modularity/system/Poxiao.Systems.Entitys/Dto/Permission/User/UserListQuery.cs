using Poxiao.DependencyInjection;
using Poxiao.Infrastructure.Filter;

namespace Poxiao.Systems.Entitys.Dto.User;

/// <summary>
/// 用户列表查询输入.
/// </summary>
[SuppressSniffer]
public class UserListQuery : PageInputBase
{
    /// <summary>
    /// 机构ID.
    /// </summary>
    public string organizeId { get; set; }

    /// <summary>
    /// 岗位ID.
    /// </summary>
    public string positionId { get; set; }
}