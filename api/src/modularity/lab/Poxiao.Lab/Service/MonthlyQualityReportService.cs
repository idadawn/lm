using MiniExcelLibs;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Poxiao.DatabaseAccessor;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.MonthlyQualityReport;
using Poxiao.Lab.Entity.Enum;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// 月度质量报表服务实现
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "monthly-quality-report", Order = 220)]
[Route("api/lab/monthly-quality-report")]
public class MonthlyQualityReportService : IMonthlyQualityReportService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<IntermediateDataEntity> _intermediateDataRepository;
    private readonly ISqlSugarRepository<IntermediateDataJudgmentLevelEntity> _judgmentLevelRepository;
    private Dictionary<int, int> _exportColumnWidths;

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
        // 获取需要统计的等级列
        var statisticLevels = await GetStatisticLevelsAsync();
        var qualifiedLevels = statisticLevels.Where(l => l.QualityStatus == QualityStatusEnum.Qualified).ToList();
        var unqualifiedLevels = statisticLevels.Where(l => l.QualityStatus == QualityStatusEnum.Unqualified).ToList();

        var response = new MonthlyQualityReportResponseDto
        {
            Summary = await GetSummaryAsync(query),
            Details = await GetDetailsAsync(query),
            ShiftGroups = await GetShiftGroupsAsync(query),
            QualityTrends = await GetQualityTrendAsync(query),
            UnqualifiedCategoryStats = await GetUnqualifiedCategoriesAsync(query),
            ShiftComparisons = await GetShiftComparisonAsync(query),
            QualifiedColumns = qualifiedLevels.Select(l => new JudgmentLevelColumnDto
            {
                Id = l.Id,
                Code = l.Code,
                Name = l.Name,
                QualityStatus = l.QualityStatus,
                Color = l.Color,
                Priority = l.Priority
            }).ToList(),
            UnqualifiedColumns = unqualifiedLevels.Select(l => new JudgmentLevelColumnDto
            {
                Id = l.Id,
                Code = l.Code,
                Name = l.Name,
                QualityStatus = l.QualityStatus,
                Color = l.Color,
                Priority = l.Priority
            }).ToList()
        };

        return response;
    }

    /// <inheritdoc/>
    [HttpGet("summary")]
    public async Task<MonthlyQualityReportSummaryDto> GetSummaryAsync([FromQuery] MonthlyQualityReportQueryDto query)
    {
        var baseQuery = BuildBaseQuery(query);
        // var levelMap = await GetJudgmentLevelMapAsync(); // Unused variable
        var statisticLevels = await GetStatisticLevelsAsync();
        var qualifiedLevels = statisticLevels.Where(l => l.QualityStatus == QualityStatusEnum.Qualified).ToList();
        var unqualifiedLevels = statisticLevels.Where(l => l.QualityStatus == QualityStatusEnum.Unqualified).ToList();

        var data = await baseQuery.ToListAsync();
        var totalWeight = data.Sum(d => d.SingleCoilWeight ?? 0);

        // 合格分类统计
        var qualifiedCategories = new Dictionary<string, LevelStatDto>();
        decimal qualifiedWeight = 0;

        foreach (var level in qualifiedLevels)
        {
            var weight = data.Where(d => d.Labeling == level.Name).Sum(d => d.SingleCoilWeight ?? 0);
            qualifiedWeight += weight;
            qualifiedCategories[level.Name] = new LevelStatDto
            {
                Weight = weight,
                Rate = totalWeight > 0 ? Math.Round(weight / totalWeight * 100, 2) : 0
            };
        }

        // 不合格分类统计
        var unqualifiedCategories = new Dictionary<string, decimal>();
        decimal unqualifiedWeight = 0;

        foreach (var level in unqualifiedLevels)
        {
            var weight = data.Where(d => d.Labeling == level.Name).Sum(d => d.SingleCoilWeight ?? 0);
            unqualifiedWeight += weight;
            unqualifiedCategories[level.Name] = weight;
        }

        // 提取A类和B类数据
        decimal classAWeight = 0;
        decimal classBWeight = 0;

        if (qualifiedCategories.ContainsKey("A"))
        {
            classAWeight = qualifiedCategories["A"].Weight;
        }

        if (qualifiedCategories.ContainsKey("B"))
        {
            classBWeight = qualifiedCategories["B"].Weight;
        }

        decimal classARate = totalWeight > 0 ? Math.Round(classAWeight / totalWeight * 100, 2) : 0;
        decimal classBRate = totalWeight > 0 ? Math.Round(classBWeight / totalWeight * 100, 2) : 0;

        return new MonthlyQualityReportSummaryDto
        {
            TotalWeight = totalWeight,
            QualifiedCategories = qualifiedCategories,
            QualifiedWeight = qualifiedWeight,
            QualifiedRate = totalWeight > 0 ? Math.Round(qualifiedWeight / totalWeight * 100, 2) : 0,
            UnqualifiedCategories = unqualifiedCategories,
            UnqualifiedWeight = unqualifiedWeight,
            UnqualifiedRate = totalWeight > 0 ? Math.Round(unqualifiedWeight / totalWeight * 100, 2) : 0,
            ClassAWeight = classAWeight,
            ClassARate = classARate,
            ClassBWeight = classBWeight,
            ClassBRate = classBRate
        };
    }

    /// <inheritdoc/>
    [HttpGet("details")]
    public async Task<List<MonthlyQualityReportDetailDto>> GetDetailsAsync([FromQuery] MonthlyQualityReportQueryDto query)
    {
        var baseQuery = BuildBaseQuery(query);
        // var levelMap = await GetJudgmentLevelMapAsync(); // Unused
        var statisticLevels = await GetStatisticLevelsAsync();
        var qualifiedLevels = statisticLevels.Where(l => l.QualityStatus == QualityStatusEnum.Qualified).ToList();
        var unqualifiedLevels = statisticLevels.Where(l => l.QualityStatus == QualityStatusEnum.Unqualified).ToList();

        // 查出原始数据
        var rawData = await baseQuery.ToListAsync();

        // 按 生产日期+班次+炉次号+带宽 分组
        var groupedData = rawData
            .GroupBy(d => new {
                ProdDate = d.ProdDate?.Date,
                d.Shift,
                FurnaceBatchNo = d.FurnaceBatchNo ?? int.MaxValue,
                ProductSpecCode = d.ProductSpecCode ?? d.Width?.ToString() ?? "未知"
            })
            .Select(g => new {
                g.Key.ProdDate,
                g.Key.Shift,
                g.Key.FurnaceBatchNo,
                g.Key.ProductSpecCode,
                Items = g.ToList()
            })
            .OrderBy(g => g.ProdDate)
            .ThenBy(g => g.FurnaceBatchNo)
            .ThenBy(g => GetShiftOrder(g.Shift))
            .ToList();

        var result = new List<MonthlyQualityReportDetailDto>();

        // 直接遍历所有分组数据
        foreach (var group in groupedData)
        {
            // 从分组项中获取第一条记录的ShiftNo用于显示
            var firstItem = group.Items.FirstOrDefault();
            var shiftNo = firstItem?.ShiftNo ?? "";

            var dto = CreateDetailDto(
                group.ProdDate,
                group.Shift,
                shiftNo,
                group.ProductSpecCode,
                group.Items,
                qualifiedLevels,
                unqualifiedLevels);
            result.Add(dto);
        }

        // 月度合计
        var monthlyTotal = CreateSubtotalDto(
            "合计",
            "MonthlyTotal",
            rawData,
            qualifiedLevels,
            unqualifiedLevels);
        result.Add(monthlyTotal);

        return result;
    }

    /// <inheritdoc/>
    [HttpGet("shift-groups")]
    public async Task<List<MonthlyQualityReportShiftGroupDto>> GetShiftGroupsAsync([FromQuery] MonthlyQualityReportQueryDto query)
    {
        var baseQuery = BuildBaseQuery(query);
        var statisticLevels = await GetStatisticLevelsAsync();
        var qualifiedLevels = statisticLevels.Where(l => l.QualityStatus == QualityStatusEnum.Qualified).ToList();

        var data = await baseQuery.ToListAsync();
        var result = new List<MonthlyQualityReportShiftGroupDto>();

        // 按班次分组
        var shiftGroups = data
            .GroupBy(d => d.Shift)
            .OrderBy(g => GetShiftOrder(g.Key));

        foreach (var shiftGroup in shiftGroups)
        {
            // 按产品规格分组
            var specGroups = shiftGroup
                .GroupBy(d => d.ProductSpecCode ?? d.Width?.ToString() ?? "未知")
                .OrderBy(g => g.Key);

            foreach (var specGroup in specGroups)
            {
                var items = specGroup.ToList();
                result.Add(CreateShiftGroupDto(shiftGroup.Key, specGroup.Key, items, qualifiedLevels, false, null));
            }

            // 班次小计
            var shiftItems = shiftGroup.ToList();
            result.Add(CreateShiftGroupDto(shiftGroup.Key, "小计", shiftItems, qualifiedLevels, true, "ShiftSubtotal"));
        }

        // 月度合计
        result.Add(CreateShiftGroupDto("合计", "", data, qualifiedLevels, true, "MonthlyTotal"));

        return result;
    }

    /// <inheritdoc/>
    [HttpGet("quality-trend")]
    public async Task<List<QualityTrendDto>> GetQualityTrendAsync([FromQuery] MonthlyQualityReportQueryDto query)
    {
        var baseQuery = BuildBaseQuery(query);
        var statisticLevels = await GetStatisticLevelsAsync();
        var qualifiedLevels = statisticLevels.Where(l => l.QualityStatus == QualityStatusEnum.Qualified).ToList();

        var data = await baseQuery.ToListAsync();

        var dateGroups = data
            .GroupBy(d => d.ProdDate?.Date ?? DateTime.MinValue)
            .Where(g => g.Key != DateTime.MinValue)
            .OrderBy(g => g.Key);

        var result = new List<QualityTrendDto>();

        foreach (var dateGroup in dateGroups)
        {
            var items = dateGroup.ToList();
            var totalWeight = items.Sum(d => d.SingleCoilWeight ?? 0);
            var qualifiedWeight = items
                .Where(d => qualifiedLevels.Any(l => l.Name == d.Labeling))
                .Sum(d => d.SingleCoilWeight ?? 0);

            // 动态计算各合格等级的占比
            var qualifiedRates = new Dictionary<string, decimal>();
            foreach (var level in qualifiedLevels)
            {
                var weight = items.Where(d => d.Labeling == level.Name).Sum(d => d.SingleCoilWeight ?? 0);
                qualifiedRates[level.Name] = totalWeight > 0 ? Math.Round(weight / totalWeight * 100, 2) : 0;
            }

            result.Add(new QualityTrendDto
            {
                Date = dateGroup.Key,
                QualifiedRate = totalWeight > 0 ? Math.Round(qualifiedWeight / totalWeight * 100, 2) : 0,
                // 为了兼容现有前端，暂时保留这两个字段
                ClassARate = qualifiedRates.GetValueOrDefault("A", 0),
                ClassBRate = qualifiedRates.GetValueOrDefault("B", 0)
            });
        }

        return result;
    }

    /// <inheritdoc/>
    [HttpGet("unqualified-categories")]
    public async Task<List<UnqualifiedCategoryDto>> GetUnqualifiedCategoriesAsync([FromQuery] MonthlyQualityReportQueryDto query)
    {
        var baseQuery = BuildBaseQuery(query);
        var statisticLevels = await GetStatisticLevelsAsync();
        var unqualifiedLevels = statisticLevels.Where(l => l.QualityStatus == QualityStatusEnum.Unqualified).ToList();

        var data = await baseQuery.ToListAsync();

        var totalUnqualifiedWeight = data
            .Where(d => unqualifiedLevels.Any(l => l.Name == d.Labeling))
            .Sum(d => d.SingleCoilWeight ?? 0);

        var result = new List<UnqualifiedCategoryDto>();

        foreach (var level in unqualifiedLevels)
        {
            var categoryWeight = data
                .Where(d => d.Labeling == level.Name)
                .Sum(d => d.SingleCoilWeight ?? 0);

            if (categoryWeight > 0)
            {
                result.Add(new UnqualifiedCategoryDto
                {
                    CategoryName = level.Name,
                    Weight = categoryWeight,
                    Rate = totalUnqualifiedWeight > 0
                        ? Math.Round(categoryWeight / totalUnqualifiedWeight * 100, 2)
                        : 0
                });
            }
        }

        return result.OrderByDescending(c => c.Weight).ToList();
    }

    /// <inheritdoc/>
    [HttpGet("shift-comparison")]
    public async Task<List<ShiftComparisonDto>> GetShiftComparisonAsync([FromQuery] MonthlyQualityReportQueryDto query)
    {
        var baseQuery = BuildBaseQuery(query);
        var statisticLevels = await GetStatisticLevelsAsync();
        var qualifiedLevels = statisticLevels.Where(l => l.QualityStatus == QualityStatusEnum.Qualified).ToList();

        var data = await baseQuery.ToListAsync();

        var shiftGroups = data.GroupBy(d => d.Shift).OrderBy(g => GetShiftOrder(g.Key));

        var result = new List<ShiftComparisonDto>();

        foreach (var shiftGroup in shiftGroups)
        {
            var items = shiftGroup.ToList();
            var totalWeight = items.Sum(d => d.SingleCoilWeight ?? 0);
            var qualifiedWeight = items
                .Where(d => qualifiedLevels.Any(l => l.Name == d.Labeling))
                .Sum(d => d.SingleCoilWeight ?? 0);

            // 获取A类重量和占比
            var classAWeight = items
                .Where(d => d.Labeling == "A")
                .Sum(d => d.SingleCoilWeight ?? 0);

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

        if (query.StartDate != default)
        {
            queryable = queryable.Where(d => d.ProdDate >= query.StartDate);
        }
        if (query.EndDate != default)
        {
            var endDate = query.EndDate.Date.AddDays(1);
            queryable = queryable.Where(d => d.ProdDate < endDate);
        }

        if (!string.IsNullOrWhiteSpace(query.Shift))
        {
            queryable = queryable.Where(d => d.Shift == query.Shift);
        }

        if (!string.IsNullOrWhiteSpace(query.ShiftNo))
        {
            queryable = queryable.Where(d => d.ShiftNo.Contains(query.ShiftNo));
        }

        if (!string.IsNullOrWhiteSpace(query.ProductSpecCode))
        {
            queryable = queryable.Where(d => d.ProductSpecCode == query.ProductSpecCode);
        }

        if (query.LineNo.HasValue)
        {
            queryable = queryable.Where(d => d.LineNo == query.LineNo);
        }

        return queryable;
    }

    /// <summary>
    /// 获取所有需要统计的等级（FormulaId=Labeling, IsStatistic=true）
    /// </summary>
    private async Task<List<IntermediateDataJudgmentLevelEntity>> GetStatisticLevelsAsync()
    {
        return await _judgmentLevelRepository.AsQueryable()
            .Where(l => l.DeleteMark == null)
            .Where(l => l.FormulaId == "Labeling")
            .Where(l => l.IsStatistic == true)
            .OrderBy(l => l.Priority)
            .ToListAsync();
    }

    /// <summary>
    /// 获取判定等级映射（按Name索引）
    /// </summary>
    private async Task<Dictionary<string, IntermediateDataJudgmentLevelEntity>> GetJudgmentLevelMapAsync()
    {
        var levels = await _judgmentLevelRepository.AsQueryable()
            .Where(l => l.DeleteMark == null)
            .Where(l => l.FormulaId == "Labeling")
            .ToListAsync();

        return levels
            .GroupBy(l => l.Name)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(l => l.Priority).First());
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

    /// <summary>
    /// 创建明细行 DTO
    /// </summary>
    private MonthlyQualityReportDetailDto CreateDetailDto(
        DateTime? prodDate,
        string shift,
        string shiftNo,
        string productSpecCode,
        List<IntermediateDataEntity> items,
        List<IntermediateDataJudgmentLevelEntity> qualifiedLevels,
        List<IntermediateDataJudgmentLevelEntity> unqualifiedLevels)
    {
        var totalWeight = items.Sum(d => d.SingleCoilWeight ?? 0);

        // 合格分类统计
        var qualifiedCategories = new Dictionary<string, LevelStatDto>();
        decimal qualifiedWeight = 0;

        foreach (var level in qualifiedLevels)
        {
            var weight = items.Where(d => d.Labeling == level.Name).Sum(d => d.SingleCoilWeight ?? 0);
            qualifiedWeight += weight;
            qualifiedCategories[level.Name] = new LevelStatDto
            {
                Weight = weight,
                Rate = totalWeight > 0 ? Math.Round(weight / totalWeight * 100, 2) : 0
            };
        }

        // 不合格分类统计
        var unqualifiedCategories = new Dictionary<string, decimal>();
        decimal unqualifiedWeight = 0;
        foreach (var level in unqualifiedLevels)
        {
            var weight = items.Where(d => d.Labeling == level.Name).Sum(d => d.SingleCoilWeight ?? 0);
            unqualifiedWeight += weight;
            unqualifiedCategories[level.Name] = weight;
        }

        // 提取A类和B类数据
        decimal classAWeight = qualifiedCategories.ContainsKey("A") ? qualifiedCategories["A"].Weight : 0;
        decimal classBWeight = qualifiedCategories.ContainsKey("B") ? qualifiedCategories["B"].Weight : 0;
        decimal classARate = totalWeight > 0 ? Math.Round(classAWeight / totalWeight * 100, 2) : 0;
        decimal classBRate = totalWeight > 0 ? Math.Round(classBWeight / totalWeight * 100, 2) : 0;

        return new MonthlyQualityReportDetailDto
        {
            ProdDate = prodDate,
            Shift = shift,
            ShiftNo = shiftNo,
            ProductSpecCode = productSpecCode,
            DetectionWeight = totalWeight,
            QualifiedCategories = qualifiedCategories,
            QualifiedWeight = qualifiedWeight,
            QualifiedRate = totalWeight > 0 ? Math.Round(qualifiedWeight / totalWeight * 100, 2) : 0,
            UnqualifiedCategories = unqualifiedCategories,
            UnqualifiedWeight = unqualifiedWeight,
            ClassAWeight = classAWeight,
            ClassARate = classARate,
            ClassBWeight = classBWeight,
            ClassBRate = classBRate
        };
    }

    /// <summary>
    /// 创建汇总行 DTO（班次小计/月度合计）
    /// </summary>
    private MonthlyQualityReportDetailDto CreateSubtotalDto(
        string shift,
        string summaryType,
        List<IntermediateDataEntity> items,
        List<IntermediateDataJudgmentLevelEntity> qualifiedLevels,
        List<IntermediateDataJudgmentLevelEntity> unqualifiedLevels)
    {
        var totalWeight = items.Sum(d => d.SingleCoilWeight ?? 0);

        // 合格分类统计
        var qualifiedCategories = new Dictionary<string, LevelStatDto>();
        decimal qualifiedWeight = 0;

        foreach (var level in qualifiedLevels)
        {
            var weight = items.Where(d => d.Labeling == level.Name).Sum(d => d.SingleCoilWeight ?? 0);
            qualifiedWeight += weight;
            qualifiedCategories[level.Name] = new LevelStatDto
            {
                Weight = weight,
                Rate = totalWeight > 0 ? Math.Round(weight / totalWeight * 100, 2) : 0
            };
        }

        // 不合格分类统计
        var unqualifiedCategories = new Dictionary<string, decimal>();
        decimal unqualifiedWeight = 0;
        foreach (var level in unqualifiedLevels)
        {
            var weight = items.Where(d => d.Labeling == level.Name).Sum(d => d.SingleCoilWeight ?? 0);
            unqualifiedWeight += weight;
            unqualifiedCategories[level.Name] = weight;
        }

        // 提取A类和B类数据
        decimal classAWeight = qualifiedCategories.ContainsKey("A") ? qualifiedCategories["A"].Weight : 0;
        decimal classBWeight = qualifiedCategories.ContainsKey("B") ? qualifiedCategories["B"].Weight : 0;
        decimal classARate = totalWeight > 0 ? Math.Round(classAWeight / totalWeight * 100, 2) : 0;
        decimal classBRate = totalWeight > 0 ? Math.Round(classBWeight / totalWeight * 100, 2) : 0;

        return new MonthlyQualityReportDetailDto
        {
            Shift = shift,
            ProductSpecCode = summaryType == "MonthlyTotal" ? "" : "小计",
            DetectionWeight = totalWeight,
            QualifiedCategories = qualifiedCategories,
            QualifiedWeight = qualifiedWeight,
            QualifiedRate = totalWeight > 0 ? Math.Round(qualifiedWeight / totalWeight * 100, 2) : 0,
            UnqualifiedCategories = unqualifiedCategories,
            UnqualifiedWeight = unqualifiedWeight,
            ClassAWeight = classAWeight,
            ClassARate = classARate,
            ClassBWeight = classBWeight,
            ClassBRate = classBRate,
            IsSummaryRow = true,
            SummaryType = summaryType
        };
    }

    /// <summary>
    /// 创建班组统计 DTO
    /// </summary>
    private MonthlyQualityReportShiftGroupDto CreateShiftGroupDto(
        string shift,
        string productSpecCode,
        List<IntermediateDataEntity> items,
        List<IntermediateDataJudgmentLevelEntity> qualifiedLevels,
        bool isSummaryRow,
        string summaryType)
    {
        var totalWeight = items.Sum(d => d.SingleCoilWeight ?? 0);

        // 合格分类统计
        var qualifiedCategories = new Dictionary<string, LevelStatDto>();
        decimal qualifiedWeight = 0;

        foreach (var level in qualifiedLevels)
        {
            var weight = items.Where(d => d.Labeling == level.Name).Sum(d => d.SingleCoilWeight ?? 0);
            qualifiedWeight += weight;
            qualifiedCategories[level.Name] = new LevelStatDto
            {
                Weight = weight,
                Rate = totalWeight > 0 ? Math.Round(weight / totalWeight * 100, 2) : 0
            };
        }

        // 提取A类和B类数据
        decimal classAWeight = qualifiedCategories.ContainsKey("A") ? qualifiedCategories["A"].Weight : 0;
        decimal classBWeight = qualifiedCategories.ContainsKey("B") ? qualifiedCategories["B"].Weight : 0;
        decimal classARate = totalWeight > 0 ? Math.Round(classAWeight / totalWeight * 100, 2) : 0;
        decimal classBRate = totalWeight > 0 ? Math.Round(classBWeight / totalWeight * 100, 2) : 0;

        // 计算不合格重量
        decimal unqualifiedWeight = totalWeight - qualifiedWeight;

        return new MonthlyQualityReportShiftGroupDto
        {
            Shift = shift,
            ProductSpecCode = productSpecCode,
            DetectionWeight = totalWeight,
            QualifiedCategories = qualifiedCategories,
            QualifiedWeight = qualifiedWeight,
            QualifiedRate = totalWeight > 0 ? Math.Round(qualifiedWeight / totalWeight * 100, 2) : 0,
            UnqualifiedWeight = unqualifiedWeight,
            ClassAWeight = classAWeight,
            ClassARate = classARate,
            ClassBWeight = classBWeight,
            ClassBRate = classBRate,
            IsSummaryRow = isSummaryRow,
            SummaryType = summaryType
        };
    }

    #endregion

    /// <inheritdoc/>
    [HttpGet("columns")]
    public async Task<MonthlyQualityReportColumnsDto> GetColumnsAsync()
    {
        var statisticLevels = await GetStatisticLevelsAsync();
        var qualifiedLevels = statisticLevels.Where(l => l.QualityStatus == QualityStatusEnum.Qualified).ToList();
        var unqualifiedLevels = statisticLevels.Where(l => l.QualityStatus == QualityStatusEnum.Unqualified).ToList();

        return new MonthlyQualityReportColumnsDto
        {
            QualifiedColumns = qualifiedLevels.Select(l => new JudgmentLevelColumnDto
            {
                Id = l.Id,
                Code = l.Code,
                Name = l.Name,
                QualityStatus = l.QualityStatus,
                Color = l.Color,
                Priority = l.Priority
            }).ToList(),
            UnqualifiedColumns = unqualifiedLevels.Select(l => new JudgmentLevelColumnDto
            {
                Id = l.Id,
                Code = l.Code,
                Name = l.Name,
                QualityStatus = l.QualityStatus,
                Color = l.Color,
                Priority = l.Priority
            }).ToList()
        };
    }

    /// <summary>
    /// 导出月度质量报表Excel
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportExcelAsync([FromQuery] MonthlyQualityReportQueryDto query)
    {
        var statisticLevels = await GetStatisticLevelsAsync();
        var qualifiedLevels = statisticLevels.Where(l => l.QualityStatus == QualityStatusEnum.Qualified)
            .OrderBy(l => l.Priority).ToList();
        var unqualifiedLevels = statisticLevels.Where(l => l.QualityStatus == QualityStatusEnum.Unqualified)
            .OrderBy(l => l.Priority).ToList();

        // 初始化列宽追踪器
        _exportColumnWidths = new Dictionary<int, int>();

        var details = await GetDetailsAsync(query);
        var shiftGroups = await GetShiftGroupsAsync(query);

        var memoryStream = new MemoryStream();
        var workbook = new XSSFWorkbook();
        var sheet = workbook.CreateSheet("月度质量报表");

        var titleStyle = CreateTitleStyle(workbook);
        var headerStyle = CreateHeaderStyle(workbook);
        var dataStyle = CreateDataStyle(workbook);
        var summaryStyle = CreateSummaryStyle(workbook);

        int rowIndex = 0;

        // 标题行
        var titleRow = sheet.CreateRow(rowIndex++);
        titleRow.HeightInPoints = 24;
        var titleCell = titleRow.CreateCell(0);
        titleCell.SetCellValue("月度质量统计报表");
        titleCell.CellStyle = titleStyle;
        sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 0, GetColumnCount(qualifiedLevels, unqualifiedLevels) - 1));

        // 日期范围行
        var dateRow = sheet.CreateRow(rowIndex++);
        var dateCell = dateRow.CreateCell(0);
        dateCell.SetCellValue($"统计时间：{query.StartDate:yyyy年MM月dd日} - {query.EndDate:yyyy年MM月dd日}");
        dateCell.CellStyle = dataStyle;
        sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(1, 1, 0, GetColumnCount(qualifiedLevels, unqualifiedLevels) - 1));

        // 空行
        sheet.CreateRow(rowIndex++);

        // ===== 质量检测明细 =====
        var sectionHeaderRow = sheet.CreateRow(rowIndex++);
        var sectionHeaderCell = sectionHeaderRow.CreateCell(0);
        sectionHeaderCell.SetCellValue("质量检测明细");
        sectionHeaderCell.CellStyle = titleStyle;
        sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(rowIndex - 1, rowIndex - 1, 0, GetColumnCount(qualifiedLevels, unqualifiedLevels) - 1));

        // 列头
        var headerRow = sheet.CreateRow(rowIndex++);
        headerRow.HeightInPoints = 20;
        int colIndex = 0;
        CreateHeaderCell(headerRow, colIndex++, "生产日期", headerStyle);
        CreateHeaderCell(headerRow, colIndex++, "班次", headerStyle);
        CreateHeaderCell(headerRow, colIndex++, "班次号", headerStyle);
        CreateHeaderCell(headerRow, colIndex++, "产品规格", headerStyle);
        CreateHeaderCell(headerRow, colIndex++, "检验总重", headerStyle);

        foreach (var level in qualifiedLevels)
        {
            CreateHeaderCell(headerRow, colIndex++, $"{level.Name}类", headerStyle);
            CreateHeaderCell(headerRow, colIndex++, "占比(%)", headerStyle);
        }

        CreateHeaderCell(headerRow, colIndex++, "合格总重", headerStyle);
        CreateHeaderCell(headerRow, colIndex++, "合格率(%)", headerStyle);

        foreach (var level in unqualifiedLevels)
        {
            CreateHeaderCell(headerRow, colIndex++, $"{level.Name}", headerStyle);
        }

        // 明细数据
        foreach (var detail in details)
        {
            var dataRow = sheet.CreateRow(rowIndex++);
            colIndex = 0;
            CreateCell(dataRow, colIndex++, detail.ProdDate?.ToString("yyyyMMdd") ?? "", dataStyle);
            CreateCell(dataRow, colIndex++, detail.Shift ?? "", dataStyle);
            CreateCell(dataRow, colIndex++, detail.ShiftNo ?? "", dataStyle);
            CreateCell(dataRow, colIndex++, detail.ProductSpecCode ?? "", dataStyle);
            CreateCell(dataRow, colIndex++, detail.DetectionWeight, dataStyle);

            foreach (var level in qualifiedLevels)
            {
                var stat = detail.QualifiedCategories?.GetValueOrDefault(level.Name);
                CreateCell(dataRow, colIndex++, stat?.Weight ?? 0, dataStyle);
                CreateCell(dataRow, colIndex++, stat?.Rate ?? 0, dataStyle);
            }

            CreateCell(dataRow, colIndex++, detail.QualifiedWeight, dataStyle);
            CreateCell(dataRow, colIndex++, detail.QualifiedRate, dataStyle);

            foreach (var level in unqualifiedLevels)
            {
                var weight = detail.UnqualifiedCategories?.GetValueOrDefault(level.Name) ?? 0;
                CreateCell(dataRow, colIndex++, weight, dataStyle);
            }

            // 合计行样式
            if (detail.IsSummaryRow)
            {
                foreach (var cell in dataRow.Cells)
                {
                    cell.CellStyle = summaryStyle;
                }
            }
        }

        // 空行分隔
        sheet.CreateRow(rowIndex++);

        // ===== 班组统计 =====
        int shiftOffset = 2;
        var shiftSectionRow = sheet.CreateRow(rowIndex++);
        var shiftSectionCell = shiftSectionRow.CreateCell(shiftOffset); // Title starts at offset
        shiftSectionCell.SetCellValue("班组统计");
        shiftSectionCell.CellStyle = titleStyle;
        sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(rowIndex - 1, rowIndex - 1, shiftOffset, shiftOffset + qualifiedLevels.Count * 2 + 4));

        // 班组列头
        var shiftHeaderRow = sheet.CreateRow(rowIndex++);
        colIndex = shiftOffset; // Header starts at offset
        CreateHeaderCell(shiftHeaderRow, colIndex++, "班次", headerStyle);
        CreateHeaderCell(shiftHeaderRow, colIndex++, "产品规格", headerStyle);
        CreateHeaderCell(shiftHeaderRow, colIndex++, "检验总重", headerStyle);

        foreach (var level in qualifiedLevels)
        {
            CreateHeaderCell(shiftHeaderRow, colIndex++, $"{level.Name}类", headerStyle);
            CreateHeaderCell(shiftHeaderRow, colIndex++, "占比(%)", headerStyle);
        }

        CreateHeaderCell(shiftHeaderRow, colIndex++, "合格总重", headerStyle);
        CreateHeaderCell(shiftHeaderRow, colIndex++, "合格率(%)", headerStyle);

        // 班组数据
        foreach (var shift in shiftGroups)
        {
            var dataRow = sheet.CreateRow(rowIndex++);
            colIndex = shiftOffset; // Data starts at offset
            CreateCell(dataRow, colIndex++, shift.Shift ?? "", dataStyle);
            CreateCell(dataRow, colIndex++, shift.ProductSpecCode ?? "", dataStyle);
            CreateCell(dataRow, colIndex++, shift.DetectionWeight, dataStyle);

            foreach (var level in qualifiedLevels)
            {
                var stat = shift.QualifiedCategories?.GetValueOrDefault(level.Name);
                CreateCell(dataRow, colIndex++, stat?.Weight ?? 0, dataStyle);
                CreateCell(dataRow, colIndex++, stat?.Rate ?? 0, dataStyle);
            }

            CreateCell(dataRow, colIndex++, shift.QualifiedWeight, dataStyle);
            CreateCell(dataRow, colIndex++, shift.QualifiedRate, dataStyle);

            // 合计/小计行样式
            if (shift.IsSummaryRow)
            {
                foreach (var cell in dataRow.Cells)
                {
                    cell.CellStyle = summaryStyle;
                }
            }
        }

        // 设置列宽 (手动计算)
        foreach (var kvp in _exportColumnWidths)
        {
            // 基准宽度: (中文字符数 * 2 + 英文字符数) * 256
            // 增加缓冲: +2 字符
            int width = (kvp.Value + 2) * 256;
            // 限制最大宽度 (例如 60 个字符)
            int maxWidth = 60 * 256;
            if (width > maxWidth) width = maxWidth;
            
            sheet.SetColumnWidth(kvp.Key, width);
        }

        workbook.Write(memoryStream);
        var newStream = new MemoryStream(memoryStream.ToArray());

        var fileName = $"{DateTime.Now:yyyyMMdd}_{Poxiao.Infrastructure.Security.SnowflakeIdHelper.NextId()}.xlsx";
        return new FileStreamResult(newStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = fileName
        };
    }

    private int GetColumnCount(List<IntermediateDataJudgmentLevelEntity> qualifiedLevels,
        List<IntermediateDataJudgmentLevelEntity> unqualifiedLevels)
    {
        // 基础列: 生产日期, 班次, 班次号, 产品规格, 检验总重, 合格总重, 合格率
        int baseColumns = 7;
        return baseColumns + qualifiedLevels.Count * 2 + unqualifiedLevels.Count;
    }

    private ICellStyle CreateTitleStyle(XSSFWorkbook workbook)
    {
        var style = workbook.CreateCellStyle();
        style.Alignment = HorizontalAlignment.Center;
        style.VerticalAlignment = VerticalAlignment.Center;
        style.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
        style.FillPattern = FillPattern.SolidForeground;
        style.SetFont(CreateFont(workbook, 14, true));
        SetBorder(style);
        return style;
    }

    private ICellStyle CreateHeaderStyle(XSSFWorkbook workbook)
    {
        var style = workbook.CreateCellStyle();
        style.Alignment = HorizontalAlignment.Center;
        style.VerticalAlignment = VerticalAlignment.Center;
        style.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index;
        style.FillPattern = FillPattern.SolidForeground;
        style.SetFont(CreateFont(workbook, 11, true));
        SetBorder(style);
        return style;
    }

    private ICellStyle CreateDataStyle(XSSFWorkbook workbook)
    {
        var style = workbook.CreateCellStyle();
        style.Alignment = HorizontalAlignment.Center;
        style.VerticalAlignment = VerticalAlignment.Center;
        style.SetFont(CreateFont(workbook, 10, false));
        SetBorder(style);
        return style;
    }

    private ICellStyle CreateSummaryStyle(XSSFWorkbook workbook)
    {
        var style = workbook.CreateCellStyle();
        style.Alignment = HorizontalAlignment.Center;
        style.VerticalAlignment = VerticalAlignment.Center;
        style.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightBlue.Index;
        style.FillPattern = FillPattern.SolidForeground;
        style.SetFont(CreateFont(workbook, 10, true));
        SetBorder(style);
        return style;
    }

    private IFont CreateFont(XSSFWorkbook workbook, int fontSize, bool bold)
    {
        var font = workbook.CreateFont();
        font.FontHeightInPoints = fontSize;
        font.IsBold = bold;
        return font;
    }

    private void SetBorder(ICellStyle style)
    {
        style.BorderTop = BorderStyle.Thin;
        style.BorderBottom = BorderStyle.Thin;
        style.BorderLeft = BorderStyle.Thin;
        style.BorderRight = BorderStyle.Thin;
    }

    private void CreateHeaderCell(IRow row, int colIndex, string value, ICellStyle style)
    {
        var cell = row.CreateCell(colIndex);
        cell.SetCellValue(value);
        cell.CellStyle = style;

        TrackColumnWidth(colIndex, value);
    }

    private void CreateCell(IRow row, int colIndex, string value, ICellStyle style)
    {
        var cell = row.CreateCell(colIndex);
        cell.SetCellValue(value);
        cell.CellStyle = style;

        TrackColumnWidth(colIndex, value);
    }

    private void CreateCell(IRow row, int colIndex, decimal value, ICellStyle style)
    {
        var cell = row.CreateCell(colIndex);
        cell.SetCellValue((double)value);
        cell.CellStyle = style;

        TrackColumnWidth(colIndex, value.ToString());
    }

    private void TrackColumnWidth(int colIndex, string value)
    {
        if (_exportColumnWidths != null)
        {
            int len = GetTextWidth(value);
            if (!_exportColumnWidths.ContainsKey(colIndex) || len > _exportColumnWidths[colIndex])
            {
                _exportColumnWidths[colIndex] = len;
            }
        }
    }

    private int GetTextWidth(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        int width = 0;
        foreach (var c in text)
        {
            // 中文/全角字符宽度为2，其他为1
            width += c > 255 ? 2 : 1;
        }
        return width;
    }
}