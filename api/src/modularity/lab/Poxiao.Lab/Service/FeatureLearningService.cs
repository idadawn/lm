using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Config;
using Poxiao.Lab.Entity.Dto.AppearanceFeature;
using Poxiao.Lab.Entity.Models;
using Poxiao.Lab.Interfaces;
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
    private readonly ISqlSugarRepository<ExcelImportTemplateEntity> _templateRepository;
    private readonly IAppearanceFeatureService _appearanceFeatureService;

    public FeatureLearningService(
        ISqlSugarRepository<AppearanceFeatureCorrectionEntity> correctionRepository,
        ISqlSugarRepository<AppearanceFeatureEntity> featureRepository,
        ISqlSugarRepository<AppearanceFeatureCategoryEntity> categoryRepository,
        ISqlSugarRepository<ExcelImportTemplateEntity> templateRepository,
        IAppearanceFeatureService appearanceFeatureService
    )
    {
        _correctionRepository = correctionRepository;
        _featureRepository = featureRepository;
        _categoryRepository = categoryRepository;
        _templateRepository = templateRepository;
        _appearanceFeatureService = appearanceFeatureService;
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

        if (!string.IsNullOrWhiteSpace(input.FeatureId))
        {
            correction.CorrectedFeatureId = input.FeatureId;
        }

        if (!string.IsNullOrWhiteSpace(input.Status))
        {
            correction.Status = input.Status;
        }
        else
        {
            correction.Status = "Pending"; // 修改后重置为待处理
        }

        if (input.Remark != null)
        {
            correction.Remark = input.Remark;
        }
        correction.LastModify();

        await _correctionRepository
            .AsUpdateable(correction)
            .UpdateColumns(c => new
            {
                c.CorrectedFeatureId,
                c.Status,
                c.Remark,
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

    /// <summary>
    /// 将Excel列字母转换为数字索引 (A=0, B=1, C=2, ..., Z=25, AA=26, AB=27, ...)
    /// </summary>
    private int ConvertColumnLetterToIndex(string columnLetter)
    {
        if (string.IsNullOrWhiteSpace(columnLetter))
            return 0;

        columnLetter = columnLetter.Trim().ToUpper();
        int result = 0;
        for (int i = 0; i < columnLetter.Length; i++)
        {
            result *= 26;
            result += columnLetter[i] - 'A' + 1;
        }
        return result - 1; // 转为0-based索引
    }

    /// <summary>
    /// 忽略的关键字列表（这些文字不是特性）
    /// </summary>
    private static readonly HashSet<string> IgnoredKeywords = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "合并",
        "取消",
        "正常",
        "无",
        "空",
        "N/A",
        "NA",
        "-",
        "—",
        "/",
        "待定",
        "暂无",
        "略",
        "见备注",
        "同上",
        "参考",
        "见",
        "详见",
        "合格",
        "不合格",
        "OK",
        "NG",
        "PASS",
        "FAIL",
        "是",
        "否",
        "有",
        "无异常",
        "正常品",
    };

    /// <summary>
    /// 检查关键字是否应该被忽略
    /// </summary>
    private bool IsIgnoredKeyword(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return true;

        keyword = keyword.Trim();

        // 检查完全匹配
        if (IgnoredKeywords.Contains(keyword))
            return true;

        // 检查纯数字（如 "1", "2", "123"）
        if (decimal.TryParse(keyword, out _))
            return true;

        // 检查单个字符（可能是编号）
        if (keyword.Length == 1 && !char.IsLetter(keyword[0]))
            return true;

        return false;
    }

    /// <summary>
    /// 上传Excel生成人工修正列表
    /// </summary>
    /// <param name="file">Excel文件</param>
    /// <returns>生成的修正记录数量</returns>
    [HttpPost("upload-excel")]
    public async Task<UploadExcelResult> UploadExcelForCorrection(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw Oops.Oh("请上传Excel文件");

        var result = new UploadExcelResult();

        try
        {
            // 1. 获取RawDataImport模板配置
            var templateEntity = await _templateRepository.GetFirstAsync(t =>
                t.TemplateCode == "RawDataImport" && (t.DeleteMark == 0 || t.DeleteMark == null)
            );

            if (templateEntity == null || string.IsNullOrWhiteSpace(templateEntity.ConfigJson))
            {
                throw Oops.Oh("未找到RawDataImport模板配置，请先配置导入模板");
            }

            // 解析模板配置
            ExcelTemplateConfig config;
            try
            {
                var json = templateEntity.ConfigJson;
                // 处理可能的双重序列化
                if (json.StartsWith("\"") && json.EndsWith("\""))
                {
                    json = System.Text.Json.JsonSerializer.Deserialize<string>(json);
                }
                config = System.Text.Json.JsonSerializer.Deserialize<ExcelTemplateConfig>(json);
            }
            catch (Exception ex)
            {
                throw Oops.Oh($"解析模板配置失败: {ex.Message}");
            }

            // 2. 找到炉号字段的列索引
            var furnaceNoMapping = config.FieldMappings?.FirstOrDefault(m =>
                m.Field.Equals("FurnaceNo", StringComparison.OrdinalIgnoreCase)
            );

            if (
                furnaceNoMapping == null
                || string.IsNullOrWhiteSpace(furnaceNoMapping.ExcelColumnIndex)
            )
            {
                throw Oops.Oh("模板中未配置炉号(FurnaceNo)字段的列索引，请先在模板中配置");
            }

            var furnaceNoColumnIndex = furnaceNoMapping.ExcelColumnIndex; // 例如 "A", "B", "C"
            Console.WriteLine($"[Excel] 使用模板配置，炉号列索引: {furnaceNoColumnIndex}");

            // 3. 读取Excel文件
            using var stream = file.OpenReadStream();
            var rows = MiniExcelLibs.MiniExcel.Query(stream, useHeaderRow: false).ToList();

            if (rows == null || rows.Count <= 1) // 至少要有表头行+数据行
            {
                result.Message = "Excel文件没有数据";
                return result;
            }

            // 将列索引转换为数字索引 (A=0, B=1, C=2...)
            int columnIndex = ConvertColumnLetterToIndex(furnaceNoColumnIndex);

            // 4. 提取所有炉号和解析特性后缀（跳过第一行表头）
            var featureTexts = new Dictionary<string, string>(); // FeatureSuffix -> 原始输入文本示例
            var featureCounts = new Dictionary<string, int>(); // 统计每个特性出现次数
            int noColumnCount = 0;
            int parseFailCount = 0;
            int noFeatureSuffixCount = 0;

            for (int i = 1; i < rows.Count; i++) // 从1开始，跳过表头
            {
                var row = rows[i];
                var rowDict = row as IDictionary<string, object>;
                if (rowDict == null)
                    continue;

                // 根据列索引获取炉号值
                string furnaceNoValue = null;
                var keys = rowDict.Keys.ToList();
                if (columnIndex < keys.Count)
                {
                    furnaceNoValue = rowDict[keys[columnIndex]]?.ToString();
                }

                if (string.IsNullOrWhiteSpace(furnaceNoValue))
                {
                    noColumnCount++;
                    continue;
                }

                // 解析炉号提取特性后缀
                var furnaceNo = FurnaceNo.Parse(furnaceNoValue);
                if (!furnaceNo.IsValid)
                {
                    parseFailCount++;
                    Console.WriteLine(
                        $"[Excel] 炉号解析失败: {furnaceNoValue} - {furnaceNo.ErrorMessage}"
                    );
                    continue;
                }

                if (string.IsNullOrWhiteSpace(furnaceNo.FeatureSuffix))
                {
                    noFeatureSuffixCount++;
                    continue;
                }

                var suffix = furnaceNo.FeatureSuffix.Trim();

                // 检查是否在忽略列表中
                if (IsIgnoredKeyword(suffix))
                {
                    noFeatureSuffixCount++; // 计入无特性后缀
                    continue;
                }

                if (!featureTexts.ContainsKey(suffix))
                {
                    featureTexts[suffix] = furnaceNoValue;
                    featureCounts[suffix] = 1;
                }
                else
                {
                    featureCounts[suffix]++;
                }
            }

            result.TotalRows = rows.Count - 1; // 减去表头行
            result.UniqueFeatureTexts = featureTexts.Count;

            // 构建详细的统计信息
            var statsMessage =
                $"总行数: {rows.Count}, 未找到炉号列: {noColumnCount}, 炉号解析失败: {parseFailCount}, 无特性后缀: {noFeatureSuffixCount}, 唯一特性文字: {featureTexts.Count}";
            Console.WriteLine($"[Excel] {statsMessage}");
            if (featureCounts.Count > 0)
            {
                Console.WriteLine(
                    $"[Excel] 特性统计: {string.Join(", ", featureCounts.Select(kv => $"{kv.Key}={kv.Value}次"))}"
                );
            }

            if (featureTexts.Count == 0)
            {
                result.Message =
                    $"解析了 {rows.Count} 行数据，未发现任何特性后缀文字。{statsMessage}";
                return result;
            }

            // 4. 批量匹配特性
            var matchInput = new BatchMatchInput
            {
                Items = featureTexts
                    .Select(
                        (kvp, idx) => new MatchItemInput { Id = idx.ToString(), Query = kvp.Key }
                    )
                    .ToList(),
            };

            var matchResults = await _appearanceFeatureService.BatchMatch(matchInput);

            // 5. 筛选非100%匹配的结果，创建修正记录
            var newCorrectionCount = 0;
            foreach (var matchResult in matchResults)
            {
                // 跳过100%匹配
                if (matchResult.IsPerfectMatch)
                    continue;

                var featureText = matchResult.Query;
                var originalInput = featureTexts.ContainsKey(featureText)
                    ? featureTexts[featureText]
                    : featureText;

                // 检查是否已存在相同InputText的记录
                var existingCorrection = await _correctionRepository.GetFirstAsync(c =>
                    c.InputText == featureText && c.DeleteMark == null
                );

                if (existingCorrection != null)
                    continue; // 跳过重复

                // 创建新的修正记录
                var correction = new AppearanceFeatureCorrectionEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    InputText = featureText,
                    MatchMode = "excel_import",
                    Scenario = "import",
                    Status = "Pending",
                    Remark = $"从Excel导入，原始炉号: {originalInput}",
                };
                correction.Create();

                await _correctionRepository.InsertAsync(correction);
                newCorrectionCount++;
            }

            result.NewCorrections = newCorrectionCount;
            result.PerfectMatches = matchResults.Count(r => r.IsPerfectMatch);

            // 构建特性统计字符串
            var featureStatsStr =
                featureCounts.Count > 0
                    ? string.Join(", ", featureCounts.Select(kv => $"'{kv.Key}'出现{kv.Value}次"))
                    : "";

            result.Message =
                $"处理完成：总{rows.Count}行，找到{featureTexts.Count}种唯一特性（{featureStatsStr}），{result.PerfectMatches}个100%匹配（跳过），新增{newCorrectionCount}条待修正记录。「无特性后缀:{noFeatureSuffixCount}行，解析失败:{parseFailCount}行」";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FeatureLearning] Excel上传处理失败: {ex.Message}");
            throw Oops.Oh($"处理Excel失败: {ex.Message}");
        }

        return result;
    }
}

/// <summary>
/// Excel上传结果
/// </summary>
public class UploadExcelResult
{
    public int TotalRows { get; set; }
    public int UniqueFeatureTexts { get; set; }
    public int PerfectMatches { get; set; }
    public int NewCorrections { get; set; }
    public string Message { get; set; }
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
    public string Status { get; set; }
    public string Remark { get; set; }
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
