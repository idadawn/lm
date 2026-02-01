using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using Poxiao.AI.Interfaces;
using Poxiao.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Poxiao.AI.Service;

/// <summary>
/// 外观特性分析服务实现
/// </summary>
public class AppearanceFeatureAnalysisService : IAppearanceFeatureAnalysisService, ITransient
{
    private readonly IChatClient _chatClient;
    private readonly IConfiguration _configuration;

    public AppearanceFeatureAnalysisService(IConfiguration configuration)
    {
        _configuration = configuration;
        var chatConfig = configuration.GetSection("AI:Chat");
        var endpoint = chatConfig["Endpoint"];
        var modelId = chatConfig["ModelId"] ?? "/data/qwen2.5-7b";
        var apiKey = chatConfig["Key"] ?? "dummy-key";

        if (string.IsNullOrEmpty(endpoint))
        {
            throw new InvalidOperationException("AI:Chat:Endpoint 配置不能为空");
        }

        // Initialize OpenAI Client
        // Note: OpenAI library is used as the underlying client, compatible with vLLM
        var openAiClient = new OpenAI.OpenAIClient(
            new System.ClientModel.ApiKeyCredential(apiKey),
            new OpenAI.OpenAIClientOptions { Endpoint = new Uri(endpoint) }
        );

        // Use Microsoft.Extensions.AI abstraction
        _chatClient = openAiClient.AsChatClient(modelId);
    }

    /// <inheritdoc />
    public async Task<AppearanceFeatureAnalysisResult> AnalyzeAsync(
        string featureSuffix,
        Dictionary<string, List<string>> categoryFeatures,
        List<string> severityLevels,
        string defaultSeverityLevel = null
    )
    {
        if (string.IsNullOrEmpty(featureSuffix))
        {
            return new AppearanceFeatureAnalysisResult
            {
                Success = false,
                ErrorMessage = "特性描述不能为空",
            };
        }

        try
        {
            // Set default if not provided
            if (string.IsNullOrEmpty(defaultSeverityLevel))
            {
                defaultSeverityLevel = "默认";
            }

            // 从配置文件读取提示词模板
            var promptTemplate = GetPromptTemplate();

            // 替换动态占位符
            var systemPrompt = BuildSystemPrompt(
                promptTemplate,
                categoryFeatures,
                severityLevels,
                defaultSeverityLevel
            );

            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, systemPrompt),
                new ChatMessage(ChatRole.User, $"请分析特性描述：{featureSuffix}"),
            };

            var chatOptions = new ChatOptions
            {
                Temperature = 0.1f, // Low temperature for deterministic output
            };

            var response = await _chatClient.CompleteAsync(messages, chatOptions);
            var jsonContent = response.Message.Text;

            // Clean up potentially wrapped markdown code blocks ```json ... ```
            if (jsonContent.StartsWith("```json"))
            {
                jsonContent = jsonContent.Replace("```json", "").Replace("```", "").Trim();
            }
            else if (jsonContent.StartsWith("```"))
            {
                jsonContent = jsonContent.Replace("```", "").Trim();
            }

            if (
                string.IsNullOrWhiteSpace(jsonContent)
                || jsonContent.Trim().Equals("null", StringComparison.OrdinalIgnoreCase)
            )
            {
                return new AppearanceFeatureAnalysisResult
                {
                    Success = false,
                    ErrorMessage = "AI返回结果为空",
                };
            }

            // 解析为数组格式
            var features = JsonSerializer.Deserialize<List<AIFeatureItem>>(jsonContent);

            if (features == null || !features.Any())
            {
                return new AppearanceFeatureAnalysisResult
                {
                    Success = false,
                    ErrorMessage = "无法解析AI返回结果",
                };
            }

            return new AppearanceFeatureAnalysisResult { Success = true, Features = features };
        }
        catch (System.ClientModel.ClientResultException ex)
        {
            // 处理 OpenAI 客户端异常，提供更详细的错误信息
            var errorMessage = $"AI 服务请求失败: {ex.Message}";
            if (ex.Status == 404)
            {
                var endpoint = _configuration.GetSection("AI:Chat")["Endpoint"];
                var modelId = _configuration.GetSection("AI:Chat")["ModelId"] ?? "/data/qwen2.5-7b";
                errorMessage =
                    $"AI 服务端点未找到 (404)。请检查：\n"
                    + $"1. vLLM 服务是否正在运行\n"
                    + $"2. 端点配置是否正确: {endpoint}\n"
                    + $"3. 模型 ID 是否存在: {modelId}\n"
                    + $"4. 完整的请求 URL 应该是: {endpoint}/chat/completions";
            }
            else if (ex.Status == 401)
            {
                errorMessage = "AI 服务认证失败 (401)。请检查 API Key 配置是否正确。";
            }
            else if (ex.Status >= 500)
            {
                errorMessage = $"AI 服务内部错误 ({ex.Status})。请检查 vLLM 服务状态。";
            }

            Console.WriteLine($"Error calling AI service: {errorMessage}");
            Console.WriteLine($"Exception details: {ex}");

            return new AppearanceFeatureAnalysisResult
            {
                Success = false,
                ErrorMessage = errorMessage,
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling AI service: {ex.Message}");
            Console.WriteLine($"Exception type: {ex.GetType().Name}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return new AppearanceFeatureAnalysisResult
            {
                Success = false,
                ErrorMessage = $"AI 服务调用异常: {ex.Message}",
            };
        }
    }

    /// <summary>
    /// 从配置文件获取提示词模板
    /// </summary>
    private string GetPromptTemplate()
    {
        var promptConfig = _configuration.GetSection(
            "AppearanceFeaturePrompt:SystemPromptTemplate"
        );
        if (promptConfig.Exists() && !string.IsNullOrEmpty(promptConfig.Value))
        {
            return promptConfig.Value;
        }

        // 如果配置不存在，返回默认模板
        return @"# Role
你是一个金属带材外观检测专家，专门负责识别和分类外观特性。

# Task
从用户输入中提取""特性名称""和""特性等级""。

# Reference
## 特性大类和特性名称（关联关系）
{{CATEGORY_FEATURES}}

## 可选等级（标准化）
{{SEVERITY_LEVELS}}

# Constraints
1. 如果没有提到等级，默认为""{{DEFAULT_LEVEL}}""
2. 如果是多个特性（如""脆有划痕""），请返回列表
3. 必须返回 JSON 格式，不要有任何额外文字
4. name 字段只包含核心特性名称（如""脆""、""划痕""），不包含等级词
5. level 字段标准化为上述可选等级之一
6. category 字段尽量匹配特性大类，如果无法确定可为空

# Output Format
如果是单个特性：
[{""name"": ""脆"", ""level"": ""{{DEFAULT_LEVEL}}"", ""category"": ""韧性""}]

如果是多个特性：
[
  {""name"": ""脆"", ""level"": ""{{DEFAULT_LEVEL}}"", ""category"": ""韧性""},
  {""name"": ""划痕"", ""level"": ""{{DEFAULT_LEVEL}}"", ""category"": ""划痕""}
]

# Examples
输入: ""微脆""
输出: [{""name"": ""脆"", ""level"": ""微"", ""category"": ""韧性""}]

输入: ""超级脆""
输出: [{""name"": ""脆"", ""level"": ""超级"", ""category"": ""韧性""}]

输入: ""脆有划痕""
输出: [
  {""name"": ""脆"", ""level"": ""{{DEFAULT_LEVEL}}"", ""category"": ""韧性""},
  {""name"": ""划痕"", ""level"": ""{{DEFAULT_LEVEL}}"", ""category"": ""划痕""}
]

输入: ""看起来容易折断""
输出: [{""name"": ""脆"", ""level"": ""{{DEFAULT_LEVEL}}"", ""category"": ""韧性""}]";
    }

    /// <summary>
    /// 构建系统提示词，替换动态占位符
    /// </summary>
    private string BuildSystemPrompt(
        string template,
        Dictionary<string, List<string>> categoryFeatures,
        List<string> severityLevels,
        string defaultSeverityLevel
    )
    {
        // 按照"大类: 特性名称1、特性名称2..."的格式组织
        var categoryFeaturesText = new List<string>();
        if (categoryFeatures != null && categoryFeatures.Any())
        {
            foreach (var kvp in categoryFeatures.OrderBy(x => x.Key))
            {
                var featuresText = string.Join("、", kvp.Value);
                categoryFeaturesText.Add($"{kvp.Key}: {featuresText}");
            }
        }

        var categoryFeaturesFormatted = categoryFeaturesText.Any()
            ? string.Join("\n", categoryFeaturesText)
            : "韧性: 脆\n脆边: 烂边\n麻点: 小麻点、大麻点\n划痕: 轻微划痕、严重划痕";

        var severityLevelsText =
            severityLevels != null && severityLevels.Any()
                ? string.Join("、", severityLevels)
                : "默认、轻微、微、中等、严重、超级";

        // Use Regex for case-insensitive replacement to be robust
        var result = template;

        // 1. Replace Category Features
        var categoryPlaceholder = "{{CATEGORY_FEATURES}}";
        if (
            System.Text.RegularExpressions.Regex.IsMatch(
                result,
                categoryPlaceholder,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            )
        )
        {
            result = System.Text.RegularExpressions.Regex.Replace(
                result,
                categoryPlaceholder,
                categoryFeaturesFormatted,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );
        }

        // 2. Replace Severity Levels
        var severityPlaceholder = "{{SEVERITY_LEVELS}}";
        if (
            System.Text.RegularExpressions.Regex.IsMatch(
                result,
                severityPlaceholder,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            )
        )
        {
            result = System.Text.RegularExpressions.Regex.Replace(
                result,
                severityPlaceholder,
                severityLevelsText,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );
        }

        // 3. Replace Default Level
        // Replace {{DEFAULT_LEVEL}} if exists
        var defaultLevelPlaceholder = "{{DEFAULT_LEVEL}}";
        if (
            System.Text.RegularExpressions.Regex.IsMatch(
                result,
                defaultLevelPlaceholder,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            )
        )
        {
            result = System.Text.RegularExpressions.Regex.Replace(
                result,
                defaultLevelPlaceholder,
                defaultSeverityLevel,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );
        }

        // Also replace hardcoded "默认" in specific instruction if placeholder not found or just to be safe for legacy templates
        // Pattern: 默认为""默认"" or 默认为“默认”
        result = result.Replace("默认为\"\"默认\"\"", $"默认为\"\"{defaultSeverityLevel}\"\"");
        result = result.Replace("默认为“默认”", $"默认为“{defaultSeverityLevel}”");
        result = result.Replace("\"level\": \"默认\"", $"\"level\": \"{defaultSeverityLevel}\""); // Examples replacement

        return result;
    }
}
