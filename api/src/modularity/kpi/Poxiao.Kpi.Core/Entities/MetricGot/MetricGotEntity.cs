namespace Poxiao.Kpi.Core.Entitys;

/// <summary>
/// 指标思维图.
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2023-11-18.
/// </summary>
[SugarTable("metric_got", TableDescription = "指标思维图")]
public class MetricGotEntity : CUEntityBase
{
    /// <summary>
    /// 思维图类型.
    /// </summary>
    [SugarColumn(ColumnName = "type", ColumnDescription = "思维图类型", SqlParameterDbType = typeof(EnumToStringConvert))]
    public GotType? Type { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "sort", ColumnDescription = "排序码")]
    public long? Sort { get; set; }

    /// <summary>
    /// 价值链名称.
    /// </summary>
    [SugarColumn(ColumnName = "name", ColumnDescription = "价值链名称")]
    public string Name { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "description", ColumnDescription = "描述")]
    public string? Description { get; set; }

    /// <summary>
    /// 图片名称.
    /// </summary>
    [SugarColumn(ColumnName = "img_name", ColumnDescription = "图片名称")]
    public string ImgName { get; set; }

    /// <summary>
    /// 标签.
    /// </summary>
    [SugarColumn(ColumnName = "metric_tag", ColumnDescription = "标签")]
    public string? MetricTag { get; set; }

}