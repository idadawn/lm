using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Lab.Entity.Entity;

/// <summary>
/// 外观特性大类定义.
/// </summary>
[SugarTable("LAB_APPEARANCE_FEATURE_CATEGORY")]
[Tenant(ClaimConst.TENANTID)]
public class AppearanceFeatureCategoryEntity : CLDEntityBase
{
    /// <summary>
    /// 大类名称（如 韧性、脆边、麻点）.
    /// </summary>
    [SugarColumn(ColumnName = "F_NAME", Length = 100, IsNullable = false)]
    public string Name { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION", Length = 500, IsNullable = true)]
    public string Description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }

    /// <summary>
    /// 父级分类ID（为空表示顶级分类）.
    /// </summary>
    [SugarColumn(ColumnName = "F_PARENTID", Length = 50, IsNullable = true)]
    public string ParentId { get; set; }

    /// <summary>
    /// 根分类ID（最顶层分类的ID，用于快速追溯到顶层分类）.
    /// </summary>
    [SugarColumn(ColumnName = "F_ROOTID", Length = 50, IsNullable = true)]
    public string RootId { get; set; }

    /// <summary>
    /// 分类路径（从根分类到当前分类的完整路径，用逗号分隔ID，如："rootId,parentId,currentId"）.
    /// </summary>
    [SugarColumn(ColumnName = "F_PATH", Length = 500, IsNullable = true)]
    public string Path { get; set; }
}
