using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.WorkFlow.Entitys.Entity;

/// <summary>
/// 流程任务发起人.
/// </summary>
[SugarTable("FLOW_USER")]
[Tenant(ClaimConst.TENANTID)]
public class FlowUserEntity : OEntityBase<string>
{
    /// <summary>
    /// 任务id.
    /// </summary>
    [SugarColumn(ColumnName = "F_TASKID")]
    public string? TaskId { get; set; }

    /// <summary>
    /// 组织主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_ORGANIZEID")]
    public string? OrganizeId { get; set; }

    /// <summary>
    /// 岗位主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_POSITIONID")]
    public string? PositionId { get; set; }

    /// <summary>
    /// 主管主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_MANAGERID")]
    public string? ManagerId { get; set; }

    /// <summary>
    /// 上级用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_SUPERIOR")]
    public string? Superior { get; set; }

    /// <summary>
    /// 下属用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_SUBORDINATE")]
    public string? Subordinate { get; set; }
}
