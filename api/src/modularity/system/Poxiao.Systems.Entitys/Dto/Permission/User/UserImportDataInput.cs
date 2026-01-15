using Poxiao.DependencyInjection;

namespace Poxiao.Systems.Entitys.Dto.User;

/// <summary>
/// 用户数据导入 输入.
/// </summary>
[SuppressSniffer]
public class UserImportDataInput
{
    /// <summary>
    /// 导入的数据列表.
    /// </summary>
    public List<UserListImportDataInput> list { get; set; }
}