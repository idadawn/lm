using System.Text.Json.Serialization;

namespace Poxiao.Lab.Entity.Dto.IntermediateDataFormula;

/// <summary>
/// 中间数据表可用列信息及相关配置数据结果.
/// </summary>
public class IntermediateDataAvailableColumnsResult
{
    /// <summary>
    /// 可用列列表.
    /// </summary>
    [JsonPropertyName("columns")]
    public List<IntermediateDataColumnInfo> Columns { get; set; }

    /// <summary>
    /// 外观特性大类列表.
    /// </summary>
    [JsonPropertyName("featureCategories")]
    public List<AppearanceFeatureCategoryEntity> FeatureCategories { get; set; }

    /// <summary>
    /// 外观特性等级列表.
    /// </summary>
    [JsonPropertyName("featureLevels")]
    public List<AppearanceFeatureLevelEntity> FeatureLevels { get; set; }
}
