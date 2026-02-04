using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Poxiao.DatabaseAccessor;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.Dashboard;
using Poxiao.Lab.Entity.Enum;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// 驾驶舱服务实现.
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "dashboard", Order = 210)]
[Route("api/lab/dashboard")]
public class DashboardService : IDashboardService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<IntermediateDataEntity> _intermediateDataRepository;
    private readonly ISqlSugarRepository<IntermediateDataJudgmentLevelEntity> _judgmentLevelRepository;
    private readonly ISqlSugarRepository<AppearanceFeatureCategoryEntity> _appearanceFeatureCategoryRepository;

    public DashboardService(
        ISqlSugarRepository<IntermediateDataEntity> intermediateDataRepository,
        ISqlSugarRepository<IntermediateDataJudgmentLevelEntity> judgmentLevelRepository,
        ISqlSugarRepository<AppearanceFeatureCategoryEntity> appearanceFeatureCategoryRepository)
    {
        _intermediateDataRepository = intermediateDataRepository;
        _judgmentLevelRepository = judgmentLevelRepository;
        _appearanceFeatureCategoryRepository = appearanceFeatureCategoryRepository;
    }

    /// <inheritdoc/>
    [HttpGet("kpi")]
    public async Task<DashboardKpiDto> GetKpiAsync([FromQuery] DashboardQueryDto query)
    {
        var baseQuery = BuildBaseQuery(query);
        var data = await baseQuery.ToListAsync();

        if (data.Count == 0)
        {
            return new DashboardKpiDto
            {
                TotalWeight = 0,
                QualifiedRate = 0,
                LaminationFactorAvg = 0,
                LaminationFactorTrend = new List<decimal>(),
                Warnings = new List<DashboardWarningDto>()
            };
        }

        var levelMap = await GetJudgmentLevelMapAsync();

        // 总重量
        var totalWeight = data.Sum(d => d.SingleCoilWeight ?? 0);

        // 合格率（A级+B级）
        var qualifiedWeight = data
            .Where(d => IsQualified(d.Labeling, levelMap))
            .Sum(d => d.SingleCoilWeight ?? 0);
        var qualifiedRate = totalWeight > 0 ? Math.Round(qualifiedWeight / totalWeight * 100, 2) : 0;

        // 平均叠片系数
        var laminationFactorData = data
            .Where(d => d.LaminationFactor.HasValue)
            .Select(d => d.LaminationFactor!.Value)
            .ToList();
        var laminationFactorAvg = laminationFactorData.Count > 0 ? laminationFactorData.Average() : 0m;

        // 叠片系数趋势（按日期分组）
        var laminationTrend = data
            .Where(d => d.ProdDate.HasValue && d.LaminationFactor.HasValue)
            .GroupBy(d => d.ProdDate!.Value.Date)
            .OrderBy(g => g.Key)
            .Select(g => g.Average(d => d.LaminationFactor!.Value))
            .ToList();

        // 警告信息
        var warnings = new List<DashboardWarningDto>();

        // 合格率预警
        if (qualifiedRate < 90)
        {
            warnings.Add(new DashboardWarningDto
            {
                Type = "quality",
                Message = $"当前合格率为{qualifiedRate:F1}%，低于90%预警线",
                Level = qualifiedRate < 85 ? "error" : "warning"
            });
        }

        // 叠片系数预警
        if (laminationFactorAvg < 95)
        {
            warnings.Add(new DashboardWarningDto
            {
                Type = "process",
                Message = $"当前平均叠片系数为{laminationFactorAvg:F2}%，低于95%标准",
                Level = "warning"
            });
        }

        return new DashboardKpiDto
        {
            TotalWeight = Math.Round(totalWeight, 2),
            QualifiedRate = qualifiedRate,
            LaminationFactorAvg = Math.Round(laminationFactorAvg, 2),
            LaminationFactorTrend = laminationTrend,
            Warnings = warnings
        };
    }

    /// <inheritdoc/>
    [HttpGet("quality-distribution")]
    public async Task<List<QualityDistributionDto>> GetQualityDistributionAsync([FromQuery] DashboardQueryDto query)
    {
        var baseQuery = BuildBaseQuery(query);
        var data = await baseQuery.ToListAsync();

        var levelMap = await GetJudgmentLevelMapAsync();

        // 按Labeling分组统计
        var grouped = data
            .GroupBy(d => d.Labeling ?? "未判定")
            .Select(g => new
            {
                Labeling = g.Key,
                Count = g.Count(),
                Weight = g.Sum(d => d.SingleCoilWeight ?? 0)
            })
            .ToList();

        var totalWeight = grouped.Sum(g => g.Weight);

        var result = new List<QualityDistributionDto>();

        // 定义显示顺序和颜色
        var predefinedCategories = new List<(string Label, string DisplayName, string Color)>
        {
            ("A", "A级", "#52c41a"),
            ("B", "B级", "#1890ff"),
            ("性能不合", "性能不合", "#ff4d4f"),
            ("其他不合", "其他不合", "#faad14"),
            ("未判定", "未判定", "#d9d9d9")
        };

        // 按预定义顺序填充数据
        foreach (var (label, displayName, color) in predefinedCategories)
        {
            var found = grouped.FirstOrDefault(g =>
                g.Labeling.Contains(label, StringComparison.OrdinalIgnoreCase));
            if (found != null)
            {
                result.Add(new QualityDistributionDto
                {
                    Category = displayName,
                    Count = found.Count,
                    Weight = Math.Round(found.Weight, 2),
                    Rate = totalWeight > 0 ? Math.Round(found.Weight / totalWeight * 100, 2) : 0,
                    Color = color
                });
            }
        }

        // 处理其他未匹配的类别
        foreach (var group in grouped)
        {
            if (!predefinedCategories.Any(p => group.Labeling.Contains(p.Label, StringComparison.OrdinalIgnoreCase)))
            {
                result.Add(new QualityDistributionDto
                {
                    Category = group.Labeling,
                    Count = group.Count,
                    Weight = Math.Round(group.Weight, 2),
                    Rate = totalWeight > 0 ? Math.Round(group.Weight / totalWeight * 100, 2) : 0,
                    Color = "#8c8c8c"
                });
            }
        }

        return result;
    }

    /// <inheritdoc/>
    [HttpGet("lamination-trend")]
    public async Task<List<LaminationTrendDto>> GetLaminationTrendAsync([FromQuery] DashboardQueryDto query)
    {
        var baseQuery = BuildBaseQuery(query);
        var data = await baseQuery
            .Where(d => d.ProdDate.HasValue && d.LaminationFactor.HasValue)
            .OrderBy(d => d.ProdDate)
            .ToListAsync();

        var trend = data
            .GroupBy(d => d.ProdDate!.Value.Date)
            .Select(g => new LaminationTrendDto
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                Value = Math.Round(g.Average(d => d.LaminationFactor!.Value), 2),
                Min = Math.Round(g.Min(d => d.LaminationFactor!.Value), 2),
                Max = Math.Round(g.Max(d => d.LaminationFactor!.Value), 2)
            })
            .ToList();

        return trend;
    }

    /// <inheritdoc/>
    [HttpGet("defect-top5")]
    public async Task<List<DefectTopDto>> GetDefectTop5Async([FromQuery] DashboardQueryDto query)
    {
        var baseQuery = BuildBaseQuery(query);
        var data = await baseQuery
            .Where(d => !string.IsNullOrWhiteSpace(d.AppearanceFeatureCategoryIds))
            .ToListAsync();

        // 获取所有缺陷大类映射
        var categoryQuery = await _appearanceFeatureCategoryRepository
            .AsQueryable()
            .Where(c => c.DeleteMark == null)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();
        var categories = categoryQuery.ToDictionary(c => c.Id.ToString(), c => c.Name);

        // 统计各缺陷类别出现次数
        var categoryStats = new Dictionary<string, (int Count, decimal Weight)>();

        foreach (var item in data)
        {
            try
            {
                var categoryIds = JsonConvert.DeserializeObject<List<string>>(item.AppearanceFeatureCategoryIds!);
                if (categoryIds != null)
                {
                    foreach (var categoryId in categoryIds)
                    {
                        if (categoryId != null && categories.TryGetValue(categoryId, out string categoryName))
                        {
                            if (!categoryStats.ContainsKey(categoryName))
                            {
                                categoryStats[categoryName] = (0, 0);
                            }
                            categoryStats[categoryName] = (
                                categoryStats[categoryName].Count + 1,
                                categoryStats[categoryName].Weight + (item.SingleCoilWeight ?? 0)
                            );
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // 忽略JSON解析错误
            }
        }

        // 取Top5按数量排序
        var top5 = categoryStats
            .OrderByDescending(kvp => kvp.Value.Count)
            .Take(5)
            .Select(kvp => new DefectTopDto
            {
                Category = kvp.Key,
                Count = kvp.Value.Count,
                Weight = Math.Round(kvp.Value.Weight, 2)
            })
            .ToList();

        return top5;
    }

    /// <inheritdoc/>
    [HttpGet("production-heatmap")]
    public async Task<List<ProductionHeatmapDto>> GetProductionHeatmapAsync([FromQuery] DashboardQueryDto query)
    {
        var baseQuery = BuildBaseQuery(query);
        var data = await baseQuery
            .Where(d => d.ProdDate.HasValue)
            .ToListAsync();

        var levelMap = await GetJudgmentLevelMapAsync();

        // 按星期和小时分组统计
        var heatmapData = new List<ProductionHeatmapDto>();

        for (int day = 0; day < 7; day++) // 周一到周日
        {
            for (int hour = 0; hour < 24; hour++)
            {
                var dayData = data.Where(d =>
                {
                    var date = d.ProdDate!.Value;
                    var dayOfWeek = ((int)date.DayOfWeek + 6) % 7; // 将周日(0)转为6，周一转为0
                    return dayOfWeek == day;
                }).ToList();

                // 统计该时段的合格率
                var qualified = dayData.Count(d => IsQualified(d.Labeling, levelMap));
                var total = dayData.Count;

                heatmapData.Add(new ProductionHeatmapDto
                {
                    DayOfWeek = day,
                    Hour = hour,
                    Value = total > 0 ? Math.Round((decimal)qualified / total * 100, 2) : 0,
                    Count = total
                });
            }
        }

        return heatmapData;
    }

    /// <inheritdoc/>
    [HttpGet("thickness-correlation")]
    public async Task<List<ThicknessCorrelationDto>> GetThicknessCorrelationAsync([FromQuery] DashboardQueryDto query)
    {
        var baseQuery = BuildBaseQuery(query);
        var data = await baseQuery
            .Where(d => d.AvgThickness.HasValue && d.LaminationFactor.HasValue)
            .OrderBy(d => d.AvgThickness)
            .ToListAsync();

        var result = data.Select(d => new ThicknessCorrelationDto
        {
            Thickness = Math.Round(d.AvgThickness!.Value, 2),
            LaminationFactor = Math.Round(d.LaminationFactor!.Value, 2),
            QualityLevel = d.Labeling ?? "未判定",
            Id = d.Id
        }).ToList();

        return result;
    }

    #region Private Methods

    /// <summary>
    /// 构建基础查询.
    /// </summary>
    private ISugarQueryable<IntermediateDataEntity> BuildBaseQuery(DashboardQueryDto query)
    {
        var q = _intermediateDataRepository
            .AsQueryable()
            .Where(d => d.DeleteMark == null)
            .Where(d => d.ProdDate >= query.StartDate && d.ProdDate < query.EndDate.AddDays(1));

        if (!string.IsNullOrWhiteSpace(query.Shift))
        {
            q = q.Where(d => d.Shift == query.Shift);
        }

        return q;
    }

    /// <summary>
    /// 获取判定等级映射.
    /// </summary>
    private async Task<Dictionary<string, IntermediateDataJudgmentLevelEntity>> GetJudgmentLevelMapAsync()
    {
        var levels = await _judgmentLevelRepository
            .AsQueryable()
            .Where(d => d.DeleteMark == null)
            .ToListAsync();

        return levels.ToDictionary(d => d.Code, d => d);
    }

    /// <summary>
    /// 判断是否合格（A级或B级）.
    /// </summary>
    private bool IsQualified(string? labeling, Dictionary<string, IntermediateDataJudgmentLevelEntity> levelMap)
    {
        if (string.IsNullOrWhiteSpace(labeling))
            return false;

        // 直接包含A级或B级
        if (labeling.Contains("A级", StringComparison.OrdinalIgnoreCase) ||
            labeling.Contains("B级", StringComparison.OrdinalIgnoreCase))
            return true;

        // 通过判定等级表判断
        if (levelMap.TryGetValue(labeling, out var level))
        {
            return level.QualityStatus == QualityStatusEnum.Qualified;
        }

        return false;
    }

    /// <summary>
    /// 判断是否A级.
    /// </summary>
    private bool IsClassA(string? labeling, Dictionary<string, IntermediateDataJudgmentLevelEntity> levelMap)
    {
        if (string.IsNullOrWhiteSpace(labeling))
            return false;

        if (labeling.Contains("A级", StringComparison.OrdinalIgnoreCase))
            return true;

        if (levelMap.TryGetValue(labeling, out var level))
        {
            return level.Code.StartsWith("A", StringComparison.OrdinalIgnoreCase) &&
                   level.QualityStatus == QualityStatusEnum.Qualified;
        }

        return false;
    }

    /// <summary>
    /// 判断是否B级.
    /// </summary>
    private bool IsClassB(string? labeling, Dictionary<string, IntermediateDataJudgmentLevelEntity> levelMap)
    {
        if (string.IsNullOrWhiteSpace(labeling))
            return false;

        if (labeling.Contains("B级", StringComparison.OrdinalIgnoreCase))
            return true;

        if (levelMap.TryGetValue(labeling, out var level))
        {
            return level.Code.StartsWith("B", StringComparison.OrdinalIgnoreCase) &&
                   level.QualityStatus == QualityStatusEnum.Qualified;
        }

        return false;
    }

    /// <summary>
    /// 判断是否不合格.
    /// </summary>
    private bool IsUnqualified(string? labeling, Dictionary<string, IntermediateDataJudgmentLevelEntity> levelMap)
    {
        if (string.IsNullOrWhiteSpace(labeling))
            return false;

        if (labeling.Contains("不合", StringComparison.OrdinalIgnoreCase))
            return true;

        if (levelMap.TryGetValue(labeling, out var level))
        {
            return level.QualityStatus == QualityStatusEnum.Unqualified;
        }

        return false;
    }

    #endregion
}
