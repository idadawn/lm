using System.Collections.Generic;
using System.Linq;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Entity;
using Poxiao.Lab.Service;
using Xunit;

namespace Poxiao.UnitTests.Lab
{
    public class AppearanceFeatureRuleMatcherTests
    {
        [Fact]
        public void Match_Should_Prioritize_Exact_Name_Match_Over_Keyword_Match()
        {
            // Arrange
            var matcher = new AppearanceFeatureRuleMatcher();

            var features = new List<AppearanceFeatureEntity>();

            // Feature 1: Scratch (Has keyword "亮线")
            features.Add(
                new AppearanceFeatureEntity
                {
                    Id = "1",
                    Name = "划痕",
                    Keywords = "[\"划伤\", \"亮线\", \"刮痕\"]",
                    SeverityLevelId = "level1",
                    CategoryId = "cat1",
                }
            );

            // Feature 2: Bright Line (Name is "亮线")
            features.Add(
                new AppearanceFeatureEntity
                {
                    Id = "2",
                    Name = "亮线",
                    Keywords = "[\"明线\"]",
                    SeverityLevelId = "level1",
                    CategoryId = "cat2",
                }
            );

            var severityMap = new Dictionary<string, string> { { "level1", "默认" } };

            var catMap = new Dictionary<string, string>
            {
                { "cat1", "表面缺陷" },
                { "cat2", "亮线类" },
            };

            var degreeWords = new List<string> { "轻微", "严重", "明显" };
            var input = "亮线";

            // Act
            var results = matcher.Match(input, features, degreeWords, catMap, severityMap);

            // Assert
            Assert.NotEmpty(results);
            var bestMatch = results.First();

            // Should match "亮线" (Feature 2), NOT "划痕" (Feature 1)
            Assert.Equal("亮线", bestMatch.Feature.Name);
            Assert.Equal("name", bestMatch.MatchMethod);
        }
    }
}
