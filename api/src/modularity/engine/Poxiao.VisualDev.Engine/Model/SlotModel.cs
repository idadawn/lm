using Poxiao.DependencyInjection;

namespace Poxiao.VisualDev.Engine;

/// <summary>
/// 插槽模型.
/// </summary>
[SuppressSniffer]
public class SlotModel
{
    /// <summary>
    /// 前.
    /// </summary>
    public string prepend { get; set; }

    /// <summary>
    /// 后.
    /// </summary>
    public string append { get; set; }

    /// <summary>
    /// 默认.
    /// </summary>
    public string @default { get; set; }
}