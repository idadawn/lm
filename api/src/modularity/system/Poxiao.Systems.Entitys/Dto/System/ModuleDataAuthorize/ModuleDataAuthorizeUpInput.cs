using Poxiao.DependencyInjection;

namespace Poxiao.Systems.Entitys.Dto.ModuleDataAuthorize;

/// <summary>
/// 功能权限数据修改输入.
/// </summary>
[SuppressSniffer]
public class ModuleDataAuthorizeUpInput : ModuleDataAuthorizeCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }
}
