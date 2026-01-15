using Poxiao.DependencyInjection;

namespace Poxiao.VisualDev.Engine;

/// <summary>
/// 显示列模型.
/// </summary>
[SuppressSniffer]
public class IndexGridFieldModel : IndexEachConfigBase
{
    /// <summary>
    /// 对齐.
    /// </summary>
    public string align { get; set; }

    /// <summary>
    /// 固定.
    /// </summary>
    public string @fixed { get; set; }

    /// <summary>
    /// 宽度.
    /// </summary>
    public int? width { get; set; }

    /// <summary>
    /// 是否排序.
    /// </summary>
    public bool sortable { get; set; }
}