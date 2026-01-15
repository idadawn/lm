using System;
using System.Collections.Generic;
using System.Linq;

public class Program
{
    public static void Main()
    {
        var template =
            @"# Role
你是一个金属带材外观检测专家，专门负责识别和分类外观特性。

# Task
从用户输入中提取""特性名称""和""特性等级""。

# Reference
## 特性大类和特性名称（关联关系）
{{CATEGORY_FEATURES}}

## 可选等级（标准化）
{{SEVERITY_LEVELS}}

# Constraints
1. 如果没有提到等级，默认为""默认""";

        var categoryFeatures = new Dictionary<string, List<string>>
        {
            {
                "Cat1",
                new List<string> { "F1", "F2" }
            },
            {
                "Cat2",
                new List<string> { "F3" }
            },
        };

        var severityLevels = new List<string> { "L1", "L2", "L3" };

        var result = BuildSystemPrompt(template, categoryFeatures, severityLevels);

        Console.WriteLine("--- Result ---");
        Console.WriteLine(result);

        if (result.Contains("{{CATEGORY_FEATURES}}"))
        {
            Console.WriteLine("FAIL: CATEGORY_FEATURES placeholder still exists!");
        }
        else
        {
            Console.WriteLine("PASS: CATEGORY_FEATURES replaced.");
        }

        if (result.Contains("{{SEVERITY_LEVELS}}"))
        {
            Console.WriteLine("FAIL: SEVERITY_LEVELS placeholder still exists!");
        }
        else
        {
            Console.WriteLine("PASS: SEVERITY_LEVELS replaced.");
        }
    }

    private static string BuildSystemPrompt(
        string template,
        Dictionary<string, List<string>> categoryFeatures,
        List<string> severityLevels
    )
    {
        // 按照"大类: 特性名称1、特性名称2..."的格式组织
        var categoryFeaturesText = new List<string>();
        if (categoryFeatures != null && categoryFeatures.Any())
        {
            foreach (var kvp in categoryFeatures.OrderBy(x => x.Key))
            {
                if (kvp.Value != null && kvp.Value.Any())
                {
                    var featuresText = string.Join("、", kvp.Value);
                    categoryFeaturesText.Add($"{kvp.Key}: {featuresText}");
                }
            }
        }

        var categoryFeaturesFormatted = categoryFeaturesText.Any()
            ? string.Join("\n", categoryFeaturesText)
            : "韧性: 脆\n脆边: 烂边\n麻点: 小麻点、大麻点\n划痕: 轻微划痕、严重划痕";

        var severityLevelsText =
            severityLevels != null && severityLevels.Any()
                ? string.Join("、", severityLevels)
                : "默认、轻微、微、中等、严重、超级";

        return template
            .Replace("{{CATEGORY_FEATURES}}", categoryFeaturesFormatted)
            .Replace("{{SEVERITY_LEVELS}}", severityLevelsText);
    }
}
