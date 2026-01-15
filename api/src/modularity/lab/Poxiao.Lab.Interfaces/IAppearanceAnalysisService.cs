using Poxiao.Lab.Entity.Dto.AppearanceFeature;

namespace Poxiao.Lab.Interfaces;

public interface IAppearanceAnalysisService
{
    /// <summary>
    /// 使用AI分析外观特性描述
    /// </summary>
    /// <param name="featureSuffix">特性后缀 (e.g. "脆")</param>
    /// <returns>标准化分类结果</returns>
    Task<FeatureClassification> DefineAppearanceFeatureAsync(string featureSuffix);
}
