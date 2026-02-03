using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Manager;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.AppearanceFeature;
using Poxiao.Lab.Interfaces;
using Poxiao.Logging;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// 外观特性服务 - 规则引擎 + AI补位的混合识别系统.
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "appearance-features", Order = 101)]
[Route("api/lab/appearance-features")]
public class AppearanceFeatureService : IAppearanceFeatureService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<AppearanceFeatureEntity> _repository;
    private readonly ISqlSugarRepository<AppearanceFeatureCorrectionEntity> _correctionRepository;
    private readonly ISqlSugarRepository<AppearanceFeatureCategoryEntity> _categoryRepository;
    private readonly ISqlSugarRepository<AppearanceFeatureLevelEntity> _featureLevelRepository;
    private readonly IConfiguration _configuration;
    private readonly IAppearanceAnalysisService _analysisService;
    private readonly IAppearanceFeatureLevelService _featureLevelService;
    private readonly ICacheManager _cacheManager;
    private readonly IUserManager _userManager;

    private const string CachePrefix = "LAB:AppearanceFeature";

    public AppearanceFeatureService(
        ISqlSugarRepository<AppearanceFeatureEntity> repository,
        ISqlSugarRepository<AppearanceFeatureCorrectionEntity> correctionRepository,
        ISqlSugarRepository<AppearanceFeatureCategoryEntity> categoryRepository,
        ISqlSugarRepository<AppearanceFeatureLevelEntity> featureLevelRepository,
        IConfiguration configuration,
        IAppearanceAnalysisService analysisService,
        IAppearanceFeatureLevelService featureLevelService,
        ICacheManager cacheManager,
        IUserManager userManager
    )
    {
        _repository = repository;
        _correctionRepository = correctionRepository;
        _categoryRepository = categoryRepository;
        _featureLevelRepository = featureLevelRepository;
        _configuration = configuration;
        _analysisService = analysisService;
        _featureLevelService = featureLevelService;
        _cacheManager = cacheManager;
        _userManager = userManager;
    }

    private string GetCacheKey(string suffix)
    {
        var tenantId = _userManager?.TenantId ?? "global";
        return $"{CachePrefix}:{tenantId}:{suffix}";
    }

    /// <summary>
    /// 填充输出DTO的名称字段（从ID关联查询）
    /// </summary>
    private async Task FillOutputNamesAsync(List<AppearanceFeatureListOutput> outputs)
    {
        if (outputs == null || !outputs.Any())
            return;

        var categoryIds = outputs
            .Select(o => o.CategoryId)
            .Distinct()
            .Where(id => !string.IsNullOrEmpty(id))
            .ToList();
        var severityLevelIds = outputs
            .Select(o => o.SeverityLevelId)
            .Distinct()
            .Where(id => !string.IsNullOrEmpty(id))
            .ToList();

        var categories = await _categoryRepository
            .AsQueryable()
            .Where(c => categoryIds.Contains(c.Id) && c.DeleteMark == null)
            .ToListAsync();

        var featureLevels = await _featureLevelRepository
            .AsQueryable()
            .Where(s => severityLevelIds.Contains(s.Id) && s.DeleteMark == null)
            .ToListAsync();

        var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);
        var featureLevelDict = featureLevels.ToDictionary(s => s.Id, s => s.Name);

        foreach (var output in outputs)
        {
            if (
                !string.IsNullOrEmpty(output.CategoryId)
                && categoryDict.ContainsKey(output.CategoryId)
            )
            {
                output.Category = categoryDict[output.CategoryId];
            }
            if (
                !string.IsNullOrEmpty(output.SeverityLevelId)
                && featureLevelDict.ContainsKey(output.SeverityLevelId)
            )
            {
                output.SeverityLevel = featureLevelDict[output.SeverityLevelId];
            }
        }
    }

    /// <summary>
    /// 构建分类路径（从顶级分类到当前分类，不包含特性名称，用➡️分隔）
    /// </summary>
    /// <returns>返回分类路径和顶级分类名称的元组</returns>
    private (string categoryPath, string rootCategory) BuildCategoryPath(
        string categoryId,
        Dictionary<string, AppearanceFeatureCategoryEntity> categoryDict
    )
    {
        if (string.IsNullOrEmpty(categoryId) || !categoryDict.ContainsKey(categoryId))
        {
            return (null, null);
        }

        var category = categoryDict[categoryId];
        if (category == null)
        {
            return (null, null);
        }

        var pathNames = new List<string>();
        string rootCategory = null;

        // 如果分类有Path字段，使用Path构建路径
        if (!string.IsNullOrEmpty(category.Path))
        {
            var pathIds = category.Path.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var pathId in pathIds)
            {
                if (categoryDict.ContainsKey(pathId))
                {
                    pathNames.Add(categoryDict[pathId].Name);
                }
            }

            // 第一个路径ID对应的分类就是顶级分类
            if (pathIds.Length > 0 && categoryDict.ContainsKey(pathIds[0]))
            {
                rootCategory = categoryDict[pathIds[0]].Name;
            }
        }
        else
        {
            // 如果没有Path，尝试通过ParentId向上查找
            var current = category;
            var visited = new HashSet<string>(); // 防止循环引用

            while (current != null && !visited.Contains(current.Id))
            {
                visited.Add(current.Id);
                pathNames.Insert(0, current.Name);

                if (string.IsNullOrEmpty(current.ParentId) || current.ParentId == "-1")
                {
                    rootCategory = current.Name;
                    break;
                }

                if (categoryDict.ContainsKey(current.ParentId))
                {
                    current = categoryDict[current.ParentId];
                }
                else
                {
                    rootCategory = current.Name;
                    break;
                }
            }
        }

        var categoryPath = pathNames.Any() ? string.Join("➡️", pathNames) : null;
        return (categoryPath, rootCategory);
    }

    /// <summary>
    /// 填充单个输出DTO的名称字段
    /// </summary>
    private async Task FillOutputNamesAsync(AppearanceFeatureInfoOutput output)
    {
        if (output == null)
            return;

        if (!string.IsNullOrEmpty(output.CategoryId))
        {
            var category = await _categoryRepository.GetFirstAsync(c =>
                c.Id == output.CategoryId && c.DeleteMark == null
            );
            if (category != null)
            {
                output.Category = category.Name;
            }
        }

        if (!string.IsNullOrEmpty(output.SeverityLevelId))
        {
            var featureLevel = await _featureLevelRepository.GetFirstAsync(s =>
                s.Id == output.SeverityLevelId && s.DeleteMark == null
            );
            if (featureLevel != null)
            {
                output.SeverityLevel = featureLevel.Name;
            }
        }
    }

    /// <inheritdoc />
    [HttpGet("")]
    public async Task<List<AppearanceFeatureListOutput>> GetList(
        [FromQuery] AppearanceFeatureListQuery input
    )
    {
        // 构建查询，支持通过名称或ID查询
        var query = _repository.AsQueryable().Where(t => t.DeleteMark == null);

        // 关键词搜索：需要关联查询名称
        if (!string.IsNullOrEmpty(input.Keyword))
        {
            var categoryIds = await _categoryRepository
                .AsQueryable()
                .Where(c => c.Name.Contains(input.Keyword) && c.DeleteMark == null)
                .Select(c => c.Id)
                .ToListAsync();

            var featureLevelIds = await _featureLevelRepository
                .AsQueryable()
                .Where(s => s.Name.Contains(input.Keyword) && s.DeleteMark == null)
                .Select(s => s.Id)
                .ToListAsync();

            query = query.Where(t =>
                t.Name.Contains(input.Keyword)
                || (!string.IsNullOrEmpty(t.Description) && t.Description.Contains(input.Keyword))
                || categoryIds.Contains(t.CategoryId)
                || featureLevelIds.Contains(t.SeverityLevelId)
            );
        }

        // 按ID筛选：如果选择的是根分类，查询该根分类及其所有子分类下的特性
        if (!string.IsNullOrEmpty(input.CategoryId))
        {
            // 检查选中的分类是否是根分类（ParentId为"-1"）
            var selectedCategory = await _categoryRepository.GetFirstAsync(c =>
                c.Id == input.CategoryId && c.DeleteMark == null
            );

            if (selectedCategory != null)
            {
                if (
                    selectedCategory.ParentId == "-1"
                    || string.IsNullOrEmpty(selectedCategory.ParentId)
                )
                {
                    // 如果是根分类，使用 Path 字段查询所有子分类下的特性
                    // Path 字段格式：根ID,父ID,当前ID（用逗号分隔，根节点在前）
                    // 使用 Contains 方法查询所有包含该分类ID的路径（参考 OrganizeIdTree 的实现）
                    var categoryIds = await _categoryRepository
                        .AsQueryable()
                        .Where(c => c.Path.Contains(input.CategoryId) && c.DeleteMark == null)
                        .Select(c => c.Id)
                        .ToListAsync();

                    // 查询这些分类下的所有特性
                    query = query.Where(t => categoryIds.Contains(t.CategoryId));
                }
                else
                {
                    // 如果不是根分类，只查询直接属于该分类的特性
                    query = query.Where(t => t.CategoryId == input.CategoryId);
                }
            }
            else
            {
                // 分类不存在，返回空结果
                query = query.Where(t => false);
            }
        }
        if (!string.IsNullOrEmpty(input.Name))
        {
            query = query.Where(t => t.Name == input.Name);
        }
        if (!string.IsNullOrEmpty(input.SeverityLevelId))
        {
            query = query.Where(t => t.SeverityLevelId == input.SeverityLevelId);
        }

        var data = await query.OrderBy(t => t.SortCode ?? 0).ToListAsync();

        // 在内存中按名称排序（SqlSugar 的 ISugarQueryable 不支持 ThenBy）
        data = data.OrderBy(t => t.SortCode ?? 0)
            .ThenBy<AppearanceFeatureEntity, string>(t => t.Name ?? string.Empty)
            .ToList();


        var outputs = data.Adapt<List<AppearanceFeatureListOutput>>();
        await FillOutputNamesAsync(outputs);
        return outputs;
    }

    /// <inheritdoc />
    [HttpGet("{id}")]
    public async Task<AppearanceFeatureInfoOutput> GetInfo(string id)
    {
        var cacheKey = GetCacheKey($"info:{id}");
        var cached = await _cacheManager.GetAsync<AppearanceFeatureInfoOutput>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var entity = await _repository.GetFirstAsync(t => t.Id == id && t.DeleteMark == null);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);

        var output = entity.Adapt<AppearanceFeatureInfoOutput>();
        await FillOutputNamesAsync(output);

        await _cacheManager.SetAsync(cacheKey, output, TimeSpan.FromHours(6));
        return output;
    }

    /// <inheritdoc />
    [HttpPost("")]
    public async Task Create([FromBody] AppearanceFeatureCrInput input)
    {
        // 验证必填字段
        if (string.IsNullOrWhiteSpace(input.CategoryId))
        {
            throw Oops.Oh("特性大类不能为空");
        }
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw Oops.Oh("特性名称不能为空");
        }
        if (string.IsNullOrWhiteSpace(input.SeverityLevelId))
        {
            throw Oops.Oh("特性等级不能为空");
        }

        // 验证大类是否存在
        var category = await _categoryRepository.GetFirstAsync(c =>
            c.Id == input.CategoryId && c.DeleteMark == null
        );
        if (category == null)
        {
            throw Oops.Oh("特性大类不存在");
        }

        // 验证等级是否存在
        var featureLevel = await _featureLevelRepository.GetFirstAsync(s =>
            s.Id == input.SeverityLevelId && s.DeleteMark == null
        );
        if (featureLevel == null)
        {
            throw Oops.Oh("特性等级不存在");
        }

        // 唯一性检查：特性名称必须全局唯一
        if (await _repository.IsAnyAsync(t => t.Name == input.Name && t.DeleteMark == null))
        {
            throw Oops.Oh($"特性名称 '{input.Name}' 已存在，不能重复");
        }

        // 手动映射以确保字段正确传输
        var entity = input.Adapt<AppearanceFeatureEntity>();
        entity.CategoryId = input.CategoryId;
        entity.Name = input.Name;
        entity.SeverityLevelId = input.SeverityLevelId;
        entity.SortCode = input.SortCode ?? 0;
        entity.Description = input.Description;
        entity.Keywords = input.Keywords;

        entity.Creator();
        entity.LastModifyUserId = entity.CreatorUserId;
        entity.LastModifyTime = entity.CreatorTime;

        try
        {
            var isOk = await _repository
                .AsInsertable(entity)
                .IgnoreColumns(ignoreNullColumn: true)
                .ExecuteCommandAsync();
            if (isOk < 1)
                throw Oops.Oh(ErrorCode.COM1000);
        }
        catch (Exception ex)
        {
            Log.Error($"[Create] 创建外观特性失败: {ex.Message}");
            throw Oops.Oh("创建失败: " + ex.Message);
        }

        await ClearCacheAsync();
    }

    /// <inheritdoc />
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] AppearanceFeatureUpInput input)
    {
        // 验证必填字段
        if (string.IsNullOrWhiteSpace(input.CategoryId))
        {
            throw Oops.Oh("特性大类不能为空");
        }
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw Oops.Oh("特性名称不能为空");
        }
        if (string.IsNullOrWhiteSpace(input.SeverityLevelId))
        {
            throw Oops.Oh("特性等级不能为空");
        }

        // 验证大类是否存在
        var category = await _categoryRepository.GetFirstAsync(c =>
            c.Id == input.CategoryId && c.DeleteMark == null
        );
        if (category == null)
        {
            throw Oops.Oh("特性大类不存在");
        }

        // 验证等级是否存在
        var featureLevel = await _featureLevelRepository.GetFirstAsync(s =>
            s.Id == input.SeverityLevelId && s.DeleteMark == null
        );
        if (featureLevel == null)
        {
            throw Oops.Oh("特性等级不存在");
        }

        // 唯一性检查：特性名称必须全局唯一（排除自身）
        if (
            await _repository.IsAnyAsync(t =>
                t.Name == input.Name && t.Id != id && t.DeleteMark == null
            )
        )
        {
            throw Oops.Oh($"特性名称 '{input.Name}' 已存在，不能重复");
        }

        // 获取旧的特征实体
        var oldEntity = await _repository.GetFirstAsync(t => t.Id == id && t.DeleteMark == null);

        var entity = input.Adapt<AppearanceFeatureEntity>();
        entity.LastModify();
        var isOk = await _repository
            .AsUpdateable(entity)
            .IgnoreColumns(ignoreAllNullColumns: true)
            .ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);

        await ClearCacheAsync(id);
    }

    /// <inheritdoc />
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _repository.GetFirstAsync(t => t.Id == id && t.DeleteMark == null);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);

        entity.Delete();
        var isOk = await _repository
            .AsUpdateable(entity)
            .UpdateColumns(it => new
            {
                it.DeleteMark,
                it.DeleteTime,
                it.DeleteUserId,
            })
            .ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);

        await ClearCacheAsync(id);
    }

    /// <summary>
    /// 批量匹配（新版本，简化规则）
    /// </summary>
    [HttpPost("batch-match")]
    public async Task<List<MatchItemOutput>> BatchMatch([FromBody] BatchMatchInput input)
    {
        if (input?.Items == null || !input.Items.Any())
        {
            return new List<MatchItemOutput>();
        }

        // 加载所有数据
        var allFeatures = await _repository.GetListAsync(t => t.DeleteMark == null);
        var allCategories = await _categoryRepository.GetListAsync(c => c.DeleteMark == null);
        var categoryDict = allCategories.ToDictionary(c => c.Id, c => c);
        var categoryIdToName = allCategories.ToDictionary(c => c.Id, c => c.Name);
        var allFeatureLevels = await _featureLevelRepository.GetListAsync(s =>
            s.DeleteMark == null
        );
        var featureLevelIdToName = allFeatureLevels.ToDictionary(s => s.Id, s => s.Name);

        var results = new List<MatchItemOutput>();

        foreach (var item in input.Items)
        {
            if (string.IsNullOrWhiteSpace(item.Query))
            {
                results.Add(
                    new MatchItemOutput
                    {
                        Id = item.Id,
                        Query = item.Query,
                        IsPerfectMatch = false,
                        ManualCorrections = new List<ManualCorrectionOption>(),
                    }
                );
                continue;
            }

            var query = item.Query.Trim();
            // 移除标点符号后再匹配
            var cleanedQuery = RemovePunctuation(query);
            var resultsList = await MatchSingleQueryAsync(
                cleanedQuery,
                allFeatures,
                categoryDict,
                categoryIdToName,
                featureLevelIdToName
            );

            foreach (var r in resultsList)
            {
                r.Id = item.Id;
                r.Query = item.Query;
                results.Add(r);
            }
        }

        return results;
    }

    /// <summary>
    /// 移除标点符号
    /// </summary>
    private string RemovePunctuation(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        // 需要移除的标点符号列表
        var punctuationChars = new[]
        {
            '。',
            '，',
            '、',
            '；',
            '：',
            '？',
            '！',
            '…',
            '—',
            '～',
            '.',
            ',',
            ';',
            ':',
            '?',
            '!',
            '-',
            '_',
            '~',
            '(',
            ')',
            '[',
            ']',
            '{',
            '}',
            '\u201C',
            '\u201D',
            '\u2018',
            '\u2019', // 智能引号 " " ' '
            '「',
            '」',
            '『',
            '』',
            '【',
            '】',
            '（',
            '）',
            '/',
            '\\',
            '|',
            '*',
            '+',
            '=',
            '<',
            '>',
            '@',
            '#',
            '$',
            '%',
            '^',
            '&',
        };

        var result = new System.Text.StringBuilder();
        foreach (var c in text)
        {
            if (!punctuationChars.Contains(c))
            {
                result.Append(c);
            }
        }

        return result.ToString().Trim();
    }

    /// <summary>
    /// 匹配单个查询（简化规则）
    /// </summary>
    private async Task<List<MatchItemOutput>> MatchSingleQueryAsync(
        string query,
        List<AppearanceFeatureEntity> allFeatures,
        Dictionary<string, AppearanceFeatureCategoryEntity> categoryDict,
        Dictionary<string, string> categoryIdToName,
        Dictionary<string, string> featureLevelIdToName
    )
    {
        var result = new MatchItemOutput
        {
            Query = query,
            IsPerfectMatch = false,
            ManualCorrections = new List<ManualCorrectionOption>(),
        };

        // 规则1: 匹配特征名称（精确匹配）
        var nameMatch = allFeatures.FirstOrDefault(f =>
            f.Name.Equals(query, StringComparison.OrdinalIgnoreCase) && f.DeleteMark == null
        );

        if (nameMatch != null)
        {
            result.IsPerfectMatch = true;
            result.MatchMethod = "name";
            FillMatchResult(
                result,
                nameMatch,
                categoryDict,
                categoryIdToName,
                featureLevelIdToName
            );
            return new List<MatchItemOutput> { result };
        }

        // 规则2: 匹配关键词
        var keywordLower = query.ToLowerInvariant();
        var keywordMatches = allFeatures
            .Where(f =>
                !string.IsNullOrEmpty(f.Keywords)
                && ParseKeywords(f.Keywords).Contains(keywordLower)
                && f.DeleteMark == null
            )
            .ToList();

        if (keywordMatches.Any())
        {
            var keywordResults = new List<MatchItemOutput>();
            foreach (var match in keywordMatches)
            {
                var kwResult = new MatchItemOutput
                {
                    Query = query,
                    IsPerfectMatch = true,
                    MatchMethod = "keyword",
                    ManualCorrections = new List<ManualCorrectionOption>(),
                };

                FillMatchResult(
                    kwResult,
                    match,
                    categoryDict,
                    categoryIdToName,
                    featureLevelIdToName
                );

                keywordResults.Add(kwResult);
            }


            return keywordResults;
        }

        // 规则3: AI匹配
        try
        {
            FeatureClassification aiClassification;
            if (query.StartsWith("TEST_MOCK_AI:"))
            {
                aiClassification = await _analysisService.DefineAppearanceFeatureAsync(
                    query.Replace("TEST_MOCK_AI:", "")
                );
            }
            else
            {
                aiClassification = await _analysisService.DefineAppearanceFeatureAsync(query);
            }
            if (
                aiClassification != null
                && aiClassification.Features != null
                && aiClassification.Features.Any()
            )
            {
                var manualCorrections = new List<ManualCorrectionOption>();

                // AI返回几条记录，就创建几条人工修正匹配列表
                foreach (var aiFeature in aiClassification.Features)
                {

                    // 1. 查找匹配的特性（根据名称和等级）
                    var matchedFeature = FindFeatureByNameAndLevel(
                        aiFeature.Name,
                        aiFeature.Level,
                        allFeatures,
                        featureLevelIdToName
                    );

                    // 2. 判断匹配程度并确定ActionType
                    string actionType = "review_correction"; // 默认：跳转到修正列表
                    string suggestion = "";
                    string matchedFeatureId = null;

                    if (matchedFeature != null)
                    {
                        matchedFeatureId = matchedFeature.Id;

                        // 检查名称和等级是否都存在且匹配
                        bool nameAndLevelMatch = false;
                        if (!string.IsNullOrWhiteSpace(aiFeature.Level))
                        {
                            var matchedLevel = featureLevelIdToName.ContainsKey(
                                matchedFeature.SeverityLevelId
                            )
                                ? featureLevelIdToName[matchedFeature.SeverityLevelId]
                                : null;
                            nameAndLevelMatch =
                                matchedLevel != null
                                && matchedLevel.Equals(
                                    aiFeature.Level,
                                    StringComparison.OrdinalIgnoreCase
                                );
                        }

                        if (nameAndLevelMatch)
                        {
                            // 名称和等级完全匹配 -> 建议添加关键词 (前端显示确认按钮)
                            actionType = "add_keyword";
                            suggestion =
                                $"AI识别到特性：{aiFeature.Name}（等级：{aiFeature.Level}），建议将'{query}'添加到关键词列表";
                        }
                        else
                        {
                            // 仅名称匹配 -> 建议人工确认 (前端跳转修正列表)
                            suggestion =
                                $"AI识别到特性名称：{aiFeature.Name}（等级：{aiFeature.Level ?? "未识别"}），但等级不完全匹配，请人工确认";
                        }
                    }
                    else
                    {
                        // 未找到匹配 -> 建议人工确认 (前端跳转修正列表)
                        suggestion =
                            $"AI识别到特性：{aiFeature.Name}（等级：{aiFeature.Level ?? "未识别"}），但在系统中未找到对应特性，请人工确认";
                    }

                    // 3. 插入到人工修正列表数据库 (按InputText不去重，总是插入新记录)
                    // 逻辑：AI返回几条记录就创建几条人工修正匹配列表 (MatchMode="ai_proposal")

                    string correctionId = null;
                    try
                    {
                        var correction = new AppearanceFeatureCorrectionEntity
                        {
                            InputText = query,
                            AutoMatchedFeatureId = matchedFeatureId,
                            CorrectedFeatureId = matchedFeatureId,
                            MatchMode = "ai_proposal",
                            Scenario = "learning",
                            Remark =
                                $"AI识别: {aiFeature.Name} {(string.IsNullOrEmpty(aiFeature.Level) ? "" : $"({aiFeature.Level})")} - {aiFeature.Category ?? ""}",
                            Status = "Pending",
                        };
                        correction.Creator();
                        await _correctionRepository.InsertAsync(correction);
                        correctionId = correction.Id;
                        // }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[BatchMatch] 插入/获取修正记录失败: {ex.Message}");
                    }

                    // 4. 构建返回给前端的选项
                    if (matchedFeature != null)
                    {
                        var option = CreateManualCorrectionOption(
                            matchedFeature,
                            categoryDict,
                            categoryIdToName,
                            featureLevelIdToName,
                            actionType,
                            suggestion
                        );
                        option.CorrectionId = correctionId;
                        manualCorrections.Add(option);
                    }
                    else
                    {
                        var option = new ManualCorrectionOption
                        {
                            FeatureId = "",
                            FeatureName = aiFeature.Name,
                            SeverityLevel = aiFeature.Level ?? "",
                            Category = aiFeature.Category ?? "",
                            CategoryPath = "",
                            ActionType = actionType,
                            Suggestion = suggestion,
                            CorrectionId = correctionId,
                        };
                        manualCorrections.Add(option);
                    }
                }

                result.MatchMethod = "ai";
                result.IsPerfectMatch = false;
                result.ManualCorrections = manualCorrections;
                return new List<MatchItemOutput> { result };
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[BatchMatch] AI匹配失败: {ex.Message}");
            // AI分析失败，直接返回错误信息，不跳转人工修正
            result.MatchMethod = "ai_error";
            result.IsPerfectMatch = true; // 设置为true以避免前端跳转修正列表
            result.FeatureName = $"AI分析失败: {ex.Message}";
            result.ManualCorrections = new List<ManualCorrectionOption>();
            return new List<MatchItemOutput> { result };
        }

        // 如果所有规则都不匹配，返回空结果和所有特性供选择
        result.MatchMethod = "none";
        result.IsPerfectMatch = false;
        result.ManualCorrections = new List<ManualCorrectionOption>();

        return new List<MatchItemOutput> { result };
    }

    /// <summary>
    /// 填充匹配结果
    /// </summary>
    private void FillMatchResult(
        MatchItemOutput result,
        AppearanceFeatureEntity feature,
        Dictionary<string, AppearanceFeatureCategoryEntity> categoryDict,
        Dictionary<string, string> categoryIdToName,
        Dictionary<string, string> featureLevelIdToName
    )
    {
        result.FeatureName = feature.Name;
        result.SeverityLevel = featureLevelIdToName.ContainsKey(feature.SeverityLevelId)
            ? featureLevelIdToName[feature.SeverityLevelId]
            : null;

        var (categoryPath, rootCategory) = BuildCategoryPath(feature.CategoryId, categoryDict);
        result.CategoryPath = categoryPath;
        result.Category =
            rootCategory
            ?? (
                categoryIdToName.ContainsKey(feature.CategoryId)
                    ? categoryIdToName[feature.CategoryId]
                    : null
            );
    }

    /// <summary>
    /// 创建人工修正选项
    /// </summary>
    private ManualCorrectionOption CreateManualCorrectionOption(
        AppearanceFeatureEntity feature,
        Dictionary<string, AppearanceFeatureCategoryEntity> categoryDict,
        Dictionary<string, string> categoryIdToName,
        Dictionary<string, string> featureLevelIdToName,
        string actionType,
        string suggestion
    )
    {
        var (categoryPath, rootCategory) = BuildCategoryPath(feature.CategoryId, categoryDict);
        return new ManualCorrectionOption
        {
            FeatureId = feature.Id,
            FeatureName = feature.Name,
            SeverityLevel = featureLevelIdToName.ContainsKey(feature.SeverityLevelId)
                ? featureLevelIdToName[feature.SeverityLevelId]
                : null,
            CategoryPath = categoryPath,
            Category =
                rootCategory
                ?? (
                    categoryIdToName.ContainsKey(feature.CategoryId)
                        ? categoryIdToName[feature.CategoryId]
                        : null
                ),
            ActionType = actionType,
            Suggestion = suggestion,
        };
    }

    /// <summary>
    /// 根据名称和等级查找特性
    /// </summary>
    private AppearanceFeatureEntity FindFeatureByNameAndLevel(
        string featureName,
        string levelName,
        List<AppearanceFeatureEntity> allFeatures,
        Dictionary<string, string> featureLevelIdToName
    )
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            return null;
        }

        // 先按名称查找
        var nameMatches = allFeatures
            .Where(f =>
                f.Name.Equals(featureName, StringComparison.OrdinalIgnoreCase)
                && f.DeleteMark == null
            )
            .ToList();

        if (!nameMatches.Any())
        {
            return null;
        }

        // 如果提供了等级名称，尝试匹配等级
        if (!string.IsNullOrWhiteSpace(levelName))
        {
            var levelMatch = nameMatches.FirstOrDefault(f =>
                featureLevelIdToName.ContainsKey(f.SeverityLevelId)
                && featureLevelIdToName[f.SeverityLevelId]
                    .Equals(levelName, StringComparison.OrdinalIgnoreCase)
            );

            if (levelMatch != null)
            {
                return levelMatch;
            }
        }

        // 如果等级不匹配或未提供等级，返回第一个名称匹配的
        return nameMatches.FirstOrDefault();
    }

    [HttpPost("match")]
    public async Task<List<AppearanceFeatureInfoOutput>> Match([FromBody] MatchInput input)
    {
        // 获取所有未删除的特性
        var allFeatures = await _repository.GetListAsync(t => t.DeleteMark == null);

        // 加载所有大类和等级，建立ID到名称的映射（用于匹配时使用名称）
        var allCategories = await _categoryRepository
            .AsQueryable()
            .Where(c => c.DeleteMark == null)
            .ToListAsync();
        var categoryIdToName = allCategories.ToDictionary(c => c.Id, c => c.Name);
        var categoryDict = allCategories.ToDictionary(c => c.Id, c => c);

        var allFeatureLevels = await _featureLevelRepository
            .AsQueryable()
            .Where(s => s.DeleteMark == null)
            .ToListAsync();
        var featureLevelIdToName = allFeatureLevels.ToDictionary(s => s.Id, s => s.Name);

        // 为特性添加名称字段（用于匹配）
        var featuresWithNames = allFeatures
            .Select(f => new
            {
                Feature = f,
                CategoryName = categoryIdToName.ContainsKey(f.CategoryId)
                    ? categoryIdToName[f.CategoryId]
                    : null,
                SeverityLevelName = featureLevelIdToName.ContainsKey(f.SeverityLevelId)
                    ? featureLevelIdToName[f.SeverityLevelId]
                    : null,
            })
            .ToList();

        // 获取启用的严重程度等级列表（用于程度词识别）
        var enabledLevels = await _featureLevelService.GetEnabledLevels();
        var degreeWords =
            enabledLevels?.Select(l => l.Name).Where(n => !string.IsNullOrEmpty(n)).ToList()
            ?? new List<string>();

        // 如果没有启用的特性等级，提示用户创建特性等级
        if (degreeWords == null || !degreeWords.Any())
        {
            throw Oops.Oh(
                "未找到启用的特性等级，请先创建特性等级后再进行匹配。特性等级用于识别输入文本中的程度词（如：微、轻、重等）。"
            );
        }

        // ========== 优先级 1: 规则引擎匹配（最快、最准） ==========
        var ruleMatcher = new AppearanceFeatureRuleMatcher();
        var ruleResults = ruleMatcher.Match(
            input.Text,
            featuresWithNames.Select(f => f.Feature).ToList(),
            degreeWords,
            categoryIdToName,
            featureLevelIdToName
        );


        if (ruleResults.Any())
        {            var outputs = new List<AppearanceFeatureInfoOutput>();
            foreach (var result in ruleResults)
            {
                var output = result.Feature.Adapt<AppearanceFeatureInfoOutput>();
                output.Category = categoryIdToName.ContainsKey(result.Feature.CategoryId)
                    ? categoryIdToName[result.Feature.CategoryId]
                    : null;
                output.SeverityLevel = featureLevelIdToName.ContainsKey(
                    result.Feature.SeverityLevelId
                )
                    ? featureLevelIdToName[result.Feature.SeverityLevelId]
                    : null;
                output.MatchMethod = result.MatchMethod;
                output.DegreeWord = result.DegreeWord;
                output.RequiresSeverityConfirmation = result.RequiresSeverityConfirmation;
                output.SuggestedSeverity = result.SuggestedSeverity;

                // 如果需要确认等级，检查是否存在该特性（名称+等级组合）
                if (
                    result.RequiresSeverityConfirmation
                    && !string.IsNullOrEmpty(result.SuggestedSeverity)
                )
                {
                    // 先设置建议的特性名称（无论是否找到等级ID）
                    output.SuggestedFeatureName =
                        $"{result.SuggestedSeverity}{result.Feature.Name}";

                    // 查找建议的等级ID
                    var suggestedSeverityLevelId = featureLevelIdToName
                        .Where(kvp =>
                            kvp.Value.Equals(
                                result.SuggestedSeverity,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                        .Select(kvp => kvp.Key)
                        .FirstOrDefault();

                    // 检查等级是否存在
                    output.SeverityLevelExists = !string.IsNullOrEmpty(suggestedSeverityLevelId);

                    if (output.SeverityLevelExists)
                    {
                        // 等级存在，检查是否存在该特性（相同分类、相同名称、建议的等级）
                        var existingFeature = allFeatures.FirstOrDefault(f =>
                            f.CategoryId == result.Feature.CategoryId
                            && f.Name.Equals(
                                result.Feature.Name,
                                StringComparison.OrdinalIgnoreCase
                            )
                            && f.SeverityLevelId == suggestedSeverityLevelId
                            && f.DeleteMark == null
                        );

                        if (existingFeature != null)
                        {
                            output.FeatureExists = true;
                            output.ExistingFeatureId = existingFeature.Id;
                        }
                        else
                        {
                            output.FeatureExists = false;
                        }
                    }
                    else
                    {
                        // 如果找不到等级ID，说明等级不存在，需要先创建等级
                        output.FeatureExists = false;
                    }
                }

                // 构建分类路径（不包含特性名称）和顶级分类
                var (categoryPath, rootCategory) = BuildCategoryPath(
                    result.Feature.CategoryId,
                    categoryDict
                );
                output.CategoryPath = categoryPath;
                output.RootCategory = rootCategory;
                // 如果顶级分类存在，使用顶级分类作为特征大类
                if (!string.IsNullOrEmpty(rootCategory))
                {
                    output.Category = rootCategory;
                }
                outputs.Add(output);
            }
            return outputs;
        }

        // ========== 优先级 2: AI分析（处理复杂描述和多特性） ==========
        try
        {
            var aiClassification = await _analysisService.DefineAppearanceFeatureAsync(input.Text);
            if (
                aiClassification != null
                && aiClassification.Features != null
                && aiClassification.Features.Any()
            )
            {                var outputs = new List<AppearanceFeatureInfoOutput>();

                foreach (var aiFeature in aiClassification.Features)
                {

                    // 根据 AI 返回的 name、category 和 level 查找特性（使用名称匹配）
                    var matchedFeature = featuresWithNames.FirstOrDefault(f =>
                        (
                            !string.IsNullOrEmpty(aiFeature.Name)
                            && f.Feature.Name.Equals(
                                aiFeature.Name,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                        && (
                            !string.IsNullOrEmpty(aiFeature.Category)
                            && f.CategoryName != null
                            && f.CategoryName.Equals(
                                aiFeature.Category,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                        && (
                            !string.IsNullOrEmpty(aiFeature.Level)
                            && f.SeverityLevelName != null
                            && f.SeverityLevelName.Equals(
                                aiFeature.Level,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                    );

                    // 如果精确匹配没找到，尝试只匹配 name 和 category
                    if (matchedFeature == null)
                    {
                        matchedFeature = featuresWithNames.FirstOrDefault(f =>
                            (
                                !string.IsNullOrEmpty(aiFeature.Name)
                                && f.Feature.Name.Equals(
                                    aiFeature.Name,
                                    StringComparison.OrdinalIgnoreCase
                                )
                            )
                            && (
                                !string.IsNullOrEmpty(aiFeature.Category)
                                && f.CategoryName != null
                                && f.CategoryName.Equals(
                                    aiFeature.Category,
                                    StringComparison.OrdinalIgnoreCase
                                )
                            )
                        );
                    }

                    // 如果只匹配到 Name + Category，但 Level 不匹配，尝试只匹配 Name + Level
                    if (matchedFeature == null && !string.IsNullOrEmpty(aiFeature.Category))
                    {
                        // 尝试忽略 Category，只匹配 Name + Level (有时 AI 推断的大类与 DB 不一致)
                        matchedFeature = featuresWithNames.FirstOrDefault(f =>
                            (
                                !string.IsNullOrEmpty(aiFeature.Name)
                                && f.Feature.Name.Equals(
                                    aiFeature.Name,
                                    StringComparison.OrdinalIgnoreCase
                                )
                            )
                            && (
                                !string.IsNullOrEmpty(aiFeature.Level)
                                && f.SeverityLevelName != null
                                && f.SeverityLevelName.Equals(
                                    aiFeature.Level,
                                    StringComparison.OrdinalIgnoreCase
                                )
                            )
                        );
                    }

                    // 如果还是没找到，尝试只匹配 Name (最宽泛的匹配)
                    if (matchedFeature == null)
                    {
                        matchedFeature = featuresWithNames.FirstOrDefault(f =>
                            !string.IsNullOrEmpty(aiFeature.Name)
                            && f.Feature.Name.Equals(
                                aiFeature.Name,
                                StringComparison.OrdinalIgnoreCase
                            )
                        );
                    }

                    if (matchedFeature != null)
                    {
                        var output = matchedFeature.Feature.Adapt<AppearanceFeatureInfoOutput>();
                        output.Category = matchedFeature.CategoryName;
                        output.SeverityLevel = matchedFeature.SeverityLevelName;
                        output.MatchMethod = "ai";
                        output.DegreeWord = aiFeature.Level;

                        // 如果等级不匹配，才需要确认
                        bool isLevelMismatch =
                            !string.IsNullOrEmpty(aiFeature.Level)
                            && matchedFeature.SeverityLevelName != null
                            && !matchedFeature.SeverityLevelName.Equals(
                                aiFeature.Level,
                                StringComparison.OrdinalIgnoreCase
                            );

                        if (isLevelMismatch)
                        {
                            output.RequiresSeverityConfirmation = true;
                            output.SuggestedSeverity = aiFeature.Level;
                            output.SuggestedFeatureName =
                                $"{aiFeature.Level}{matchedFeature.Feature.Name}";

                            // ... (Existing logic for verifying severity existence) ...
                            var suggestedSeverityLevelId = featureLevelIdToName
                                .Where(kvp =>
                                    kvp.Value.Equals(
                                        aiFeature.Level,
                                        StringComparison.OrdinalIgnoreCase
                                    )
                                )
                                .Select(kvp => kvp.Key)
                                .FirstOrDefault();

                            output.SeverityLevelExists = !string.IsNullOrEmpty(
                                suggestedSeverityLevelId
                            );
                            if (output.SeverityLevelExists)
                            {
                                var existingFeature = allFeatures.FirstOrDefault(f =>
                                    f.CategoryId == matchedFeature.Feature.CategoryId
                                    && f.Name.Equals(
                                        matchedFeature.Feature.Name,
                                        StringComparison.OrdinalIgnoreCase
                                    )
                                    && f.SeverityLevelId == suggestedSeverityLevelId
                                    && f.DeleteMark == null
                                );
                                if (existingFeature != null)
                                {
                                    output.FeatureExists = true;
                                    output.ExistingFeatureId = existingFeature.Id;
                                }
                                else
                                {
                                    output.FeatureExists = false;
                                }
                            }
                            else
                            {
                                output.FeatureExists = false;
                            }
                        }
                        else
                        {
                            // 完美匹配 (Name + Level Match, Category might differ but we trusted Name)
                            output.RequiresSeverityConfirmation = false;
                            output.FeatureExists = true;
                            output.ExistingFeatureId = matchedFeature.Feature.Id;
                        }

                        // 构建分类路径（不包含特性名称）和顶级分类
                        var (categoryPath, rootCategory) = BuildCategoryPath(
                            matchedFeature.Feature.CategoryId,
                            categoryDict
                        );
                        output.CategoryPath = categoryPath;
                        output.RootCategory = rootCategory;
                        // 如果顶级分类存在，使用顶级分类作为特征大类
                        if (!string.IsNullOrEmpty(rootCategory))
                        {
                            output.Category = rootCategory;
                        }

                        // 注意：不自动添加关键词，由用户手动确认后通过 AddKeyword API 添加
                        outputs.Add(output);
                    }
                    else
                    {
                        // AI识别到了特性，但匹配不到现有特性记录，创建一个建议的输出
                        // 查找分类ID
                        var categoryId = categoryIdToName
                            .Where(kvp =>
                                kvp.Value.Equals(
                                    aiFeature.Category,
                                    StringComparison.OrdinalIgnoreCase
                                )
                            )
                            .Select(kvp => kvp.Key)
                            .FirstOrDefault();

                        if (!string.IsNullOrEmpty(categoryId))
                        {
                            // 查找等级ID
                            var severityLevelId = featureLevelIdToName
                                .Where(kvp =>
                                    kvp.Value.Equals(
                                        aiFeature.Level,
                                        StringComparison.OrdinalIgnoreCase
                                    )
                                )
                                .Select(kvp => kvp.Key)
                                .FirstOrDefault();

                            // 创建一个建议的输出（用于提示用户创建）
                            var suggestedOutput = new AppearanceFeatureInfoOutput
                            {
                                Id = Guid.NewGuid().ToString(), // 临时ID
                                CategoryId = categoryId,
                                Name = aiFeature.Name,
                                SeverityLevelId = severityLevelId ?? string.Empty,
                                Category = aiFeature.Category,
                                SeverityLevel = aiFeature.Level,
                                MatchMethod = "ai",
                                DegreeWord = aiFeature.Level,
                                RequiresSeverityConfirmation = !string.IsNullOrEmpty(
                                    severityLevelId
                                ),
                                SuggestedSeverity = aiFeature.Level,
                                SuggestedFeatureName = $"{aiFeature.Level}{aiFeature.Name}",
                                SeverityLevelExists = !string.IsNullOrEmpty(severityLevelId),
                                FeatureExists = false,
                            };

                            // 构建分类路径
                            var (categoryPath, rootCategory) = BuildCategoryPath(
                                categoryId,
                                categoryDict
                            );
                            suggestedOutput.CategoryPath = categoryPath;
                            suggestedOutput.RootCategory = rootCategory;
                            if (!string.IsNullOrEmpty(rootCategory))
                            {
                                suggestedOutput.Category = rootCategory;
                            }

                            outputs.Add(suggestedOutput);
                        }
                    }
                }

                if (outputs.Any())
                {
                    return outputs;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[Match] AI分析失败: {ex.Message}");
        }

        // ========== 优先级 3: 文本模糊匹配（最后兜底） ==========
        var fallbackFeatures = featuresWithNames
            .Where(f =>
            {
                // 匹配类别名称
                if (
                    !string.IsNullOrEmpty(f.CategoryName)
                    && f.CategoryName.Contains(input.Text, StringComparison.OrdinalIgnoreCase)
                )
                    return true;

                // 匹配特性名称
                if (
                    !string.IsNullOrEmpty(f.Feature.Name)
                    && f.Feature.Name.Contains(input.Text, StringComparison.OrdinalIgnoreCase)
                )
                    return true;

                // 匹配描述
                if (
                    !string.IsNullOrEmpty(f.Feature.Description)
                    && f.Feature.Description.Contains(
                        input.Text,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                    return true;

                return false;
            })
            .Take(3)
            .ToList();

        if (fallbackFeatures.Any())
        {            var fallbackOutputs = fallbackFeatures
                .Select(f =>
                {
                    var output = f.Feature.Adapt<AppearanceFeatureInfoOutput>();
                    output.Category = f.CategoryName;
                    output.SeverityLevel = f.SeverityLevelName;
                    output.MatchMethod = "fuzzy";
                    // 构建分类路径（不包含特性名称）和顶级分类
                    var (categoryPath, rootCategory) = BuildCategoryPath(
                        f.Feature.CategoryId,
                        categoryDict
                    );
                    output.CategoryPath = categoryPath;
                    output.RootCategory = rootCategory;
                    // 如果顶级分类存在，使用顶级分类作为特征大类
                    if (!string.IsNullOrEmpty(rootCategory))
                    {
                        output.Category = rootCategory;
                    }
                    return output;
                })
                .ToList();
            return fallbackOutputs;
        }

        // 未匹配到任何结果
        return new List<AppearanceFeatureInfoOutput>();
    }

    /// <summary>
    /// 添加特性记录（当用户确认创建新等级时）
    /// </summary>
    [HttpPost("add-with-severity")]
    public async Task<AppearanceFeatureInfoOutput> AddWithSeverity(
        [FromBody] AddWithSeverityInput input
    )
    {
        if (string.IsNullOrWhiteSpace(input.CategoryId))
        {
            throw Oops.Oh("特性大类不能为空");
        }
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw Oops.Oh("特性名称不能为空");
        }
        if (string.IsNullOrWhiteSpace(input.SeverityLevelId))
        {
            throw Oops.Oh("特性等级不能为空");
        }

        // 验证大类是否存在
        var category = await _categoryRepository.GetFirstAsync(c =>
            c.Id == input.CategoryId && c.DeleteMark == null
        );
        if (category == null)
        {
            throw Oops.Oh("特性大类不存在");
        }

        // 验证等级是否存在
        var featureLevel = await _featureLevelRepository.GetFirstAsync(s =>
            s.Id == input.SeverityLevelId && s.DeleteMark == null
        );
        if (featureLevel == null)
        {
            throw Oops.Oh("特性等级不存在");
        }

        // 检查特性名称是否已存在（全局唯一性）
        var exists = await _repository.IsAnyAsync(t =>
            t.Name == input.Name && t.DeleteMark == null
        );

        if (exists)
        {
            throw Oops.Oh($"特性名称 '{input.Name}' 已存在，不能重复");
        }

        // 创建新记录
        var entity = new AppearanceFeatureEntity
        {
            Id = Guid.NewGuid().ToString(),
            CategoryId = input.CategoryId,
            Name = input.Name,
            SeverityLevelId = input.SeverityLevelId,
            Description = input.Description,
            Keywords = input.Keywords,
            SortCode = input.SortCode ?? 0,
        };

        entity.Creator();
        entity.LastModifyUserId = entity.CreatorUserId;
        entity.LastModifyTime = entity.CreatorTime;

        var isOk = await _repository.AsInsertable(entity).ExecuteCommandAsync();

        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);


        // 返回创建的特性信息
        var output = entity.Adapt<AppearanceFeatureInfoOutput>();
        output.Category = category.Name;
        output.SeverityLevel = featureLevel.Name;

        // 构建分类路径
        var allCategories = await _categoryRepository
            .AsQueryable()
            .Where(c => c.DeleteMark == null)
            .ToListAsync();
        var categoryDict = allCategories.ToDictionary(c => c.Id, c => c);
        var (categoryPath, rootCategory) = BuildCategoryPath(entity.CategoryId, categoryDict);
        output.CategoryPath = categoryPath;
        output.RootCategory = rootCategory;
        if (!string.IsNullOrEmpty(rootCategory))
        {
            output.Category = rootCategory;
        }

        return output;
    }

    /// <summary>
    /// 保存人工修正记录
    /// </summary>
    [HttpPost("save-correction")]
    public async Task SaveCorrection([FromBody] AppearanceFeatureCorrectionInput input)
    {
        if (string.IsNullOrWhiteSpace(input.InputText))
        {
            throw Oops.Oh("输入文本不能为空");
        }

        if (string.IsNullOrWhiteSpace(input.CorrectedFeatureId))
        {
            throw Oops.Oh("修正后的特征ID不能为空");
        }

        // 验证特征是否存在
        var feature = await _repository.GetFirstAsync(t =>
            t.Id == input.CorrectedFeatureId && t.DeleteMark == null
        );
        if (feature == null)
        {
            throw Oops.Oh("修正后的特征不存在");
        }

        var entity = new AppearanceFeatureCorrectionEntity
        {
            Id = Guid.NewGuid().ToString(),
            InputText = input.InputText,
            AutoMatchedFeatureId = input.AutoMatchedFeatureId,
            CorrectedFeatureId = input.CorrectedFeatureId,
            MatchMode = input.MatchMode ?? "manual",
            Scenario = input.Scenario ?? "test",
            Remark = input.Remark,
        };

        entity.Creator();
        entity.LastModifyUserId = entity.CreatorUserId;
        entity.LastModifyTime = entity.CreatorTime;

        await _correctionRepository.InsertAsync(entity);

    }

    /// <summary>
    /// 将关键字添加到特性记录
    /// </summary>
    [HttpPost("add-keyword")]
    public async Task AddKeyword([FromBody] AddKeywordInput input)
    {
        if (string.IsNullOrWhiteSpace(input.FeatureId))
        {
            throw Oops.Oh("特性ID不能为空");
        }
        if (string.IsNullOrWhiteSpace(input.Keyword))
        {
            throw Oops.Oh("关键字不能为空");
        }

        var feature = await _repository.GetFirstAsync(f =>
            f.Id == input.FeatureId && f.DeleteMark == null
        );
        if (feature == null)
        {
            throw Oops.Oh("特性不存在");
        }

        // 获取已有关键词
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
                // JSON解析失败，尝试按逗号分割
                existingKeywords = feature
                    .Keywords.Split(
                        new[] { ',', '，', ';', '；' },
                        StringSplitOptions.RemoveEmptyEntries
                    )
                    .Select(k => k.Trim())
                    .Where(k => !string.IsNullOrEmpty(k))
                    .ToList();
            }
        }

        // 添加新关键词（去重，不区分大小写）
        var trimmedKeyword = input.Keyword.Trim();
        if (!existingKeywords.Contains(trimmedKeyword, StringComparer.OrdinalIgnoreCase))
        {
            existingKeywords.Add(trimmedKeyword);

            // 序列化并更新
            feature.Keywords = Newtonsoft.Json.JsonConvert.SerializeObject(existingKeywords);
            feature.LastModify();

            await _repository
                .AsUpdateable(feature)
                .UpdateColumns(f => new
                {
                    f.Keywords,
                    f.LastModifyUserId,
                    f.LastModifyTime,
                })
                .ExecuteCommandAsync();

            }
    }

    /// <summary>
    /// 创建或添加关键词（智能处理：如果特性存在则添加关键词，不存在则创建特性）
    /// </summary>
    [HttpPost("create-or-add-keyword")]
    public async Task<CreateOrAddKeywordOutput> CreateOrAddKeyword(
        [FromBody] CreateOrAddKeywordInput input
    )
    {
        if (string.IsNullOrWhiteSpace(input.InputText))
        {
            throw Oops.Oh("输入文本不能为空");
        }
        if (string.IsNullOrWhiteSpace(input.CategoryId))
        {
            throw Oops.Oh("特性大类不能为空");
        }
        if (string.IsNullOrWhiteSpace(input.FeatureName))
        {
            throw Oops.Oh("特性名称不能为空");
        }
        if (string.IsNullOrWhiteSpace(input.SeverityLevelName))
        {
            throw Oops.Oh("特性等级名称不能为空");
        }

        // 验证大类是否存在
        var category = await _categoryRepository.GetFirstAsync(c =>
            c.Id == input.CategoryId && c.DeleteMark == null
        );
        if (category == null)
        {
            throw Oops.Oh("特性大类不存在");
        }

        // 查找等级ID
        var featureLevel = await _featureLevelRepository.GetFirstAsync(s =>
            s.Name.Equals(input.SeverityLevelName, StringComparison.OrdinalIgnoreCase)
            && s.DeleteMark == null
        );
        if (featureLevel == null)
        {
            throw Oops.Oh($"特性等级 '{input.SeverityLevelName}' 不存在");
        }

        // 1. 检查特性名称是否已存在（全局唯一性）
        var existingFeature = await _repository.GetFirstAsync(f =>
            f.Name.Equals(input.FeatureName, StringComparison.OrdinalIgnoreCase)
            && f.DeleteMark == null
        );

        string finalFeatureId;
        string action;
        string remark;

        if (existingFeature != null)
        {
            // 3. 如果已经存在，将输入文本（如"严重脆"）添加到关键词列表中
            var existingKeywords = new List<string>();
            if (!string.IsNullOrEmpty(existingFeature.Keywords))
            {
                try
                {
                    existingKeywords =
                        Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(
                            existingFeature.Keywords
                        )
                        ?? new List<string>();
                }
                catch
                {
                    existingKeywords = existingFeature
                        .Keywords.Split(
                            new[] { ',', '，', ';', '；' },
                            StringSplitOptions.RemoveEmptyEntries
                        )
                        .Select(k => k.Trim())
                        .Where(k => !string.IsNullOrEmpty(k))
                        .ToList();
                }
            }

            var trimmedKeyword = input.InputText.Trim();
            if (!existingKeywords.Contains(trimmedKeyword, StringComparer.OrdinalIgnoreCase))
            {
                existingKeywords.Add(trimmedKeyword);
                existingFeature.Keywords = Newtonsoft.Json.JsonConvert.SerializeObject(
                    existingKeywords
                );
                existingFeature.LastModify();

                await _repository
                    .AsUpdateable(existingFeature)
                    .UpdateColumns(f => new
                    {
                        f.Keywords,
                        f.LastModifyUserId,
                        f.LastModifyTime,
                    })
                    .ExecuteCommandAsync();

            }

            finalFeatureId = existingFeature.Id;
            action = "add_keyword";
            remark = $"已存在特性，添加关键词: {input.InputText}";
        }
        else
        {
            // 2. 不存在需要创建外观特性
            var entity = new AppearanceFeatureEntity
            {
                Id = Guid.NewGuid().ToString(),
                CategoryId = input.CategoryId,
                Name = input.FeatureName,
                SeverityLevelId = featureLevel.Id,
                Description = input.Description,
                Keywords = Newtonsoft.Json.JsonConvert.SerializeObject(
                    new List<string> { input.InputText.Trim() }
                ),
                SortCode = input.SortCode ?? 0,
            };

            entity.Creator();
            entity.LastModifyUserId = entity.CreatorUserId;
            entity.LastModifyTime = entity.CreatorTime;

            var isOk = await _repository.AsInsertable(entity).ExecuteCommandAsync();

            if (isOk < 1)
                throw Oops.Oh(ErrorCode.COM1000);


            finalFeatureId = entity.Id;
            action = "create";
            remark = $"创建新特性: {input.InputText}";
        }

        // 4. 记录人工修改
        var correctionEntity = new AppearanceFeatureCorrectionEntity
        {
            Id = Guid.NewGuid().ToString(),
            InputText = input.InputText,
            AutoMatchedFeatureId = input.AutoMatchedFeatureId,
            CorrectedFeatureId = finalFeatureId,
            MatchMode = action,
            Scenario = input.Scenario ?? "test",
            Remark = remark,
        };

        correctionEntity.Creator();
        correctionEntity.LastModifyUserId = correctionEntity.CreatorUserId;
        correctionEntity.LastModifyTime = correctionEntity.CreatorTime;

        await _correctionRepository.InsertAsync(correctionEntity);


        // 返回结果
        var resultFeature = await _repository.GetFirstAsync(f =>
            f.Id == finalFeatureId && f.DeleteMark == null
        );
        if (resultFeature == null)
        {
            throw Oops.Oh("创建的特性不存在");
        }

        var output = resultFeature.Adapt<AppearanceFeatureInfoOutput>();
        output.Category = category.Name;
        output.SeverityLevel = featureLevel.Name;

        // 构建分类路径
        var allCategories = await _categoryRepository
            .AsQueryable()
            .Where(c => c.DeleteMark == null)
            .ToListAsync();
        var categoryDict = allCategories.ToDictionary(c => c.Id, c => c);
        var (categoryPath, rootCategory) = BuildCategoryPath(
            resultFeature.CategoryId,
            categoryDict
        );
        output.CategoryPath = categoryPath;
        output.RootCategory = rootCategory;
        if (!string.IsNullOrEmpty(rootCategory))
        {
            output.Category = rootCategory;
        }

        return new CreateOrAddKeywordOutput
        {
            Feature = output,
            Action = action,
            Message =
                action == "create"
                    ? $"已创建外观特性 \"{input.InputText}\""
                    : $"已将 \"{input.InputText}\" 添加到关键词列表",
        };
    }

    #region 关键词管理

    /// <summary>
    /// 根据关键词获取特征ID列表（从数据库查询）
    /// </summary>
    /// <param name="keyword">关键词</param>
    /// <returns>特征ID列表</returns>
    private async Task<List<string>> GetFeatureIdsByKeywordAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return new List<string>();
        }

        var normalizedKeyword = keyword.Trim().ToLowerInvariant();

        try
        {
            // 获取所有未删除的特性
            var allFeatures = await _repository.GetListAsync(t => t.DeleteMark == null);

            var matchedFeatureIds = new List<string>();

            // 遍历所有特性，检查关键词是否在 Keywords 字段中
            foreach (var feature in allFeatures)
            {
                if (string.IsNullOrWhiteSpace(feature.Keywords))
                {
                    continue;
                }

                // 解析关键词列表
                var keywords = ParseKeywords(feature.Keywords);

                // 检查是否包含该关键词（不区分大小写）
                if (
                    keywords.Any(k =>
                        k.Trim().Equals(normalizedKeyword, StringComparison.OrdinalIgnoreCase)
                    )
                )
                {
                    matchedFeatureIds.Add(feature.Id);
                }
            }


            return matchedFeatureIds;
        }
        catch (Exception ex)
        {
            Log.Error($"[GetFeatureIdsByKeyword] 数据库查询异常：关键词 '{keyword}', 错误: {ex.Message}");
            // 查询失败时返回空列表，不抛出异常
            return new List<string>();
        }
    }

    /// <summary>
    /// 解析关键词字符串为关键词列表
    /// </summary>
    /// <param name="keywordsJson">关键词JSON字符串</param>
    /// <returns>关键词列表</returns>
    private List<string> ParseKeywords(string keywordsJson)
    {
        var keywords = new List<string>();

        if (string.IsNullOrWhiteSpace(keywordsJson))
        {
            return keywords;
        }

        try
        {
            // 尝试解析为JSON数组
            var parsedKeywords = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(
                keywordsJson
            );
            if (parsedKeywords != null)
            {
                foreach (var keyword in parsedKeywords)
                {
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        var trimmed = keyword.Trim().ToLowerInvariant();
                        if (!keywords.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
                        {
                            keywords.Add(trimmed);
                        }
                    }
                }
            }
        }
        catch
        {
            // JSON解析失败，尝试按逗号分割
            var parts = keywordsJson.Split(
                new[] { ',', '，', ';', '；' },
                StringSplitOptions.RemoveEmptyEntries
            );
            foreach (var part in parts)
            {
                var trimmed = part.Trim().ToLowerInvariant();
                if (
                    !string.IsNullOrWhiteSpace(trimmed)
                    && !keywords.Contains(trimmed, StringComparer.OrdinalIgnoreCase)
                )
                {
                    keywords.Add(trimmed);
                }
            }
        }

        return keywords;
    }

    /// <summary>
    /// 测试重复创建逻辑 (验证: 多次输入相同文本是否创建多条AI建议记录)
    /// </summary>
    [HttpPost("test-duplicate-creation")]
    public async Task<object> TestDuplicateCreation()
    {
        var testKey = $"TEST_MOCK_AI:DupeTest_{DateTime.Now.Ticks}";

        // 1. 调用第一次
        var input = new BatchMatchInput
        {
            Items = new List<MatchItemInput>
            {
                new MatchItemInput { Id = "1", Query = testKey },
            },
        };
        await BatchMatch(input);

        // 2. 调用第二次
        var input2 = new BatchMatchInput
        {
            Items = new List<MatchItemInput>
            {
                new MatchItemInput { Id = "2", Query = testKey },
            },
        };
        await BatchMatch(input2);

        // 3. 验证数据库
        var corrections = await _correctionRepository.GetListAsync(c =>
            c.InputText == testKey && c.MatchMode == "ai_proposal"
        );
        var count = corrections.Count;

        // 4. 清理
        if (count > 0)
        {
            await _correctionRepository.DeleteAsync(corrections);
        }

        return new
        {
            Success = count >= 2,
            Count = count,
            Message = count >= 2
                ? "测试通过：成功创建了多条重复记录"
                : $"测试失败：只创建了 {count} 条记录",
            TestKey = testKey,
        };
    }

    /// <summary>
    /// 测试不匹配逻辑 (验证: 无效输入不应匹配到任何特性)
    /// </summary>
    [HttpPost("test-mismatch-logic")]
    public async Task<object> TestMismatchLogic()
    {
        // 1. 输入一个绝对不存在的文本
        var testKey = $"NO_MATCH_TEXT_{DateTime.Now.Ticks}";
        var input = new BatchMatchInput
        {
            Items = new List<MatchItemInput>
            {
                new MatchItemInput { Id = "1", Query = testKey },
            },
        };

        var results = await BatchMatch(input);
        var result = results.FirstOrDefault();

        if (result == null)
            return new { Success = false, Message = "未返回结果" };

        // 2. 验证
        var isPerfectMatch = result.IsPerfectMatch;
        var correctionCount = result.ManualCorrections?.Count ?? 0;

        return new
        {
            Success = !isPerfectMatch && correctionCount == 0,
            Query = testKey,
            IsPerfectMatch = isPerfectMatch,
            CorrectionCount = correctionCount,
            Corrections = result.ManualCorrections?.Select(c => c.FeatureName).ToList(),
            Message = (!isPerfectMatch && correctionCount == 0)
                ? "测试通过：未匹配到任何特性"
                : "测试失败：匹配到了特性或推荐了无关特性",
        };
    }

    #endregion

    private async Task ClearCacheAsync(string? id = null)
    {
        if (!string.IsNullOrEmpty(id))
        {
            await _cacheManager.DelAsync(GetCacheKey($"info:{id}"));
        }
    }
}

/// <summary>
/// 添加关键字输入
/// </summary>
public class AddKeywordInput
{
    /// <summary>
    /// 特性ID（必填）
    /// </summary>
    public string FeatureId { get; set; }

    /// <summary>
    /// 要添加的关键字（必填）
    /// </summary>
    public string Keyword { get; set; }
}

/// <summary>
/// 创建或添加关键词输入
/// </summary>
public class CreateOrAddKeywordInput
{
    /// <summary>
    /// 输入文本（如"严重脆"）
    /// </summary>
    public string InputText { get; set; }

    /// <summary>
    /// 自动匹配的特征ID（如果有）
    /// </summary>
    public string AutoMatchedFeatureId { get; set; }

    /// <summary>
    /// 特性大类ID（必填）
    /// </summary>
    public string CategoryId { get; set; }

    /// <summary>
    /// 特性名称（必填，如"脆"）
    /// </summary>
    public string FeatureName { get; set; }

    /// <summary>
    /// 特性等级名称（必填，如"严重"）
    /// </summary>
    public string SeverityLevelName { get; set; }

    /// <summary>
    /// 描述（可选）
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 排序码（可选）
    /// </summary>
    public long? SortCode { get; set; }

    /// <summary>
    /// 使用场景（test/import）
    /// </summary>
    public string Scenario { get; set; }
}

/// <summary>
/// 创建或添加关键词输出
/// </summary>
public class CreateOrAddKeywordOutput
{
    /// <summary>
    /// 创建或更新的特性信息
    /// </summary>
    public AppearanceFeatureInfoOutput Feature { get; set; }

    /// <summary>
    /// 执行的操作（create/add_keyword）
    /// </summary>
    public string Action { get; set; }

    /// <summary>
    /// 操作结果消息
    /// </summary>
    public string Message { get; set; }
}

/// <summary>
/// 添加带等级的特性输入
/// </summary>
public class AddWithSeverityInput
{
    /// <summary>
    /// 特性大类ID（必填）
    /// </summary>
    public string CategoryId { get; set; }

    /// <summary>
    /// 特性名称（必填）
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 特性等级ID（必填）
    /// </summary>
    public string SeverityLevelId { get; set; }

    /// <summary>
    /// 描述（可选）
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 关键字列表（可选）
    /// </summary>
    public string Keywords { get; set; }

    /// <summary>
    /// 排序码（可选）
    /// </summary>
    public long? SortCode { get; set; }
}
