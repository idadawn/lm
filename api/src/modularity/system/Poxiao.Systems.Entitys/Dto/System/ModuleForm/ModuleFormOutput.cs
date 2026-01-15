using Poxiao.DependencyInjection;
using Poxiao.Systems.Entitys.Dto.Module;

namespace Poxiao.Systems.Entitys.Dto.ModuleForm;

/// <summary>
/// 表单权限列表输出.
/// </summary>
[SuppressSniffer]
public class ModuleFormOutput : ModuleAuthorizeBase
{
    /// <summary>
    /// 按钮编码.
    /// </summary>
    public string enCode { get; set; }
}