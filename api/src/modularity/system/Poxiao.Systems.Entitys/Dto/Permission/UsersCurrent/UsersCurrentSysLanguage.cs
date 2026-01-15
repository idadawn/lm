using Poxiao.DependencyInjection;

namespace Poxiao.Systems.Entitys.Dto.UsersCurrent;

/// <summary>
/// 当前用户系统语言.
/// </summary>
[SuppressSniffer]
public class UsersCurrentSysLanguage
{
    /// <summary>
    /// 语言.
    /// </summary>
    public string language { get; set; }
}