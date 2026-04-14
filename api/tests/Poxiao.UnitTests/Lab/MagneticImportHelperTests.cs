using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.MagneticData;
using Poxiao.Lab.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Poxiao.UnitTests.Lab;

public class MagneticImportHelperTests
{
    [Fact]
    public void SelectPreferredItem_WhenMultipleNonScratchedRows_UsesBestRow()
    {
        var items = new List<MagneticDataImportItem>
        {
            new() { FurnaceNo = "1甲20260110-1-1-1", IsScratched = false, RowIndex = 2, PsLoss = 1.20m, SsPower = 1.05m, Hc = 0.50m },
            new() { FurnaceNo = "1甲20260110-1-1-1", IsScratched = false, RowIndex = 5, PsLoss = 1.10m, SsPower = 1.10m, Hc = 0.55m },
            new() { FurnaceNo = "1甲20260110-1-1-1", IsScratched = false, RowIndex = 8, PsLoss = 1.30m, SsPower = 1.00m, Hc = 0.40m },
        };

        var result = MagneticImportHelper.SelectPreferredItem(items);

        Assert.Equal(5, result.RowIndex);
        Assert.Equal(1.10m, result.PsLoss);
    }

    [Fact]
    public void GroupByFurnaceAndScratch_ShouldSeparateScratchedAndNonScratchedRows()
    {
        var items = new List<MagneticDataImportItem>
        {
            new() { FurnaceNo = "1甲20260110-1-1-1", IsScratched = false, RowIndex = 1 },
            new() { FurnaceNo = "1甲20260110-1-1-1", IsScratched = true, RowIndex = 2 },
        };

        var groups = MagneticImportHelper.GroupByFurnaceAndScratch(items).ToList();

        Assert.Equal(2, groups.Count);
        Assert.Contains(groups, group => group.Key == ("1甲20260110-1-1-1", false));
        Assert.Contains(groups, group => group.Key == ("1甲20260110-1-1-1", true));
    }

    [Fact]
    public void SelectDateAgnosticIntermediate_WhenOnlyDateDiffers_UsesIntermediateRecord()
    {
        var candidates = new List<IntermediateDataEntity>
        {
            new()
            {
                Id = "A",
                FurnaceNoFormatted = "1甲20260109-1-1-1",
                LineNo = 1,
                Shift = "甲",
                FurnaceBatchNo = 1,
                CoilNo = 1,
                SubcoilNo = 1,
                DetectionDate = new DateTime(2026, 1, 9),
                ProdDate = new DateTime(2026, 1, 9),
            },
        };

        var result = MagneticImportHelper.SelectDateAgnosticIntermediate(
            candidates,
            "1甲20260110-1-1-1",
            new DateTime(2026, 1, 10, 8, 30, 0));

        Assert.NotNull(result);
        Assert.Equal("A", result!.Id);
        Assert.Equal(new DateTime(2026, 1, 9), result.DetectionDate);
    }
}
