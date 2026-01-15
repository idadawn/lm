using Poxiao.Infrastructure.Security;
using Poxiao.DependencyInjection;

namespace Poxiao.Systems.Entitys.Model.PrintDev;

/// <summary>
/// 打印模板配置字段模型.
/// </summary>
[SuppressSniffer]
public class PrintDevFieldModel : TreeModel
{
    /// <summary>
    /// 字段说明.
    /// </summary>
    public string fullName { get; set; }
}