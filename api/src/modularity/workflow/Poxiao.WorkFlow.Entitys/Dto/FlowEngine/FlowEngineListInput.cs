using Poxiao.DependencyInjection;
using Poxiao.Infrastructure.Filter;

namespace Poxiao.WorkFlow.Entitys.Dto.FlowEngine;

[SuppressSniffer]
public class FlowEngineListInput : PageInputBase
{
    /// <summary>
    /// 分类.
    /// </summary>
    public string? category { get; set; }

    /// <summary>
    /// 流程类型.
    /// </summary>
    public int flowType { get; set; }
}

