using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.VisualDev.Entitys;

/// <summary>
/// 可视化开发功能实体
/// 版 本：V2.6.200612
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2020-10-28.
/// </summary>
[SugarTable("BASE_VISUALDEV_MODELDATA")]
[Tenant(ClaimConst.TENANTID)]
public class VisualDevModelDataEntity : CLDEntityBase
{
    /// <summary>
    /// 功能ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_VISUALDEVID")]
    public string VisualDevId { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }

    /// <summary>
    /// 区分主子表.
    /// </summary>
    [SugarColumn(ColumnName = "F_PARENTID")]
    public string ParentId { get; set; }

    /// <summary>
    /// 数据包.
    /// </summary>
    [SugarColumn(ColumnName = "F_DATA")]
    public string Data { get; set; }
}