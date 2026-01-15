using Poxiao.AI.Interfaces;
using Poxiao.DependencyInjection;
using Poxiao.Lab.Entity.Dto.AppearanceFeature;
using Poxiao.Lab.Entity.Entity;
using Poxiao.Lab.Interfaces;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// 外观特性分析服务 - 业务逻辑层
/// 负责从数据库获取动态数据，并调用AI模块进行分析
/// </summary>
public class AppearanceAnalysisService : IAppearanceAnalysisService, ITransient
{
    private readonly IAppearanceFeatureAnalysisService _aiAnalysisService;
    private readonly ISqlSugarRepository<AppearanceFeatureCategoryEntity> _categoryRepository;
    private readonly ISqlSugarRepository<AppearanceFeatureEntity> _featureRepository;
    private readonly ISqlSugarRepository<AppearanceFeatureLevelEntity> _featureLevelRepository;

    public AppearanceAnalysisService(
        IAppearanceFeatureAnalysisService aiAnalysisService,
        ISqlSugarRepository<AppearanceFeatureCategoryEntity> categoryRepository,
        ISqlSugarRepository<AppearanceFeatureEntity> featureRepository,
        ISqlSugarRepository<AppearanceFeatureLevelEntity> featureLevelRepository
    )
    {
        _aiAnalysisService = aiAnalysisService;
        _categoryRepository = categoryRepository;
        _featureRepository = featureRepository;
        _featureLevelRepository = featureLevelRepository;
    }

    /// <inheritdoc />
    public async Task<FeatureClassification> DefineAppearanceFeatureAsync(string featureSuffix)
    {
        if (string.IsNullOrEmpty(featureSuffix))
            return null;

        try
        {
            // 从数据库动态获取所有特性
            var allFeatures = await _featureRepository
                .AsQueryable()
                .Where(f =>
                    f.DeleteMark == null
                    && !string.IsNullOrEmpty(f.Name)
                    && !string.IsNullOrEmpty(f.CategoryId)
                )
                .ToListAsync();

            // 获取所有大类，建立ID到名称的映射
            var allCategories = await _categoryRepository
                .AsQueryable()
                .Where(c => c.DeleteMark == null)
                .ToListAsync();
            var categoryIdToName = allCategories.ToDictionary(c => c.Id, c => c.Name);

            // 按照特性大类分组组织（使用名称）
            // 先按大类分组特征
            var featuresByCategory = allFeatures
                .Where(f => categoryIdToName.ContainsKey(f.CategoryId))
                .GroupBy(f => categoryIdToName[f.CategoryId])
                .ToDictionary(g => g.Key, g => g.Select(f => f.Name).Distinct().ToList());

            // 确保所有大类都包含在字典中，即使没有特征定义也显示（空列表）
            var categoryFeatures = categoryIdToName.Values.ToDictionary(
                categoryName => categoryName,
                categoryName =>
                    featuresByCategory.ContainsKey(categoryName)
                        ? featuresByCategory[categoryName]
                        : new List<string>()
            );

            // 从数据库动态获取启用的外观特性等级
            var featureLevelEntities = await _featureLevelRepository
                .AsQueryable()
                .Where(s => s.Enabled == true && s.DeleteMark == null)
                .OrderBy(s => s.SortCode)
                .ToListAsync();

            var featureLevels = featureLevelEntities.Select(s => s.Name).ToList();
            var defaultLevel =
                featureLevelEntities.FirstOrDefault(s => s.IsDefault)?.Name ?? "默认";

            // 调用AI模块的分析服务
            var aiResult = await _aiAnalysisService.AnalyzeAsync(
                featureSuffix,
                categoryFeatures,
                featureLevels,
                defaultLevel
            );

            if (!aiResult.Success || aiResult.Features == null || !aiResult.Features.Any())
            {
                Console.WriteLine($"[AppearanceAnalysis] AI分析失败: {aiResult.ErrorMessage}");
                return null;
            }

            // 转换为业务层的DTO格式
            var features = aiResult
                .Features.Select(f => new Poxiao.Lab.Entity.Dto.AppearanceFeature.AIFeatureItem
                {
                    Name = f.Name,
                    Level = f.Level,
                    Category = f.Category,
                })
                .ToList();

            // 返回兼容格式
            return new FeatureClassification
            {
                Features = features,
                MainCategory = features[0].Category,
                SubCategory = features[0].Name,
                Severity = features[0].Level,
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AppearanceAnalysis] Error: {ex.Message}");
            Console.WriteLine($"[AppearanceAnalysis] StackTrace: {ex.StackTrace}");
            return null;
        }
    }
}
