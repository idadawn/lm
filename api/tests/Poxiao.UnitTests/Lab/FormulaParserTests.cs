using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Poxiao.Lab.Helpers;
using Poxiao.Lab.Interfaces;
using Xunit;
using Xunit.Abstractions;

namespace Poxiao.UnitTests.Lab;

/// <summary>
/// 公式解析器单元测试.
/// </summary>
public class FormulaParserTests
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<IUnitConversionService> _unitConversionServiceMock;
    private readonly FormulaParser _formulaParser;

    public FormulaParserTests(ITestOutputHelper output)
    {
        _output = output;
        _unitConversionServiceMock = new Mock<IUnitConversionService>();

        // 设置默认模拟行为
        _unitConversionServiceMock
            .Setup(x => x.ConvertAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((decimal value, string from, string to) => value); // 默认返回原值

        // FormulaParser需要RangeFormulaCalculator
        _formulaParser = new FormulaParser(new RangeFormulaCalculator());
    }

    /// <summary>
    /// 测试基本四则运算.
    /// </summary>
    [Theory]
    [InlineData("2 + 3", 5)]
    [InlineData("10 - 4", 6)]
    [InlineData("3 * 4", 12)]
    [InlineData("12 / 4", 3)]
    [InlineData("2 + 3 * 4", 14)] // 乘法优先级
    [InlineData("(2 + 3) * 4", 20)] // 括号改变优先级
    [InlineData("10 % 3", 1)] // 取模运算
    public void Calculate_BasicArithmetic_ReturnsCorrectResult(string formula, decimal expected)
    {
        // Arrange
        var variables = new Dictionary<string, object>();

        // Act
        var result = _formulaParser.Calculate(formula, variables);

        // Assert
        Assert.Equal(expected, result);
        _output.WriteLine($"公式: {formula} = {result}");
    }

    /// <summary>
    /// 测试带变量的计算.
    /// </summary>
    [Fact]
    public void Calculate_WithVariables_ReturnsCorrectResult()
    {
        // Arrange
        string formula = "(width * height) / 2";
        var variables = new Dictionary<string, object> { { "width", 10 }, { "height", 5 } };

        // Act
        var result = _formulaParser.Calculate(formula, variables);

        // Assert
        Assert.Equal(25, result);
        _output.WriteLine($"公式: {formula} (width=10, height=5) = {result}");
    }

    /// <summary>
    /// 测试复杂公式计算.
    /// </summary>
    [Fact]
    public void Calculate_ComplexFormula_ReturnsCorrectResult()
    {
        // Arrange
        string formula = "(a + b) * c - d / e";
        var variables = new Dictionary<string, object>
        {
            { "a", 5 },
            { "b", 3 },
            { "c", 2 },
            { "d", 10 },
            { "e", 2 },
        };

        // Act
        var result = _formulaParser.Calculate(formula, variables);

        // Assert
        // (5 + 3) * 2 - 10 / 2 = 8 * 2 - 5 = 16 - 5 = 11
        Assert.Equal(11, result);
    }

    /// <summary>
    /// 测试变量名包含下划线.
    /// </summary>
    [Fact]
    public void Calculate_VariableWithUnderscore_ReturnsCorrectResult()
    {
        // Arrange
        string formula = "item_count * price_per_item";
        var variables = new Dictionary<string, object>
        {
            { "item_count", 10 },
            { "price_per_item", 25.5m },
        };

        // Act
        var result = _formulaParser.Calculate(formula, variables);

        // Assert
        Assert.Equal(255, result);
    }

    /// <summary>
    /// 测试空公式抛出异常.
    /// </summary>
    [Fact]
    public void Calculate_EmptyFormula_ThrowsException()
    {
        // Arrange
        string formula = "";
        var variables = new Dictionary<string, object>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _formulaParser.Calculate(formula, variables));
    }

    /// <summary>
    /// 测试无效公式抛出异常.
    /// </summary>
    [Fact]
    public void Calculate_InvalidFormula_ThrowsException()
    {
        // Arrange
        string formula = "10 / 0"; // 除以零
        var variables = new Dictionary<string, object>();

        // Act & Assert
        Assert.Throws<Exception>(() => _formulaParser.Calculate(formula, variables));
    }

    /// <summary>
    /// 测试验证有效表达式.
    /// </summary>
    [Theory]
    [InlineData("2 + 3")]
    [InlineData("(a + b) * c")]
    [InlineData("width * height / 2")]
    public void ValidateExpression_ValidExpression_ReturnsTrue(string expression)
    {
        // Act
        var isValid = _formulaParser.ValidateExpression(expression);

        // Assert
        Assert.True(isValid);
        _output.WriteLine($"表达式验证通过: {expression}");
    }

    /// <summary>
    /// 测试验证无效表达式.
    /// </summary>
    [Theory]
    [InlineData("2 + ")] // 不完整的表达式
    [InlineData("* 3")] // 缺少左操作数
    [InlineData("(2 + 3")] // 括号不匹配
    public void ValidateExpression_InvalidExpression_ReturnsFalse(string expression)
    {
        // Act
        var isValid = _formulaParser.ValidateExpression(expression);

        // Assert
        Assert.False(isValid);
        _output.WriteLine($"表达式验证失败: {expression}");
    }

    /// <summary>
    /// 测试提取变量.
    /// </summary>
    [Fact]
    public void ExtractVariables_ValidFormula_ReturnsVariableList()
    {
        // Arrange
        string formula = "(width * height) + (depth * density) - offset";

        // Act
        var variables = _formulaParser.ExtractVariables(formula);

        // Assert
        Assert.Equal(5, variables.Count);
        Assert.Contains("width", variables);
        Assert.Contains("height", variables);
        Assert.Contains("depth", variables);
        Assert.Contains("density", variables);
        Assert.Contains("offset", variables);

        _output.WriteLine($"公式: {formula}");
        _output.WriteLine($"提取的变量: {string.Join(", ", variables)}");
    }

    /// <summary>
    /// 测试提取变量时排除运算符和函数.
    /// </summary>
    [Fact]
    public void ExtractVariables_ExcludesOperatorsAndFunctions()
    {
        // Arrange
        string formula = "sqrt(x) + max(a, b) * sin(angle)";

        // Act
        var variables = _formulaParser.ExtractVariables(formula);

        // Assert
        Assert.Equal(4, variables.Count);
        Assert.Contains("x", variables);
        Assert.Contains("a", variables);
        Assert.Contains("b", variables);
        Assert.Contains("angle", variables);
        Assert.DoesNotContain("sqrt", variables);
        Assert.DoesNotContain("max", variables);
        Assert.DoesNotContain("sin", variables);
    }

    /// <summary>
    /// 测试提取变量时排除数字.
    /// </summary>
    [Fact]
    public void ExtractVariables_ExcludesNumbers()
    {
        // Arrange
        string formula = "a + 3.14 * b - 100";

        // Act
        var variables = _formulaParser.ExtractVariables(formula);

        // Assert
        Assert.Equal(2, variables.Count);
        Assert.Contains("a", variables);
        Assert.Contains("b", variables);
        Assert.DoesNotContain("3.14", variables);
        Assert.DoesNotContain("100", variables);
    }

    /// <summary>
    /// 测试空公式返回空变量列表.
    /// </summary>
    [Fact]
    public void ExtractVariables_EmptyFormula_ReturnsEmptyList()
    {
        // Arrange
        string formula = "";

        // Act
        var variables = _formulaParser.ExtractVariables(formula);

        // Assert
        Assert.Empty(variables);
    }

    /// <summary>
    /// 测试单位转换集成（阶段二）.
    /// 注意：阶段一公式解析器尚未集成单位转换.
    /// </summary>
    [Fact(Skip = "单位转换将在阶段二实现")]
    public async Task Calculate_WithUnitConversion_ReturnsConvertedResult()
    {
        // Arrange
        string formula = "length * width";
        var variables = new Dictionary<string, object> { { "length", 10 }, { "width", 5 } };

        // 阶段一：基本计算测试
        var result = _formulaParser.Calculate(formula, variables);

        // Assert
        Assert.Equal(50, result); // 基本计算结果
        _output.WriteLine($"阶段一：基本计算公式验证，单位转换将在阶段二实现");
    }

    /// <summary>
    /// 测试变量值包含null.
    /// </summary>
    [Fact]
    public void Calculate_WithNullVariableValue_HandlesGracefully()
    {
        // Arrange
        string formula = "a + b";
        var variables = new Dictionary<string, object> { { "a", 10 }, { "b", null } };

        // Act
        var result = _formulaParser.Calculate(formula, variables);

        // Assert
        // 根据当前实现，null值会被替换为0
        Assert.Equal(10, result);
        _output.WriteLine($"变量b为null时，计算结果: {result}");
    }

    /// <summary>
    /// 性能测试：多次计算相同公式.
    /// </summary>
    [Fact]
    public void Calculate_PerformanceTest_RepeatedCalculations()
    {
        // Arrange
        string formula = "a * b + c / d";
        var variables = new Dictionary<string, object>
        {
            { "a", 100 },
            { "b", 50 },
            { "c", 200 },
            { "d", 4 },
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        const int iterations = 1000;

        // Act
        for (int i = 0; i < iterations; i++)
        {
            var result = _formulaParser.Calculate(formula, variables);
            // 验证结果一致性
            Assert.Equal(100 * 50 + 200 / 4, result); // 5000 + 50 = 5050
        }

        stopwatch.Stop();

        // Assert
        _output.WriteLine($"{iterations}次计算耗时: {stopwatch.ElapsedMilliseconds}ms");
        Assert.True(
            stopwatch.ElapsedMilliseconds < 1000,
            $"计算耗时过长: {stopwatch.ElapsedMilliseconds}ms"
        );
    }
}
