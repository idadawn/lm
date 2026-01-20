using System.Text.Json.Serialization;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Lab.Entity;

/// <summary>
/// 外观特性定义.
/// 唯一性约束：特征大类 + 特性名称 + 特性等级 组合唯一
/// </summary>
[SugarTable("LAB_APPEARANCE_FEATURE")]
[Tenant(ClaimConst.TENANTID)]
public class AppearanceFeatureEntity : CLDEntityBase
{
    /// <summary>
    /// 特性大类ID（外键，关联到 LAB_APPEARANCE_FEATURE_CATEGORY 表）.
    /// </summary>
    [SugarColumn(ColumnName = "F_CATEGORY_ID", IsNullable = false, Length = 50)]
    [JsonPropertyName("categoryId")]
    public string CategoryId { get; set; }

    /// <summary>
    /// 特性名称（如 脆）.
    /// </summary>
    [SugarColumn(ColumnName = "F_NAME", IsNullable = false, Length = 100)]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 特性等级ID（外键，关联到 LAB_APPEARANCE_FEATURE_LEVEL 表）.
    /// </summary>
    [SugarColumn(ColumnName = "F_SEVERITY_LEVEL_ID", IsNullable = false, Length = 50)]
    [JsonPropertyName("severityLevelId")]
    public string SeverityLevelId { get; set; }

    /// <summary>
    /// 关键字列表（JSON数组，用于精确匹配）.
    /// 例如: ["脆", "碎", "易断", "发脆"]
    /// </summary>
    [SugarColumn(ColumnName = "F_KEYWORDS", Length = 1000, IsNullable = true)]
    [JsonPropertyName("keywords")]
    public string Keywords { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION", IsNullable = true, Length = 500)]
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    [JsonPropertyName("sortCode")]
    public long? SortCode { get; set; }
}
