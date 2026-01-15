namespace Poxiao.Kpi.Core.Enums;

/// <summary>
/// 思维图类型.
/// </summary>
[JsonConverter(typeof(EnumUseNameConverter<GotType>))]
public enum GotType
{
    /// <summary>
    /// 价值链
    /// </summary>
    [Description("价值链")]
    Cov = 0,

    /// <summary>
    /// 仪表盘
    /// </summary>
    [Description("仪表盘")]
    Dash = 1
}
