using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Lab.Entity;

/// <summary>
/// 产品规格.
/// </summary>
[SugarTable("LAB_PRODUCT_SPEC")]
[Tenant(ClaimConst.TENANTID)]
public class ProductSpecEntity : CLDEntityBase
{
    /// <summary>
    /// 规格代码（如 120/142/170/213）.
    /// </summary>
    [SugarColumn(ColumnName = "F_CODE")]
    public string Code { get; set; }

    /// <summary>
    /// 规格名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_NAME")]
    public string Name { get; set; }

    /// <summary>
    /// 版本.
    /// </summary>
    [SugarColumn(ColumnName = "F_VERSION", Length = 50, IsNullable = true)]
    public string Version { get; set; }

    /// <summary>
    /// 对应原始数据有效列（如 13,15,18,22）.
    /// </summary>
    [SugarColumn(ColumnName = "F_DETECTION_COLUMNS", IsNullable = true)]
    public int DetectionColumns { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION", IsNullable = true)]
    public string Description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }
}
