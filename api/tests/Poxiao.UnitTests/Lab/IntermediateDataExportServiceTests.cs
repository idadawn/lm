using System.Collections.Generic;
using Poxiao.Lab.Service;
using Xunit;

namespace Poxiao.UnitTests.Lab;

public class IntermediateDataExportServiceTests
{
    [Fact]
    public void ShouldConvertExportHeaderCellToNumeric_WhenLaminationDistributionSubtitleIsNumber_ReturnsTrue()
    {
        var result = IntermediateDataExportService.ShouldConvertExportHeaderCellToNumeric(1, "叠片系数厚度分布", "15");

        Assert.True(result);
    }

    [Fact]
    public void BuildColumnFieldNames_WhenDetectionColumnsIncrease_ContainsMatchingDetectionField()
    {
        var fields = IntermediateDataExportService.BuildColumnFieldNames(17, new List<KeyValuePair<string, string>>());

        Assert.Contains("detection17", fields);
    }
}
