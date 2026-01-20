using System.Text.Json.Serialization;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Lab.Entity;

/// <summary>
/// 外观特性等级定义.
/// </summary>
[SugarTable("LAB_APPEARANCE_FEATURE_LEVEL")]
[Tenant(ClaimConst.TENANTID)]
public class AppearanceFeatureLevelEntity : CLDEntityBase
{
    /// <summary>
    /// 等级名称（如: 微、轻微、中等、严重、超级，唯一，不能重复）.
    /// </summary>
    [SugarColumn(ColumnName = "F_NAME", IsNullable = false, Length = 50)]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 等级描述（可为空）.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION", IsNullable = true, Length = 200)]
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    [JsonPropertyName("sortCode")]
    public long? SortCode { get; set; }

    /// <summary>
    /// 是否启用.
    /// </summary>
    [SugarColumn(ColumnName = "F_ENABLED")]
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 是否默认.
    /// </summary>
    [SugarColumn(ColumnName = "F_ISDEFAULT")]
    [JsonPropertyName("isDefault")]
    public bool IsDefault { get; set; } = false;
}
