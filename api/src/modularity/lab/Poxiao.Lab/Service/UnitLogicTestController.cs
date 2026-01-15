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
    }
}
