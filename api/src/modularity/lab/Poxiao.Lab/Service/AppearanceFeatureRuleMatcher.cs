using Poxiao.DependencyInjection;
using Poxiao.Lab.Entity.Dto.AppearanceFeature;
using Poxiao.Lab.Entity.Entity;

namespace Poxiao.Lab.Service;

/// <summary>
/// 外观特性规则匹配器 - 基于关键词和规则引擎的精确匹配
/// 匹配逻辑：
/// 1. 用户输入格式：特性名称 或 程度词+特性名称（如 "脆"、"微脆"）
/// 2. 通过特性名称和程度词匹配特性记录（特征大类+特性名称+特性等级）
/// 3. 如果没有明确程度，优先匹配默认等级
/// </summary>
public class AppearanceFeatureRuleMatcher : ITransient
{
    /// <summary>
    /// 特性分隔符（用于组合特性，包含标点符号）
    /// </summary>
    private static readonly string[] FeatureSeparators = { "、", "，" };

    /// <summary>
    /// 匹配结果
    /// </summary>
    public class MatchResult
    {
        /// <summary>
        /// 匹配到的特性
        /// </summary>
        public AppearanceFeatureEntity Feature { get; set; }

        /// <summary>
        /// 识别到的程度词（如 "微"、"轻"、"重"）
        /// </summary>
        public string DegreeWord { get; set; }

        /// <summary>
        /// 置信度 0-1
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// 匹配方式：name/keyword
        /// </summary>
        public string MatchMethod { get; set; }

        /// <summary>
        /// 是否需要确认等级（当程度词匹配不到对应记录时，需要用户确认是否创建新记录）
        /// </summary>
        public bool RequiresSeverityConfirmation { get; set; }

        /// <summary>
        /// 建议的等级名称（当需要确认时，显示给用户）
        /// </summary>
        public string SuggestedSeverity { get; set; }
    }

    /// <summary>
    /// 匹配输入文本到特征列表
    /// </summary>
    /// <param name="inputText">输入文本</param>
    /// <param name="features">特性列表</param>
    /// <param name="degreeWords">程度词列表（从特征等级接口获取，按长度降序排列）</param>
    /// <param name="categoryIdToName">大类ID到名称的映射（用于匹配时使用名称）</param>
    /// <param name="severityLevelIdToName">等级ID到名称的映射（用于匹配时使用名称）</param>
    public List<MatchResult> Match(
        string inputText,
        List<AppearanceFeatureEntity> features,
        List<string> degreeWords = null,
        Dictionary<string, string> categoryIdToName = null,
        Dictionary<string, string> severityLevelIdToName = null
    )
    {
        if (string.IsNullOrWhiteSpace(inputText) || features == null || !features.Any())
        {
            return new List<MatchResult>();
        }

        // 如果没有提供程度词列表或列表为空，无法进行程度词提取，直接进行特性匹配（不提取程度词）
        // 注意：调用方应该确保提供程度词列表，如果没有程度词，应该在调用方提示用户创建特性等级
        var sortedDegreeWords =
            (degreeWords != null && degreeWords.Any())
                ? degreeWords.OrderByDescending(d => d.Length).ToList()
                : new List<string>();

        var results = new List<MatchResult>();
        var normalizedInput = inputText.Trim();

        // 1. 分解组合特性："脆有划痕" -> ["脆", "划痕"]
        var featureParts = SplitFeatures(normalizedInput);

        foreach (var part in featureParts)
        {
            // 1. 优先尝试完全匹配（检查特性名称或关键词）
            // 这样可以确保如果数据库中有 "嘎嘎脆" 这种特性，优先匹配它，而不是拆分成 "嘎嘎"+"脆"
            var fullMatch = MatchFeature(
                part,
                part,
                features,
                null,
                categoryIdToName,
                severityLevelIdToName
            );
            bool isStrongFullMatch =
                fullMatch != null
                && (fullMatch.MatchMethod == "name" || fullMatch.MatchMethod == "keyword");

            if (isStrongFullMatch)
            {
                results.Add(fullMatch);
                continue;
            }

            // 2. 提取程度词："微脆" -> (程度="微", 核心="脆")
            var (degreeWord, coreName) = ExtractDegree(part, sortedDegreeWords);

            // 如果提取到了程度词，且核心词不为空
            if (!string.IsNullOrEmpty(degreeWord) && !string.IsNullOrEmpty(coreName))
            {
                // 3. 匹配特征（带程度词）
                var degreeMatch = MatchFeature(
                    coreName,
                    part,
                    features,
                    degreeWord,
                    categoryIdToName,
                    severityLevelIdToName
                );
                if (degreeMatch != null)
                {
                    results.Add(degreeMatch);
                    continue;
                }
            }

            // 4. 如果带程度词没匹配到（或者没提取到程度词），使用之前的全匹配结果（可能包含后缀匹配）
            if (fullMatch != null)
            {
                results.Add(fullMatch);
            }
        }

        // 按置信度排序
        return results.OrderByDescending(r => r.Confidence).ToList();
    }

    /// <summary>
    /// 分解组合特性
    /// </summary>
    private List<string> SplitFeatures(string text)
    {
        var parts = new List<string>();

        // 尝试用分隔符分割
        foreach (var separator in FeatureSeparators)
        {
            if (text.Contains(separator))
            {
                var splits = text.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var split in splits)
                {
                    var trimmed = split.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        parts.Add(trimmed);
                    }
                }
                return parts;
            }
        }

        // 如果没有分隔符，返回整个文本
        if (!string.IsNullOrWhiteSpace(text))
        {
            parts.Add(text);
        }

        return parts;
    }

    /// <summary>
    /// 提取程度词
    /// </summary>
    /// <param name="text">输入文本</param>
    /// <param name="degreeWords">程度词列表（按长度降序排列）</param>
    private (string degreeWord, string coreName) ExtractDegree(
        string text,
        List<string> degreeWords
    )
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return (null, text);
        }

        // 按长度降序匹配程度词（优先匹配长词，如"超级"而不是"超"）
        foreach (var degree in degreeWords)
        {
            if (!string.IsNullOrEmpty(degree) && text.StartsWith(degree))
            {
                var core = text.Substring(degree.Length).Trim();
                if (!string.IsNullOrEmpty(core))
                {
                    return (degree, core);
                }
            }
        }

        // 没有程度词
        return (null, text);
    }

    /// <summary>
    /// 匹配特征
    /// </summary>
    /// <param name="coreName">核心特性名称（去掉程度词后的部分）</param>
    /// <param name="originalInput">原始输入（包含程度词）</param>
    /// <param name="features">特性列表</param>
    /// <param name="degreeWord">程度词</param>
    /// <param name="categoryIdToName">大类ID到名称的映射</param>
    /// <param name="severityLevelIdToName">等级ID到名称的映射</param>
    private MatchResult MatchFeature(
        string coreName,
        string originalInput,
        List<AppearanceFeatureEntity> features,
        string degreeWord,
        Dictionary<string, string> categoryIdToName,
        Dictionary<string, string> severityLevelIdToName
    )
    {
        if (string.IsNullOrWhiteSpace(coreName))
        {
            return null;
        }

        // ========== 优先级1: 精确匹配特性名称和等级 ==========
        // 例如：输入"微脆"，匹配 Name="脆" 且 SeverityLevelName="微" 的特性
        if (!string.IsNullOrEmpty(degreeWord) && severityLevelIdToName != null)
        {
            // 找到匹配该等级名称的特性
            var exactMatch = features.FirstOrDefault(f =>
                !string.IsNullOrEmpty(f.Name)
                && f.Name.Equals(coreName, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(f.SeverityLevelId)
                && severityLevelIdToName.ContainsKey(f.SeverityLevelId)
                && severityLevelIdToName[f.SeverityLevelId]
                    .Equals(degreeWord, StringComparison.OrdinalIgnoreCase)
            );
            if (exactMatch != null)
            {
                var categoryName =
                    categoryIdToName != null && categoryIdToName.ContainsKey(exactMatch.CategoryId)
                        ? categoryIdToName[exactMatch.CategoryId]
                        : null;
                var severityName = severityLevelIdToName[exactMatch.SeverityLevelId];
                Console.WriteLine(
                    $"[RuleMatcher] 精确匹配特性名称和等级: {coreName}+{degreeWord} -> {categoryName}/{exactMatch.Name}/{severityName}"
                );
                return new MatchResult
                {
                    Feature = exactMatch,
                    DegreeWord = degreeWord,
                    Confidence = 1.0,
                    MatchMethod = "name",
                    RequiresSeverityConfirmation = false,
                    SuggestedSeverity = null,
                };
            }
        }

        // ========== 优先级2: 匹配特性名称（优先于关键词） ==========
        // 对应策略第一阶段第1步：特征名称（F_NAME）全匹配
        var nameMatches = features
            .Where(f =>
                !string.IsNullOrEmpty(f.Name)
                && f.Name.Equals(coreName, StringComparison.OrdinalIgnoreCase)
            )
            .ToList();

        if (nameMatches.Any())
        {
            // 如果有程度词，尝试匹配对应等级
            if (!string.IsNullOrEmpty(degreeWord) && severityLevelIdToName != null)
            {
                var nameMatchWithLevel = nameMatches.FirstOrDefault(f =>
                    !string.IsNullOrEmpty(f.SeverityLevelId)
                    && severityLevelIdToName.ContainsKey(f.SeverityLevelId)
                    && severityLevelIdToName[f.SeverityLevelId]
                        .Equals(degreeWord, StringComparison.OrdinalIgnoreCase)
                );

                if (nameMatchWithLevel != null)
                {
                    var categoryName =
                        categoryIdToName != null
                        && categoryIdToName.ContainsKey(nameMatchWithLevel.CategoryId)
                            ? categoryIdToName[nameMatchWithLevel.CategoryId]
                            : null;
                    var severityName = severityLevelIdToName[nameMatchWithLevel.SeverityLevelId];
                    Console.WriteLine(
                        $"[RuleMatcher] 匹配特性名称和等级: {coreName}+{degreeWord} -> {categoryName}/{nameMatchWithLevel.Name}/{severityName}"
                    );
                    return new MatchResult
                    {
                        Feature = nameMatchWithLevel,
                        DegreeWord = degreeWord,
                        Confidence = 0.95,
                        MatchMethod = "name",
                        RequiresSeverityConfirmation = false,
                        SuggestedSeverity = null,
                    };
                }
            }

            // 否则选择默认等级
            var defaultMatch =
                nameMatches.FirstOrDefault(f =>
                    severityLevelIdToName != null
                    && severityLevelIdToName.ContainsKey(f.SeverityLevelId)
                    && (
                        severityLevelIdToName[f.SeverityLevelId] == "默认"
                        || severityLevelIdToName[f.SeverityLevelId] == "中"
                    )
                ) ?? nameMatches.FirstOrDefault();

            if (defaultMatch != null)
            {
                var categoryName =
                    categoryIdToName != null
                    && categoryIdToName.ContainsKey(defaultMatch.CategoryId)
                        ? categoryIdToName[defaultMatch.CategoryId]
                        : null;
                var severityName =
                    severityLevelIdToName != null
                    && severityLevelIdToName.ContainsKey(defaultMatch.SeverityLevelId)
                        ? severityLevelIdToName[defaultMatch.SeverityLevelId]
                        : null;

                bool requiresConfirmation =
                    !string.IsNullOrEmpty(degreeWord)
                    && severityName != null
                    && !severityName.Equals(degreeWord, StringComparison.OrdinalIgnoreCase);

                Console.WriteLine(
                    $"[RuleMatcher] 匹配特性名称: {coreName} -> {categoryName}/{defaultMatch.Name}/{severityName}"
                );
                return new MatchResult
                {
                    Feature = defaultMatch,
                    DegreeWord = degreeWord,
                    Confidence = 0.95,
                    MatchMethod = "name",
                    RequiresSeverityConfirmation = requiresConfirmation,
                    SuggestedSeverity = requiresConfirmation ? degreeWord : null,
                };
            }
        }

        // ========== 优先级3: 匹配关键词 ==========
        // 对应策略第一阶段第2步：关键字（F_KEYWORDS）包含匹配
        foreach (var feature in features)
        {
            var keywords = GetKeywords(feature);
            if (keywords.Any(k => k.Equals(coreName, StringComparison.OrdinalIgnoreCase)))
            {
                // 如果有程度词，尝试匹配对应等级
                if (!string.IsNullOrEmpty(degreeWord) && severityLevelIdToName != null)
                {
                    var keywordMatchWithLevel = features.FirstOrDefault(f =>
                        f.Id == feature.Id
                        && !string.IsNullOrEmpty(f.SeverityLevelId)
                        && severityLevelIdToName.ContainsKey(f.SeverityLevelId)
                        && severityLevelIdToName[f.SeverityLevelId]
                            .Equals(degreeWord, StringComparison.OrdinalIgnoreCase)
                    );
                    if (keywordMatchWithLevel != null)
                    {
                        var categoryName =
                            categoryIdToName != null
                            && categoryIdToName.ContainsKey(keywordMatchWithLevel.CategoryId)
                                ? categoryIdToName[keywordMatchWithLevel.CategoryId]
                                : null;
                        var severityName = severityLevelIdToName[
                            keywordMatchWithLevel.SeverityLevelId
                        ];
                        Console.WriteLine(
                            $"[RuleMatcher] 匹配关键词和等级: {coreName}+{degreeWord} -> {categoryName}/{keywordMatchWithLevel.Name}/{severityName}"
                        );
                        return new MatchResult
                        {
                            Feature = keywordMatchWithLevel,
                            DegreeWord = degreeWord,
                            Confidence = 0.90,
                            MatchMethod = "keyword",
                            RequiresSeverityConfirmation = false,
                            SuggestedSeverity = null,
                        };
                    }
                }

                // 否则返回默认等级
                var defaultKeywordMatch =
                    features.FirstOrDefault(f =>
                        f.Id == feature.Id
                        && severityLevelIdToName != null
                        && severityLevelIdToName.ContainsKey(f.SeverityLevelId)
                        && (
                            severityLevelIdToName[f.SeverityLevelId] == "默认"
                            || severityLevelIdToName[f.SeverityLevelId] == "中"
                        )
                    ) ?? feature;

                var matchCategoryName =
                    categoryIdToName != null
                    && categoryIdToName.ContainsKey(defaultKeywordMatch.CategoryId)
                        ? categoryIdToName[defaultKeywordMatch.CategoryId]
                        : null;
                var matchSeverityName =
                    severityLevelIdToName != null
                    && severityLevelIdToName.ContainsKey(defaultKeywordMatch.SeverityLevelId)
                        ? severityLevelIdToName[defaultKeywordMatch.SeverityLevelId]
                        : null;

                bool requiresConfirmation =
                    !string.IsNullOrEmpty(degreeWord)
                    && matchSeverityName != null
                    && !matchSeverityName.Equals(degreeWord, StringComparison.OrdinalIgnoreCase);

                Console.WriteLine(
                    $"[RuleMatcher] 匹配关键词: {coreName} -> {matchCategoryName}/{defaultKeywordMatch.Name}/{matchSeverityName}"
                );
                return new MatchResult
                {
                    Feature = defaultKeywordMatch,
                    DegreeWord = degreeWord,
                    Confidence = 0.88,
                    MatchMethod = "keyword",
                    RequiresSeverityConfirmation = requiresConfirmation,
                    SuggestedSeverity = requiresConfirmation ? degreeWord : null,
                };
            }
        }

        // ========== 优先级4: 后缀匹配（处理未识别的程度词） ==========
        // 当输入包含未在系统定义的程度词时（如"严重脆"中的"严重"），尝试提取并匹配
        if (string.IsNullOrEmpty(degreeWord))
        {
            var suffixMatch = features
                .SelectMany(f =>
                {
                    var kws = GetKeywords(f);
                    var list = kws.Select(k => new
                        {
                            Feature = f,
                            MatchText = k,
                            Type = "keyword",
                        })
                        .ToList();
                    if (!string.IsNullOrEmpty(f.Name))
                    {
                        list.Add(
                            new
                            {
                                Feature = f,
                                MatchText = f.Name,
                                Type = "name",
                            }
                        );
                    }
                    return list;
                })
                .Where(x =>
                    !string.IsNullOrEmpty(x.MatchText)
                    && coreName.EndsWith(x.MatchText, StringComparison.OrdinalIgnoreCase)
                    && x.MatchText.Length < coreName.Length
                ) // 必须有前缀（即程度词）
                .OrderByDescending(x => x.MatchText.Length) // 优先匹配更长的特征名
                .FirstOrDefault();

            if (suffixMatch != null)
            {
                var matchedFeature = suffixMatch.Feature;
                var detectedDegree = coreName
                    .Substring(0, coreName.Length - suffixMatch.MatchText.Length)
                    .Trim();

                // 尝试查找是否存在匹配该程度词的现有变体
                if (severityLevelIdToName != null)
                {
                    // 在同名或同关键词的特性中查找
                    var sameGroupFeatures = features
                        .Where(f => f.Name == matchedFeature.Name)
                        .ToList();

                    var exactHeightMatch = sameGroupFeatures.FirstOrDefault(f =>
                        !string.IsNullOrEmpty(f.SeverityLevelId)
                        && severityLevelIdToName.ContainsKey(f.SeverityLevelId)
                        && severityLevelIdToName[f.SeverityLevelId]
                            .Equals(detectedDegree, StringComparison.OrdinalIgnoreCase)
                    );

                    if (exactHeightMatch != null)
                    {
                        matchedFeature = exactHeightMatch;
                    }
                }

                var categoryName =
                    categoryIdToName != null
                    && categoryIdToName.ContainsKey(matchedFeature.CategoryId)
                        ? categoryIdToName[matchedFeature.CategoryId]
                        : null;
                var severityName =
                    severityLevelIdToName != null
                    && !string.IsNullOrEmpty(matchedFeature.SeverityLevelId)
                    && severityLevelIdToName.ContainsKey(matchedFeature.SeverityLevelId)
                        ? severityLevelIdToName[matchedFeature.SeverityLevelId]
                        : null;

                bool requiresConfirmation =
                    severityName == null
                    || !severityName.Equals(detectedDegree, StringComparison.OrdinalIgnoreCase);

                Console.WriteLine(
                    $"[RuleMatcher] 后缀匹配: {coreName} -> {matchedFeature.Name} (提取程度词: {detectedDegree})"
                );

                return new MatchResult
                {
                    Feature = matchedFeature,
                    DegreeWord = detectedDegree,
                    Confidence = 0.85,
                    MatchMethod = suffixMatch.Type + "_suffix",
                    RequiresSeverityConfirmation = requiresConfirmation,
                    SuggestedSeverity = requiresConfirmation ? detectedDegree : null,
                };
            }
        }

        // ========== 不再进行部分匹配，避免误匹配 ==========
        // "脆" 不应该匹配到 "脆边"
        Console.WriteLine($"[RuleMatcher] 未找到匹配: {coreName}");
        return null;
    }

    /// <summary>
    /// 获取特性的关键词列表
    /// </summary>
    private List<string> GetKeywords(AppearanceFeatureEntity feature)
    {
        var keywords = new List<string>();

        if (feature == null || string.IsNullOrWhiteSpace(feature.Keywords))
        {
            return keywords;
        }

        try
        {
            // 尝试解析为JSON数组
            var parsedKeywords = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(
                feature.Keywords
            );
            if (parsedKeywords != null)
            {
                foreach (var keyword in parsedKeywords)
                {
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        var trimmed = keyword.Trim();
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
            var parts = feature.Keywords.Split(
                new[] { ',', '，', ';', '；' },
                StringSplitOptions.RemoveEmptyEntries
            );
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
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
}
