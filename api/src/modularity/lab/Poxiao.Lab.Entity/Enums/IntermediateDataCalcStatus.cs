using Newtonsoft.Json;
using System.ComponentModel;

namespace Poxiao.Lab.Entity.Enums;

/// <summary>
/// 中间数据公式计算状态.
/// </summary>
[JsonConverter(typeof(EnumUseNameConverter<IntermediateDataCalcStatus>))]
public enum IntermediateDataCalcStatus
{
    /// <summary>
    /// 待计算（默认状态）.
    /// </summary>
    [Description("待计算")]
    PENDING = 0,

    /// <summary>
    /// 计算中.
    /// </summary>
    [Description("计算中")]
    PROCESSING = 1,

    /// <summary>
    /// 计算成功.
    /// </summary>
    [Description("计算成功")]
    SUCCESS = 2,

    /// <summary>
    /// 计算失败.
    /// </summary>
    [Description("计算失败")]
    FAILED = 3,
}
