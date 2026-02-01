using Poxiao.DependencyInjection;
using Poxiao.Infrastructure.Filter;

namespace Poxiao.Systems.Entitys.Dto.Module;

/// <summary>
/// 功能列表查询
/// </summary>
[SuppressSniffer]
public class ModuleListQuery : KeywordInput
{
    /// <summary>
    /// 分类
    /// </summary>
    public string category { get; set; }
}
