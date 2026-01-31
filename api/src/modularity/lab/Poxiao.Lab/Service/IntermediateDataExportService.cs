using System.Data;
using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.IntermediateData;
using Poxiao.Lab.Entity.Enums;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// 中间数据导出服务.
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "intermediate-data-export", Order = 200)]
[Route("api/lab/intermediate-data")]
public partial class IntermediateDataExportService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<IntermediateDataEntity> _repository;
    private readonly ISqlSugarRepository<ProductSpecEntity> _productSpecRepository;
    private readonly ISqlSugarRepository<AppearanceFeatureCategoryEntity> _categoryRepository;
    private readonly IUserManager _userManager;

    public IntermediateDataExportService(
        ISqlSugarRepository<IntermediateDataEntity> repository,
        ISqlSugarRepository<ProductSpecEntity> productSpecRepository,
        ISqlSugarRepository<AppearanceFeatureCategoryEntity> categoryRepository,
        IUserManager userManager)
    {
        _repository = repository;
        _productSpecRepository = productSpecRepository;
        _categoryRepository = categoryRepository;
        _userManager = userManager;
    }

    /// <summary>
    /// 导出中间数据Excel（按月+产品规格分组）.
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportIntermediateData([FromQuery] IntermediateDataExportInput input)
    {
        // 1. 验证日期范围
        if (!input.StartDate.HasValue || !input.EndDate.HasValue)
        {
            throw Oops.Bah("请选择生产日期范围");
        }

        var startDate = input.StartDate.Value;
        var endDate = input.EndDate.Value;
        
        // 验证最多一年
        if ((endDate - startDate).TotalDays > 366)
        {
            throw Oops.Bah("导出时间范围不能超过一年");
        }

        // 2. 获取所有产品规格
        var productSpecs = await _productSpecRepository.GetListAsync(
            ps => ps.DeleteMark == null || ps.DeleteMark == 0
        );
        
        if (productSpecs.Count == 0)
        {
            throw Oops.Bah("没有可用的产品规格");
        }

        // 3. 获取所有特性大类（用于外观特性列）
        var categories = await _categoryRepository.GetListAsync(
            c => (c.DeleteMark == null || c.DeleteMark == 0) &&
                 (c.ParentId == null || c.ParentId == "-1")
        );
        var categoryNames = categories.OrderBy(c => c.SortCode ?? 0).Select(c => c.Name).ToList();

        // 4. 获取数据
        var data = await _repository.AsQueryable()
            .Where(t => t.DeleteMark == null)
            .Where(t => t.ProdDate >= startDate && t.ProdDate < endDate.AddDays(1))
            .Where(t => !string.IsNullOrEmpty(t.ProductSpecId))
            .ToListAsync();

        if (data.Count == 0)
        {
            throw Oops.Bah("指定时间范围内没有数据");
        }

        // 5. 按月份+产品规格分组
        var groupedData = data
            .GroupBy(d => new
            {
                Month = d.ProdDate?.Month ?? 0,
                Year = d.ProdDate?.Year ?? 0,
                ProductSpecId = d.ProductSpecId,
                ProductSpecCode = d.ProductSpecCode ?? "未知",
                ProductSpecName = d.ProductSpecName ?? "未知规格"
            })
            .Select(g =>
            {
                var spec = productSpecs.FirstOrDefault(ps => ps.Id == g.Key.ProductSpecId);
                var detectionColumns = spec?.DetectionColumns ?? 15;
                
                return new IntermediateDataExportResult
                {
                    MonthName = $"{g.Key.Month}月",
                    MonthOrder = g.Key.Month,
                    ProductSpecCode = g.Key.ProductSpecCode,
                    ProductSpecName = g.Key.ProductSpecName,
                    DetectionColumns = detectionColumns,
                    DataList = g.Select(d => ConvertToExportItem(d, detectionColumns, categoryNames)).ToList()
                };
            })
            .OrderBy(r => r.MonthOrder)
            .ThenBy(r => r.ProductSpecCode)
            .ToList();

        // 6. 生成Excel
        var memoryStream = new MemoryStream();
        
        // 使用 MiniExcel 的 SaveAs 多 Sheet 功能
        var sheets = new Dictionary<string, object>();
        
        foreach (var group in groupedData)
        {
            // 构建 DataTable（动态列根据 detectionColumns 确定）
            var dataTable = BuildDataTable(group, categoryNames);
            sheets[group.SheetName] = dataTable;
        }

        // 保存到内存流
        memoryStream.SaveAs(sheets);
        memoryStream.Position = 0;

        // 7. 返回文件
        var fileName = $"中间数据导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        return new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = fileName
        };
    }

    /// <summary>
    /// 将实体转换为导出项.
    /// </summary>
    private IntermediateDataExportItem ConvertToExportItem(
        IntermediateDataEntity entity, 
        int detectionColumns,
        List<string> categoryNames)
    {
        var item = new IntermediateDataExportItem
        {
            // 基础信息
            检验日期 = entity.DetectionDate?.ToString("yyyy-MM-dd"),
            喷次 = entity.SprayNo,
            贴标 = entity.Labeling,
            炉号 = entity.FurnaceNoFormatted,

            // 性能数据
            Ss激磁功率 = entity.PerfSsPower,
            Ps铁损 = entity.PerfPsLoss,
            Hc = entity.PerfHc,
            刻痕后Ss激磁功率 = entity.PerfAfterSsPower,
            刻痕后Ps铁损 = entity.PerfAfterPsLoss,
            刻痕后Hc = entity.PerfAfterHc,
            性能录入员 = entity.PerfEditorName,

            // 物理特性
            一米带重 = entity.OneMeterWeight,
            带宽 = entity.Width,

            // 带厚1-22
            带厚1 = entity.Thickness1,
            带厚2 = entity.Thickness2,
            带厚3 = entity.Thickness3,
            带厚4 = entity.Thickness4,
            带厚5 = entity.Thickness5,
            带厚6 = entity.Thickness6,
            带厚7 = entity.Thickness7,
            带厚8 = entity.Thickness8,
            带厚9 = entity.Thickness9,
            带厚10 = entity.Thickness10,
            带厚11 = entity.Thickness11,
            带厚12 = entity.Thickness12,
            带厚13 = entity.Thickness13,
            带厚14 = entity.Thickness14,
            带厚15 = entity.Thickness15,
            带厚16 = entity.Thickness16,
            带厚17 = entity.Thickness17,
            带厚18 = entity.Thickness18,
            带厚19 = entity.Thickness19,
            带厚20 = entity.Thickness20,
            带厚21 = entity.Thickness21,
            带厚22 = entity.Thickness22,

            // 带厚范围
            带厚最小值 = entity.ThicknessMin,
            带厚最大值 = entity.ThicknessMax,

            // 汇总特性
            带厚极差 = entity.ThicknessDiff,
            密度 = entity.Density,
            叠片系数 = entity.LaminationFactor,

            // 固定外观字段
            断头数 = entity.BreakCount,
            单卷重量 = entity.SingleCoilWeight,
            外观检验员 = entity.AppearEditorName,

            // 判定结果
            平均厚度 = entity.AvgThickness,
            磁性能判定 = entity.MagneticResult,
            厚度判定 = entity.ThicknessResult,
            叠片系数判定 = entity.LaminationResult,

            // 花纹数据
            中Si左 = entity.MidSiLeft,
            中Si右 = entity.MidSiRight,
            中B左 = entity.MidBLeft,
            中B右 = entity.MidBRight,
            左花纹纹宽 = entity.LeftPatternWidth,
            左花纹纹距 = entity.LeftPatternSpacing,
            中花纹纹宽 = entity.MidPatternWidth,
            中花纹纹距 = entity.MidPatternSpacing,
            右花纹纹宽 = entity.RightPatternWidth,
            右花纹纹距 = entity.RightPatternSpacing,

            // 其他
            四米带重 = entity.CoilWeight,

            // 叠片系数厚度分布1-22
            叠片系数厚度分布1 = entity.Detection1,
            叠片系数厚度分布2 = entity.Detection2,
            叠片系数厚度分布3 = entity.Detection3,
            叠片系数厚度分布4 = entity.Detection4,
            叠片系数厚度分布5 = entity.Detection5,
            叠片系数厚度分布6 = entity.Detection6,
            叠片系数厚度分布7 = entity.Detection7,
            叠片系数厚度分布8 = entity.Detection8,
            叠片系数厚度分布9 = entity.Detection9,
            叠片系数厚度分布10 = entity.Detection10,
            叠片系数厚度分布11 = entity.Detection11,
            叠片系数厚度分布12 = entity.Detection12,
            叠片系数厚度分布13 = entity.Detection13,
            叠片系数厚度分布14 = entity.Detection14,
            叠片系数厚度分布15 = entity.Detection15,
            叠片系数厚度分布16 = entity.Detection16,
            叠片系数厚度分布17 = entity.Detection17,
            叠片系数厚度分布18 = entity.Detection18,
            叠片系数厚度分布19 = entity.Detection19,
            叠片系数厚度分布20 = entity.Detection20,
            叠片系数厚度分布21 = entity.Detection21,
            叠片系数厚度分布22 = entity.Detection22,

            // 扩展数据
            最大厚度 = entity.MaxThicknessRaw,
            最大平均厚度 = entity.MaxAvgThickness,
            录入人 = entity.CreatorUserId,
            带型 = entity.StripType,
            一次交检 = entity.FirstInspection,
            班次 = entity.ShiftNo,
            计算状态 = GetCalcStatusText(entity.CalcStatus)
        };

        // 处理外观特性大类（动态列）
        if (!string.IsNullOrEmpty(entity.AppearanceFeatureCategoryIds))
        {
            try
            {
                var categoryIds = System.Text.Json.JsonSerializer.Deserialize<List<string>>(
                    entity.AppearanceFeatureCategoryIds);
                if (categoryIds != null)
                {
                    foreach (var categoryId in categoryIds)
                    {
                        var category = _categoryRepository.GetFirstAsync(
                            c => c.Id == categoryId
                        ).Result;
                        if (category != null)
                        {
                            item.外观特性[category.Name] = "✓";
                        }
                    }
                }
            }
            catch
            {
                // 忽略解析错误
            }
        }

        return item;
    }

    /// <summary>
    /// 构建 DataTable（根据 detectionColumns 动态调整列）.
    /// </summary>
    private DataTable BuildDataTable(IntermediateDataExportResult group, List<string> categoryNames)
    {
        var dataTable = new DataTable();
        
        // 添加固定列
        dataTable.Columns.Add("检验日期", typeof(string));
        dataTable.Columns.Add("喷次", typeof(string));
        dataTable.Columns.Add("贴标", typeof(string));
        dataTable.Columns.Add("炉号", typeof(string));
        
        // 性能数据
        dataTable.Columns.Add("Ss激磁功率", typeof(decimal));
        dataTable.Columns.Add("Ps铁损", typeof(decimal));
        dataTable.Columns.Add("Hc", typeof(decimal));
        dataTable.Columns.Add("刻痕后Ss激磁功率", typeof(decimal));
        dataTable.Columns.Add("刻痕后Ps铁损", typeof(decimal));
        dataTable.Columns.Add("刻痕后Hc", typeof(decimal));
        dataTable.Columns.Add("性能录入员", typeof(string));
        
        // 物理特性
        dataTable.Columns.Add("一米带重", typeof(decimal));
        dataTable.Columns.Add("带宽", typeof(decimal));
        
        // 带厚（动态1-detectionColumns）
        for (int i = 1; i <= group.DetectionColumns; i++)
        {
            dataTable.Columns.Add($"带厚{i}", typeof(decimal));
        }
        
        // 带厚范围
        dataTable.Columns.Add("带厚最小值", typeof(decimal));
        dataTable.Columns.Add("带厚最大值", typeof(decimal));
        
        // 汇总特性
        dataTable.Columns.Add("带厚极差", typeof(decimal));
        dataTable.Columns.Add("密度", typeof(decimal));
        dataTable.Columns.Add("叠片系数", typeof(decimal));
        
        // 外观特性大类（动态列）
        foreach (var categoryName in categoryNames)
        {
            dataTable.Columns.Add(categoryName, typeof(string));
        }
        
        // 固定外观字段
        dataTable.Columns.Add("断头数", typeof(int));
        dataTable.Columns.Add("单卷重量", typeof(decimal));
        dataTable.Columns.Add("外观检验员", typeof(string));
        
        // 判定结果
        dataTable.Columns.Add("平均厚度", typeof(decimal));
        dataTable.Columns.Add("磁性能判定", typeof(string));
        dataTable.Columns.Add("厚度判定", typeof(string));
        dataTable.Columns.Add("叠片系数判定", typeof(string));
        
        // 花纹数据
        dataTable.Columns.Add("中Si左", typeof(string));
        dataTable.Columns.Add("中Si右", typeof(string));
        dataTable.Columns.Add("中B左", typeof(string));
        dataTable.Columns.Add("中B右", typeof(string));
        dataTable.Columns.Add("左花纹纹宽", typeof(decimal));
        dataTable.Columns.Add("左花纹纹距", typeof(decimal));
        dataTable.Columns.Add("中花纹纹宽", typeof(decimal));
        dataTable.Columns.Add("中花纹纹距", typeof(decimal));
        dataTable.Columns.Add("右花纹纹宽", typeof(decimal));
        dataTable.Columns.Add("右花纹纹距", typeof(decimal));
        
        // 其他
        dataTable.Columns.Add("四米带重", typeof(decimal));
        
        // 叠片系数厚度分布（动态1-detectionColumns）
        for (int i = 1; i <= group.DetectionColumns; i++)
        {
            dataTable.Columns.Add($"叠片系数厚度分布{i}", typeof(decimal));
        }
        
        // 扩展数据
        dataTable.Columns.Add("最大厚度", typeof(decimal));
        dataTable.Columns.Add("最大平均厚度", typeof(decimal));
        dataTable.Columns.Add("录入人", typeof(string));
        dataTable.Columns.Add("带型", typeof(decimal));
        dataTable.Columns.Add("一次交检", typeof(string));
        dataTable.Columns.Add("班次", typeof(string));
        dataTable.Columns.Add("计算状态", typeof(string));

        // 填充数据
        foreach (var item in group.DataList)
        {
            var row = dataTable.NewRow();
            
            // 固定列
            row["检验日期"] = item.检验日期 ?? (object)DBNull.Value;
            row["喷次"] = item.喷次 ?? (object)DBNull.Value;
            row["贴标"] = item.贴标 ?? (object)DBNull.Value;
            row["炉号"] = item.炉号 ?? (object)DBNull.Value;
            
            // 性能数据
            row["Ss激磁功率"] = item.Ss激磁功率 ?? (object)DBNull.Value;
            row["Ps铁损"] = item.Ps铁损 ?? (object)DBNull.Value;
            row["Hc"] = item.Hc ?? (object)DBNull.Value;
            row["刻痕后Ss激磁功率"] = item.刻痕后Ss激磁功率 ?? (object)DBNull.Value;
            row["刻痕后Ps铁损"] = item.刻痕后Ps铁损 ?? (object)DBNull.Value;
            row["刻痕后Hc"] = item.刻痕后Hc ?? (object)DBNull.Value;
            row["性能录入员"] = item.性能录入员 ?? (object)DBNull.Value;
            
            // 物理特性
            row["一米带重"] = item.一米带重 ?? (object)DBNull.Value;
            row["带宽"] = item.带宽 ?? (object)DBNull.Value;
            
            // 带厚（动态）
            for (int i = 1; i <= group.DetectionColumns; i++)
            {
                var value = item.GetType().GetProperty($"带厚{i}")?.GetValue(item) as decimal?;
                row[$"带厚{i}"] = value ?? (object)DBNull.Value;
            }
            
            // 带厚范围
            row["带厚最小值"] = item.带厚最小值 ?? (object)DBNull.Value;
            row["带厚最大值"] = item.带厚最大值 ?? (object)DBNull.Value;
            
            // 汇总特性
            row["带厚极差"] = item.带厚极差 ?? (object)DBNull.Value;
            row["密度"] = item.密度 ?? (object)DBNull.Value;
            row["叠片系数"] = item.叠片系数 ?? (object)DBNull.Value;
            
            // 外观特性大类（动态）
            foreach (var categoryName in categoryNames)
            {
                row[categoryName] = item.外观特性.ContainsKey(categoryName) 
                    ? item.外观特性[categoryName] 
                    : (object)DBNull.Value;
            }
            
            // 固定外观字段
            row["断头数"] = item.断头数 ?? (object)DBNull.Value;
            row["单卷重量"] = item.单卷重量 ?? (object)DBNull.Value;
            row["外观检验员"] = item.外观检验员 ?? (object)DBNull.Value;
            
            // 判定结果
            row["平均厚度"] = item.平均厚度 ?? (object)DBNull.Value;
            row["磁性能判定"] = item.磁性能判定 ?? (object)DBNull.Value;
            row["厚度判定"] = item.厚度判定 ?? (object)DBNull.Value;
            row["叠片系数判定"] = item.叠片系数判定 ?? (object)DBNull.Value;
            
            // 花纹数据
            row["中Si左"] = item.中Si左 ?? (object)DBNull.Value;
            row["中Si右"] = item.中Si右 ?? (object)DBNull.Value;
            row["中B左"] = item.中B左 ?? (object)DBNull.Value;
            row["中B右"] = item.中B右 ?? (object)DBNull.Value;
            row["左花纹纹宽"] = item.左花纹纹宽 ?? (object)DBNull.Value;
            row["左花纹纹距"] = item.左花纹纹距 ?? (object)DBNull.Value;
            row["中花纹纹宽"] = item.中花纹纹宽 ?? (object)DBNull.Value;
            row["中花纹纹距"] = item.中花纹纹距 ?? (object)DBNull.Value;
            row["右花纹纹宽"] = item.右花纹纹宽 ?? (object)DBNull.Value;
            row["右花纹纹距"] = item.右花纹纹距 ?? (object)DBNull.Value;
            
            // 其他
            row["四米带重"] = item.四米带重 ?? (object)DBNull.Value;
            
            // 叠片系数厚度分布（动态）
            for (int i = 1; i <= group.DetectionColumns; i++)
            {
                var value = item.GetType().GetProperty($"叠片系数厚度分布{i}")?.GetValue(item) as decimal?;
                row[$"叠片系数厚度分布{i}"] = value ?? (object)DBNull.Value;
            }
            
            // 扩展数据
            row["最大厚度"] = item.最大厚度 ?? (object)DBNull.Value;
            row["最大平均厚度"] = item.最大平均厚度 ?? (object)DBNull.Value;
            row["录入人"] = item.录入人 ?? (object)DBNull.Value;
            row["带型"] = item.带型 ?? (object)DBNull.Value;
            row["一次交检"] = item.一次交检 ?? (object)DBNull.Value;
            row["班次"] = item.班次 ?? (object)DBNull.Value;
            row["计算状态"] = item.计算状态 ?? (object)DBNull.Value;
            
            dataTable.Rows.Add(row);
        }

        return dataTable;
    }

    /// <summary>
    /// 获取计算状态文本.
    /// </summary>
    private string GetCalcStatusText(IntermediateDataCalcStatus status)
    {
        return status switch
        {
            IntermediateDataCalcStatus.PENDING => "未计算",
            IntermediateDataCalcStatus.PROCESSING => "计算中",
            IntermediateDataCalcStatus.SUCCESS => "成功",
            IntermediateDataCalcStatus.FAILED => "失败",
            _ => "未知"
        };
    }
}
