using System;

namespace Poxiao.Lab.Entity.Attributes;

/// <summary>
/// Excel导入列特性.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class ExcelImportColumnAttribute : Attribute
{
    /// <summary>
    /// Excel列名/显示名称.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 是否为导入字段.
    /// </summary>
    public bool IsImportField { get; set; } = true;

    /// <summary>
    /// 排序码.
    /// </summary>
    public int Sort { get; set; }

    public ExcelImportColumnAttribute(string name, int sort = 0, bool isImportField = true)
    {
        Name = name;
        Sort = sort;
        IsImportField = isImportField;
    }
}
