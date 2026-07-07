using Poxiao.Lab.Entity.Dto.SingleSheet;

namespace Poxiao.Lab.Helpers;

/// <summary>
/// 单片性能导入辅助逻辑。
/// </summary>
public static class SingleSheetImportHelper
{
    /// <summary>
    /// 按炉号 + 是否刻痕分组。
    /// </summary>
    public static IEnumerable<IGrouping<(string FurnaceNo, bool IsScratched), SingleSheetImportItem>> GroupByFurnaceAndScratch(
        IEnumerable<SingleSheetImportItem> items)
    {
        return items.GroupBy(item => (item.FurnaceNo ?? string.Empty, item.IsScratched));
    }

    /// <summary>
    /// 选择同一炉号、同一刻痕类型下应导入的数据。
    /// 同一类型存在多条时，统一按最优值规则选择一条。
    /// </summary>
    public static SingleSheetImportItem SelectPreferredItem(IReadOnlyList<SingleSheetImportItem> items)
    {
        if (items == null || items.Count == 0)
        {
            throw new ArgumentException("数据列表为空", nameof(items));
        }

        if (items.Count == 1)
        {
            return items[0];
        }

        return SelectBestByMetrics(items);
    }

    private static SingleSheetImportItem SelectBestByMetrics(IReadOnlyList<SingleSheetImportItem> items)
    {
        var sortedByH = items.Where(t => t.PsLoss.HasValue).OrderBy(t => t.PsLoss.Value).ToList();
        if (sortedByH.Count > 0)
        {
            var minH = sortedByH[0].PsLoss!.Value;
            var candidates = sortedByH.Where(t => t.PsLoss == minH).ToList();
            if (candidates.Count == 1)
            {
                return candidates[0];
            }

            var sortedByI = candidates.Where(t => t.SsPower.HasValue).OrderBy(t => t.SsPower.Value).ToList();
            if (sortedByI.Count > 0)
            {
                var minI = sortedByI[0].SsPower!.Value;
                var candidates2 = sortedByI.Where(t => t.SsPower == minI).ToList();
                if (candidates2.Count == 1)
                {
                    return candidates2[0];
                }

                var sortedByF = candidates2.Where(t => t.Hc.HasValue).OrderBy(t => t.Hc.Value).ToList();
                if (sortedByF.Count > 0)
                {
                    var minF = sortedByF[0].Hc!.Value;
                    var candidates3 = sortedByF.Where(t => t.Hc == minF).ToList();
                    if (candidates3.Count == 1)
                    {
                        return candidates3[0];
                    }

                    return candidates3
                        .OrderByDescending(t => t.DetectionTime ?? DateTime.MinValue)
                        .First();
                }

                return candidates2
                    .OrderByDescending(t => t.DetectionTime ?? DateTime.MinValue)
                    .First();
            }

            return candidates
                .OrderByDescending(t => t.DetectionTime ?? DateTime.MinValue)
                .First();
        }

        var sortedByIOnly = items.Where(t => t.SsPower.HasValue).OrderBy(t => t.SsPower.Value).ToList();
        if (sortedByIOnly.Count > 0)
        {
            var minI = sortedByIOnly[0].SsPower!.Value;
            var candidates = sortedByIOnly.Where(t => t.SsPower == minI).ToList();
            if (candidates.Count == 1)
            {
                return candidates[0];
            }

            return candidates
                .OrderByDescending(t => t.DetectionTime ?? DateTime.MinValue)
                .First();
        }

        var sortedByFOnly = items.Where(t => t.Hc.HasValue).OrderBy(t => t.Hc.Value).ToList();
        if (sortedByFOnly.Count > 0)
        {
            var minF = sortedByFOnly[0].Hc!.Value;
            var candidates = sortedByFOnly.Where(t => t.Hc == minF).ToList();
            if (candidates.Count == 1)
            {
                return candidates[0];
            }

            return candidates
                .OrderByDescending(t => t.DetectionTime ?? DateTime.MinValue)
                .First();
        }

        return items
            .OrderByDescending(t => t.DetectionTime ?? DateTime.MinValue)
            .First();
    }
}
