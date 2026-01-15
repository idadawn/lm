using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.WorkFlow.Entitys.Entity;

/// <summary>
/// 流程表单关系表.
/// </summary>
[SugarTable("FLOW_ENGINEFORM_RELATION")]
[Tenant(ClaimConst.TENANTID)]
public class FlowFormRelationEntity : OEntityBase<string>
{
    /// <summary>
    /// 表单id.
    /// </summary>
    [SugarColumn(ColumnName = "F_FormId")]
    public string? FormId { get; set; }

    /// <summary>
    /// 流程id.
    /// </summary>
    [SugarColumn(ColumnName = "F_FlowId")]
    public string? FlowId { get; set; }
}
