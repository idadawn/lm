using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Lab.Entity.Entity;

/// <summary>
/// 单位维度表.
/// </summary>
[SugarTable("UNIT_CATEGORY")]
[Tenant(ClaimConst.TENANTID)]
public class UnitCategoryEntity : CLDEntityBase
{
    /// <summary>
    /// 维度名称（如：长度、质量、密度、电感）.
    /// </summary>
    [SugarColumn(ColumnName = "F_NAME", Length = 100)]
    public string Name { get; set; }

    /// <summary>
    /// 唯一编码（如：LENGTH, MASS, DENSITY）.
    /// </summary>
    [SugarColumn(ColumnName = "F_CODE", Length = 50)]
    public string Code { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION", Length = 500, IsNullable = true)]
    public string Description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE", IsNullable = true)]
    public long? SortCode { get; set; }
}
