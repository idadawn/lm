namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标筛选条件.
/// </summary>
public class MetricFilterDto
{
    /// <summary>
    /// 操作符.
    /// </summary>
    [JsonProperty("whereType")]
    public MetricWhereType WhereType { get; set; } = MetricWhereType.And;

    /// <summary>
    /// 数据类型.
    /// </summary>
    [JsonProperty("dataType")]
    public string DataType { get; set; }

    /// <summary>
    /// 字段Id.
    /// </summary>
    [JsonProperty("field")]
    public string Field { get; set; }

    /// <summary>
    /// 字段名称.
    /// </summary>
    [JsonProperty("fieldName")]
    public string FieldName { get; set; }

    /// <summary>
    /// 类别.
    /// </summary>
    [JsonProperty("type")]
    public MetricFilterModel Type { get; set; }

    /// <summary>
    /// 字段值.
    /// 多个使用,分隔.
    /// </summary>
    [JsonProperty("fieldValue")]
    public List<string> FieldValue { get; set; }

    /// <summary>
    /// 最小值.
    /// </summary>
    [JsonProperty("minValue")]
    public string MinValue { get; set; }

    /// <summary>
    /// 最大值.
    /// </summary>
    [JsonProperty("maxValue")]
    public string MaxValue { get; set; }

    /// <summary>
    /// 最小值是否选择.
    /// </summary>
    [JsonProperty("minValueChecked")]
    public bool MinValueChecked { get; set; }

    /// <summary>
    /// 最大值是否选择.
    /// </summary>
    [JsonProperty("maxValueChecked")]
    public bool MaxValueChecked { get; set; }

}

/// <summary>
/// where条件.
/// </summary>
public enum MetricWhereType
{
    /// <summary>
    /// 且.
    /// </summary>
    [Description("且")]
    And = 0,

    /// <summary>
    /// 或.
    /// </summary>
    [Description("或")]
    Or = 1
}