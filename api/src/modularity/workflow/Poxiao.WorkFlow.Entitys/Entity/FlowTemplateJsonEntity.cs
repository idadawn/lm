using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.WorkFlow.Entitys.Entity;

/// <summary>
/// 流程引擎.
/// </summary>
[SugarTable("FLOW_TEMPLATEJSON")]
[Tenant(ClaimConst.TENANTID)]
public class FlowTemplateJsonEntity : CLDEntityBase
{
    /// <summary>
    /// 流程编码.
    /// </summary>
    [SugarColumn(ColumnName = "F_TEMPLATEID")]
    public string? TemplateId { get; set; }

    /// <summary>
    /// 可见类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_VISIBLETYPE")]
    public int? VisibleType { get; set; }

    /// <summary>
    /// 流程版本.
    /// </summary>
    [SugarColumn(ColumnName = "F_VERSION")]
    public string? Version { get; set; }

    /// <summary>
    /// 流程模板.
    /// </summary>
    [SugarColumn(ColumnName = "F_FLOWTEMPLATEJSON")]
    public string? FlowTemplateJson { get; set; }

    /// <summary>
    /// 流程名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FULLNAME")]
    public string? FullName { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }

    /// <summary>
    /// 分组id.
    /// </summary>
    [SugarColumn(ColumnName = "F_GROUPID")]
    public string? GroupId { get; set; }
}
