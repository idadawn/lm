using Poxiao.DependencyInjection;

namespace Poxiao.VisualDev.Engine;

/// <summary>
/// 实体字段模型
/// 版 本：V3.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao.
/// </summary>
[SuppressSniffer]
public class EntityFieldModel
{
    /// <summary>
    /// 字段名称.
    /// </summary>
    public string Field { get; set; }

    /// <summary>
    /// 字段说明.
    /// </summary>
    public string FieldName { get; set; }

    /// <summary>
    /// 数据类型.
    /// </summary>
    public string DataType { get; set; }

    /// <summary>
    /// 数据长度.
    /// </summary>
    public string DataLength { get; set; }

    /// <summary>
    /// 主键.
    /// </summary>
    public int? PrimaryKey { get; set; }
}