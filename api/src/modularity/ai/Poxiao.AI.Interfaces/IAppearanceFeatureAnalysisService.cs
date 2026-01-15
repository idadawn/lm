using System.Text.Json;
using System.Text.Json.Serialization;

namespace Poxiao.AI.Interfaces;

/// <summary>
/// 外观特性分析服务接口
/// </summary>
public interface IAppearanceFeatureAnalysisService
{
    /// <summary>
    /// 分析外观特性描述
    /// </summary>
    /// <param name="featureSuffix">特性描述文本</param>
    /// <param name="categoryFeatures">特性大类和特性名称的关联关系（Key: 大类名称, Value: 该大类下的特性名称列表）</param>
    /// <param name="severityLevels">严重程度等级列表（动态）</param>
    /// <returns>分析结果</returns>
    Task<AppearanceFeatureAnalysisResult> AnalyzeAsync(
        string featureSuffix,
        Dictionary<string, List<string>> categoryFeatures,
        List<string> severityLevels
    );
}

/// <summary>
/// 外观特性分析结果
/// </summary>
public class AppearanceFeatureAnalysisResult
{
    /// <summary>
    /// 识别到的特性列表
    /// </summary>
    public List<AIFeatureItem> Features { get; set; } = new();

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 错误信息（如果有）
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// AI识别的单个特性
/// </summary>
public class AIFeatureItem
{
    /// <summary>
    /// 特性名称（如 脆、划痕）
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 特性等级（如 默认、轻微、严重、超级）
    /// </summary>
    [JsonPropertyName("level")]
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// 特性大类（如 韧性、划痕）
    /// </summary>
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;
}
