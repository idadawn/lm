using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.WorkFlow.Entitys.Entity;

/// <summary>
/// 流程驳回数据.
/// </summary>
[SugarTable("FLOW_REJECTDATA")]
[Tenant(ClaimConst.TENANTID)]
public class FlowRejectDataEntity : OEntityBase<string>
{
    /// <summary>
    /// 任务数据.
    /// </summary>
    [SugarColumn(ColumnName = "F_TASKJSON")]
    public string? TaskJson { get; set; }

    /// <summary>
    /// 节点数据.
    /// </summary>
    [SugarColumn(ColumnName = "F_TaskNODEJSON")]
    public string? TaskNodeJson { get; set; }

    /// <summary>
    /// 经办数据.
    /// </summary>
    [SugarColumn(ColumnName = "F_TASKOPERATORJSON")]
    public string? TaskOperatorJson { get; set; }
}
