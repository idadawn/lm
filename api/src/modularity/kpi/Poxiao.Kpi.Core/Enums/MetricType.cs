namespace Poxiao.Kpi.Core.Enums;

/// <summary>
/// 指标类型.
/// </summary>
[JsonConverter(typeof(EnumUseNameConverter<MetricType>))]
public enum MetricType
{
    /// <summary>
    /// 基础指标.
    /// </summary>
    [Description("基础指标")]
    Basic = 1,

    /// <summary>
    /// 派生指标.
    /// </summary>
    [Description("派生指标")]
    Derive = 2,

    /// <summary>
    /// 复合指标.
    /// </summary>
    [Description("复合指标")]
    Composite = 3,
}

/// <summary>
/// 指标模式.
/// </summary>
[JsonConverter(typeof(EnumUseNameConverter<MetricDisplayMode>))]
public enum MetricDisplayMode
{
    /// <summary>
    /// 普通模式.
    /// </summary>
    [Description("普通模式")]
    General = 1,

    /// <summary>
    /// 自动模式.
    /// </summary>
    [Description("自动模式")]
    Auto = 2,

    /// <summary>
    /// 表达式模式.
    /// </summary>
    [Description("表达式模式")]
    Expression = 3,
}

/// <summary>
/// 指标筛选方式
/// </summary>
[JsonConverter(typeof(EnumUseNameConverter<MetricFilterModel>))]
public enum MetricFilterModel
{
    /// <summary>
    /// 范围.
    /// </summary>
    [Description("范围")]
    ByRange = 1,

    /// <summary>
    /// 值.
    /// </summary>
    [Description("值")]
    ByValue = 2,

    /// <summary>
    /// 时间范围.
    /// </summary>
    [Description("时间范围")]
    ByDateRang = 3
}


/// <summary>
/// 派生指标类别.
/// </summary>
[JsonConverter(typeof(EnumUseNameConverter<DeriveType>))]
public enum DeriveType
{
    /// <summary>
    /// 期间累计.
    /// YTD/QTD/MTD/WTD 即年累计/季度累计/月累计/周累计。如：月累计最大销售额.
    /// </summary>
    [Description("期间累计")]
    PTD = 1,

    /// <summary>
    /// 同/环比.
    /// YOY/QOQ/MOM/WOW/DOD 即年同/环比、季度同/环比、月同/环比、周环比、日环比。如：年销售额年环比.
    /// </summary>
    [Description("同/环比")]
    POP = 2,

    /// <summary>
    /// 累计计算.
    /// 有史以来所有值参与计算。如：每年最大日销售额.
    /// </summary>
    [Description("累计计算")]
    Cumulative = 3,

    /// <summary>
    /// 移动计算.
    /// 指定区间的值参与计算。如：30天销售额移动平均.
    /// </summary>
    [Description("移动计算")]
    Moving = 4,

    /// <summary>
    /// 差值.
    /// 差值 = 当前值 - 对照值。如：后一差异销售额.
    /// </summary>
    [Description("差值")]
    Difference = 5,

    /// <summary>
    /// 差值占比.
    /// 差值占比 =（当前值 - 对照值）/ 对照值。如：后一差异销售额占比.
    /// </summary>
    [Description("差值占比")]
    DifferenceRatio = 6,

    /// <summary>
    /// 总占比.
    /// 总占比 = 当前值 / 总值。如：销售额占比.
    /// </summary>
    [Description("总占比")]
    TotalRatio = 7,

    /// <summary>
    /// 排名.
    /// 如：销售额从小到大排名.
    /// </summary>
    [Description("排名")]
    Ranking = 8
}

/// <summary>
/// 排名方式.
/// </summary>
[JsonConverter(typeof(EnumUseNameConverter<RankingType>))]
public enum RankingType
{
    /// <summary>
    /// 竞争排序.
    /// 排名值可重复且有间隔（如 1, 2, 2, 4...）.
    /// </summary>
    [Description("竞争排序")]
    Competition = 1,

    /// <summary>
    /// 密集排序.
    /// 排名值可重复且无间隔（如 1, 2, 2, 3...）.
    /// </summary>
    [Description("密集排序")]
    Dense = 2,

    /// <summary>
    /// 唯一排序.
    /// 排名值不重复（如 1, 2, 3, 4...）.
    /// </summary>
    [Description("唯一排序")]
    Unique = 3,

    /// <summary>
    /// 百分位排序.
    /// （如 1%, 5%, 20%...）.
    /// </summary>
    [Description("百分位排序")]
    Percentile = 4
}

/// <summary>
/// 趋势类型.
/// </summary>
[JsonConverter(typeof(EnumUseNameConverter<TrendType>))]
public enum TrendType
{
    /// <summary>
    /// 提升/增加.
    /// </summary>
    [Description("下降/减少")]
    Up,
    /// <summary>
    /// 下降/减少.
    /// </summary>
    [Description("下降/减少")]
    Down
}

/// <summary>
/// 趋势类型.
/// </summary>
[JsonConverter(typeof(EnumUseNameConverter<MetricGradeType>))]
public enum MetricGradeType
{
    /// <summary>
    /// 值.
    /// </summary>
    [Description("值")]
    Value,
    /// <summary>
    /// 区间.
    /// </summary>
    [Description("区间")]
    Rang
}

