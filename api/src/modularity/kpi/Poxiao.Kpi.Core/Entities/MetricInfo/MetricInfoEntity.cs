using Poxiao.Infrastructure.Enums;

namespace Poxiao.Kpi.Core.Entitys;

/// <summary>
/// 指标定义.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-14.
/// </summary>
[SugarTable("metric_info", TableDescription = "指标定义")]
public class MetricInfoEntity : CUDEntityBase
{
    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "sort", ColumnDescription = "排序码")]
    public long? Sort { get; set; }

    /// <summary>
    /// 类别.
    /// </summary>
    [SugarColumn(ColumnName = "type", ColumnDescription = "类别", SqlParameterDbType = typeof(EnumToStringConvert))]
    public MetricType Type { get; set; }

    /// <summary>
    /// 指标名称.
    /// </summary>
    [SugarColumn(ColumnName = "name", ColumnDescription = "指标名称")]
    public string Name { get; set; }

    /// <summary>
    /// 指标编码.
    /// </summary>
    [SugarColumn(ColumnName = "code", ColumnDescription = "指标编码")]
    public string Code { get; set; }

    /// <summary>
    /// 模型类别.
    /// </summary>
    [SugarColumn(ColumnName = "date_model_type", ColumnDescription = "模型类别", SqlParameterDbType = typeof(EnumToStringConvert))]
    public DataModelType DateModelType { get; set; }

    /// <summary>
    /// 模型id.
    /// </summary>
    [SugarColumn(ColumnName = "data_model_id", ColumnDescription = "模型id")]
    public string DataModelId { get; set; }

    /// <summary>
    /// 指标列.
    /// </summary>
    [SugarColumn(ColumnName = "column", ColumnDescription = "指标列")]
    public string Column { get; set; }

    /// <summary>
    /// 聚合方式.
    /// </summary>
    [SugarColumn(ColumnName = "agg_type", ColumnDescription = "聚合方式", SqlParameterDbType = typeof(EnumToStringConvert))]
    public DBAggType? AggType { get; set; }

    /// <summary>
    /// 格式.
    /// </summary>
    [SugarColumn(ColumnName = "format", ColumnDescription = "格式")]
    public string Format { get; set; }

    /// <summary>
    /// 表达式.
    /// </summary>
    [SugarColumn(ColumnName = "expression", ColumnDescription = "表达式")]
    public string Expression { get; set; }

    /// <summary>
    /// 维度.
    /// </summary>
    [SugarColumn(ColumnName = "dimensions", ColumnDescription = "维度")]
    public string? Dimensions { get; set; }

    /// <summary>
    /// 时间维度.
    /// </summary>
    [SugarColumn(ColumnName = "time_dimensions", ColumnDescription = "时间维度")]
    public string? TimeDimensions { get; set; }

    /// <summary>
    /// 模式.
    /// </summary>
    [SugarColumn(ColumnName = "display_mode", ColumnDescription = "模式", SqlParameterDbType = typeof(EnumToStringConvert))]
    public MetricDisplayMode DisplayMode { get; set; }

    /// <summary>
    /// 筛选条件.
    /// </summary>
    [SugarColumn(ColumnName = "filters", ColumnDescription = "筛选条件")]
    public string? Filters { get; set; }

    /// <summary>
    /// 指标目录.
    /// </summary>
    [SugarColumn(ColumnName = "metric_category", ColumnDescription = "指标目录")]
    public string? MetricCategory { get; set; }

    /// <summary>
    /// 标签.
    /// </summary>
    [SugarColumn(ColumnName = "metric_tag", ColumnDescription = "标签")]
    public string? MetricTag { get; set; }

    /// <summary>
    /// 父级.
    /// </summary>
    [SugarColumn(ColumnName = "parent_id", ColumnDescription = "父级")]
    public string ParentId { get; set; }

    /// <summary>
    /// 衍生指标类型.
    /// </summary>
    [SugarColumn(ColumnName = "derive_type", ColumnDescription = "衍生指标类型", SqlParameterDbType = typeof(EnumToStringConvert))]
    public DeriveType? DeriveType { get; set; }

    /// <summary>
    /// 计算区间.
    /// </summary>
    [SugarColumn(ColumnName = "ca_granularity", ColumnDescription = "计算区间", SqlParameterDbType = typeof(EnumToStringConvert))]
    public GranularityType? CaGranularity { get; set; }

    /// <summary>
    /// 时间粒度区间.
    /// </summary>
    [SugarColumn(ColumnName = "date_granularity", ColumnDescription = "时间粒度区间", SqlParameterDbType = typeof(EnumToStringConvert))]
    public GranularityType? DateGranularity { get; set; }

    /// <summary>
    /// 计算区间信息.
    /// </summary>
    [SugarColumn(ColumnName = "granularity_str", ColumnDescription = "计算区间信息")]
    public string? GranularityStr { get; set; }

    /// <summary>
    /// 排名方式.
    /// </summary>
    [SugarColumn(ColumnName = "ranking_type", ColumnDescription = "排名方式", SqlParameterDbType = typeof(EnumToStringConvert))]
    public RankingType? RankingType { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    [SugarColumn(ColumnName = "sort_type", ColumnDescription = "排序", SqlParameterDbType = typeof(EnumToStringConvert))]
    public DBSortByType? SortType { get; set; }

    /// <summary>
    /// 公式信息.
    /// </summary>
    [SugarColumn(ColumnName = "formula_data", ColumnDescription = "公式信息")]
    public string? FormulaData { get; set; }

    /// <summary>
    /// 样例值.
    /// </summary>
    [SugarColumn(ColumnName = "format_value", ColumnDescription = "样例值")]
    public string? FormatValue { get; set; }

    /// <summary>
    /// 是否启用.
    /// </summary>
    [SugarColumn(ColumnName = "is_enabled", ColumnDescription = "是否启用")]
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "description", ColumnDescription = "描述")]
    public string? Description { get; set; }

    /// <summary>
    /// 存储频率.
    /// </summary>
    [SugarColumn(ColumnName = "frequency", ColumnDescription = "存储频率", SqlParameterDbType = typeof(EnumToStringConvert))]
    public StorageFqType Frequency { get; set; }

    /// <summary>
    /// 数据类型.
    /// </summary>
    [SugarColumn(ColumnName = "data_type", ColumnDescription = "数据类型", SqlParameterDbType = typeof(EnumToStringConvert))]
    public MetricDataType MetricDataType { get; set; } = MetricDataType.Static;
}