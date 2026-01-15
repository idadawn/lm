using System.Text.Json.Serialization;
using Poxiao.Infrastructure.Contracts;
using Poxiao.Infrastructure.Filter;
using Poxiao.Lab.Entity.Entity;

namespace Poxiao.Lab.Entity.Dto.AppearanceFeature;

public class AppearanceFeatureCrInput : AppearanceFeatureEntity
{
}

public class AppearanceFeatureUpInput : AppearanceFeatureEntity
{
}

public class AppearanceFeatureListQuery : PageInputBase
{
    public string Keyword { get; set; }
    public string CategoryId { get; set; }
    public string Name { get; set; }
    public string SeverityLevelId { get; set; }
}

public class AppearanceFeatureListOutput : AppearanceFeatureEntity
{
    /// <summary>
    /// 特性大类名称（用于前端展示）
    /// </summary>
    [JsonPropertyName("category")]
    public string Category { get; set; }

    /// <summary>
    /// 特性等级名称（用于前端展示）
    /// </summary>
    [JsonPropertyName("severityLevel")]
    public string SeverityLevel { get; set; }
}

public class AppearanceFeatureInfoOutput : AppearanceFeatureEntity
{
    /// <summary>
    /// 特性大类名称（用于前端展示）
    /// </summary>
    [JsonPropertyName("category")]
    public string Category { get; set; }

    /// <summary>
    /// 特性等级名称（用于前端展示）
    /// </summary>
    [JsonPropertyName("severityLevel")]
    public string SeverityLevel { get; set; }

    /// <summary>
    /// 匹配方式（name/keyword/ai/fuzzy）
    /// </summary>
    [JsonPropertyName("matchMethod")]
    public string MatchMethod { get; set; }

    /// <summary>
    /// 识别到的程度词
    /// </summary>
    [JsonPropertyName("degreeWord")]
    public string DegreeWord { get; set; }

    /// <summary>
    /// 是否需要确认等级（当程度词匹配不到对应记录时，需要用户确认是否创建新记录）
    /// </summary>
    [JsonPropertyName("requiresSeverityConfirmation")]
    public bool RequiresSeverityConfirmation { get; set; }

    /// <summary>
    /// 建议的等级名称（当需要确认时，显示给用户）
    /// </summary>
    [JsonPropertyName("suggestedSeverity")]
    public string SuggestedSeverity { get; set; }

    /// <summary>
    /// 分类路径（从顶级分类到当前分类的完整路径，用➡️分隔，如："韧性➡️脆"）
    /// </summary>
    [JsonPropertyName("categoryPath")]
    public string CategoryPath { get; set; }

    /// <summary>
    /// 最顶级的分类名称（用于前端展示特征大类）
    /// </summary>
    [JsonPropertyName("rootCategory")]
    public string RootCategory { get; set; }

    /// <summary>
    /// 是否存在该特性（名称+等级组合），当需要确认等级时使用
    /// </summary>
    [JsonPropertyName("featureExists")]
    public bool FeatureExists { get; set; }

    /// <summary>
    /// 已存在的特性ID（当FeatureExists为true时，用于添加到关键词列表）
    /// </summary>
    [JsonPropertyName("existingFeatureId")]
    public string ExistingFeatureId { get; set; }

    /// <summary>
    /// 建议的特性名称（用于创建新特性，格式：程度词+特性名称，如"严重脆"）
    /// </summary>
    [JsonPropertyName("suggestedFeatureName")]
    public string SuggestedFeatureName { get; set; }

    /// <summary>
    /// 建议的等级是否存在（当需要确认等级时，如果等级不存在，需要先创建等级）
    /// </summary>
    [JsonPropertyName("severityLevelExists")]
    public bool SeverityLevelExists { get; set; }
}

/// <summary>
/// 人工修正记录输入
/// </summary>
public class AppearanceFeatureCorrectionInput
{
    /// <summary>
    /// 原始输入文本
    /// </summary>
    public string InputText { get; set; }

    /// <summary>
    /// 自动匹配的特征ID（如果有）
    /// </summary>
    public string AutoMatchedFeatureId { get; set; }

    /// <summary>
    /// 人工修正后的特征ID
    /// </summary>
    public string CorrectedFeatureId { get; set; }

    /// <summary>
    /// 匹配模式（auto/manual/create）
    /// </summary>
    public string MatchMode { get; set; }

    /// <summary>
    /// 使用场景（test/import）
    /// </summary>
    public string Scenario { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; }
}
