namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标定义信息.
/// </summary>
[SuppressSniffer]
public class MetricInfo4DeriveOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [JsonProperty("sort")]
    public long? Sort { get; set; }

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
    /// 衍生指标类型.
    /// </summary>
    [JsonProperty("deriveType")]
    public DeriveType? DeriveType { get; set; }

    /// <summary>
    /// 时间粒度区间.
    /// </summary>
    [JsonProperty("granularity")]
    public GranularityType? Granularity { get; set; }

    /// <summary>
    /// 计算区间.
    /// </summary>
    [JsonProperty("caGranularity")]
    public GranularityType? CaGranularity { get; set; }

    /// <summary>
    /// 时间粒度区间.
    /// </summary>
    [JsonProperty("dateGranularity")]
    public GranularityType? DateGranularity { get; set; }

    /// <summary>
    /// 计算区间信息.
    /// </summary>
    [JsonProperty("granularityStr")]
    public CalculationGranularityModel? GranularityStr { get; set; }

    /// <summary>
    /// 指标列聚合方式.
    /// </summary>
    [JsonProperty("aggType")]
    public DBAggType? AggType { get; set; }

    /// <summary>
    /// 排名方式.
    /// </summary>
    [JsonProperty("rankingType")]
    public RankingType? RankingType { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [JsonProperty("sortType")]
    public DBSortByType? SortType { get; set; }

    /// <summary>
    /// 格式.
    /// </summary>
    [JsonProperty("format")]
    public DataModelFormat? Format { get; set; }

    /// <summary>
    /// 维度.
    /// </summary>
    [JsonProperty("dimensions")]
    public List<TableFieldOutput>? Dimensions { get; set; }

    /// <summary>
    /// 维度集合.
    /// </summary>
    [JsonProperty("dimensionsItem")]
    public List<string>? DimensionItems { get; set; }

    /// <summary>
    /// 筛选.
    /// </summary>
    [JsonProperty("filters")]
    public List<MetricFilterDto>? Filters { get; set; }

    /// <summary>
    /// 时间维度.
    /// </summary>
    [JsonProperty("timeDimensions")]
    public MetricTimeDimensionDto? TimeDimensions { get; set; }

    /// <summary>
    /// 显示模式.
    /// </summary>
    [JsonProperty("displayMode")]
    public MetricDisplayMode? DisplayMode { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    [JsonProperty("createdTime")]
    public DateTime? CreatedTime { get; set; }

    /// <summary>
    /// 创建人.
    /// </summary>
    [JsonProperty("createdUserid")]
    public string? CreatedUserid { get; set; }

    /// <summary>
    /// 最后修改时间.
    /// </summary>
    [JsonProperty("lastModifiedTime")]
    public DateTime? LastModifiedTime { get; set; }

    /// <summary>
    /// 最后修改人.
    /// </summary>
    [JsonProperty("lastModifiedUserid")]
    public string? LastModifiedUserid { get; set; }

    /// <summary>
    /// 删除时间.
    /// </summary>
    [JsonProperty("deleteTime")]
    public DateTime? DeleteTime { get; set; }

    /// <summary>
    /// 删除人.
    /// </summary>
    [JsonProperty("deleteUserid")]
    public string? DeleteUserid { get; set; }

    /// <summary>
    /// 是否删除.
    /// </summary>
    [JsonProperty("isDeleted")]
    public int? IsDeleted { get; set; }

    /// <summary>
    /// 是否启用.
    /// </summary>
    [JsonProperty("isEnabled")]
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [JsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 租户Id.
    /// </summary>
    [JsonProperty("tenantId")]
    public string? TenantId { get; set; }

    /// <summary>
    /// 指标目录
    /// </summary>
    [JsonProperty("metricCategory")]
    public string MetricCategory { get; set; }

    /// <summary>
    /// 指标目录
    /// </summary>
    [JsonProperty("metricCategoryName")]
    public string? MetricCategoryName { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    [JsonProperty("metricTag")]
    public string MetricTag { get; set; }

    /// <summary>
    /// 标签名称
    /// </summary>
    [JsonProperty("metricTagName")]
    public List<string> MetricTagNames { get; set; } = new List<string>();

    /// <summary>
    /// 样例
    /// </summary>
    [JsonProperty("formatValue")]
    public string FormatValue { get; set; }


    /// <summary>
    /// 父级.
    /// </summary>
    [JsonProperty("parentId")]
    public string ParentId { get; set; }

    /// <summary>
    /// 存储频率.
    /// </summary>
    [JsonProperty("frequency")]
    public StorageFqType Frequency { get; set; }
}