using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Poxiao.Lab.Entity.Dto.AppearanceFeature;

/// <summary>
/// 匹配输入（单个）
/// </summary>
public class MatchInput
{
    /// <summary>
    /// 输入文本
    /// </summary>
    public string Text { get; set; }
}

/// <summary>
/// 批量匹配输入项
/// </summary>
public class MatchItemInput
{
    /// <summary>
    /// 唯一标识
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// 输入文字
    /// </summary>
    [JsonPropertyName("query")]
    public string Query { get; set; }
}

/// <summary>
/// 批量匹配输入
/// </summary>
public class BatchMatchInput
{
    /// <summary>
    /// 匹配项列表
    /// </summary>
    [JsonPropertyName("items")]
    public List<MatchItemInput> Items { get; set; }
}

/// <summary>
/// 匹配结果项
/// </summary>
public class MatchItemOutput
{
    /// <summary>
    /// 唯一标识（对应输入）
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// 输入文字（对应输入）
    /// </summary>
    [JsonPropertyName("query")]
    public string Query { get; set; }

    /// <summary>
    /// 特性大类
    /// </summary>
    [JsonPropertyName("category")]
    public string Category { get; set; }

    /// <summary>
    /// 特性分类（分类路径）
    /// </summary>
    [JsonPropertyName("categoryPath")]
    public string CategoryPath { get; set; }

    /// <summary>
    /// 特性名称
    /// </summary>
    [JsonPropertyName("featureName")]
    public string FeatureName { get; set; }

    /// <summary>
    /// 特性等级
    /// </summary>
    [JsonPropertyName("severityLevel")]
    public string SeverityLevel { get; set; }

    /// <summary>
    /// 匹配方式（name/keyword/ai/none）
    /// </summary>
    [JsonPropertyName("matchMethod")]
    public string MatchMethod { get; set; }

    /// <summary>
    /// 是否100%匹配（name或keyword匹配时为true，AI匹配时为false需用户确认）
    /// </summary>
    [JsonPropertyName("isPerfectMatch")]
    public bool IsPerfectMatch { get; set; }

    /// <summary>
    /// 人工修正匹配列表（当IsPerfectMatch为false时，需要用户确认选择）
    /// </summary>
    [JsonPropertyName("manualCorrections")]
    public List<ManualCorrectionOption> ManualCorrections { get; set; }

    /// <summary>
    /// 错误信息（当匹配失败时返回给前端用户展示）
    /// </summary>
    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; }
}

/// <summary>
/// 人工修正选项（需要用户确认）
/// </summary>
public class ManualCorrectionOption
{
    /// <summary>
    /// 特性ID
    /// </summary>
    [JsonPropertyName("featureId")]
    public string FeatureId { get; set; }

    /// <summary>
    /// 特性大类
    /// </summary>
    [JsonPropertyName("category")]
    public string Category { get; set; }

    /// <summary>
    /// 特性分类（分类路径）
    /// </summary>
    [JsonPropertyName("categoryPath")]
    public string CategoryPath { get; set; }

    /// <summary>
    /// 特性名称
    /// </summary>
    [JsonPropertyName("featureName")]
    public string FeatureName { get; set; }

    /// <summary>
    /// 特性等级
    /// </summary>
    [JsonPropertyName("severityLevel")]
    public string SeverityLevel { get; set; }

    /// <summary>
    /// 建议操作类型（add_keyword/select_existing）
    /// add_keyword: AI识别到特性存在，建议用户确认后将输入文字添加到关键词
    /// select_existing: 需要用户手动选择对应特性
    /// </summary>
    [JsonPropertyName("actionType")]
    public string ActionType { get; set; }

    /// <summary>
    /// 建议说明
    /// </summary>
    [JsonPropertyName("suggestion")]
    public string Suggestion { get; set; }

    /// <summary>
    /// 修正记录ID (用于确认操作)
    /// </summary>
    [JsonPropertyName("correctionId")]
    public string CorrectionId { get; set; }
}
