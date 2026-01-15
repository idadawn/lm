using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.WorkFlow.Entitys.Entity;

/// <summary>
/// 流程表单.
/// </summary>
[SugarTable("FLOW_ENGINEFORM")]
public class FlowFormEntity : CLDEntityBase
{
    /// <summary>
    /// 编码.
    /// </summary>
    [SugarColumn(ColumnName = "F_ENCODE")]
    public string? EnCode { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FULLNAME")]
    public string? FullName { get; set; }

    /// <summary>
    /// 分类.
    /// </summary>
    [SugarColumn(ColumnName = "F_CATEGORY")]
    public string? Category { get; set; }

    /// <summary>
    /// Web地址.
    /// </summary>
    [SugarColumn(ColumnName = "F_URLADDRESS")]
    public string? UrlAddress { get; set; }

    /// <summary>
    /// APP地址.
    /// </summary>
    [SugarColumn(ColumnName = "F_APPURLADDRESS")]
    public string? AppUrlAddress { get; set; }

    /// <summary>
    /// 表单json.
    /// </summary>
    [SugarColumn(ColumnName = "F_PROPERTYJSON")]
    public string? PropertyJson { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string? Description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }

    /// <summary>
    /// 流程类型（0：发起流程，1：功能流程）.
    /// </summary>
    [SugarColumn(ColumnName = "F_FLOWTYPE")]
    public int? FlowType { get; set; }

    /// <summary>
    /// 表单类型（1：系统表单 2：自定义表单）.
    /// </summary>
    [SugarColumn(ColumnName = "F_FORMTYPE")]
    public int? FormType { get; set; }

    /// <summary>
    /// 关联表单.
    /// </summary>
    [SugarColumn(ColumnName = "F_TABLEJSON")]
    public string? TableJson { get; set; }

    /// <summary>
    /// 数据源id.
    /// </summary>
    [SugarColumn(ColumnName = "F_DBLINKID")]
    public string? DbLinkId { get; set; }

    /// <summary>
    /// 接口路径.
    /// </summary>
    [SugarColumn(ColumnName = "F_INTERFACEURL")]
    public string? InterfaceUrl { get; set; }

    /// <summary>
    /// 表单json草稿.
    /// </summary>
    [SugarColumn(ColumnName = "F_DRAFTJSON")]
    public string? DraftJson { get; set; }

    /// <summary>
    /// 流程id.
    /// </summary>
    [SugarColumn(ColumnName = "F_FLOWID")]
    public string? FlowId { get; set; }
}
