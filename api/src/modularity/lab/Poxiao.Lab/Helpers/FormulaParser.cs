using System.Data;
using System.Text.RegularExpressions;
using Poxiao.DependencyInjection;

namespace Poxiao.Lab.Helpers;

/// <summary>
/// 公式解析器接口.
/// </summary>
public interface IFormulaParser
{
    /// <summary>
    /// 计算公式表达式.
    /// </summary>
    decimal? Calculate(string formula, Dictionary<string, object> variables);

    /// <summary>
    /// 验证表达式语法.
    /// </summary>
    bool ValidateExpression(string expression);

    /// <summary>
    /// 提取公式中使用的变量名.
    /// </summary>
    List<string> ExtractVariables(string formula);
}

public class FormulaParser : IFormulaParser, ITransient
{
    public decimal? Calculate(string formula, Dictionary<string, object> variables)
    {
        try
        {
            // 0. 预处理：将 IF 替换为 IIF (DataTable 使用 IIF)
            // Replace word boundary IF with IIF, but ignore if it's already IIF
            // Simple approach: regex replace \bIF\b with IIF
            string expression = Regex.Replace(formula, @"\bIF\b", "IIF", RegexOptions.IgnoreCase);

            // 1. 扩展 TO 运算符
            // 处理 [Start] TO [End] 语法，包括动态列 [DetectionColumns]
            expression = ExpandRangeOperators(expression, variables);

            // 2. 替换变量 (同时处理 null 值)
            // 注意：ExpandRangeOperators 可能会生成新的变量引用（如 [Detection3]），所以必须在之后替换变量
            expression = ReplaceVariables(expression, variables);

            // 3. 计算自定义统计函数 (SUM, AVG, MAX, MIN)
            // 此时变量已替换为数值，函数参数为数字列表：SUM(1, 2, 3)
            expression = EvaluateStatisticalFunctions(expression);

            // 4. 验证并计算最终表达式
            if (!ValidateExpression(expression))
                throw new Exception("Expression syntax error after processing");

            var dataTable = new DataTable();
            var result = dataTable.Compute(expression, null);

            if (result is DBNull || result == null)
                return 0m;

            return Convert.ToDecimal(result);
        }
        catch (Exception ex)
        {
            throw new Exception($"Calculation failed: {ex.Message} (Expr: {formula})", ex);
        }
    }

    public bool ValidateExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return false;
        try
        {
            // 尝试替换一下可能的 IF -> IIF 以便通过验证
            var tempExpr = Regex.Replace(expression, @"\bIF\b", "IIF", RegexOptions.IgnoreCase);

            // 使用DataTable.Compute验证表达式
            var dataTable = new DataTable();
            dataTable.Compute(tempExpr, null);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string ExpandRangeOperators(string formula, Dictionary<string, object> variables)
    {
        // 匹配模式：[ColumnA] TO [ColumnB]
        // 或者是：VariableA TO VariableB (如果变量不带方括号)
        // 假设公式构建器产生的都是 [Name] 格式

        // Regex: \[(.*?)\]\s+TO\s+\[(.*?)\]
        // group 1: Start, group 2: End

        return Regex.Replace(
            formula,
            @"\[(.*?)\]\s+TO\s+\[(.*?)\]",
            match =>
            {
                string startCol = match.Groups[1].Value.Trim();
                string endCol = match.Groups[2].Value.Trim();

                // 解析开始列的前缀和索引 (e.g. Detection1 -> Detection, 1)
                var startMatch = Regex.Match(startCol, @"^([a-zA-Z]+)(\d+)$");
                if (!startMatch.Success)
                {
                    // 如果无法解析数字后缀，暂时不支持，原样返回或抛错
                    // 或者如果是普通变量 TO 普通变量且没有数字规律？暂不支持。
                    return match.Value;
                }

                string prefix = startMatch.Groups[1].Value;
                int startIndex = int.Parse(startMatch.Groups[2].Value);
                int endIndex = -1;

                // 判断结束列是否是动态列 "DetectionColumns" (忽略大小写)
                // 或者检测列字段名 F_DETECTION_COLUMNS 对应的属性名 DetectionColumns
                if (
                    string.Equals(endCol, "DetectionColumns", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(endCol, "检测列", StringComparison.OrdinalIgnoreCase)
                )
                {
                    // 从变量中获取 DetectionColumns 的值
                    // 变量key可能是 "DetectionColumns"
                    if (variables.TryGetValue("DetectionColumns", out object val) && val != null)
                    {
                        endIndex = Convert.ToInt32(val);
                    }
                    else
                    {
                        // 默认值或报错？
                        return "0"; // 无法展开，返回0安全吗？
                    }
                }
                else
                {
                    // 尝试解析结束列的索引
                    // 必须具有相同的前缀
                    var endMatch = Regex.Match(endCol, @"^([a-zA-Z]+)(\d+)$");
                    if (
                        endMatch.Success
                        && string.Equals(
                            endMatch.Groups[1].Value,
                            prefix,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        endIndex = int.Parse(endMatch.Groups[2].Value);
                    }
                }

                if (endIndex < startIndex)
                {
                    return "0"; // 范围无效
                }

                // 生成列列表
                var columns = new List<string>();
                for (int i = startIndex; i <= endIndex; i++)
                {
                    columns.Add($"[{prefix}{i}]");
                }

                return string.Join(", ", columns);
            }
        );
    }

    private string EvaluateStatisticalFunctions(string expression)
    {
        // 处理 SUM, AVG, MAX, MIN
        // 这些函数此刻参数已经是数字：SUM(1.1, 2.2, 3.3)
        // 支持嵌套？目前正则只匹配最内层，简单循环处理直到没有匹配

        string[] funcs = new[] { "SUM", "AVG", "MAX", "MIN" };
        bool found = true;

        while (found)
        {
            found = false;
            // 匹配 FUNC(arg1, arg2...)
            // \( ([^()]+) \)  匹配不包含括号的内容，即最内层
            expression = Regex.Replace(
                expression,
                @"(SUM|AVG|MAX|MIN)\s*\(([^()]+)\)",
                match =>
                {
                    found = true;
                    string funcName = match.Groups[1].Value.ToUpper();
                    string argsStr = match.Groups[2].Value;

                    var args = argsStr
                        .Split(',')
                        .Select(s =>
                        {
                            if (decimal.TryParse(s.Trim(), out decimal d))
                                return d;
                            return 0m;
                        })
                        .ToList();

                    if (!args.Any())
                        return "0";

                    decimal result = 0;
                    switch (funcName)
                    {
                        case "SUM":
                            result = args.Sum();
                            break;
                        case "AVG":
                            result = args.Average();
                            break;
                        case "MAX":
                            result = args.Max();
                            break;
                        case "MIN":
                            result = args.Min();
                            break;
                    }
                    return result.ToString();
                }
            );
        }

        return expression;
    }

    private string ReplaceVariables(string formula, Dictionary<string, object> variables)
    {
        // 先按长度倒序，避免部分匹配 (e.g. Detection1 vs Detection10)
        var sortedKeys = variables.Keys.OrderByDescending(k => k.Length).ToList();

        // 1. 先替换带方括号的变量 [VarName]
        // 这样可以精确匹配
        foreach (var key in sortedKeys)
        {
            string bracketPattern = $@"\[{key}\]";
            if (formula.Contains(bracketPattern))
            {
                var val = variables[key];
                string valStr = (val == null || val is DBNull) ? "0" : val.ToString();
                formula = formula.Replace(bracketPattern, valStr);
            }
        }

        // 2. 为了兼容，尝试替换不带方括号的单词匹配
        // 但要注意，不要替换掉已经是数字的部分
        // 也不要替换掉函数名
        foreach (var key in sortedKeys)
        {
            // 只有当公式里还有这个key的单词时才替换 (且不是在 [] 里面，虽然上面已经替换了 [])
            // Regex \bKEY\b
            if (Regex.IsMatch(formula, $@"\b{key}\b"))
            {
                var val = variables[key];
                string valStr = (val == null || val is DBNull) ? "0" : val.ToString();
                formula = Regex.Replace(formula, $@"\b{key}\b", valStr);
            }
        }

        // 清理剩余的 [] ? 有些可能是未匹配到的变量，设为0
        // formula = Regex.Replace(formula, @"\[.*?\]", "0");

        return formula;
    }

    /// <inheritdoc />
    public List<string> ExtractVariables(string formula)
    {
        var variables = new List<string>();

        if (string.IsNullOrWhiteSpace(formula))
            return variables;

        // 1. 提取 [Var] 格式
        var bracketMatches = Regex.Matches(formula, @"\[(.*?)\]");
        foreach (Match match in bracketMatches)
        {
            variables.Add(match.Groups[1].Value);
        }

        // 2. 提取 TO 语句中的潜在变量
        // [A] TO [DetectionColumns] -> implicit vars A..N, and DetectionColumns
        // 这里只是静态提取，可能无法知道 DetectionColumns 的值，无法展开
        // 但我们至少应该提取出 "DetectionColumns" 及其它显式变量
        // 如果是 [Detection1] TO [DetectionColumns]，我们至少提取了 Detection1 和 DetectionColumns
        // 至于中间的隐式变量（Detection2...），在没有运行时值的情况下无法提取。
        // 这通常用于静态依赖分析。对于动态范围，可能需要一种特殊标记或容忍。

        // 3. 兼容旧的无括号变量匹配 (可选)
        // ...

        return variables.Distinct().ToList();
    }

    private bool IsOperatorOrFunction(string token)
    {
        // ... unused internal helper or keep for legacy ExtractVariables raw word logic
        return false;
    }

    private bool IsNumber(string token)
    {
        return decimal.TryParse(token, out _);
    }
}
