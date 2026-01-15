using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Poxiao.AI.Service;
using Xunit;

namespace Poxiao.UnitTests.AI
{
    public class AppearanceFeatureAnalysisServiceTests
    {
        [Fact]
        public void BuildSystemPrompt_Should_Replace_Placeholders_Correctly()
        {
            // Arrange
            // We don't need real configuration if we invoke BuildSystemPrompt directly
            // But we need minimal setup to instantiate the service if constructor requires it.
            // Constructor requires IConfiguration.
            var inMemorySettings = new Dictionary<string, string>
            {
                { "AI:Chat:Endpoint", "http://dummy" },
                { "AI:Chat:Key", "dummy" },
                {
                    "AppearanceFeaturePrompt:SystemPromptTemplate",
                    @"# Role
Test Role

# Reference
{{CATEGORY_FEATURES}}

# Levels
{{SEVERITY_LEVELS}}
"
                },
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new AppearanceFeatureAnalysisService(configuration);

            var template =
                @"# Role
Test Role

# Reference
{{CATEGORY_FEATURES}}

# Levels
{{SEVERITY_LEVELS}}

# Default
{{DEFAULT_LEVEL}}
";

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

            // Act
            // Use Reflection to invoke private method BuildSystemPrompt
            var methodInfo = typeof(AppearanceFeatureAnalysisService).GetMethod(
                "BuildSystemPrompt",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            Assert.NotNull(methodInfo);

            var defaultLevel = "Normal";
            var result = (string)
                methodInfo.Invoke(
                    service,
                    new object[] { template, categoryFeatures, severityLevels, defaultLevel }
                );

            // Assert
            Assert.Contains("Cat1: F1、F2", result);
            Assert.Contains("Cat2: F3", result);
            Assert.Contains("L1、L2、L3", result);
            Assert.Contains("Normal", result);
            Assert.DoesNotContain("{{CATEGORY_FEATURES}}", result);
            Assert.DoesNotContain("{{SEVERITY_LEVELS}}", result);
            Assert.DoesNotContain("{{DEFAULT_LEVEL}}", result);
        }
    }
}
