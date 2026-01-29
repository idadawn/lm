using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Poxiao.DependencyInjection;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Attributes;
using Poxiao.Lab.Entity.Config;
using Poxiao.Lab.Interfaces;

namespace Poxiao.Lab.Service;

public class FormulaParser : IFormulaParser, ITransient
{
    // 匹配 [ColumnName] 格式的变量
    private static readonly Regex BracketVariableRegex = new(
        @"\[([^\[\]]+)\]",
        RegexOptions.Compiled
    );

    // 匹配 $VariableName 格式的动态变量（如 $DetectionColumns）
    private static readonly Regex DollarVariableRegex = new(
        @"\$(\w+)",
        RegexOptions.Compiled
    );

    // 匹配 TO 范围语法: [Start] TO [End] 或 Start TO End
    private static readonly Regex ToRangeRegex = new(
        @"\[?(\w+)\]?\s+TO\s+\[?(\w+)\]?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // 匹配三参数 RANGE 函数: RANGE(prefix, startIndex, endIndex)
    // 支持: RANGE(Thickness, 1, $DetectionColumns) 或 RANGE(Thickness, 1, 22)
    private static readonly Regex RangeThreeArgRegex = new(
        @"RANGE\s*\(\s*(\w+)\s*,\s*(\d+|\$\w+)\s*,\s*(\d+|\$\w+)\s*\)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // 匹配单参数 RANGE 函数: RANGE(prefix) - 自动检测范围
    private static readonly Regex RangeSingleArgRegex = new(
        @"RANGE\s*\(\s*(\w+)\s*\)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // 匹配 DIFF_FIRST_LAST 函数
    private static readonly Regex DiffFirstLastRegex = new(
        @"DIFF_FIRST_LAST\s*\(\s*(\w+)\s*(?:,\s*(\d+|\$\w+)\s*(?:,\s*(\d+|\$\w+)\s*)?)?\)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // 匹配统计函数起始: SUM( / AVG( / MAX( / MIN(
    private static readonly Regex StatFunctionRegex = new(
        @"\b(SUM|AVG|MAX|MIN)\s*\(",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // 检测列默认值（Detection1 ~ Detection22）
    private const int DefaultDetectionColumns = 22;

    private readonly LabOptions _options;

    public FormulaParser(IOptions<LabOptions> options)
    {
        _options = options?.Value ?? new LabOptions();
    }

    /// <summary>
    /// 提取公式中的变量名（返回不带方括号/美元符号的名称）
    /// </summary>
    public List<string> ExtractVariables(string formula)
    {
        if (string.IsNullOrWhiteSpace(formula))
        {
            return new List<string>();
        }

        var variables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 提取 [ColumnName] 格式的变量
        foreach (Match match in BracketVariableRegex.Matches(formula))
        {
            var varName = match.Groups[1].Value.Trim();
            if (!string.IsNullOrWhiteSpace(varName) && !IsNumeric(varName))
            {
                variables.Add(varName);
            }
        }

        // 提取 $VariableName 格式的变量
        foreach (Match match in DollarVariableRegex.Matches(formula))
        {
            var varName = match.Groups[1].Value.Trim();
            if (!string.IsNullOrWhiteSpace(varName))
            {
                variables.Add(varName);
            }
        }

        return variables.ToList();
    }

    /// <summary>
    /// 验证表达式语法
    /// </summary>
    public bool ValidateExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        try
        {
            // 提取所有变量
            var variables = ExtractVariables(expression);
            var dummyContext = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            // 为每个变量设置默认值(使用非整数避免简单除零错误)
            foreach (var varName in variables)
            {
                dummyContext[varName] = 1.1m;
            }

            // 添加常用的动态变量默认值
            if (!dummyContext.ContainsKey("DetectionColumns"))
            {
                dummyContext["DetectionColumns"] = DefaultDetectionColumns;
            }

            // 尝试计算，复用 Calculate 的预处理逻辑
            Calculate(expression, dummyContext);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 计算公式
    /// </summary>
    public decimal? Calculate(string formula, object context)
    {
        if (string.IsNullOrWhiteSpace(formula))
        {
            return null;
        }

        try
        {
            // 构建上下文字典
            var contextDict = BuildContextDictionary(context);

            // 获取实体（用于范围函数）
            var entity = context as IntermediateDataEntity;

            // 预处理公式
            var processedFormula = PreprocessFormula(formula, contextDict, entity);

            // 计算表达式
            return EvaluateExpression(processedFormula);
        }
        catch (Exception ex)
        {
            throw new FormulaCalculationException(
                $"公式计算失败: {formula}", ex);
        }
    }

    /// <summary>
    /// 构建上下文字典（忽略大小写）
    /// </summary>
    private Dictionary<string, object> BuildContextDictionary(object context)
    {
        if (context is Dictionary<string, object> dict)
        {
            return new Dictionary<string, object>(dict, StringComparer.OrdinalIgnoreCase);
        }

        if (context is IDictionary<string, object> idict)
        {
            return new Dictionary<string, object>(idict, StringComparer.OrdinalIgnoreCase);
        }

        // 从对象属性构建
        var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        if (context == null) return result;

        var properties = context.GetType().GetProperties();
        foreach (var prop in properties)
        {
            try
            {
                var value = prop.GetValue(context);
                result[prop.Name] = value;
            }
            catch
            {
                // 忽略无法读取的属性
            }
        }

        return result;
    }

    /// <summary>
    /// 预处理公式：处理特殊语法并替换变量
    /// </summary>
    private string PreprocessFormula(
        string formula,
        Dictionary<string, object> contextDict,
        IntermediateDataEntity entity)
    {
        var result = formula;

        // 1. 处理 IF -> IIF 替换（兼容 DataTable.Compute）
        result = Regex.Replace(result, @"\bIF\s*\(", "IIF(", RegexOptions.IgnoreCase);
        // 2. 展开 TO 范围表达式 [Detection1] TO [Detection5]
        result = ExpandToRange(result, contextDict);

        // 3. 处理统计函数 SUM/AVG/MAX/MIN（支持 RANGE/DIFF_FIRST_LAST）
        //    支持 RANGE/DIFF_FIRST_LAST 的嵌套表达式
        result = ProcessStatFunctions(result, contextDict, entity);

        // 4. 处理三参数 RANGE：RANGE(Thickness, 1, )
        result = ProcessRangeThreeArg(result, contextDict, entity);

        // 5. 处理单参数 RANGE：RANGE(Detection)
        result = ProcessRangeSingleArg(result, contextDict, entity);

        // 6. 处理 DIFF_FIRST_LAST
        result = ProcessDiffFirstLast(result, contextDict, entity);

        // 7. 替换  动态变量
        result = ReplaceDollarVariables(result, contextDict, entity);

        // 8. 替换 [Variable] 为实际值
        result = ReplaceVariables(result, contextDict);
        // 9. 处理比较运算符
        result = result.Replace("<>", "!=");

        return result;
    }

    /// <summary>
    /// 替换 $Variable 格式的动态变量
    /// </summary>
    private string ReplaceDollarVariables(
        string formula,
        Dictionary<string, object> contextDict,
        IntermediateDataEntity entity)
    {
        return DollarVariableRegex.Replace(formula, match =>
        {
            var varName = match.Groups[1].Value;

            if (TryGetValueFromContext(contextDict, varName, out var value))
            {
                if (value is int or long or decimal or double or float)
                {
                    return Convert.ToDecimal(value).ToString(CultureInfo.InvariantCulture);
                }
                return value?.ToString() ?? "0";
            }

            // 特殊处理：DetectionColumns 默认为实体或默认值
            if (varName.Equals("DetectionColumns", StringComparison.OrdinalIgnoreCase))
            {
                return GetDetectionColumns(contextDict, entity).ToString(CultureInfo.InvariantCulture);
            }

            return "0";
        });
    }

    /// <summary>
    /// 处理三参数 RANGE 函数
    /// RANGE(Thickness, 1, 22) 或 RANGE(Thickness, 1, $DetectionColumns)
    /// 返回指定范围内字段值的极差（最大值 - 最小值）
    /// </summary>
    private string ProcessRangeThreeArg(
        string formula,
        Dictionary<string, object> contextDict,
        IntermediateDataEntity entity)
    {
        return RangeThreeArgRegex.Replace(formula, match =>
        {
            var prefix = ResolveRangePrefix(match.Groups[1].Value);
            var startStr = match.Groups[2].Value;
            var endStr = match.Groups[3].Value;
            var maxColumns = GetDetectionColumns(contextDict, entity);

            // 解析起始索引
            int startIndex;
            if (startStr.StartsWith("$"))
            {
                var varName = startStr.Substring(1);
                startIndex = GetIntFromContext(contextDict, varName, 1);
            }
            else
            {
                startIndex = int.Parse(startStr);
            }

            // 解析结束索引
            int endIndex;
            if (endStr.StartsWith("$"))
            {
                var varName = endStr.Substring(1);
                endIndex = GetIntFromContext(contextDict, varName, maxColumns);
            }
            else
            {
                endIndex = int.Parse(endStr);
            }

            // 获取范围内的值
            var values = GetRangeValues(prefix, startIndex, endIndex, contextDict, entity, maxColumns);

            if (values.Count < 2)
            {
                return "0";
            }

            // 计算极差并应用精度处理
            var range = values.Max() - values.Min();
            var precision = GetCalculationPrecision(null);
            var roundedRange = Math.Round(range, precision, MidpointRounding.AwayFromZero);
            return roundedRange.ToString(CultureInfo.InvariantCulture);
        });
    }

    /// <summary>
    /// 处理单参数 RANGE 函数
    /// RANGE(Detection) - 自动检测 Detection1 ~ DetectionN 的范围
    /// </summary>
    private string ProcessRangeSingleArg(
        string formula,
        Dictionary<string, object> contextDict,
        IntermediateDataEntity entity)
    {
        return RangeSingleArgRegex.Replace(formula, match =>
        {
            var prefix = ResolveRangePrefix(match.Groups[1].Value);

            // 获取动态列数
            var columnCount = GetDetectionColumns(contextDict, entity);

            // 获取范围内的值
            var values = GetRangeValues(prefix, 1, columnCount, contextDict, entity, columnCount);

            if (values.Count < 2)
            {
                return "0";
            }

            // 计算极差并应用精度处理
            var range = values.Max() - values.Min();
            var precision = GetCalculationPrecision(null);
            var roundedRange = Math.Round(range, precision, MidpointRounding.AwayFromZero);
            return roundedRange.ToString(CultureInfo.InvariantCulture);
        });
    }

    /// <summary>
    /// 处理 DIFF_FIRST_LAST 函数
    /// DIFF_FIRST_LAST(Detection) 或 DIFF_FIRST_LAST(Detection, 1, $DetectionColumns)
    /// 返回首尾元素的差值绝对值
    /// </summary>
    private string ProcessDiffFirstLast(
        string formula,
        Dictionary<string, object> contextDict,
        IntermediateDataEntity entity)
    {
        return DiffFirstLastRegex.Replace(formula, match =>
        {
            var prefix = ResolveRangePrefix(match.Groups[1].Value);
            var startStr = match.Groups[2].Success ? match.Groups[2].Value : "1";
            var endStr = match.Groups[3].Success ? match.Groups[3].Value : null;
            var maxColumns = GetDetectionColumns(contextDict, entity);

            // 解析起始索引
            int startIndex;
            if (startStr.StartsWith("$"))
            {
                startIndex = GetIntFromContext(contextDict, startStr.Substring(1), 1);
            }
            else
            {
                startIndex = int.Parse(startStr);
            }

            // 解析结束索引
            int endIndex;
            if (endStr != null)
            {
                if (endStr.StartsWith("$"))
                {
                    endIndex = GetIntFromContext(contextDict, endStr.Substring(1), maxColumns);
                }
                else
                {
                    endIndex = int.Parse(endStr);
                }
            }
            else
            {
                endIndex = maxColumns;
            }

            // 获取范围内的值
            var values = GetRangeValues(prefix, startIndex, endIndex, contextDict, entity, maxColumns);

            if (values.Count < 2)
            {
                return "0";
            }

            // 计算首尾差值并应用精度处理
            var diff = Math.Abs(values.First() - values.Last());
            var precision = GetCalculationPrecision(null);
            var roundedDiff = Math.Round(diff, precision, MidpointRounding.AwayFromZero);
            return roundedDiff.ToString(CultureInfo.InvariantCulture);
        });
    }

    /// <summary>
    /// 获取指定范围内的字段值列表
    /// </summary>
    private List<decimal> GetRangeValues(
        string prefix,
        int startIndex,
        int endIndex,
        Dictionary<string, object> contextDict,
        IntermediateDataEntity entity,
        int maxColumns)
    {
        var values = new List<decimal>();

        // 确保索引范围有效
        startIndex = Math.Max(1, startIndex);
        endIndex = Math.Min(maxColumns, endIndex);

        for (var i = startIndex; i <= endIndex; i++)
        {
            var fieldName = $"{prefix}{i}";
            decimal? value = null;

            // 优先从上下文字典获取
            if (TryGetDecimalFromContext(contextDict, fieldName, out var ctxValue))
            {
                value = ctxValue;
            }
            // 其次从实体获取
            else if (entity != null)
            {
                value = GetDecimalFromEntity(entity, fieldName);
            }

            if (value.HasValue)
            {
                values.Add(value.Value);
            }
        }

        return values;
    }

    /// <summary>
    /// 从实体获取 decimal 值
    /// </summary>
    private decimal? GetDecimalFromEntity(IntermediateDataEntity entity, string propertyName)
    {
        var prop = typeof(IntermediateDataEntity).GetProperty(propertyName);
        if (prop == null) return null;

        var value = prop.GetValue(entity);
        if (value == null) return null;

        if (decimal.TryParse(value.ToString(), NumberStyles.Any,
            CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return null;
    }

    /// <summary>
    /// 展开 TO 范围语法
    /// 例如: SUM([Detection1] TO [Detection5]) -> SUM([Detection1], [Detection2], ...)
    /// </summary>
    private string ExpandToRange(string formula, Dictionary<string, object> contextDict)
    {
        return ToRangeRegex.Replace(formula, match =>
        {
            var startField = match.Groups[1].Value;
            var endField = match.Groups[2].Value;

            // 解析起始字段
            var startMatch = Regex.Match(startField, @"^(\D+)(\d+)$");
            if (!startMatch.Success)
            {
                return match.Value;
            }

            var prefix = startMatch.Groups[1].Value;
            var startIndex = int.Parse(startMatch.Groups[2].Value);

            // 解析结束字段
            int endIndex;
            var endMatch = Regex.Match(endField, @"^(\D+)(\d+)$");
            if (endMatch.Success)
            {
                endIndex = int.Parse(endMatch.Groups[2].Value);
            }
            // 如果结束字段是动态列数（如 DetectionColumns）
            else if (TryGetValueFromContext(contextDict, endField, out var endValue) &&
                     int.TryParse(endValue?.ToString(), out var dynamicEnd))
            {
                endIndex = dynamicEnd;
            }
            else
            {
                return match.Value;
            }

            // 展开为逗号分隔的字段列表
            var fields = new List<string>();
            for (var i = startIndex; i <= endIndex; i++)
            {
                fields.Add($"[{prefix}{i}]");
            }

            return string.Join(", ", fields);
        });
    }

    /// <summary>
    /// 处理统计函数（递归处理嵌套）
    /// </summary>
    private string ProcessStatFunctions(
        string formula,
        Dictionary<string, object> contextDict,
        IntermediateDataEntity entity)
    {
        var result = formula;
        var maxIterations = 20; // 防止无限循环

        for (var i = 0; i < maxIterations; i++)
        {
            var match = StatFunctionRegex.Match(result);
            if (!match.Success) break;

            var funcName = match.Groups[1].Value.ToUpperInvariant();
            var argsStart = match.Index + match.Length;
            var depth = 1;
            var pos = argsStart;
            for (; pos < result.Length; pos++)
            {
                if (result[pos] == '(') depth++;
                else if (result[pos] == ')') depth--;
                if (depth == 0) break;
            }

            if (depth != 0 || pos <= argsStart)
            {
                break;
            }

            var argsStr = result.Substring(argsStart, pos - argsStart);

            // 解析参数
            var args = ParseFunctionArguments(argsStr, contextDict, entity);

            if (args.Count == 0)
            {
                result = result.Substring(0, match.Index)
                    + "0"
                    + result.Substring(pos + 1);
                continue;
            }

            decimal calcResult = funcName switch
            {
                "SUM" => args.Sum(),
                "AVG" => args.Average(),
                "MAX" => args.Max(),
                "MIN" => args.Min(),
                _ => 0
            };

            // 应用精度处理（使用默认精度）
            var precision = GetCalculationPrecision(null);
            var roundedResult = Math.Round(calcResult, precision, MidpointRounding.AwayFromZero);

            result = result.Substring(0, match.Index)
                + roundedResult.ToString(CultureInfo.InvariantCulture)
                + result.Substring(pos + 1);
        }

        return result;
    }
    private List<decimal> ParseFunctionArguments(
        string argsStr,
        Dictionary<string, object> contextDict,
        IntermediateDataEntity entity)
    {
        var results = new List<decimal>();
        if (string.IsNullOrWhiteSpace(argsStr))
        {
            return results;
        }

        var trimmedArgs = argsStr.Trim();

        // RANGE/DIFF_FIRST_LAST 作为唯一参数
        if (TryAppendRangeValues(trimmedArgs, contextDict, entity, results))
        {
            return results;
        }

        var parts = SplitArguments(trimmedArgs);

        foreach (var part in parts)
        {
            var trimmed = part.Trim();

            if (TryAppendRangeValues(trimmed, contextDict, entity, results))
            {
                continue;
            }

            // 判断是否为 [ColumnName] 形式
            var bracketMatch = BracketVariableRegex.Match(trimmed);
            if (bracketMatch.Success)
            {
                var varName = bracketMatch.Groups[1].Value;
                if (TryGetDecimalFromContext(contextDict, varName, out var val))
                {
                    results.Add(val);
                }
                continue;
            }

            // 判断是否为  形式
            var dollarMatch = DollarVariableRegex.Match(trimmed);
            if (dollarMatch.Success)
            {
                var varName = dollarMatch.Groups[1].Value;
                if (TryGetDecimalFromContext(contextDict, varName, out var val))
                {
                    results.Add(val);
                }
                continue;
            }

            // 直接解析数字
            if (decimal.TryParse(trimmed, NumberStyles.Any,
                CultureInfo.InvariantCulture, out var num))
            {
                results.Add(num);
            }
        }

        return results;
    }

    /// <summary>
    /// 处理 RANGE/DIFF_FIRST_LAST 参数并写入列表
    /// </summary>
    private bool TryAppendRangeValues(
        string expression,
        Dictionary<string, object> contextDict,
        IntermediateDataEntity entity,
        List<decimal> results)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        // RANGE(prefix, start, end)
        var rangeMatch = RangeThreeArgRegex.Match(expression);
        if (rangeMatch.Success && rangeMatch.Value.Equals(expression, StringComparison.OrdinalIgnoreCase))
        {
            var prefix = ResolveRangePrefix(rangeMatch.Groups[1].Value);
            var startStr = rangeMatch.Groups[2].Value;
            var endStr = rangeMatch.Groups[3].Value;
            var maxColumns = GetDetectionColumns(contextDict, entity);

            var startIndex = startStr.StartsWith("$")
                ? GetIntFromContext(contextDict, startStr.Substring(1), 1)
                : int.Parse(startStr);

            var endIndex = endStr.StartsWith("$")
                ? GetIntFromContext(contextDict, endStr.Substring(1), maxColumns)
                : int.Parse(endStr);

            var values = GetRangeValues(prefix, startIndex, endIndex, contextDict, entity, maxColumns);
            if (values.Count > 0)
            {
                results.AddRange(values);
            }
            return true;
        }

        // RANGE(prefix)
        var singleMatch = RangeSingleArgRegex.Match(expression);
        if (singleMatch.Success && singleMatch.Value.Equals(expression, StringComparison.OrdinalIgnoreCase))
        {
            var prefix = ResolveRangePrefix(singleMatch.Groups[1].Value);
            var columnCount = GetDetectionColumns(contextDict, entity);
            var values = GetRangeValues(prefix, 1, columnCount, contextDict, entity, columnCount);
            if (values.Count > 0)
            {
                results.AddRange(values);
            }
            return true;
        }

        // DIFF_FIRST_LAST(prefix, start?, end?)
        var diffMatch = DiffFirstLastRegex.Match(expression);
        if (diffMatch.Success && diffMatch.Value.Equals(expression, StringComparison.OrdinalIgnoreCase))
        {
            var prefix = ResolveRangePrefix(diffMatch.Groups[1].Value);
            var startStr = diffMatch.Groups[2].Success ? diffMatch.Groups[2].Value : "1";
            var endStr = diffMatch.Groups[3].Success ? diffMatch.Groups[3].Value : null;
            var maxColumns = GetDetectionColumns(contextDict, entity);

            var startIndex = startStr.StartsWith("$")
                ? GetIntFromContext(contextDict, startStr.Substring(1), 1)
                : int.Parse(startStr);

            int endIndex;
            if (endStr != null)
            {
                endIndex = endStr.StartsWith("$")
                    ? GetIntFromContext(contextDict, endStr.Substring(1), maxColumns)
                    : int.Parse(endStr);
            }
            else
            {
                endIndex = maxColumns;
            }

            var values = GetRangeValues(prefix, startIndex, endIndex, contextDict, entity, maxColumns);
            if (values.Count >= 2)
            {
                var diff = Math.Abs(values.First() - values.Last());
                results.Add(diff);
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// 通过列属性获取 RangePrefix
    /// </summary>
    private static string ResolveRangePrefix(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return prefix;
        }

        var prop = typeof(IntermediateDataEntity).GetProperty(
            prefix,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase
        );
        if (prop == null)
        {
            return prefix;
        }

        var attr = prop.GetCustomAttribute<IntermediateDataColumnAttribute>();
        if (attr?.IsRange == true && !string.IsNullOrWhiteSpace(attr.RangePrefix))
        {
            return attr.RangePrefix;
        }

        return prefix;
    }

    /// <summary>
    /// 分割参数（支持嵌套括号）
    /// </summary>
    private static List<string> SplitArguments(string argsStr)
    {
        var parts = new List<string>();
        var current = new List<char>();
        var depth = 0;

        foreach (var ch in argsStr)
        {
            if (ch == '(')
            {
                depth++;
            }
            else if (ch == ')')
            {
                depth = Math.Max(0, depth - 1);
            }

            if (ch == ',' && depth == 0)
            {
                parts.Add(new string(current.ToArray()));
                current.Clear();
                continue;
            }

            current.Add(ch);
        }

        if (current.Count > 0)
        {
            parts.Add(new string(current.ToArray()));
        }

        return parts;
    }

    private string ReplaceVariables(string formula, Dictionary<string, object> contextDict)
    {
        return BracketVariableRegex.Replace(formula, match =>
        {
            var varName = match.Groups[1].Value.Trim();

            if (TryGetDecimalFromContext(contextDict, varName, out var value))
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }

            // 变量未找到，返回 0
            return "0";
        });
    }

    /// <summary>
    /// 从上下文获取值（支持大小写不敏感）
    /// </summary>
    private bool TryGetValueFromContext(
        Dictionary<string, object> context,
        string key,
        out object result)
    {
        result = null;

        if (context.TryGetValue(key, out result))
        {
            return true;
        }

        // 大小写不敏感查找
        var actualKey = context.Keys.FirstOrDefault(k =>
            string.Equals(k, key, StringComparison.OrdinalIgnoreCase));

        if (actualKey != null)
        {
            result = context[actualKey];
            return true;
        }

        return false;
    }

    /// <summary>
    /// 从上下文获取 decimal 值
    /// </summary>
    private bool TryGetDecimalFromContext(
        Dictionary<string, object> context,
        string key,
        out decimal result)
    {
        result = 0;

        if (!TryGetValueFromContext(context, key, out var value) || value == null)
        {
            return false;
        }

        if (value is decimal dec)
        {
            result = dec;
            return true;
        }

        if (value is int intVal)
        {
            result = intVal;
            return true;
        }

        if (value is double dblVal)
        {
            result = Convert.ToDecimal(dblVal);
            return true;
        }

        if (value is long longVal)
        {
            result = longVal;
            return true;
        }

        if (value is float fltVal)
        {
            result = Convert.ToDecimal(fltVal);
            return true;
        }

        return decimal.TryParse(value.ToString(), NumberStyles.Any,
            CultureInfo.InvariantCulture, out result);
    }

    /// <summary>
    /// 获取检测列数（优先上下文，其次实体，最后默认值）
    /// </summary>
    private int GetDetectionColumns(Dictionary<string, object> context, IntermediateDataEntity entity)
    {
        var fromContext = GetIntFromContext(context, "DetectionColumns", -1);
        if (fromContext > 0)
        {
            return fromContext;
        }

        if (entity?.DetectionColumns != null && entity.DetectionColumns.Value > 0)
        {
            return entity.DetectionColumns.Value;
        }

        return DefaultDetectionColumns;
    }

    /// <summary>
    /// 从上下文获取 int 值
    /// </summary>
    private int GetIntFromContext(Dictionary<string, object> context, string key, int defaultValue)
    {
        if (!TryGetValueFromContext(context, key, out var value) || value == null)
        {
            return defaultValue;
        }

        if (value is int intVal) return intVal;
        if (value is long longVal) return (int)longVal;
        if (value is decimal decVal) return (int)decVal;
        if (value is double dblVal) return (int)dblVal;

        if (int.TryParse(value.ToString(), out var parsed))
        {
            return parsed;
        }

        return defaultValue;
    }

    /// <summary>
    /// 计算数学表达式
    /// </summary>
    private decimal? EvaluateExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return null;
        }

        try
        {
            var table = new DataTable();
            var result = table.Compute(expression, string.Empty);

            if (result == null || result == DBNull.Value)
            {
                return null;
            }

            return Convert.ToDecimal(result, CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            throw new FormulaCalculationException(
                $"表达式计算失败: {expression}", ex);
        }
    }

    /// <summary>
    /// 获取计算精度（小数点位数）
    /// 根据配置决定是否启用精度调整：
    /// - EnablePrecisionAdjustment = true: 在原始精度基础上加1位，但不超过MaxPrecision
    /// - EnablePrecisionAdjustment = false: 使用原始精度，但不超过MaxPrecision（默认行为）
    /// </summary>
    /// <param name="originalPrecision">原始精度（小数点位数）</param>
    /// <returns>调整后的精度（小数点位数）</returns>
    public int GetCalculationPrecision(int? originalPrecision)
    {
        var maxPrecision = _options.Formula?.MaxPrecision ?? 6;
        var enableAdjustment = _options.Formula?.EnablePrecisionAdjustment ?? false;

        // 如果没有原始精度，返回默认精度
        if (!originalPrecision.HasValue)
        {
            return _options.Formula?.DefaultPrecision ?? 6;
        }

        // 如果启用精度调整，在原始精度基础上加1位
        if (enableAdjustment)
        {
            var adjustedPrecision = originalPrecision.Value + 1;
            return Math.Min(adjustedPrecision, maxPrecision);
        }

        // 默认行为：使用原始精度，但不超过最大值
        return Math.Min(originalPrecision.Value, maxPrecision);
    }

    /// <summary>
    /// 对计算结果进行精度调整（用于保证计算精度）
    /// </summary>
    /// <param name="value">计算结果值</param>
    /// <param name="targetPrecision">目标精度（小数点位数）</param>
    /// <returns>调整后的值</returns>
    public decimal? RoundForPrecision(decimal? value, int? targetPrecision)
    {
        if (!value.HasValue)
        {
            return null;
        }

        var precision = GetCalculationPrecision(targetPrecision);
        return Math.Round(value.Value, precision, MidpointRounding.AwayFromZero);
    }

    private static bool IsNumeric(string value)
    {
        return decimal.TryParse(value, NumberStyles.Any,
            CultureInfo.InvariantCulture, out _);
    }
}

public class FormulaCalculationException : Exception
{
    public FormulaCalculationException(string message) : base(message) { }
    public FormulaCalculationException(string message, Exception inner) : base(message, inner) { }
}





