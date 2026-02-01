using Poxiao.DependencyInjection;
using Poxiao.Infrastructure.Filter;

namespace Poxiao.Systems.Entitys.Dto.PrintDev;

/// <summary>
/// 打印模板列表查询输入.
/// </summary>
[SuppressSniffer]
public class PrintDevListInput : PageInputBase
{
    /// <summary>
    /// 分类.
    /// </summary>
    public string category { get; set; }
}