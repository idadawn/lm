using System.Text.Json.Serialization;
using Poxiao.Infrastructure.Contracts;
using Poxiao.Infrastructure.Filter;
using Poxiao.Infrastructure.Security;
using Poxiao.Lab.Entity.Entity;

namespace Poxiao.Lab.Entity.Dto.AppearanceFeature;

public class AppearanceFeatureCategoryCrInput : AppearanceFeatureCategoryEntity
{
}

public class AppearanceFeatureCategoryUpInput : AppearanceFeatureCategoryEntity
{
}

public class AppearanceFeatureCategoryListQuery : PageInputBase
{
    public string Keyword { get; set; }
}

public class AppearanceFeatureCategoryListOutput : TreeModel
{
    /// <summary>
    /// 大类名称（如 韧性、脆边、麻点）.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [JsonPropertyName("sortCode")]
    public long? SortCode { get; set; }

    /// <summary>
    /// 该大类下的特性数量
    /// </summary>
    [JsonPropertyName("featureCount")]
    public int FeatureCount { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [JsonPropertyName("creatorTime")]
    public DateTime? CreatorTime { get; set; }

    /// <summary>
    /// 根分类ID（最顶层分类的ID）
    /// </summary>
    [JsonPropertyName("rootId")]
    public string RootId { get; set; }

    /// <summary>
    /// 分类路径（从根分类到当前分类的完整路径，用逗号分隔ID）
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; }
}

public class AppearanceFeatureCategoryInfoOutput : AppearanceFeatureCategoryEntity
{
    /// <summary>
    /// 该大类下的特性数量
    /// </summary>
    [JsonPropertyName("featureCount")]
    public int FeatureCount { get; set; }
}
