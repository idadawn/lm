using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using Poxiao.Lab.Entity.Enums;
using SqlSugar;

namespace Poxiao.Lab.Entity;

/// <summary>
/// 中间数据表公式维护实体.
/// </summary>
[SugarTable("LAB_INTERMEDIATE_DATA_FORMULA")]
[Tenant(ClaimConst.TENANTID)]
public class IntermediateDataFormulaEntity : CLDEntityBase
{
    /// <summary>
    /// 表名（枚举：INTERMEDIATE_DATA，未来可扩展其他表）.
    /// </summary>
    [SugarColumn(ColumnName = "F_TABLE_NAME", Length = 50, IsNullable = false)]
    public string TableName { get; set; } = "INTERMEDIATE_DATA";

    /// <summary>
    /// 中间数据表列名（对应 IntermediateDataEntity 的属性名，如：OneMeterWeight、Density等）.
    /// </summary>
    [SugarColumn(ColumnName = "F_COLUMN_NAME", Length = 100, IsNullable = false)]
    public string ColumnName { get; set; }

    /// <summary>
    /// 公式名称（默认使用列名，可自定义）.
    /// </summary>
    [SugarColumn(ColumnName = "F_FORMULA_NAME", Length = 100, IsNullable = false)]
    public string FormulaName { get; set; }

    /// <summary>
    /// 计算公式表达式（支持EXCEL风格公式）.
    /// </summary>
    [SugarColumn(ColumnName = "F_FORMULA", ColumnDataType = "TEXT", IsNullable = false)]
    public string Formula { get; set; }

    /// <summary>
    /// 公式语言：EXCEL（默认）、MATH.
    /// </summary>
    [SugarColumn(ColumnName = "F_FORMULA_LANGUAGE", Length = 20, DefaultValue = "EXCEL")]
    public string FormulaLanguage { get; set; } = "EXCEL";

    /// <summary>
    /// 公式类型：CALC-计算公式，JUDGE-判定公式.
    /// </summary>
    [SugarColumn(
        ColumnName = "F_FORMULA_TYPE",
        Length = 20,
        DefaultValue = "CALC",
        IsNullable = true
    )]
    public IntermediateDataFormulaType FormulaType { get; set; } = IntermediateDataFormulaType.CALC;

    /// <summary>
    /// 单位ID（关联单位定义表）.
    /// </summary>
    [SugarColumn(ColumnName = "F_UNIT_ID", Length = 50, IsNullable = true)]
    public string UnitId { get; set; }

    /// <summary>
    /// 单位名称（用于显示，冗余字段）.
    /// </summary>
    [SugarColumn(ColumnName = "F_UNIT_NAME", Length = 50, IsNullable = true)]
    public string UnitName { get; set; }

    /// <summary>
    /// 小数点保留位数（精度）.
    /// </summary>
    [SugarColumn(ColumnName = "F_PRECISION", IsNullable = true)]
    public int? Precision { get; set; }

    /// <summary>
    /// 是否启用.
    /// </summary>
    [SugarColumn(ColumnName = "F_IS_ENABLED")]
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 排序序号.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORT_ORDER")]
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// 默认值（计算公式默认为0，判定公式默认为空）.
    /// </summary>
    [SugarColumn(ColumnName = "F_DEFAULT_VALUE", Length = 100, IsNullable = true)]
    public string DefaultValue { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "F_REMARK", Length = 1000, IsNullable = true)]
    public string Remark { get; set; }
    /// <summary>
    /// 来源类型：SYSTEM-系统默认（从列生成），CUSTOM-自定义.
    /// </summary>
    [SugarColumn(
        ColumnName = "F_SOURCE_TYPE",
        Length = 20,
        DefaultValue = "SYSTEM",
        IsNullable = false
    )]
    public string SourceType { get; set; } = "SYSTEM";
}
