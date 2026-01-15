using System.Text.Json.Serialization;

namespace Poxiao.Lab.Entity.Dto.AppearanceFeature;

/// <summary>
/// AI识别的单个特性
/// </summary>
public class AIFeatureItem
{
    /// <summary>
    /// 特性名称（如 脆、划痕）
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 特性等级（如 默认、轻微、严重、超级）
    /// </summary>
    [JsonPropertyName("level")]
    public string Level { get; set; }

    /// <summary>
    /// 特性大类（如 韧性、划痕）
    /// </summary>
    [JsonPropertyName("category")]
    public string Category { get; set; }
}

/// <summary>
/// 外观特征分类结果（兼容旧格式）
/// </summary>
public class FeatureClassification
{
    /// <summary>
    /// 主要分类（兼容旧版本，对应第一个特性的category）
    /// </summary>
    [JsonPropertyName("main_category")]
    public string MainCategory { get; set; }

    /// <summary>
    /// 子分类（兼容旧版本，对应第一个特性的name）
    /// </summary>
    [JsonPropertyName("sub_category")]
    public string SubCategory { get; set; }

    /// <summary>
    /// 严重程度（兼容旧版本，对应第一个特性的level）
    /// </summary>
    [JsonPropertyName("severity")]
    public string Severity { get; set; }

    /// <summary>
    /// AI识别的多个特性列表（新格式）
    /// </summary>
    [JsonPropertyName("features")]
    public List<AIFeatureItem> Features { get; set; }
}
