using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Lab.Entity.Entity;

/// <summary>
/// 单位定义表.
/// </summary>
[SugarTable("UNIT_DEFINITION")]
[Tenant(ClaimConst.TENANTID)]
public class UnitDefinitionEntity : CLDEntityBase
{
    /// <summary>
    /// 关联维度 ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_CATEGORY_ID", Length = 50)]
    public string CategoryId { get; set; }

    /// <summary>
    /// 单位全称（如：毫米、微米）.
    /// </summary>
    [SugarColumn(ColumnName = "F_NAME", Length = 100)]
    public string Name { get; set; }

    /// <summary>
    /// 单位符号（如：mm, μm, kg/cm³）.
    /// </summary>
    [SugarColumn(ColumnName = "F_SYMBOL", Length = 20)]
    public string Symbol { get; set; }

    /// <summary>
    /// 是否为该维度的基准单位 (1-是, 0-否).
    /// </summary>
    [SugarColumn(ColumnName = "F_IS_BASE")]
    public int IsBase { get; set; }

    /// <summary>
    /// 换算至基准单位的比例系数.
    /// </summary>
    [SugarColumn(ColumnName = "F_SCALE_TO_BASE", ColumnDataType = "DECIMAL(18,10)")]
    public decimal ScaleToBase { get; set; }

    /// <summary>
    /// 换算偏移量（默认 0，用于摄氏度/华氏度等）.
    /// </summary>
    [SugarColumn(ColumnName = "F_OFFSET", ColumnDataType = "DECIMAL(18,10)")]
    public decimal Offset { get; set; } = 0;

    /// <summary>
    /// 该单位推荐的显示精度（小数位数）.
    /// </summary>
    [SugarColumn(ColumnName = "F_PRECISION")]
    public int Precision { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE", IsNullable = true)]
    public long? SortCode { get; set; }
}
