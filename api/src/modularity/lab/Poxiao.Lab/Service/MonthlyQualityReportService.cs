using Microsoft.AspNetCore.Mvc;
using Poxiao.DatabaseAccessor;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.MonthlyQualityReport;
using Poxiao.Lab.Entity.Enum;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// 月度质量报表服务实现.
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "monthly-quality-report", Order = 220)]
[Route("api/lab/monthly-quality-report")]
public class MonthlyQualityReportService : IMonthlyQualityReportService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<IntermediateDataEntity> _intermediateDataRepository;
    private readonly ISqlSugarRepository<IntermediateDataJudgmentLevelEntity> _judgmentLevelRepository;

    public MonthlyQualityReportService(
        ISqlSugarRepository<IntermediateDataEntity> intermediateDataRepository,
        ISqlSugarRepository<IntermediateDataJudgmentLevelEntity> judgmentLevelRepository)
    {
        _intermediateDataRepository = intermediateDataRepository;
        _judgmentLevelRepository = judgmentLevelRepository;
    }

    /// <inheritdoc/>
    [HttpGet("")]
    public async Task<MonthlyQualityReportResponseDto> GetReportAsync([FromQuery] MonthlyQualityReportQueryDto query)
    {
        // 获取需要统计的不合格分类列
        var unqualifiedColumns = await GetUnqualifiedColumnsAsync();

        var response = new MonthlyQualityReportResponseDto
        {
            Summary = await GetSummaryAsync(query),
            Details = await GetDetailsAsync(query),
            ShiftGroups = await GetShiftGroupsAsync(query),
            QualityTrends = await GetQualityTrendAsync(query),
            UnqualifiedCategories = await GetUnqualifiedCategoriesAsync(query),
            ShiftComparisons = await GetShiftComparisonAsync(query),
            UnqualifiedColumns = unqualifiedColumns
        };

        return response;
    }

    /// <inheritdoc/>
    [HttpGet("summary")]
    public async Task<MonthlyQualityReportSummaryDto> GetSummaryAsync([FromQuery] MonthlyQualityReportQueryDto query)
    {
        var baseQuery = BuildBaseQuery(query);
        
        // 获取判定等级映射
        var levelMap = await GetJudgmentLevelMapAsync();

        var data = await baseQuery.ToListAsync();

        // 汇总计算
        var totalWeight = data.Sum(d => d.SingleCoilWeight ?? 0);
        
        // 按Labeling分组计算各类别重量
        var classAWeight = data.Where(d => IsClassA(d.Labeling, levelMap))
                               .Sum(d => d.SingleCoilWeight ?? 0);
        var classBWeight = data.Where(d => IsClassB(d.Labeling, levelMap))
                               .Sum(d => d.SingleCoilWeight ?? 0);
        var unqualifiedWeight = data.Where(d => IsUnqualified(d.Labeling, levelMap))
                                     .Sum(d => d.SingleCoilWeight ?? 0);
        var qualifiedWeight = classAWeight + classBWeight;

        return new MonthlyQualityReportSummaryDto
        {
            TotalWeight = totalWeight,
            QualifiedRate = totalWeight > 0 ? Math.Round(qualifiedWeight / totalWeight * 100, 2) : 0,
            ClassAWeight = classAWeight,
            ClassARate = totalWeight > 0 ? Math.Round(classAWeight / totalWeight * 100, 2) : 0,
            ClassBWeight = classBWeight,
            ClassBRate = totalWeight > 0 ? Math.Round(classBWeight / totalWeight * 100, 2) : 0,
            UnqualifiedWeight = unqualifiedWeight,
            UnqualifiedRate = totalWeight > 0 ? Math.Round(unqualifiedWeight / totalWeight * 100, 2) : 0
        };
    }

    /// <inheritdoc/>
    [HttpGet("details")]
    public async Task<List<MonthlyQualityReportDetailDto>> GetDetailsAsync([FromQuery] MonthlyQualityReportQueryDto query)
    {
        var baseQuery = BuildBaseQuery(query);
        var levelMap = await GetJudgmentLevelMapAsync();
        var unqualifiedLevels = await GetUnqualifiedLevelsAsync();

        var data = await baseQuery
            .OrderBy(d => d.ProdDate)
            .OrderBy(d => d.ShiftNumeric)
            .OrderBy(d => d.FurnaceBatchNo)
            .ToListAsync();

        var result = new List<MonthlyQualityReportDetailDto>();

        // 按班次分组
        var shiftGroups = data.GroupBy(d => d.Shift).OrderBy(g => GetShiftOrder(g.Key));

        foreach (var shiftGroup in shiftGroups)
        {
            // 按产品规格分组
            var specGroups = shiftGroup.GroupBy(d => d.ProductSpecCode ?? d.Width?.ToString() ?? "未知")
                                        .OrderBy(g => g.Key);

            foreach (var specGroup in specGroups)
            {
                // 明细行
                foreach (var item in specGroup.OrderBy(d => d.ProdDate).ThenBy(d => d.FurnaceBatchNo))
                {
                    var detailDto = CreateDetailDto(item, levelMap, unqualifiedLevels);
                    result.Add(detailDto);
                }
            }

            // 班次小计
            var shiftSubtotal = CreateShiftSubtotalDto(shiftGroup.Key, shiftGroup.ToList(), levelMap, unqualifiedLevels);
            result.Add(shiftSubtotal);
        }

        // 月度合计
        var monthlyTotal = CreateMonthlyTotalDto(data, levelMap, unqualifiedLevels);
        result.Add(monthlyTotal);

        return result;
    }

    /// <inheritdoc/>
    [HttpGet("shift-groups")]
    public async Task<List<MonthlyQualityReportShiftGroupDto>> GetShiftGroupsAsync([FromQuery] MonthlyQualityReportQueryDto query)
    {
        var baseQuery = BuildBaseQuery(query);
        var levelMap = await GetJudgmentLevelMapAsync();

        var data = await baseQuery.ToListAsync();

        var result = new List<MonthlyQualityReportShiftGroupDto>();

        // 按班次分组
        var shiftGroups = data.GroupBy(d => d.Shift).OrderBy(g => GetShiftOrder(g.Key));

        foreach (var shiftGroup in shiftGroups)
        {
            // 按产品规格分组
            var specGroups = shiftGroup.GroupBy(d => d.ProductSpecCode ?? d.Width?.ToString() ?? "未知")
                                        .OrderBy(g => g.Key);

            foreach (var specGroup in specGroups)
            {
                var items = specGroup.ToList();
                var totalWeight = items.Sum(d => d.SingleCoilWeight ?? 0);
                var classAWeight = items.Where(d => IsClassA(d.Labeling, levelMap)).Sum(d => d.SingleCoilWeight ?? 0);
                var classBWeight = items.Where(d => IsClassB(d.Labeling, levelMap)).Sum(d => d.SingleCoilWeight ?? 0);
                var unqualifiedWeight = items.Where(d => IsUnqualified(d.Labeling, levelMap)).Sum(d => d.SingleCoilWeight ?? 0);
                var qualifiedWeight = classAWeight + classBWeight;

                result.Add(new MonthlyQualityReportShiftGroupDto
                {
                    Shift = shiftGroup.Key,
                    ProductSpecCode = specGroup.Key,
                    DetectionWeight = totalWeight,
                    ClassAWeight = classAWeight,
                    ClassARate = totalWeight > 0 ? Math.Round(classAWeight / totalWeight * 100, 2) : 0,
                    ClassBWeight = classBWeight,
                    ClassBRate = totalWeight > 0 ? Math.Round(classBWeight / totalWeight * 100, 2) : 0,
                    UnqualifiedWeight = unqualifiedWeight,
                    QualifiedRate = totalWeight > 0 ? Math.Round(qualifiedWeight / totalWeight * 100, 2) : 0
                });
            }

            // 班次小计
            var shiftItems = shiftGroup.ToList();
            var shiftTotalWeight = shiftItems.Sum(d => d.SingleCoilWeight ?? 0);
            var shiftClassAWeight = shiftItems.Where(d => IsClassA(d.Labeling, levelMap)).Sum(d => d.SingleCoilWeight ?? 0);
            var shiftClassBWeight = shiftItems.Where(d => IsClassB(d.Labeling, levelMap)).Sum(d => d.SingleCoilWeight ?? 0);
            var shiftUnqualifiedWeight = shiftItems.Where(d => IsUnqualified(d.Labeling, levelMap)).Sum(d => d.SingleCoilWeight ?? 0);
            var shiftQualifiedWeight = shiftClassAWeight + shiftClassBWeight;

            result.Add(new MonthlyQualityReportShiftGroupDto
            {
                Shift = shiftGroup.Key,
                ProductSpecCode = "小计",
                DetectionWeight = shiftTotalWeight,
                ClassAWeight = shiftClassAWeight,
                ClassARate = shiftTotalWeight > 0 ? Math.Round(shiftClassAWeight / shiftTotalWeight * 100, 2) : 0,
                ClassBWeight = shiftClassBWeight,
                ClassBRate = shiftTotalWeight > 0 ? Math.Round(shiftClassBWeight / shiftTotalWeight * 100, 2) : 0,
                UnqualifiedWeight = shiftUnqualifiedWeight,
                QualifiedRate = shiftTotalWeight > 0 ? Math.Round(shiftQualifiedWeight / shiftTotalWeight * 100, 2) : 0,
                IsSummaryRow = true,
                SummaryType = "ShiftSubtotal"
            });
        }

        // 月度合计
        var totalWeight2 = data.Sum(d => d.SingleCoilWeight ?? 0);
        var totalClassAWeight = data.Where(d => IsClassA(d.Labeling, levelMap)).Sum(d => d.SingleCoilWeight ?? 0);
        var totalClassBWeight = data.Where(d => IsClassB(d.Labeling, levelMap)).Sum(d => d.SingleCoilWeight ?? 0);
        var totalUnqualifiedWeight = data.Where(d => IsUnqualified(d.Labeling, levelMap)).Sum(d => d.SingleCoilWeight ?? 0);
        var totalQualifiedWeight = totalClassAWeight + totalClassBWeight;

        result.Add(new MonthlyQualityReportShiftGroupDto
        {
            Shift = "本月合计",
            ProductSpecCode = "",
            DetectionWeight = totalWeight2,
            ClassAWeight = totalClassAWeight,
            ClassARate = totalWeight2 > 0 ? Math.Round(totalClassAWeight / totalWeight2 * 100, 2) : 0,
            ClassBWeight = totalClassBWeight,
            ClassBRate = totalWeight2 > 0 ? Math.Round(totalClassBWeight / totalWeight2 * 100, 2) : 0,
            UnqualifiedWeight = totalUnqualifiedWeight,
            QualifiedRate = totalWeight2 > 0 ? Math.Round(totalQualifiedWeight / totalWeight2 * 100, 2) : 0,
            IsSummaryRow = true,
            SummaryType = "MonthlyTotal"
        });

        return result;
    }

    /// <inheritdoc/>
    [HttpGet("quality-trend")]
    public async Task<List<QualityTrendDto>> GetQualityTrendAsync([FromQuery] MonthlyQualityReportQueryDto query)
    {
        var baseQuery = BuildBaseQuery(query);
        var levelMap = await GetJudgmentLevelMapAsync();

        var data = await baseQuery.ToListAsync();

        // 按日期分组
        var dateGroups = data.GroupBy(d => d.ProdDate?.Date ?? DateTime.MinValue)
                              .OrderBy(g => g.Key);

        var result = new List<QualityTrendDto>();

        foreach (var dateGroup in dateGroups)
        {
            if (dateGroup.Key == DateTime.MinValue) continue;

            var items = dateGroup.ToList();
            var totalWeight = items.Sum(d => d.SingleCoilWeight ?? 0);
            var classAWeight = items.Where(d => IsClassA(d.Labeling, levelMap)).Sum(d => d.SingleCoilWeight ?? 0);
            var classBWeight = items.Where(d => IsClassB(d.Labeling, levelMap)).Sum(d => d.SingleCoilWeight ?? 0);
            var qualifiedWeight = classAWeight + classBWeight;

            result.Add(new QualityTrendDto
            {
                Date = dateGroup.Key,
                QualifiedRate = totalWeight > 0 ? Math.Round(qualifiedWeight / totalWeight * 100, 2) : 0,
                ClassARate = totalWeight > 0 ? Math.Round(classAWeight / totalWeight * 100, 2) : 0,
                ClassBRate = totalWeight > 0 ? Math.Round(classBWeight / totalWeight * 100, 2) : 0
            });
        }

        return result;
    }

    /// <inheritdoc/>
    [HttpGet("unqualified-categories")]
    public async Task<List<UnqualifiedCategoryDto>> GetUnqualifiedCategoriesAsync([FromQuery] MonthlyQualityReportQueryDto query)
    {
        var baseQuery = BuildBaseQuery(query);
        var levelMap = await GetJudgmentLevelMapAsync();
        var unqualifiedLevels = await GetUnqualifiedLevelsAsync();

        var data = await baseQuery.ToListAsync();

        var totalUnqualifiedWeight = data.Where(d => IsUnqualified(d.Labeling, levelMap))
                                          .Sum(d => d.SingleCoilWeight ?? 0);

        var result = new List<UnqualifiedCategoryDto>();

        foreach (var level in unqualifiedLevels)
        {
            var categoryWeight = data.Where(d => d.Labeling == level.Name)
                                      .Sum(d => d.SingleCoilWeight ?? 0);

            if (categoryWeight > 0)
            {
                result.Add(new UnqualifiedCategoryDto
                {
                    CategoryName = level.Name,
                    Weight = categoryWeight,
                    Rate = totalUnqualifiedWeight > 0 ? Math.Round(categoryWeight / totalUnqualifiedWeight * 100, 2) : 0
                });
            }
        }

        // 按重量降序排序
        return result.OrderByDescending(c => c.Weight).ToList();
    }

    /// <inheritdoc/>
    [HttpGet("shift-comparison")]
    public async Task<List<ShiftComparisonDto>> GetShiftComparisonAsync([FromQuery] MonthlyQualityReportQueryDto query)
    {
        var baseQuery = BuildBaseQuery(query);
        var levelMap = await GetJudgmentLevelMapAsync();

        var data = await baseQuery.ToListAsync();

        // 按班次分组
        var shiftGroups = data.GroupBy(d => d.Shift).OrderBy(g => GetShiftOrder(g.Key));

        var result = new List<ShiftComparisonDto>();

        foreach (var shiftGroup in shiftGroups)
        {
            var items = shiftGroup.ToList();
            var totalWeight = items.Sum(d => d.SingleCoilWeight ?? 0);
            var classAWeight = items.Where(d => IsClassA(d.Labeling, levelMap)).Sum(d => d.SingleCoilWeight ?? 0);
            var classBWeight = items.Where(d => IsClassB(d.Labeling, levelMap)).Sum(d => d.SingleCoilWeight ?? 0);
            var qualifiedWeight = classAWeight + classBWeight;

            result.Add(new ShiftComparisonDto
            {
                Shift = shiftGroup.Key ?? "未知",
                TotalWeight = totalWeight,
                QualifiedRate = totalWeight > 0 ? Math.Round(qualifiedWeight / totalWeight * 100, 2) : 0,
                ClassARate = totalWeight > 0 ? Math.Round(classAWeight / totalWeight * 100, 2) : 0
            });
        }

        return result;
    }

    #region 私有辅助方法

    private ISugarQueryable<IntermediateDataEntity> BuildBaseQuery(MonthlyQualityReportQueryDto query)
    {
        var queryable = _intermediateDataRepository.AsQueryable()
            .Where(d => d.DeleteMark == null);

        // 日期范围筛选
        if (query.StartDate != default)
        {
            queryable = queryable.Where(d => d.ProdDate >= query.StartDate);
        }
        if (query.EndDate != default)
        {
            var endDate = query.EndDate.Date.AddDays(1);
            queryable = queryable.Where(d => d.ProdDate < endDate);
        }

        // 班次筛选
        if (!string.IsNullOrWhiteSpace(query.Shift))
        {
            queryable = queryable.Where(d => d.Shift == query.Shift);
        }

        // 炉号筛选
        if (!string.IsNullOrWhiteSpace(query.ShiftNo))
        {
            queryable = queryable.Where(d => d.ShiftNo.Contains(query.ShiftNo));
        }

        // 产品规格筛选
        if (!string.IsNullOrWhiteSpace(query.ProductSpecCode))
        {
            queryable = queryable.Where(d => d.ProductSpecCode == query.ProductSpecCode);
        }

        // 产线筛选
        if (query.LineNo.HasValue)
        {
            queryable = queryable.Where(d => d.LineNo == query.LineNo);
        }

        return queryable;
    }

    private async Task<Dictionary<string, IntermediateDataJudgmentLevelEntity>> GetJudgmentLevelMapAsync()
    {
        var levels = await _judgmentLevelRepository.AsQueryable()
            .Where(l => l.DeleteMark == null)
            .ToListAsync();

        // 存在重复的 Name (如 "B")，按优先级排序取第一个
        return levels.GroupBy(l => l.Name)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(l => l.Priority).First());
    }

    private async Task<List<IntermediateDataJudgmentLevelEntity>> GetUnqualifiedLevelsAsync()
    {
        return await _judgmentLevelRepository.AsQueryable()
            .Where(l => l.DeleteMark == null 
                     && l.QualityStatus == QualityStatusEnum.Unqualified
                     && l.IsStatistic == true)
            .OrderBy(l => l.Priority)
            .ToListAsync();
    }

    private async Task<List<JudgmentLevelColumnDto>> GetUnqualifiedColumnsAsync()
    {
        var levels = await GetUnqualifiedLevelsAsync();
        return levels.Select(l => new JudgmentLevelColumnDto
        {
            Id = l.Id,
            Code = l.Code,
            Name = l.Name,
            QualityStatus = l.QualityStatus,
            Color = l.Color
        }).ToList();
    }

    private bool IsClassA(string labeling, Dictionary<string, IntermediateDataJudgmentLevelEntity> levelMap)
    {
        if (string.IsNullOrWhiteSpace(labeling)) return false;
        // A级判定：名称包含"A"且是合格的
        if (levelMap.TryGetValue(labeling, out var level))
        {
            return labeling.Contains("A") && level.QualityStatus == QualityStatusEnum.Qualified;
        }
        return false;
    }

    private bool IsClassB(string labeling, Dictionary<string, IntermediateDataJudgmentLevelEntity> levelMap)
    {
        if (string.IsNullOrWhiteSpace(labeling)) return false;
        // B级判定：名称包含"B"且是合格的，或者是合格但不是A级的
        if (levelMap.TryGetValue(labeling, out var level))
        {
            if (level.QualityStatus == QualityStatusEnum.Qualified)
            {
                return labeling.Contains("B") || !labeling.Contains("A");
            }
        }
        return false;
    }

    private bool IsUnqualified(string labeling, Dictionary<string, IntermediateDataJudgmentLevelEntity> levelMap)
    {
        if (string.IsNullOrWhiteSpace(labeling)) return false;
        if (levelMap.TryGetValue(labeling, out var level))
        {
            return level.QualityStatus == QualityStatusEnum.Unqualified;
        }
        return false;
    }



    private int GetShiftOrder(string shift)
    {
        return shift switch
        {
            "甲" => 1,
            "乙" => 2,
            "丙" => 3,
            _ => 99
        };
    }

    private MonthlyQualityReportDetailDto CreateDetailDto(
        IntermediateDataEntity item,
        Dictionary<string, IntermediateDataJudgmentLevelEntity> levelMap,
        List<IntermediateDataJudgmentLevelEntity> unqualifiedLevels)
    {
        var weight = item.SingleCoilWeight ?? 0;
        var isClassA = IsClassA(item.Labeling, levelMap);
        var isClassB = IsClassB(item.Labeling, levelMap);
        var isUnqualified = IsUnqualified(item.Labeling, levelMap);

        var unqualifiedCategories = new Dictionary<string, decimal>();
        foreach (var level in unqualifiedLevels)
        {
            unqualifiedCategories[level.Name] = item.Labeling == level.Name ? weight : 0;
        }

        return new MonthlyQualityReportDetailDto
        {
            ProdDate = item.ProdDate,
            Shift = item.Shift,
            ShiftNo = item.ShiftNo,
            ProductSpecCode = item.ProductSpecCode ?? item.Width?.ToString(),
            DetectionWeight = weight,
            ClassAWeight = isClassA ? weight : 0,
            ClassARate = isClassA ? 100 : 0,
            ClassBWeight = isClassB ? weight : 0,
            ClassBRate = isClassB ? 100 : 0,
            UnqualifiedWeight = isUnqualified ? weight : 0,
            QualifiedRate = (isClassA || isClassB) ? 100 : 0,
            UnqualifiedCategories = unqualifiedCategories
        };
    }

    private MonthlyQualityReportDetailDto CreateShiftSubtotalDto(
        string shift,
        List<IntermediateDataEntity> items,
        Dictionary<string, IntermediateDataJudgmentLevelEntity> levelMap,
        List<IntermediateDataJudgmentLevelEntity> unqualifiedLevels)
    {
        var totalWeight = items.Sum(d => d.SingleCoilWeight ?? 0);
        var classAWeight = items.Where(d => IsClassA(d.Labeling, levelMap)).Sum(d => d.SingleCoilWeight ?? 0);
        var classBWeight = items.Where(d => IsClassB(d.Labeling, levelMap)).Sum(d => d.SingleCoilWeight ?? 0);
        var unqualifiedWeight = items.Where(d => IsUnqualified(d.Labeling, levelMap)).Sum(d => d.SingleCoilWeight ?? 0);
        var qualifiedWeight = classAWeight + classBWeight;

        var unqualifiedCategories = new Dictionary<string, decimal>();
        foreach (var level in unqualifiedLevels)
        {
            unqualifiedCategories[level.Name] = items.Where(d => d.Labeling == level.Name)
                                                      .Sum(d => d.SingleCoilWeight ?? 0);
        }

        return new MonthlyQualityReportDetailDto
        {
            Shift = shift,
            ProductSpecCode = "小计",
            DetectionWeight = totalWeight,
            ClassAWeight = classAWeight,
            ClassARate = totalWeight > 0 ? Math.Round(classAWeight / totalWeight * 100, 2) : 0,
            ClassBWeight = classBWeight,
            ClassBRate = totalWeight > 0 ? Math.Round(classBWeight / totalWeight * 100, 2) : 0,
            UnqualifiedWeight = unqualifiedWeight,
            QualifiedRate = totalWeight > 0 ? Math.Round(qualifiedWeight / totalWeight * 100, 2) : 0,
            UnqualifiedCategories = unqualifiedCategories,
            IsSummaryRow = true,
            SummaryType = "ShiftSubtotal"
        };
    }

    private MonthlyQualityReportDetailDto CreateMonthlyTotalDto(
        List<IntermediateDataEntity> data,
        Dictionary<string, IntermediateDataJudgmentLevelEntity> levelMap,
        List<IntermediateDataJudgmentLevelEntity> unqualifiedLevels)
    {
        var totalWeight = data.Sum(d => d.SingleCoilWeight ?? 0);
        var classAWeight = data.Where(d => IsClassA(d.Labeling, levelMap)).Sum(d => d.SingleCoilWeight ?? 0);
        var classBWeight = data.Where(d => IsClassB(d.Labeling, levelMap)).Sum(d => d.SingleCoilWeight ?? 0);
        var unqualifiedWeight = data.Where(d => IsUnqualified(d.Labeling, levelMap)).Sum(d => d.SingleCoilWeight ?? 0);
        var qualifiedWeight = classAWeight + classBWeight;

        var unqualifiedCategories = new Dictionary<string, decimal>();
        foreach (var level in unqualifiedLevels)
        {
            unqualifiedCategories[level.Name] = data.Where(d => d.Labeling == level.Name)
                                                     .Sum(d => d.SingleCoilWeight ?? 0);
        }

        return new MonthlyQualityReportDetailDto
        {
            Shift = "本月合计",
            ProductSpecCode = "",
            DetectionWeight = totalWeight,
            ClassAWeight = classAWeight,
            ClassARate = totalWeight > 0 ? Math.Round(classAWeight / totalWeight * 100, 2) : 0,
            ClassBWeight = classBWeight,
            ClassBRate = totalWeight > 0 ? Math.Round(classBWeight / totalWeight * 100, 2) : 0,
            UnqualifiedWeight = unqualifiedWeight,
            QualifiedRate = totalWeight > 0 ? Math.Round(qualifiedWeight / totalWeight * 100, 2) : 0,
            UnqualifiedCategories = unqualifiedCategories,
            IsSummaryRow = true,
            SummaryType = "MonthlyTotal"
        };
    }

    #endregion
}
