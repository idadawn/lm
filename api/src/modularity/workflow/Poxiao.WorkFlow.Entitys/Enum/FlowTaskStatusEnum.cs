using System.ComponentModel;
using Poxiao.DependencyInjection;

namespace Poxiao.WorkFlow.Entitys.Enum;

/// <summary>
/// 任务状态枚举.
/// </summary>
[SuppressSniffer]
public enum FlowTaskStatusEnum
{
    /// <summary>
    /// 等待提交.
    /// </summary>
    [Description("等待提交")]
    Draft = 0,

    /// <summary>
    /// 等待审核.
    /// </summary>
    [Description("等待提交")]
    Handle = 1,

    /// <summary>
    /// 审核通过.
    /// </summary>
    [Description("等待提交")]
    Adopt = 2,

    /// <summary>
    /// 审核驳回.
    /// </summary>
    [Description("等待提交")]
    Reject = 3,

    /// <summary>
    /// 审核撤销.
    /// </summary>
    [Description("等待提交")]
    Revoke = 4,

    /// <summary>
    /// 审核作废.
    /// </summary>
    [Description("等待提交")]
    Cancel = 5,
}