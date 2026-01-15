using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Systems.Entitys.Entity.System;

/// <summary>
/// 行政区划.
/// </summary>
[SugarTable("BASE_PROVINCE_ATLAS")]
public class ProvinceAtlasEntity : CLDEntityBase
{
    /// <summary>
    /// 区域上级.
    /// </summary>
    [SugarColumn(ColumnName = "F_PARENTID")]
    public string ParentId { get; set; }

    /// <summary>
    /// 区域编码.
    /// </summary>
    [SugarColumn(ColumnName = "F_ENCODE")]
    public string EnCode { get; set; }

    /// <summary>
    /// 区域名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FULLNAME")]
    public string FullName { get; set; }

    /// <summary>
    /// 快速查询.
    /// </summary>
    [SugarColumn(ColumnName = "F_QUICKQUERY")]
    public string QuickQuery { get; set; }

    /// <summary>
    /// 区域类型：1-省份、2-城市、3-县区、4-街道.
    /// </summary>
    [SugarColumn(ColumnName = "F_TYPE")]
    public string Type { get; set; }

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
    /// 租户id.
    /// </summary>
    [SugarColumn(ColumnName = "F_TENANTID")]
    public string TenantId { get; set; }

    /// <summary>
    /// 行政区划编码.
    /// </summary>
    [SugarColumn(ColumnName = "F_DIVISIONCODE")]
    public string DivisionCode { get; set; }

    /// <summary>
    /// 中心经纬度.
    /// </summary>
    [SugarColumn(ColumnName = "F_ATLASCENTER")]
    public string AtlasCenter { get; set; }
}
