using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Lab.Entity.Entity;

/// <summary>
/// 产品规格扩展属性.
/// </summary>
[SugarTable("LAB_PRODUCT_SPEC_ATTRIBUTE")]
[Tenant(ClaimConst.TENANTID)]
public class ProductSpecAttributeEntity : CLDEntityBase
{
    /// <summary>
    /// 产品规格ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_PRODUCT_SPEC_ID", IsNullable = false)]
    public string ProductSpecId { get; set; }

    /// <summary>
    /// 属性名称（如：长度、层数、密度）.
    /// </summary>
    [SugarColumn(ColumnName = "F_ATTRIBUTE_NAME", IsNullable = false, Length = 100)]
    public string AttributeName { get; set; }

    /// <summary>
    /// 属性键名（如：length、layers、density）.
    /// </summary>
    [SugarColumn(ColumnName = "F_ATTRIBUTE_KEY", IsNullable = false, Length = 100)]
    public string AttributeKey { get; set; }

    /// <summary>
    /// 属性值类型（decimal、int、text）.
    /// </summary>
    [SugarColumn(ColumnName = "F_VALUE_TYPE", IsNullable = false, Length = 20)]
    public string ValueType { get; set; }

    /// <summary>
    /// 属性值（存储为字符串，使用时根据类型转换）.
    /// </summary>
    [SugarColumn(ColumnName = "F_ATTRIBUTE_VALUE", IsNullable = true, Length = 500)]
    public string AttributeValue { get; set; }

    /// <summary>
    /// 属性单位（如：m、mm、kg、MPa、%）.
    /// </summary>
    [SugarColumn(ColumnName = "F_UNIT", IsNullable = true, Length = 20)]
    public string Unit { get; set; }

    /// <summary>
    /// 单位ID（关联单位定义表）.
    /// </summary>
    [SugarColumn(ColumnName = "F_UNIT_ID", IsNullable = true, Length = 50)]
    public string UnitId { get; set; }

    /// <summary>
    /// 精度（仅用于数字类型，小数位数）.
    /// </summary>
    [SugarColumn(ColumnName = "F_PRECISION", IsNullable = true)]
    public int? Precision { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }

    /// <summary>
    /// 重写基类的 EnabledMark 属性，忽略数据库映射（表中无此字段）.
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public override int? EnabledMark { get; set; }

    /// <summary>
    /// 版本号（从1开始递增）.
    /// </summary>
    [SugarColumn(ColumnName = "F_VERSION", IsNullable = false)]
    public int Version { get; set; } = 1;

    /// <summary>
    /// 版本创建时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_VERSION_CREATE_TIME", IsNullable = true)]
    public DateTime? VersionCreateTime { get; set; }

    /// <summary>
    /// 版本说明（记录变更原因）.
    /// </summary>
    [SugarColumn(ColumnName = "F_VERSION_DESCRIPTION", Length = 500, IsNullable = true)]
    public string VersionDescription { get; set; }
}
