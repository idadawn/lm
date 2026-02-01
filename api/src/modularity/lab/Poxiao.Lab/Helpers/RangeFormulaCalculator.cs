using Poxiao.DependencyInjection;
using System.Text.RegularExpressions;

namespace Poxiao.Lab.Helpers;

/// <summary>
/// 范围公式计算器
/// 支持格式: AVG(RANGE(Thickness, 1, $DetectionColumns))
/// </summary>
public class RangeFormulaCalculator : ITransient
{
    /// <summary>
    /// 计算范围公式
    /// </summary>
    /// <param name="formula">公式字符串</param>
    /// <param name="dataRow">数据行对象</param>
    /// <returns>计算结果</returns>
    public decimal? Calculate(string formula, object dataRow)
    {
        if (string.IsNullOrWhiteSpace(formula))
        {
            return null;
        }

        try
        {
            // 解析公式
            var parsed = ParseFormula(formula);

            // 解析结束位置
            int endIndex = ResolveEndIndex(parsed.End, dataRow);

            // 收集范围内的值
            var values = GetRangeValues(dataRow, parsed.Prefix, parsed.Start, endIndex);

            // 执行计算
            return ExecuteOperation(parsed, values);
        }
        catch (Exception ex)
        {
            throw new Exception($"范围公式计算失败: {formula}", ex);
        }
    }

    /// <summary>
    /// 解析公式字符串
    /// </summary>
    private ParsedFormula ParseFormula(string formula)
    {
        // 匹配标准格式: OPERATION(RANGE(Prefix, Start, End))
        var match = Regex.Match(formula, @"^(\w+)\(RANGE\((\w+),\s*(\d+),\s*([\w$]+)\)\)$");

        if (match.Success)
        {
            return new ParsedFormula
            {
                Operation = match.Groups[1].Value,
                Prefix = match.Groups[2].Value,
                Start = int.Parse(match.Groups[3].Value),
                End = match.Groups[4].Value,
            };
        }

        // 匹配差值格式: DIFF_FIRST_LAST(N1, N2, RANGE(Prefix, Start, End))
        var diffMatch = Regex.Match(
            formula,
            @"^DIFF_FIRST_LAST\((\d+),\s*(\d+),\s*RANGE\((\w+),\s*(\d+),\s*([\w$]+)\)\)$"
        );

        if (diffMatch.Success)
        {
            return new ParsedFormula
            {
                Operation = "DIFF_FIRST_LAST",
                FirstN = int.Parse(diffMatch.Groups[1].Value),
                LastN = int.Parse(diffMatch.Groups[2].Value),
                Prefix = diffMatch.Groups[3].Value,
                Start = int.Parse(diffMatch.Groups[4].Value),
                End = diffMatch.Groups[5].Value,
            };
        }

        throw new FormatException($"无法解析公式: {formula}");
    }

    /// <summary>
    /// 解析结束位置
    /// </summary>
    private int ResolveEndIndex(string end, object dataRow)
    {
        // 如果以 $ 开头,表示引用字段的值
        if (end.StartsWith("$"))
        {
            string fieldName = end.Substring(1);
            var property = dataRow.GetType().GetProperty(fieldName);

            if (property == null)
            {
                throw new Exception($"找不到字段: {fieldName}");
            }

            var value = property.GetValue(dataRow);

            if (value == null)
            {
                // 如果字段值为空,默认使用22
                return 22;
            }

            return Convert.ToInt32(value);
        }

        // 否则直接解析为数字
        return int.Parse(end);
    }

    /// <summary>
    /// 获取范围内的值
    /// </summary>
    private List<decimal> GetRangeValues(object dataRow, string prefix, int start, int end)
    {
        var values = new List<decimal>();
        var rowType = dataRow.GetType();

        // 确保范围在1-22之间
        start = Math.Max(1, start);
        end = Math.Min(22, end);

        for (int i = start; i <= end; i++)
        {
            // 构造属性名: Detection1, Detection2, ..., Thickness1, Thickness2, ...
            string propertyName = $"{prefix}{i}";
            var property = rowType.GetProperty(propertyName);

            if (property == null)
            {
                continue;
            }

            var value = property.GetValue(dataRow);

            // 只收集非空的decimal值
            if (value != null)
            {
                try
                {
                    decimal decimalValue = Convert.ToDecimal(value);
                    values.Add(decimalValue);
                }
                catch { }
            }
        }

        return values;
    }

    /// <summary>
    /// 执行运算
    /// </summary>
    private decimal? ExecuteOperation(ParsedFormula parsed, List<decimal> values)
    {
        // 如果没有有效值,返回null
        if (values.Count == 0)
        {
            return null;
        }

        switch (parsed.Operation?.ToUpper())
        {
            case "AVG":
                return values.Average();

            case "MAX":
                return values.Max();

            case "MIN":
                return values.Min();

            case "SUM":
                return values.Sum();

            case "COUNT":
                return values.Count;

            case "DIFF_FIRST_LAST":
                return CalculateDiffFirstLast(values, parsed.FirstN ?? 2, parsed.LastN ?? 2);

            default:
                throw new NotSupportedException($"不支持的操作: {parsed.Operation}");
        }
    }

    /// <summary>
    /// 计算前后差值
    /// </summary>
    private decimal? CalculateDiffFirstLast(List<decimal> values, int firstN, int lastN)
    {
        if (values.Count < firstN + lastN)
        {
            // 数据不足,返回null
            return null;
        }

        // 前N个的平均值
        var firstAvg = values.Take(firstN).Average();

        // 后N个的平均值
        var lastAvg = values.TakeLast(lastN).Average();

        // 返回差值的绝对值
        return Math.Abs(firstAvg - lastAvg);
    }
}

/// <summary>
/// 解析后的公式结构
/// </summary>
public class ParsedFormula
{
    /// <summary>
    /// 操作类型: AVG, MAX, MIN, SUM, COUNT, DIFF_FIRST_LAST
    /// </summary>
    public string Operation { get; set; }

    /// <summary>
    /// 列前缀: Detection, Thickness
    /// </summary>
    public string Prefix { get; set; }

    /// <summary>
    /// 起始索引
    /// </summary>
    public int Start { get; set; }

    /// <summary>
    /// 结束索引 (数字字符串或 $字段名)
    /// </summary>
    public string End { get; set; }

    /// <summary>
    /// 前N个 (用于DIFF_FIRST_LAST)
    /// </summary>
    public int? FirstN { get; set; }

    /// <summary>
    /// 后N个 (用于DIFF_FIRST_LAST)
    /// </summary>
    public int? LastN { get; set; }
}
