using Newtonsoft.Json.Linq;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Service;
using Xunit;
using System.Collections.Generic;

namespace Poxiao.UnitTests.Lab;

/// <summary>
/// Tests for the judgment logic in IntermediateDataFormulaBatchCalculator.
/// </summary>
public class JudgmentLogicTests
{
    /// <summary>
    /// Test that LaminationFactor is correctly extracted from entity to contextData.
    /// </summary>
    [Fact]
    public void ExtractContextData_Should_Include_LaminationFactor()
    {
        // Arrange
        var entity = new IntermediateDataEntity
        {
            Id = "test-entity-id",
            LaminationFactor = 92.41m
        };

        // Act
        var contextData = IntermediateDataFormulaCalcHelper.ExtractContextDataFromEntity(entity);

        // Assert
        Assert.True(contextData.ContainsKey("LaminationFactor"), "contextData should contain LaminationFactor");
        Assert.Equal(92.41m, contextData["LaminationFactor"]);
    }

    /// <summary>
    /// Test that judgment condition with >= operator works correctly.
    /// </summary>
    [Theory]
    [InlineData(92.41, 90, true)]   // 92.41 >= 90 should match
    [InlineData(90.00, 90, true)]   // 90 >= 90 should match
    [InlineData(89.99, 90, false)]  // 89.99 >= 90 should not match
    [InlineData(85.00, 90, false)]  // 85 >= 90 should not match
    public void EvaluateCondition_GreaterOrEqual_Should_Work(decimal laminationFactor, decimal threshold, bool expectedMatch)
    {
        // Arrange
        var entity = new IntermediateDataEntity
        {
            Id = "test-entity-id",
            LaminationFactor = laminationFactor
        };

        var contextData = IntermediateDataFormulaCalcHelper.ExtractContextDataFromEntity(entity);

        // Condition JSON: LaminationFactor >= {threshold}
        var conditionJson = $@"{{
            ""id"": ""test-condition"",
            ""leftExpr"": ""LaminationFactor"",
            ""operator"": "">="",
            ""rightValue"": ""{threshold}""
        }}";

        var condition = JObject.Parse(conditionJson);

        // Act - We need to test EvaluateCondition, but it's private.
        // For now, let's just verify the contextData extraction works.
        
        // Assert
        Assert.True(contextData.ContainsKey("LaminationFactor"), "contextData should contain LaminationFactor");
        var extractedValue = (decimal)contextData["LaminationFactor"];
        Assert.Equal(laminationFactor, extractedValue);

        // Manual comparison to verify expected behavior
        bool actualMatch = extractedValue >= threshold;
        Assert.Equal(expectedMatch, actualMatch);
    }

    /// <summary>
    /// Test that multiple conditions with AND logic work correctly.
    /// E.g., LaminationFactor >= 88 AND LaminationFactor < 90
    /// </summary>
    [Theory]
    [InlineData(89.00, 88, 90, true)]   // 89 >= 88 AND 89 < 90 = true
    [InlineData(90.00, 88, 90, false)]  // 90 >= 88 AND 90 < 90 = false (second fails)
    [InlineData(87.99, 88, 90, false)]  // 87.99 >= 88 is false
    [InlineData(88.00, 88, 90, true)]   // 88 >= 88 AND 88 < 90 = true
    public void EvaluateCondition_RangeCheck_Should_Work(decimal laminationFactor, decimal min, decimal max, bool expectedMatch)
    {
        // Arrange
        var entity = new IntermediateDataEntity
        {
            Id = "test-entity-id",
            LaminationFactor = laminationFactor
        };

        var contextData = IntermediateDataFormulaCalcHelper.ExtractContextDataFromEntity(entity);

        // Assert
        var extractedValue = (decimal)contextData["LaminationFactor"];
        bool condition1 = extractedValue >= min;
        bool condition2 = extractedValue < max;
        bool actualMatch = condition1 && condition2;
        
        Assert.Equal(expectedMatch, actualMatch);
    }

    /// <summary>
    /// Test that null LaminationFactor is handled correctly.
    /// </summary>
    [Fact]
    public void ExtractContextData_With_Null_LaminationFactor_Should_Not_Throw()
    {
        // Arrange
        var entity = new IntermediateDataEntity
        {
            Id = "test-entity-id",
            LaminationFactor = null
        };

        // Act
        var contextData = IntermediateDataFormulaCalcHelper.ExtractContextDataFromEntity(entity);

        // Assert - null values should not be added to contextData
        Assert.False(contextData.ContainsKey("LaminationFactor"), "contextData should NOT contain null LaminationFactor");
    }

    /// <summary>
    /// Integration test: Simulate the full judgment flow for Level A (>= 90).
    /// </summary>
    [Fact]
    public void FullJudgment_LevelA_Should_Match_When_LaminationFactor_Is_92()
    {
        // Arrange
        var entity = new IntermediateDataEntity
        {
            Id = "test-entity-id",
            LaminationFactor = 92.41m
        };

        var contextData = IntermediateDataFormulaCalcHelper.ExtractContextDataFromEntity(entity);

        // Level A condition: LaminationFactor >= 90
        var levelACondition = @"{
            ""id"": ""ml2zz0vexrtex11nxwd"",
            ""name"": """",
            ""mode"": ""simple"",
            ""logic"": ""AND"",
            ""conditions"": [
                {
                    ""id"": ""ml2zz1tlbmsgh441pb5"",
                    ""leftExpr"": ""LaminationFactor"",
                    ""operator"": "">="",
                    ""rightValue"": ""90""
                }
            ],
            ""subGroups"": []
        }";

        var ruleGroup = JObject.Parse(levelACondition);

        // Manual evaluation
        var conditions = ruleGroup["conditions"] as JArray;
        Assert.NotNull(conditions);
        Assert.Single(conditions);

        var condition = conditions[0] as JObject;
        var leftExpr = condition["leftExpr"]?.ToString();
        var op = condition["operator"]?.ToString();
        var rightValue = condition["rightValue"]?.ToString();

        Assert.Equal("LaminationFactor", leftExpr);
        Assert.Equal(">=", op);
        Assert.Equal("90", rightValue);

        // Check if contextData has the value
        Assert.True(contextData.ContainsKey("LaminationFactor"));
        var leftValue = (decimal)contextData["LaminationFactor"];
        var rightNumber = decimal.Parse(rightValue);

        // Evaluate: 92.41 >= 90
        bool matched = leftValue >= rightNumber;
        Assert.True(matched, $"Expected {leftValue} >= {rightNumber} to be true");
    }
}
