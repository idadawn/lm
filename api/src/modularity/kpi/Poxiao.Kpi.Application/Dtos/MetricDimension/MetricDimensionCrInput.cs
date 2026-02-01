namespace Poxiao.Kpi.Application;

/// <summary>
/// 新建公共维度.
/// </summary>
[SuppressSniffer]
public class MetricDimensionCrInput
{
    /// <summary>
    /// 模型类别.
    /// </summary>
    [JsonProperty("dateModelType")]
    public DataModelType DateModelType { get; set; }

    /// <summary>
    /// 模型id.
    /// </summary>
    [JsonProperty("dataModelId")]
    public DbSchemaOutput DataModelId { get; set; }

    /// <summary>
    /// 维度名称.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// 数据类型.
    /// </summary>
    [JsonProperty("dataType")]
    public string DataType { get; set; }

    /// <summary>
    /// 列.
    /// </summary>
    [JsonProperty("column")]
    public TableFieldOutput Column { get; set; }

}