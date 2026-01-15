namespace Poxiao.Kpi.Core.Enums;

/// <summary>
/// 价值链规则值类型.
/// </summary>
[JsonConverter(typeof(EnumUseNameConverter<CovRuleValueType>))]
public enum CovRuleValueType
{
    /// <summary>
    /// 数值.
    /// </summary>
    [Description("数值")]
    Value = 0,

    /// <summary>
    /// 百分比.
    /// </summary>
    [Description("百分比")]
    Percent = 1,
}


/// <summary>
/// 价值链规则操作符.
/// </summary>
[JsonConverter(typeof(EnumUseNameConverter<CovRuleOperatorsType>))]
public enum CovRuleOperatorsType
{
    /// <summary>
    /// 大于
    /// </summary>
    [Description("大于")]
    GreaterThan = 0,

    /// <summary>
    /// 在两值之间
    /// </summary>
    [Description("在两值之间")]
    Between = 1,

    /// <summary>
    /// 小于
    /// </summary>
    [Description("小于")]
    LessThan = 2
}