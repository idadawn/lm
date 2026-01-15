using Mapster;
using Microsoft.AspNetCore.Mvc;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Lab.Entity.Dto.AppearanceFeature;
using Poxiao.Lab.Entity.Entity;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// 特性学习服务 - 闭环学习机制
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "feature-learning", Order = 105)]
[Route("api/lab/feature-learning")]
public class FeatureLearningService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<AppearanceFeatureCorrectionEntity> _correctionRepository;
    private readonly ISqlSugarRepository<AppearanceFeatureEntity> _featureRepository;
    private readonly ISqlSugarRepository<AppearanceFeatureCategoryEntity> _categoryRepository;

    public FeatureLearningService(
        ISqlSugarRepository<AppearanceFeatureCorrectionEntity> correctionRepository,
        ISqlSugarRepository<AppearanceFeatureEntity> featureRepository,
        ISqlSugarRepository<AppearanceFeatureCategoryEntity> categoryRepository
    )
    {
        _correctionRepository = correctionRepository;
        _featureRepository = featureRepository;
        _categoryRepository = categoryRepository;
    }

    /// <summary>
    /// 分析修正记录，提取潜在的新关键词
    /// </summary>
    [HttpGet("suggested-keywords")]
    public async Task<List<KeywordSuggestion>> GetSuggestedKeywords()
    {
        // 获取最近的人工修正记录（最多100条）
        var corrections = await _correctionRepository
            .AsQueryable()
            .Where(c => c.DeleteMark == null)
            .OrderByDescending(c => c.CreatorTime)
            .Take(100)
            .ToListAsync();

        // 获取所有特性及其已有关键词
        var features = await _featureRepository
            .AsQueryable()
            .Where(f => f.DeleteMark == null)
            .ToListAsync();

        // 获取所有大类，建立ID到名称的映射
        var categories = await _categoryRepository
            .AsQueryable()
            .Where(c => c.DeleteMark == null)
            .ToListAsync();
        var categoryIdToName = categories.ToDictionary(c => c.Id, c => c.Name);

        var suggestions = new List<KeywordSuggestion>();

        // 按特性分组
        var correctionsByFeature = corrections.GroupBy(c => c.CorrectedFeatureId);

        foreach (var group in correctionsByFeature)
        {
            var featureId = group.Key;
            var feature = features.FirstOrDefault(f => f.Id == featureId);

            if (feature == null)
                continue;

            // 获取大类名称
            var categoryName =
                !string.IsNullOrEmpty(feature.CategoryId)
                && categoryIdToName.ContainsKey(feature.CategoryId)
                    ? categoryIdToName[feature.CategoryId]
                    : null;

            // 获取该特性已有的关键词
            var existingKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 添加特性名称
            if (!string.IsNullOrEmpty(feature.Name))
                existingKeywords.Add(feature.Name);

            // 添加已有关键词
            if (!string.IsNullOrEmpty(feature.Keywords))
            {
                try
                {
                    var keywords = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(
                        feature.Keywords
                    );
                    if (keywords != null)
                    {
                        foreach (var kw in keywords)
                            existingKeywords.Add(kw);
                    }
                }
                catch
                {
                    // 如果不是JSON格式，尝试逗号分隔
                    var keywords = feature
                        .Keywords.Split(',', '，')
                        .Select(k => k.Trim())
                        .Where(k => !string.IsNullOrEmpty(k));
                    foreach (var kw in keywords)
                        existingKeywords.Add(kw);
                }
            }

            // 分析输入文本，提取潜在的新关键词
            var inputTexts = group.Select(c => c.InputText).ToList();
            var potentialKeywords = new Dictionary<string, int>();

            foreach (var inputText in inputTexts)
            {
                if (string.IsNullOrWhiteSpace(inputText))
                    continue;

                // 清洗文本：移除程度词
                var cleanedText = RemoveDegreeWords(inputText);

                // 如果清洗后的文本不在已有关键词中，且不为空
                if (
                    !string.IsNullOrWhiteSpace(cleanedText)
                    && !existingKeywords.Contains(cleanedText)
                )
                {
                    if (potentialKeywords.ContainsKey(cleanedText))
                        potentialKeywords[cleanedText]++;
                    else
                        potentialKeywords[cleanedText] = 1;
                }
            }

            // 只推荐出现次数 >= 2 的关键词
            foreach (var kvp in potentialKeywords.Where(k => k.Value >= 2))
            {
                suggestions.Add(
                    new KeywordSuggestion
                    {
                        FeatureId = featureId,
                        FeatureName = feature.Name,
                        FeatureCategory = categoryName,
                        SuggestedKeyword = kvp.Key,
                        Frequency = kvp.Value,
                        Status = "pending",
                    }
                );
            }
        }

        return suggestions.OrderByDescending(s => s.Frequency).ToList();
    }

    /// <summary>
    /// 批量应用关键词建议
    /// </summary>
    [HttpPost("apply-keywords")]
    public async Task ApplyKeywordSuggestions([FromBody] ApplyKeywordsInput input)
    {
        if (input.Suggestions == null || !input.Suggestions.Any())
        {
            throw Oops.Oh("没有要应用的关键词建议");
        }

        foreach (var suggestion in input.Suggestions)
        {
            var feature = await _featureRepository.GetFirstAsync(f =>
                f.Id == suggestion.FeatureId && f.DeleteMark == null
            );

            if (feature == null)
                continue;

            // 获取已有关键词
            var existingKeywords = new List<string>();
            if (!string.IsNullOrEmpty(feature.Keywords))
            {
                try
                {
                    existingKeywords =
                        Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(
                            feature.Keywords
                        )
                        ?? new List<string>();
                }
                catch
                {
                    existingKeywords = feature
                        .Keywords.Split(',', '，')
                        .Select(k => k.Trim())
                        .Where(k => !string.IsNullOrEmpty(k))
                        .ToList();
                }
            }

            // 添加新关键词（去重）
            if (
                !existingKeywords.Contains(
                    suggestion.SuggestedKeyword,
                    StringComparer.OrdinalIgnoreCase
                )
            )
            {
                existingKeywords.Add(suggestion.SuggestedKeyword);

                // 序列化并更新
                feature.Keywords = Newtonsoft.Json.JsonConvert.SerializeObject(existingKeywords);
                feature.LastModify();

                await _featureRepository
                    .AsUpdateable(feature)
                    .UpdateColumns(f => new
                    {
                        f.Keywords,
                        f.LastModifyUserId,
                        f.LastModifyTime,
                    })
                    .ExecuteCommandAsync();

                Console.WriteLine(
                    $"[FeatureLearning] Added keyword '{suggestion.SuggestedKeyword}' to feature '{feature.Name}'"
                );
            }
        }
    }

    /// <summary>
    /// 获取学习统计信息
    /// </summary>
    [HttpGet("learning-stats")]
    public async Task<LearningStats> GetLearningStats()
    {
        var totalCorrections = await _correctionRepository.CountAsync(c => c.DeleteMark == null);
        var recentCorrections = await _correctionRepository.CountAsync(c =>
            c.DeleteMark == null && c.CreatorTime >= DateTime.Now.AddDays(-30)
        );

        var manualCorrections = await _correctionRepository.CountAsync(c =>
            c.DeleteMark == null && c.MatchMode == "manual"
        );

        var autoCorrections = await _correctionRepository.CountAsync(c =>
            c.DeleteMark == null && c.MatchMode == "auto"
        );

        return new LearningStats
        {
            TotalCorrections = totalCorrections,
            RecentCorrections = recentCorrections,
            ManualCorrections = manualCorrections,
            AutoCorrections = autoCorrections,
            AccuracyRate =
                totalCorrections > 0 ? (double)autoCorrections / totalCorrections * 100 : 0,
        };
    }

    /// <summary>
    /// 获取人工修正列表
    /// </summary>
    /// <summary>
    /// 获取人工修正列表
    /// </summary>
    [HttpGet("corrections")]
    public async Task<List<CorrectionListOutput>> GetCorrectionList()
    {
        var list = await _correctionRepository
            .AsQueryable()
            .Where(c => c.DeleteMark == null)
            .OrderByDescending(c => c.CreatorTime)
            .ToListAsync();

        // 填充特性名称
        // 填充特性名称
        var featureIds = list.Select(c => c.CorrectedFeatureId)
            .Concat(list.Select(c => c.AutoMatchedFeatureId))
            .Where(id => !string.IsNullOrEmpty(id))
            .Distinct()
            .ToList();

        var features = new List<AppearanceFeatureEntity>();
        if (featureIds.Any())
        {
            features = await _featureRepository
                .AsQueryable()
                .Where(f => featureIds.Contains(f.Id))
                .ToListAsync();
        }
        var featureDict = features.ToDictionary(f => f.Id, f => f.Name);

        var result = new List<CorrectionListOutput>();
        foreach (var item in list)
        {
            var dto = item.Adapt<CorrectionListOutput>();

            // 填充人工修正特征名称
            if (
                !string.IsNullOrEmpty(item.CorrectedFeatureId)
                && featureDict.ContainsKey(item.CorrectedFeatureId)
            )
            {
                dto.CorrectedFeatureName = featureDict[item.CorrectedFeatureId];
            }
            else
            {
                dto.CorrectedFeatureName = "-";
            }

            // 填充自动匹配特征名称
            if (
                !string.IsNullOrEmpty(item.AutoMatchedFeatureId)
                && featureDict.ContainsKey(item.AutoMatchedFeatureId)
            )
            {
                dto.AutoMatchedFeatureName = featureDict[item.AutoMatchedFeatureId];
            }
            else
            {
                dto.AutoMatchedFeatureName = "-";
            }

            // 填充状态
            dto.Status = !string.IsNullOrEmpty(item.Status) ? item.Status : "Pending";

            // 填充匹配模式文本
            dto.MatchModeText = item.MatchMode switch
            {
                "ai_proposal" => "AI建议",
                "manual" => "人工添加",
                "create" => "新建",
                _ => item.MatchMode,
            };

            result.Add(dto);
        }
        return result;
    }

    /// <summary>
    /// 删除修正记录
    /// </summary>
    [HttpDelete("corrections/{id}")]
    public async Task DeleteCorrection(string id)
    {
        await _correctionRepository.DeleteByIdAsync(id);
    }

    /// <summary>
    /// 确认修正记录（添加为关键词）
    /// </summary>
    [HttpPost("corrections/{id}/confirm")]
    public async Task ConfirmCorrection(string id)
    {
        var correction = await _correctionRepository.GetFirstAsync(c => c.Id == id);
        if (correction == null)
            throw Oops.Oh("记录不存在");

        var feature = await _featureRepository.GetFirstAsync(f =>
            f.Id == correction.CorrectedFeatureId
        );
        if (feature == null)
            throw Oops.Oh("关联特性不存在");

        // 添加关键词
        var existingKeywords = new List<string>();
        if (!string.IsNullOrEmpty(feature.Keywords))
        {
            try
            {
                existingKeywords =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(feature.Keywords)
                    ?? new List<string>();
            }
            catch
            {
                existingKeywords = feature
                    .Keywords.Split(',', '，')
                    .Select(k => k.Trim())
                    .Where(k => !string.IsNullOrEmpty(k))
                    .ToList();
            }
        }

        // 清洗输入文本（移除程度词）
        var keywordToAdd = RemoveDegreeWords(correction.InputText);

        if (
            !string.IsNullOrWhiteSpace(keywordToAdd)
            && !existingKeywords.Contains(keywordToAdd, StringComparer.OrdinalIgnoreCase)
        )
        {
            existingKeywords.Add(keywordToAdd);
            feature.Keywords = Newtonsoft.Json.JsonConvert.SerializeObject(existingKeywords);
            feature.LastModify();

            await _featureRepository
                .AsUpdateable(feature)
                .UpdateColumns(f => new
                {
                    f.Keywords,
                    f.LastModifyUserId,
                    f.LastModifyTime,
                })
                .ExecuteCommandAsync();
        }

        // 更新状态为已确认
        correction.Status = "Confirmed";
        correction.LastModify();
        await _correctionRepository
            .AsUpdateable(correction)
            .UpdateColumns(c => new
            {
                c.Status,
                c.LastModifyTime,
                c.LastModifyUserId,
            })
            .ExecuteCommandAsync();
    }

    /// <summary>
    /// 更新修正记录（关联特性）
    /// </summary>
    [HttpPut("corrections/{id}")]
    public async Task UpdateCorrection(string id, [FromBody] UpdateCorrectionInput input)
    {
        var correction = await _correctionRepository.GetFirstAsync(c => c.Id == id);
        if (correction == null)
            throw Oops.Oh("记录不存在");

        if (correction.Status == "Confirmed")
            throw Oops.Oh("已确认的记录无法修改");

        correction.CorrectedFeatureId = input.FeatureId;
        correction.Status = "Pending"; // 修改后重置为待处理
        correction.LastModify();

        await _correctionRepository
            .AsUpdateable(correction)
            .UpdateColumns(c => new
            {
                c.CorrectedFeatureId,
                c.Status,
                c.LastModifyTime,
                c.LastModifyUserId,
            })
            .ExecuteCommandAsync();
    }

    /// <summary>
    /// 移除程度词
    /// </summary>
    private string RemoveDegreeWords(string text)
    {
        var degreeWords = new[]
        {
            "微",
            "轻微",
            "严重",
            "超级",
            "非常",
            "很",
            "极",
            "特别",
            "有些",
            "稍",
            "稍微",
            "略",
        };

        foreach (var word in degreeWords)
        {
            text = text.Replace(word, "");
        }

        return text.Trim();
    }
}

/// <summary>
/// 关键词建议
/// </summary>
public class KeywordSuggestion
{
    /// <summary>
    /// 特性ID
    /// </summary>
    public string FeatureId { get; set; }

    /// <summary>
    /// 特性名称
    /// </summary>
    public string FeatureName { get; set; }

    /// <summary>
    /// 特性大类
    /// </summary>
    public string FeatureCategory { get; set; }

    /// <summary>
    /// 建议的关键词
    /// </summary>
    public string SuggestedKeyword { get; set; }

    /// <summary>
    /// 出现频率
    /// </summary>
    public int Frequency { get; set; }

    /// <summary>
    /// 状态（pending=待处理, applied=已应用, rejected=已拒绝）
    /// </summary>
    public string Status { get; set; }
}

public class UpdateCorrectionInput
{
    public string FeatureId { get; set; }
}

/// <summary>
/// 应用关键词输入
/// </summary>
public class ApplyKeywordsInput
{
    public List<KeywordSuggestion> Suggestions { get; set; }
}

/// <summary>
/// 学习统计信息
/// </summary>
public class LearningStats
{
    /// <summary>
    /// 总修正次数
    /// </summary>
    public int TotalCorrections { get; set; }

    /// <summary>
    /// 最近30天修正次数
    /// </summary>
    public int RecentCorrections { get; set; }

    /// <summary>
    /// 手动修正次数
    /// </summary>
    public int ManualCorrections { get; set; }

    /// <summary>
    /// 自动匹配次数
    /// </summary>
    public int AutoCorrections { get; set; }

    /// <summary>
    /// 准确率（自动匹配占比）
    /// </summary>
    public double AccuracyRate { get; set; }
}
