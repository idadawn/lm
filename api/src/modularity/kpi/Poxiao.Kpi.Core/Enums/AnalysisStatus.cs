namespace Poxiao.Kpi.Core.Enums;

/// <summary>
/// 分析状态.
/// </summary>
[JsonConverter(typeof(EnumUseNameConverter<AnalysisStatus>))]
public enum AnalysisStatus
{
    /// <summary>
    /// 未开始.
    /// </summary>
    [Description("未开始")]
    NotStarted = 0,

    /// <summary>
    /// 进行中.
    /// </summary>
    [Description("进行中")]
    InProgress = 1,

    /// <summary>
    /// 已完成.
    /// </summary>
    [Description("已完成")]
    Completed = 2,

    /// <summary>
    ///  已取消.
    /// </summary>
    [Description("已取消")]
    Canceled = 3,

    /// <summary>
    /// 已失败.
    /// </summary>
    [Description("已失败")]
    Failed = 4
}
