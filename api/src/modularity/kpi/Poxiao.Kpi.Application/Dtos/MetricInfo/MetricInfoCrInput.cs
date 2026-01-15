using SqlSugar.DbConvert;

namespace Poxiao.Kpi.Application;

/// <summary>
/// 新建基础指标定义.
/// </summary>
[SuppressSniffer]
public class MetricInfoCrInput
{
    /// <summary>
    /// 类别.
    /// </summary>
    [JsonProperty("type")]
    public MetricType Type { get; set; }

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
    /// 指标列.
    /// </summary>
    [JsonProperty("column")]
    public TableFieldOutput Column { get; set; }

    /// <summary>
    /// 指标列聚合方式.
    /// </summary>
    [JsonProperty("aggType")]
    public DBAggType AggType { get; set; }

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
    public MetricDisplayMode DisplayMode = MetricDisplayMode.General;

    /// <summary>
    /// 上级指标
    /// </summary>
    [JsonProperty("parentId")]
    public string ParentId = "-1";

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

    /// <summary>
    /// 数据类型.
    /// </summary>
    [JsonProperty("data_type")]
    public MetricDataType MetricDataType { get; set; }
}