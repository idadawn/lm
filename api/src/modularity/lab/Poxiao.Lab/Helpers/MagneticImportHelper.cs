using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.MagneticData;

namespace Poxiao.Lab.Helpers;

/// <summary>
/// 磁性能导入辅助逻辑。
/// </summary>
public static class MagneticImportHelper
{
    /// <summary>
    /// 按炉号 + 是否刻痕分组。
    /// </summary>
    public static IEnumerable<IGrouping<(string FurnaceNo, bool IsScratched), MagneticDataImportItem>> GroupByFurnaceAndScratch(
        IEnumerable<MagneticDataImportItem> items)
    {
        return items.GroupBy(item => (item.FurnaceNo ?? string.Empty, item.IsScratched));
    }

    /// <summary>
    /// 选择同一炉号、同一刻痕类型下应导入的数据。
    /// 同一类型存在多条时，统一按最优值规则选择一条。
    /// </summary>
    public static MagneticDataImportItem SelectPreferredItem(IReadOnlyList<MagneticDataImportItem> items)
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

    /// <summary>
    /// 精确炉号匹配失败时，按炉号结构字段忽略日期回退匹配中间数据。
    /// </summary>
    public static IntermediateDataEntity? SelectDateAgnosticIntermediate(
        IEnumerable<IntermediateDataEntity> candidates,
        string furnaceNo,
        DateTime? detectionTime)
    {
        var candidateList = candidates?.Where(item => item != null).ToList() ?? new List<IntermediateDataEntity>();
        if (candidateList.Count == 0 || string.IsNullOrWhiteSpace(furnaceNo))
        {
            return null;
        }

        var exactMatch = candidateList.FirstOrDefault(item =>
            !string.IsNullOrWhiteSpace(item.FurnaceNoFormatted)
            && string.Equals(item.FurnaceNoFormatted, furnaceNo, StringComparison.OrdinalIgnoreCase));
        if (exactMatch != null)
        {
            return exactMatch;
        }

        var identity = TryParseIdentity(furnaceNo);
        if (identity == null)
        {
            return null;
        }

        var structuralMatches = candidateList
            .Where(item =>
                item.LineNo == identity.LineNo
                && string.Equals(item.Shift, identity.Shift, StringComparison.OrdinalIgnoreCase)
                && item.FurnaceBatchNo == identity.FurnaceBatchNo
                && NullableEquals(item.CoilNo, identity.CoilNo)
                && NullableEquals(item.SubcoilNo, identity.SubcoilNo))
            .ToList();

        if (structuralMatches.Count == 0)
        {
            return null;
        }

        if (detectionTime.HasValue)
        {
            var targetDate = detectionTime.Value.Date;
            var sameDateMatch = structuralMatches.FirstOrDefault(item =>
                (item.DetectionDate?.Date == targetDate) || (item.ProdDate?.Date == targetDate));
            if (sameDateMatch != null)
            {
                return sameDateMatch;
            }
        }

        return structuralMatches
            .OrderByDescending(item => item.DetectionDate ?? item.ProdDate ?? DateTime.MinValue)
            .ThenByDescending(item => item.LastModifyTime ?? item.CreatorTime ?? DateTime.MinValue)
            .First();
    }

    private static bool NullableEquals(decimal? left, decimal? right)
    {
        if (!left.HasValue && !right.HasValue)
        {
            return true;
        }

        return left.HasValue && right.HasValue && left.Value == right.Value;
    }

    private static FurnaceIdentity? TryParseIdentity(string furnaceNo)
    {
        var parseResult = FurnaceNoHelper.ParseFurnaceNo(furnaceNo);
        if (!parseResult.Success)
        {
            return null;
        }

        return new FurnaceIdentity
        {
            LineNo = parseResult.LineNoNumeric,
            Shift = parseResult.Shift,
            FurnaceBatchNo = parseResult.FurnaceNoNumeric,
            CoilNo = parseResult.CoilNoNumeric,
            SubcoilNo = parseResult.SubcoilNoNumeric,
        };
    }

    private static MagneticDataImportItem SelectBestByMetrics(IReadOnlyList<MagneticDataImportItem> items)
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

    private sealed class FurnaceIdentity
    {
        public int? LineNo { get; set; }

        public string Shift { get; set; } = string.Empty;

        public int? FurnaceBatchNo { get; set; }

        public decimal? CoilNo { get; set; }

        public decimal? SubcoilNo { get; set; }
    }
}
