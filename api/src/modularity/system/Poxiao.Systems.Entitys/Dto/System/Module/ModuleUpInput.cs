using Poxiao.DependencyInjection;

namespace Poxiao.Systems.Entitys.Dto.Module;

/// <summary>
/// 功能修改输入.
/// </summary>
[SuppressSniffer]
public class ModuleUpInput : ModuleCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }
}