using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Systems.Entitys.Permission;

/// <summary>
/// 分级管理
/// 版 本：V1.0.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021.09.27.
/// </summary>
[SugarTable("BASE_ORGANIZEADMINISTRATOR")]
public class OrganizeAdministratorEntity : CLDEntityBase
{
    /// <summary>
    /// 用户ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_USERID")]
    public string UserId { get; set; }

    /// <summary>
    /// 机构ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_ORGANIZEID")]
    public string OrganizeId { get; set; }

    /// <summary>
    /// 机构类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_ORGANIZETYPE")]
    public string OrganizeType { get; set; }

    /// <summary>
    /// 本层级添加.
    /// </summary>
    [SugarColumn(ColumnName = "F_THISLAYERADD")]
    public int ThisLayerAdd { get; set; }

    /// <summary>
    /// 本层级编辑.
    /// </summary>
    [SugarColumn(ColumnName = "F_THISLAYEREDIT")]
    public int ThisLayerEdit { get; set; }

    /// <summary>
    /// 本层级删除.
    /// </summary>
    [SugarColumn(ColumnName = "F_THISLAYERDELETE")]
    public int ThisLayerDelete { get; set; }

    /// <summary>
    /// 子层级添加.
    /// </summary>
    [SugarColumn(ColumnName = "F_SUBLAYERADD")]
    public int SubLayerAdd { get; set; }

    /// <summary>
    /// 子层级编辑.
    /// </summary>
    [SugarColumn(ColumnName = "F_SUBLAYEREDIT")]
    public int SubLayerEdit { get; set; }

    /// <summary>
    /// 子层级删除.
    /// </summary>
    [SugarColumn(ColumnName = "F_SUBLAYERDELETE")]
    public int SubLayerDelete { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string Description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }

    /// <summary>
    /// 本层级查看.
    /// </summary>
    [SugarColumn(ColumnName = "F_ThisLayerSelect")]
    public int ThisLayerSelect { get; set; }

    /// <summary>
    /// 子层级查看.
    /// </summary>
    [SugarColumn(ColumnName = "F_SubLayerSelect")]
    public int SubLayerSelect { get; set; }
}