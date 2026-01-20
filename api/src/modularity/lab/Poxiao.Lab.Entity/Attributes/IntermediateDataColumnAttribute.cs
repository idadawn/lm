using System;

namespace Poxiao.Lab.Entity.Attributes;

/// <summary>
/// 中间数据表列特性.
/// 用于标识中间数据表中的列，用于公式维护和数据列名返回.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class IntermediateDataColumnAttribute : Attribute
{
    /// <summary>
    /// 列显示名称（中文名称）.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// 是否可用于公式计算（数字类型字段）.
    /// </summary>
    public bool IsCalculable { get; set; } = true;

    /// <summary>
    /// 排序码（用于界面显示顺序）.
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 数据类型（decimal、int、string、datetime等）.
    /// </summary>
    public string DataType { get; set; }

    /// <summary>
    /// 是否在公式维护中显示（默认true）.
    /// </summary>
    public bool ShowInFormulaMaintenance { get; set; } = true;

    /// <summary>
    /// 列描述/备注.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 是否忽略（不映射到数据库）.
    /// </summary>
    public bool IsIgnore { get; set; } = false;

    /// <summary>
    /// 小数位数（精度，用于 decimal 类型）.
    /// </summary>
    public int? DecimalDigits { get; set; }

    /// <summary>
    /// 列描述（用于数据库注释）.
    /// </summary>
    public string ColumnDescription { get; set; }

    /// <summary>
    /// 单位（如：VA/kg、W/kg、A/m等）.
    /// </summary>
    public string Unit { get; set; }

    /// <summary>
    /// 构造函数.
    /// </summary>
    /// <param name="displayName">列显示名称</param>
    /// <param name="sort">排序码</param>
    /// <param name="isCalculable">是否可用于公式计算</param>
    /// <param name="dataType">数据类型</param>
    /// <param name="showInFormulaMaintenance">是否在公式维护中显示</param>
    /// <param name="description">列描述</param>
    /// <param name="isIgnore">是否忽略（不映射到数据库）</param>
    /// <param name="columnDescription">列描述（用于数据库注释）</param>
    /// <param name="unit">单位</param>
    /// <remarks>
    /// 注意：decimalDigits 不能作为构造函数参数（C# 特性不支持可空类型），只能通过属性设置。
    /// </remarks>
    public IntermediateDataColumnAttribute(
        string displayName,
        int sort = 0,
        bool isCalculable = true,
        string dataType = "decimal",
        bool showInFormulaMaintenance = true,
        string description = null,
        bool isIgnore = false,
        string columnDescription = null,
        string unit = null)
    {
        DisplayName = displayName;
        Sort = sort;
        IsCalculable = isCalculable;
        DataType = dataType ?? "decimal";
        ShowInFormulaMaintenance = showInFormulaMaintenance;
        Description = description;
        IsIgnore = isIgnore;
        ColumnDescription = columnDescription;
        Unit = unit;
    }
}
