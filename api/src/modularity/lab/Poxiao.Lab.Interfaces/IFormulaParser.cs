namespace Poxiao.Lab.Interfaces;

/// <summary>
/// 公式解析器接口.
/// </summary>
public interface IFormulaParser
{
    /// <summary>
    /// 提取公式中的变量名（不带方括号）.
    /// </summary>
    /// <param name="formula">公式表达式.</param>
    /// <returns>变量名列表.</returns>
    List<string> ExtractVariables(string formula);

    /// <summary>
    /// 计算公式，返回结果.
    /// </summary>
    /// <param name="formula">公式表达式.</param>
    /// <param name="context">上下文数据（支持 Dictionary 或实体对象）.</param>
    /// <returns>计算结果.</returns>
    decimal? Calculate(string formula, object context);

    /// <summary>
    /// 验证表达式语法.
    /// </summary>
    /// <param name="expression">表达式.</param>
    /// <returns>是否有效.</returns>
    bool ValidateExpression(string expression);
}
