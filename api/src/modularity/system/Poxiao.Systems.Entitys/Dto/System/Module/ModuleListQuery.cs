using Poxiao.Infrastructure.Filter;
using Poxiao.DependencyInjection;

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
