namespace Poxiao.Kpi.Application;

/// <summary>
/// 新增复合指标定义信息.
/// </summary>
[SuppressSniffer]
public class MetricInfo4CompositeCrInput
{
    /// <summary>
    /// 类别.
    /// </summary>
    [JsonProperty("type")]
    public MetricType Type { get; set; } = MetricType.Derive;

    /// <summary>
    /// 指标名称.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// 指标编码.
    /// </summary>
    [JsonProperty("code")]
    public string Code { get; set; }

    /// <summary>
    /// 模型id.
    /// </summary>
    [JsonProperty("dataModelId")]
    public string DataModelId { get; set; }

    /// <summary>
    /// 指标列.
    /// </summary>
    [JsonProperty("column")]
    public string Column { get; set; }

    /// <summary>
    /// 公式信息.
    /// </summary>
    [JsonProperty("formulaData")]
    public string FormulaData { get; set; }

    /// <summary>
    /// 表达式.
    /// </summary>
    [JsonProperty("expression")]
    public string Expression { get; set; }

    /// <summary>
    /// 格式.
    /// </summary>
    [JsonProperty("format")]
    public DataModelFormat? Format { get; set; } = new DataModelFormat();

    /// <summary>
    /// 维度.
    /// </summary>
    [JsonProperty("dimensions")]
    public List<TableFieldOutput>? Dimensions { get; set; }

    /// <summary>
    /// 筛选.
    /// </summary>
    [JsonProperty("filters")]
    public List<MetricFilterDto>? Filters { get; set; } = new List<MetricFilterDto>();

    /// <summary>
    /// 时间维度.
    /// </summary>
    [JsonProperty("timeDimensions")]
    public MetricTimeDimensionDto? TimeDimensions { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [JsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 指标目录
    /// </summary>
    [JsonProperty("metricCategory")]
    public string? MetricCategory { get; set; }

    /// <summary>
    /// 标签集合
    /// </summary>
    [JsonProperty("metricTag")]
    public List<string>? MetricTag { get; set; }

    /// <summary>
    /// 模式
    /// </summary>
    [JsonProperty("displayMode")]
    public MetricDisplayMode DisplayMode = MetricDisplayMode.Auto;

    /// <summary>
    /// 上级指标
    /// </summary>
    [JsonProperty("parentIds")]
    public List<string> ParentIds;

    /// <summary>
    /// 样例
    /// </summary>
    [JsonProperty("formatValue")]
    public string FormatValue { get; set; }

    /// <summary>
    /// 存储频率.
    /// </summary>
    [JsonProperty("frequency")]
    public StorageFqType Frequency { get; set; }
}
