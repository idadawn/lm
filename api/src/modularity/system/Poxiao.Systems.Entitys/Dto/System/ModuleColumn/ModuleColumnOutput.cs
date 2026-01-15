using Poxiao.DependencyInjection;
using Poxiao.Systems.Entitys.Dto.Module;

namespace Poxiao.Systems.Entitys.Dto.ModuleColumn;

/// <summary>
/// 功能列输出.
/// </summary>
[SuppressSniffer]
public class ModuleColumnOutput : ModuleAuthorizeBase
{
    /// <summary>
    /// 按钮编码.
    /// </summary>
    public string enCode { get; set; }
}