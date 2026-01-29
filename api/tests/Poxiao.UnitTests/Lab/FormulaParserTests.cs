using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Poxiao.Lab.Entity.Config;
using Poxiao.Lab.Helpers;
using Poxiao.Lab.Interfaces;
using Poxiao.Lab.Service;
using Xunit;
using Xunit.Abstractions;

namespace Poxiao.UnitTests.Lab;

/// <summary>
/// 公式解析器单元测试.
/// </summary>
public class FormulaParserTests
{
    private readonly ITestOutputHelper _output;
    private readonly FormulaParser _formulaParser;

    public FormulaParserTests(ITestOutputHelper output)
    {
        _output = output;

        // 创建默认配置
        var labOptions = new LabOptions
        {
            Formula = new FormulaOptions
            {
                EnablePrecisionAdjustment = false,
                DefaultPrecision = 6,
                MaxPrecision = 6
            }
        };

        var optionsMock = new Mock<IOptions<LabOptions>>();
        optionsMock.Setup(x => x.Value).Returns(labOptions);

        _formulaParser = new FormulaParser(optionsMock.Object);
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
    /// 测试带变量的计算 (使用方括号).
    /// </summary>
    [Fact]
    public void Calculate_WithVariables_ReturnsCorrectResult()
    {
        // Arrange
        string formula = "([width] * [height]) / 2";
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
        string formula = "([a] + [b]) * [c] - [d] / [e]";
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
        Assert.Equal(11, result);
    }

    [Fact]
    public void Calculate_BracketFormat_Success()
    {
        // Arrange
        var context = new Dictionary<string, object>
        {
            ["N3"] = 10.5m,
            ["O3"] = 9.0m
        };

        // Act
        var result = _formulaParser.Calculate("([N3] - [O3])", context);

        // Assert
        Assert.Equal(1.5m, result);
    }
    
    [Fact]
    public void Calculate_CaseInsensitive_Success()
    {
        // Arrange
        var context = new Dictionary<string, object>
        {
            ["n3"] = 10.5m, // Lowercase key
            ["O3"] = 9.0m
        };

        // Act - Formula uses Uppercase N3
        var result = _formulaParser.Calculate("([N3] - [O3])", context);

        // Assert
        Assert.Equal(1.5m, result);
    }

    [Fact]
    public void Calculate_SumFunction_Success()
    {
        var context = new Dictionary<string, object>
        {
            ["Detection1"] = 1.0m,
            ["Detection2"] = 2.0m,
            ["Detection3"] = 3.0m
        };

        var result = _formulaParser.Calculate("SUM([Detection1], [Detection2], [Detection3])", context);

        Assert.Equal(6.0m, result);
    }

    [Fact]
    public void Calculate_ToRange_Success()
    {
        var context = new Dictionary<string, object>
        {
            ["Detection1"] = 1.0m,
            ["Detection2"] = 2.0m,
            ["Detection3"] = 3.0m,
            ["Detection4"] = 4.0m,
            ["Detection5"] = 5.0m
        };

        // Act: SUM([Detection1] TO [Detection5]) -> Sum(1,2,3,4,5) = 15
        var result = _formulaParser.Calculate("SUM([Detection1] TO [Detection5])", context);

        Assert.Equal(15.0m, result);
    }
    
    [Fact]
    public void Calculate_RangeWithDollarVariable_Success()
    {
        var context = new Dictionary<string, object>
        {
            ["Thickness1"] = 1.0m,
            ["Thickness2"] = 2.0m,
            ["Thickness3"] = 5.0m,
            ["Thickness4"] = 3.0m,
            ["DetectionColumns"] = 4
        };

        // RANGE(Thickness, 1, $DetectionColumns) = Max(1,2,5,3) - Min(1,2,5,3) = 5 - 1 = 4
        var result = _formulaParser.Calculate("RANGE(Thickness, 1, $DetectionColumns)", context);

        Assert.Equal(4.0m, result);
    }

    [Fact]
    public void Calculate_DiffFirstLastWithDollarVariable_Success()
    {
        var context = new Dictionary<string, object>
        {
            ["Detection1"] = 10.0m,
            ["Detection2"] = 15.0m,
            ["Detection3"] = 12.0m,
            ["DetectionColumns"] = 3
        };

        // DIFF_FIRST_LAST = |Detection1 - Detection3| = |10 - 12| = 2
        var result = _formulaParser.Calculate("DIFF_FIRST_LAST(Detection, 1, $DetectionColumns)", context);

        Assert.Equal(2.0m, result);
    }
    
    [Fact]
    public void ExtractVariables_BracketFormat_ReturnsWithoutBrackets()
    {
        var vars = _formulaParser.ExtractVariables("([N3] - [O3]) * [Factor]");

        Assert.Contains("N3", vars);
        Assert.Contains("O3", vars);
        Assert.Contains("Factor", vars);
        Assert.DoesNotContain("[N3]", vars); 
        Assert.Equal(3, vars.Count);
    }
    
    [Fact]
    public void ExtractVariables_DollarFormat_ReturnsWithoutDollar()
    {
        var vars = _formulaParser.ExtractVariables("RANGE(D, 1, $DetectionColumns)");

        Assert.Contains("DetectionColumns", vars);
        Assert.DoesNotContain("$DetectionColumns", vars);
        Assert.Single(vars);
    }

    /// <summary>
    /// 测试空公式返回Null.
    /// </summary>
    [Fact]
    public void Calculate_EmptyFormula_ReturnsNull()
    {
        // Arrange
        string formula = "";
        var variables = new Dictionary<string, object>();

        // Act
        var result = _formulaParser.Calculate(formula, variables);
        
        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// 测试验证有效表达式.
    /// </summary>
    [Theory]
    [InlineData("2 + 3")]
    [InlineData("([a] + [b]) * [c]")]
    [InlineData("[width] * [height] / 2")]
    [InlineData("RANGE(Thickness, 1, $DetectionColumns)")]
    public void ValidateExpression_ValidExpression_ReturnsTrue(string expression)
    {
        // Act
        var isValid = _formulaParser.ValidateExpression(expression);

        // Assert
        Assert.True(isValid);
        _output.WriteLine($"表达式验证通过: {expression}");
    }
    
    [Fact]
    public void Calculate_RangeFunction_Success()
    {
        // Arrange
        var context = new Dictionary<string, object>();
        // Detection 1 to 5
        context["Detection1"] = 10m;
        context["Detection2"] = 20m;
        context["Detection3"] = 5m; // min
        context["Detection4"] = 25m; // max
        context["Detection5"] = 15m;
        context["DetectionColumns"] = 5;
        
        // Act: RANGE(Detection) = Max - Min = 25 - 5 = 20
        var result = _formulaParser.Calculate("RANGE(Detection)", context);
        
        // Assert
        Assert.Equal(20m, result);
    }
    
    [Fact]
    public void Calculate_DiffFirstLastFunction_Success()
    {
        // Arrange
        var context = new Dictionary<string, object>();
        // Detection 1 to 5
        context["Detection1"] = 10m; // first
        context["Detection2"] = 20m;
        context["Detection3"] = 5m;
        context["Detection4"] = 25m;
        context["Detection5"] = 15m; // last
        context["DetectionColumns"] = 5;
        
        // Act: DIFF_FIRST_LAST(Detection) = |First - Last| = |10 - 15| = 5
        var result = _formulaParser.Calculate("DIFF_FIRST_LAST(Detection)", context);
        
        // Assert
        Assert.Equal(5m, result);
    }
}
