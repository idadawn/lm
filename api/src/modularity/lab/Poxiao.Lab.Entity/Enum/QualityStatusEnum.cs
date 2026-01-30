using System.ComponentModel;

namespace Poxiao.Lab.Entity.Enum;

/// <summary>
/// 质量状态枚举.
/// </summary>
public enum QualityStatusEnum
{
    /// <summary>
    /// 合格.
    /// </summary>
    [Description("合格")]
    Qualified,

    /// <summary>
    /// 不合格.
    /// </summary>
    [Description("不合格")]
    Unqualified,

    /// <summary>
    /// 其他.
    /// </summary>
    [Description("其他")]
    Other
}
