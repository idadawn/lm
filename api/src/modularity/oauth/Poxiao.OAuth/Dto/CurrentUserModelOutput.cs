using Poxiao.DependencyInjection;
using Poxiao.Systems.Entitys.Dto.Module;
using Poxiao.Systems.Entitys.Dto.ModuleButton;
using Poxiao.Systems.Entitys.Dto.ModuleColumn;
using Poxiao.Systems.Entitys.Dto.ModuleDataAuthorizeScheme;
using Poxiao.Systems.Entitys.Dto.ModuleForm;

namespace Poxiao.OAuth.Dto;

/// <summary>
/// 当前用户模型输出.
/// </summary>
[SuppressSniffer]
public class CurrentUserModelOutput
{
    /// <summary>
    /// 菜单权限.
    /// </summary>
    public List<ModuleOutput> moduleList { get; set; }

    /// <summary>
    /// 按钮权限.
    /// </summary>
    public List<ModuleButtonOutput> buttonList { get; set; }

    /// <summary>
    /// 列表权限.
    /// </summary>
    public List<ModuleColumnOutput> columnList { get; set; }

    /// <summary>
    /// 数据权限.
    /// </summary>
    public List<ModuleDataAuthorizeSchemeOutput> resourceList { get; set; }

    /// <summary>
    /// 表单权限.
    /// </summary>
    public List<ModuleFormOutput> formList { get; set; }
}