using Poxiao.DependencyInjection;

namespace Poxiao.Systems.Entitys.Model.Menu;

/// <summary>
/// 功能表单模型.
/// </summary>
[SuppressSniffer]
public class FunctionalFormModel : FunctionalBase
{
    /// <summary>
    /// 功能主键.
    /// </summary>
    public string ModuleId { get; set; }
}