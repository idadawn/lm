using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Poxiao.DynamicApiController;

namespace Poxiao.Lab.Service
{
    [Route("api/lab/test/unit-logic")]
    public class UnitLogicTestController : IDynamicApiController
    {
        [HttpGet("simulate-switch")]
        public object SimulateSwitch(decimal currentGramScale, decimal inputScale)
        {
            // Scenario:
            // Existing:
            // KG (Base) = 1.0
            // G (Target) = currentGramScale (e.g. 1.0 WRONG, or 0.001 CORRECT)
            // T (Other) = 1000.0

            // User Action: Set G as Base. Input Scale = inputScale.

            var log = new List<string>();
            log.Add($"Initial State: KG(Base)=1.0, G={currentGramScale}, T=1000.0");
            log.Add($"Action: Set G as Base. Input Scale={inputScale}");

            decimal newBaseUnitOldScale;

            // My Logic
            if (inputScale != 0 && inputScale != 1)
            {
                newBaseUnitOldScale = inputScale;
                log.Add("Using Input Scale");
            }
            else
            {
                newBaseUnitOldScale = currentGramScale;
                log.Add("Using Entity Scale (Input was 1 or 0)");
            }

            log.Add($"NewBaseUnitOldScale (G relative to KG) = {newBaseUnitOldScale}");

            decimal oldBaseToNewBaseRatio = 1.0m / newBaseUnitOldScale;
            log.Add($"Ratio (1 KG = ? G) = {oldBaseToNewBaseRatio}");

            // Recalculate
            // KG (Old Base)
            var newKgScale = 1.0m * oldBaseToNewBaseRatio;
            log.Add($"New KG Scale = 1.0 * {oldBaseToNewBaseRatio} = {newKgScale}");

            // T (Other)
            var newTScale = 1000.0m * oldBaseToNewBaseRatio;
            log.Add($"New T Scale = 1000.0 * {oldBaseToNewBaseRatio} = {newTScale}");

            // G (New Base)
            var newGScale = 1.0m;
            log.Add($"New G Scale = {newGScale}");

            return new
            {
                Log = log,
                Result = new
                {
                    G = newGScale,
                    KG = newKgScale,
                    T = newTScale,
                },
            };
        }

        [HttpGet("test-match-rule")]
        public object TestMatchRule(string input)
        {
            var matcher = new AppearanceFeatureRuleMatcher();
            var features = new List<Entity.Entity.AppearanceFeatureEntity>();

            // Mock Data
            // 1. Feature: Scratch (Name="划痕", Keywords include "亮线")
            features.Add(
                new Entity.Entity.AppearanceFeatureEntity
                {
                    Id = "1",
                    Name = "划痕",
                    Keywords = "[\"划伤\", \"亮线\", \"刮痕\"]",
                    SeverityLevelId = "level1",
                    CategoryId = "cat1",
                }
            );

            // 2. Feature: BrightLine (Name="亮线")
            features.Add(
                new Entity.Entity.AppearanceFeatureEntity
                {
                    Id = "2",
                    Name = "亮线",
                    Keywords = "[\"明线\"]",
                    SeverityLevelId = "level1",
                    CategoryId = "cat2",
                }
            );

            var severityMap = new Dictionary<string, string>
            {
                { "level1", "默认" },
                { "level2", "轻微" },
            };

            var catMap = new Dictionary<string, string>
            {
                { "cat1", "表面缺陷" },
                { "cat2", "亮线类" },
            };

            var degreeWords = new List<string> { "轻微", "严重", "明显" };

            var result = matcher.Match(input, features, degreeWords, catMap, severityMap);

            return new
            {
                Input = input,
                Results = result.Select(r => new
                {
                    FeatureName = r.Feature.Name,
                    Method = r.MatchMethod,
                    Confidence = r.Confidence,
                    Degree = r.DegreeWord,
                }),
            };
        }
    }
}
