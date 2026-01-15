namespace Poxiao.Infrastructure.Models;

/// <summary>
/// 静态数据模型.
/// </summary>
[SuppressSniffer]
public class StaticDataModel
{
    /// <summary>
    /// 选项名称.
    /// </summary>
    public string fullName { get; set; }

    /// <summary>
    /// 选项值.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 子级.
    /// </summary>
    public List<StaticDataModel> children { get; set; }
}