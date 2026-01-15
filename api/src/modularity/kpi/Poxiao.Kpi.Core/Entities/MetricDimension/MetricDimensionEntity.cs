namespace Poxiao.Kpi.Core.Entitys;

/// <summary>
/// 公共维度.
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2023-12-6
/// </summary>
[SugarTable("metric_dimension", TableDescription = "公共维度")]
public class MetricDimensionEntity : CUEntityBase
{
    /// <summary>
    /// 模型类别.
    /// </summary>
    [SugarColumn(ColumnName = "date_model_type", ColumnDescription = "模型类别")]
    public string DateModelType { get; set; }

    /// <summary>
    /// 模型id.
    /// </summary>
    [SugarColumn(ColumnName = "data_model_id", ColumnDescription = "模型id")]
    public string DataModelId { get; set; }

    /// <summary>
    /// 维度名称.
    /// </summary>
    [SugarColumn(ColumnName = "name", ColumnDescription = "维度名称")]
    public string Name { get; set; }

    /// <summary>
    /// 数据类型.
    /// </summary>
    [SugarColumn(ColumnName = "data_type", ColumnDescription = "数据类型")]
    public string DataType { get; set; }

    /// <summary>
    /// 列.
    /// </summary>
    [SugarColumn(ColumnName = "column", ColumnDescription = "列")]
    public string Column { get; set; }

}