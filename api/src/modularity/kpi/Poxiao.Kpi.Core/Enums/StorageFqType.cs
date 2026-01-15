namespace Poxiao.Kpi.Core.Enums;

/// <summary>
/// 存储频率.
/// </summary>
[JsonConverter(typeof(EnumUseNameConverter<StorageFqType>))]
public enum StorageFqType
{
    /// <summary>
    /// 秒.
    /// </summary>
    [Description("秒")]
    Second,

    /// <summary>
    /// 分.
    /// </summary>
    [Description("分")]
    Minute,

    /// <summary>
    /// 时.
    /// </summary>
    [Description("时")]
    Hour,

    /// <summary>
    /// 日.
    /// </summary>
    [Description("日")]
    Day,

    /// <summary>
    /// 周.
    /// </summary>
    [Description("周")]
    Week
}
