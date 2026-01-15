using Poxiao.Infrastructure.Filter;
using Poxiao.DependencyInjection;

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