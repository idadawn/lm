namespace Poxiao.Kpi.Application;

/// <summary>
/// 时间维度.
/// </summary>
public class MetricTimeDimensionDto
{
    /// <summary>
    /// 类型.
    /// </summary>
    [JsonProperty("dataType")]
    public string DataType { get; set; }

    /// <summary>
    /// 列名.
    /// </summary>
    [JsonProperty("field")]
    public string Field { get; set; }

    /// <summary>
    /// 字段注释.
    /// </summary>
    [JsonProperty("fieldName")]
    public string FieldName { get; set; }

    /// <summary>
    /// 时间粒度.
    /// </summary>
    [JsonProperty("granularity")]
    public GranularityType Granularity { get; set; }

    /// <summary>
    /// 展示方式.
    /// </summary>
    [JsonProperty("displayOption")]
    public DisplayOption DisplayOption { get; set; }
}