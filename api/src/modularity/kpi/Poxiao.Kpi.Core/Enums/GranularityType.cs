namespace Poxiao.Kpi.Core.Enums;

/// <summary>
/// 时间维度时间粒度类型.
/// </summary>
[JsonConverter(typeof(EnumUseNameConverter<GranularityType>))]
public enum GranularityType
{
    /// <summary>
    /// 日.
    /// </summary>
    [Description("日")]
    Day,

    /// <summary>
    /// 周.
    /// </summary>
    [Description("周")]
    Week,

    /// <summary>
    /// 月.
    /// </summary>
    [Description("月")]
    Month,

    /// <summary>
    /// 季.
    /// </summary>
    [Description("季")]
    Quarter,

    /// <summary>
    /// 年.
    /// </summary>
    [Description("年")]
    Year,

    /// <summary>
    /// 时.
    /// </summary>
    [Description("时")]
    Hour,

    /// <summary>
    /// 分.
    /// </summary>
    [Description("分")]
    Minute,

    /// <summary>
    /// 秒.
    /// </summary>
    [Description("秒")]
    Second
}

/// <summary>
/// 时间维度展示方式.
/// </summary>
[JsonConverter(typeof(EnumUseNameConverter<DisplayOption>))]
public enum DisplayOption
{
    /// <summary>
    /// 所有数据.
    /// </summary>
    [Description("所有数据")]
    All,

    /// <summary>
    /// 最新数据.
    /// </summary>
    [Description("最新数据")]
    Latest
}
