using System.Data;
using System.Text.RegularExpressions;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Filter;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.RawData;
using Poxiao.Lab.Entity.Models;
using Poxiao.Lab.Helpers;
using Poxiao.Lab.Interfaces;
using Poxiao.Systems.Entitys.Permission;
using SqlSugar;

using Poxiao.Lab.Entity.Dto.ProductSpec;
using Poxiao.Lab.Entity.Dto.AppearanceFeature;
using Poxiao.Lab.Entity.Dto.AppearanceFeatureLevel;

namespace Poxiao.Lab.Service;

/// <summary>
/// 原始数据服务.
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "raw-data", Order = 200)]
[Route("api/lab/raw-data")]
public class RawDataService : IRawDataService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<RawDataEntity> _repository;
    private readonly IProductSpecService _productSpecService;
    private readonly ISqlSugarRepository<RawDataImportLogEntity> _logRepository;
    private readonly IUserManager _userManager;
    private readonly IAppearanceFeatureService _appearanceFeatureService;
    private readonly IAppearanceFeatureCategoryService _categoryService;
    private readonly AppearanceFeatureRuleMatcher _featureRuleMatcher;
    private readonly IAppearanceFeatureLevelService _featureLevelService;

    public RawDataService(
        ISqlSugarRepository<RawDataEntity> repository,
        IProductSpecService productSpecService,
        ISqlSugarRepository<RawDataImportLogEntity> logRepository,
        IUserManager userManager,
        IAppearanceFeatureService appearanceFeatureService,
        IAppearanceFeatureCategoryService categoryService,
        AppearanceFeatureRuleMatcher featureRuleMatcher,
        IAppearanceFeatureLevelService featureLevelService
    )
    {
        _repository = repository;
        _productSpecService = productSpecService;
        _logRepository = logRepository;
        _userManager = userManager;
        _appearanceFeatureService = appearanceFeatureService;
        _categoryService = categoryService;
        _featureRuleMatcher = featureRuleMatcher;
        _featureLevelService = featureLevelService;
    }

    /// <inheritdoc />
    [HttpPost("preview")]
    public async Task<RawDataPreviewOutput> Preview([FromBody] RawDataImportInput input)
    {
        var result = new RawDataPreviewOutput();

        try
        {
            // 解码Base64文件
            var fileBytes = Convert.FromBase64String(input.FileData);
            using var stream = new MemoryStream(fileBytes);

            // 读取Excel
            IWorkbook workbook = input.FileName.EndsWith(".xlsx")
                ? new XSSFWorkbook(stream)
                : new HSSFWorkbook(stream);

            var sheet = workbook.GetSheetAt(0);
            var headerRow = sheet.GetRow(0);

            if (headerRow == null)
            {
                throw Oops.Oh("Excel文件格式错误：缺少表头行");
            }

            // 获取表头索引和顺序
            var headerIndexes = GetHeaderIndexes(headerRow);

            // 设置表头顺序（按照Excel中的物理顺序）
            result.HeaderOrder = new List<string>(headerIndexes.Count);
            // 重新按索引排序
            var orderedHeaders = headerIndexes
                .OrderBy(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();
            result.HeaderOrder.AddRange(orderedHeaders);

            // 获取上次导入日志，确定跳过行数
            // 逻辑：查找同名文件的最后一次成功导入日志，获取其TotalRows作为本次跳过的基准
            // 注意：这里假设用户是在追加数据。如果用户是重新导入修改过的文件，可能不应该跳过。
            // 但为了满足"记住上次位置"的需求，默认执行跳过。
            var lastLogList = await _logRepository
                .AsQueryable()
                .Where(t => t.FileName == input.FileName && t.Status == "success")
                .OrderByDescending(t => t.ImportTime)
                .Take(1)
                .ToListAsync();
            var lastLog = lastLogList.FirstOrDefault();

            int skipRows = 0;
            if (lastLog != null)
            {
                skipRows = lastLog.TotalRows;
            }
            result.SkippedRows = skipRows;

            // 获取所有产品规格（用于规格识别）
            // 获取所有产品规格（用于规格识别）
            var productSpecs = (await _productSpecService.GetList(new ProductSpecListQuery()))
                .Cast<ProductSpecEntity>()
                .ToList();

            // 处理数据行
            // 跳过表头(1行) + 上次导入的行数(skipRows)
            int startRowIndex = 1 + skipRows;

            // 如果Excel总行数 <= 跳过的行数，说明没有新数据
            // 但也有可能是用户删除了部分数据重新导入，这里我们还是从 calculated start row 开始

            for (int rowIndex = startRowIndex; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);
                if (row == null)
                    continue;

                // 1. 获取原始数据
                var rawData = GetRowData(row, headerIndexes);
                result.OriginalData.Add(rawData);

                // 2. 解析数据
                var item = new RawDataPreviewItem();
                // 设置行号（Excel行号，从1开始，rowIndex是Excel行索引，所以行号是rowIndex+1）
                item.SortCode = rowIndex + 1;
                var errors = new List<string>();

                try
                {
                    // 读取基础字段
                    item.DetectionDate = GetCellValue<DateTime?>(row, headerIndexes, "日期");
                    item.FurnaceNo = GetCellValue<string>(row, headerIndexes, "炉号");
                    item.Width = GetCellValue<decimal?>(row, headerIndexes, "宽度");
                    item.CoilWeight = GetCellValue<decimal?>(row, headerIndexes, "带材重量");
                    // 读取AA列（索引26）和AB列（索引27）
                    item.BreakCount = GetCellValueByIndex<int?>(row, 26);
                    item.SingleCoilWeight = GetCellValueByIndex<decimal?>(row, 27);

                    // 检测数据列1-22（固定22列）
                    for (int colIndex = 1; colIndex <= 22; colIndex++)
                    {
                        decimal? detectionValue = null;

                        // 尝试多种表头格式：数字、检测+数字、列+数字等
                        var possibleHeaders = new[]
                        {
                            colIndex.ToString(),
                            $"检测{colIndex}",
                            $"列{colIndex}",
                            $"第{colIndex}列",
                            $"检测列{colIndex}",
                        };

                        foreach (var header in possibleHeaders)
                        {
                            if (headerIndexes.ContainsKey(header))
                            {
                                detectionValue = GetCellValue<decimal?>(row, headerIndexes, header);
                                if (detectionValue.HasValue)
                                {
                                    // 使用反射设置对应的Detection属性
                                    var propName = $"Detection{colIndex}";
                                    var prop = typeof(RawDataPreviewItem).GetProperty(propName);
                                    prop?.SetValue(item, detectionValue.Value);
                                    break;
                                }
                            }
                        }
                    }

                    // 校验必填字段
                    if (item.DetectionDate == null)
                        errors.Add("日期不能为空");
                    if (string.IsNullOrWhiteSpace(item.FurnaceNo))
                        errors.Add("炉号不能为空");
                    if (item.Width == null)
                        errors.Add("宽度不能为空");
                    if (item.CoilWeight == null)
                        errors.Add("带材重量不能为空");

                    // 解析炉号（使用FurnaceNoHelper）
                    if (!string.IsNullOrWhiteSpace(item.FurnaceNo))
                    {
                        var parseResult = FurnaceNoHelper.ParseFurnaceNo(item.FurnaceNo);
                        if (!parseResult.Success)
                        {
                            errors.Add($"炉号格式错误：{parseResult.ErrorMessage}");
                        }
                        else
                        {
                            item.LineNo = parseResult.LineNoNumeric;
                            item.Shift = parseResult.Shift;
                            item.ShiftNumeric = parseResult.ShiftNumeric;
                            item.FurnaceBatchNo = parseResult.FurnaceNoNumeric;
                            item.CoilNo = parseResult.CoilNoNumeric;
                            item.SubcoilNo = parseResult.SubcoilNoNumeric;
                            item.FeatureSuffix = parseResult.FeatureSuffix;

                            // 生产日期（ProdDate）：优先使用从原始炉号（FurnaceNo）中解析出的日期
                            // 如果炉号解析失败，使用检测日期（DetectionDate，从Excel"日期"列读取）作为后备
                            if (parseResult.ProdDate.HasValue)
                            {
                                item.ProdDate = parseResult.ProdDate;
                            }
                            else if (item.DetectionDate.HasValue)
                            {
                                item.ProdDate = item.DetectionDate;
                            }

                            // 计算格式化炉号（格式：[产线数字][班次汉字][8位日期]-[炉次号]-[卷号]-[分卷号]）
                            var furnaceNoObj = FurnaceNo.Parse(item.FurnaceNo);
                            if (furnaceNoObj.IsValid)
                            {
                                item.FurnaceNoFormatted = furnaceNoObj.GetFurnaceNo();
                            }

                            // 判断是否为有效数据（符合炉号解析规则）
                            item.IsValidData = parseResult.Success ? 1 : 0;

                            // 特性汉字匹配（如果有特性汉字，在预览阶段也进行匹配，但允许用户修改）
                            if (!string.IsNullOrWhiteSpace(parseResult.FeatureSuffix))
                            {
                                try
                                {
                                    var matchedFeatureIds = await MatchAppearanceFeatures(
                                        parseResult.FeatureSuffix
                                    );
                                    if (matchedFeatureIds.Count > 0)
                                    {
                                        item.AppearanceFeatureIds = SerializeFeatureIds(
                                            matchedFeatureIds
                                        );
                                    }
                                }
                                catch
                                {
                                    // 预览阶段匹配失败不影响预览，只记录
                                }
                            }
                        }
                    }
                    else
                    {
                        item.IsValidData = 0;
                    }

                    // 识别产品规格
                    if (errors.Count == 0)
                    {
                        var spec = IdentifyProductSpec(item, productSpecs);
                        if (spec != null)
                        {
                            item.ProductSpecId = spec.Id;
                            item.ProductSpecCode = spec.Code;
                            item.ProductSpecName = spec.Name;
                            item.DetectionColumns = spec.DetectionColumns;
                        }
                        else
                        {
                            // 获取有效检测列信息用于错误提示（从detection1-detection22字段读取）
                            var detectionProps = new[]
                            {
                                item.Detection1,
                                item.Detection2,
                                item.Detection3,
                                item.Detection4,
                                item.Detection5,
                                item.Detection6,
                                item.Detection7,
                                item.Detection8,
                                item.Detection9,
                                item.Detection10,
                                item.Detection11,
                                item.Detection12,
                                item.Detection13,
                                item.Detection14,
                                item.Detection15,
                                item.Detection16,
                                item.Detection17,
                                item.Detection18,
                                item.Detection19,
                                item.Detection20,
                                item.Detection21,
                                item.Detection22,
                            };

                            var validColumns = new List<int>();
                            for (int i = 0; i < detectionProps.Length; i++)
                            {
                                if (detectionProps[i].HasValue)
                                {
                                    validColumns.Add(i + 1);
                                }
                            }

                            if (validColumns.Count > 0)
                            {
                                int maxIndex = validColumns.Max();
                                int minIndex = validColumns.Min();

                                // 计算连续列数
                                int continuousCount = 0;
                                for (int i = 1; i <= maxIndex; i++)
                                {
                                    if (validColumns.Contains(i))
                                    {
                                        continuousCount = i;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                // 检查是否有缺失的列（不连续）
                                bool isContinuous = continuousCount == maxIndex && minIndex == 1;

                                if (isContinuous)
                                {
                                    errors.Add(
                                        $"无法识别产品规格，当前数据有效检测列为1-{maxIndex}。请检查产品定义是否包含检测列配置\"{maxIndex}\"。"
                                    );
                                }
                                else
                                {
                                    errors.Add(
                                        $"无法识别产品规格，当前数据有效检测列不连续（最小：{minIndex}，最大：{maxIndex}，连续列数：{continuousCount}）。请检查数据完整性。"
                                    );
                                }
                            }
                            else
                            {
                                errors.Add("无法识别产品规格，当前数据没有有效的检测列数据。");
                            }
                        }
                    }

                    if (errors.Count > 0)
                    {
                        item.Status = "failed";
                        item.ErrorMessage = string.Join("; ", errors);
                    }
                    else
                    {
                        item.Status = "success";
                    }
                }
                catch (Exception ex)
                {
                    item.Status = "failed";
                    item.ErrorMessage = $"处理异常：{ex.Message}";
                }

                result.ParsedData.Add(item);
            }

            // 检查炉号重复（格式：[产线数字][班次汉字][8位日期]-[炉次号]）
            // 只检查符合规则的有效数据
            CheckDuplicateFurnaceNo(result.ParsedData);

            // 检查数据库中已存在的炉号
            await CheckExistingFurnaceNoInDatabase(result.ParsedData);
        }
        catch (Exception ex)
        {
            throw Oops.Oh($"预览失败：{ex.Message}");
        }

        return result;
    }

    /// <inheritdoc />
    [HttpPost("import")]
    public async Task<RawDataImportOutput> ImportExcel([FromBody] RawDataImportInput input)
    {
        var result = new RawDataImportOutput();
        var errorDetails = new List<RawDataImportErrorDetail>();
        var successEntities = new List<RawDataEntity>();

        // 创建日志实体
        var log = new RawDataImportLogEntity
        {
            FileName = input.FileName,
            ImportTime = DateTime.Now,
            Status = "failed", // 默认失败，成功后更新
        };
        log.Creator(); // 设置创建者信息

        try
        {
            // 解码Base64文件
            var fileBytes = Convert.FromBase64String(input.FileData);
            using var stream = new MemoryStream(fileBytes);

            // 读取Excel
            IWorkbook workbook = input.FileName.EndsWith(".xlsx")
                ? new XSSFWorkbook(stream)
                : new HSSFWorkbook(stream);

            var sheet = workbook.GetSheetAt(0);

            // 记录Excel中的总数据行数（不含表头）
            log.TotalRows = sheet.LastRowNum;

            var headerRow = sheet.GetRow(0);

            if (headerRow == null)
            {
                throw Oops.Oh("Excel文件格式错误：缺少表头行");
            }

            // 获取表头索引
            var headerIndexes = GetHeaderIndexes(headerRow);

            // 获取所有产品规格（用于规格识别）
            // 获取所有产品规格（用于规格识别）
            var productSpecs = (await _productSpecService.GetList(new ProductSpecListQuery()))
                .Cast<ProductSpecEntity>()
                .ToList();

            // 获取上次导入日志，确定跳过行数
            var lastLogList = await _logRepository
                .AsQueryable()
                .Where(t => t.FileName == input.FileName && t.Status == "success")
                .OrderByDescending(t => t.ImportTime)
                .Take(1)
                .ToListAsync();
            var lastLog = lastLogList.FirstOrDefault();

            int skipRows = 0;
            if (lastLog != null)
            {
                skipRows = lastLog.TotalRows;
            }

            // 开始行 = 表头(1) + 跳过行数
            int startRowIndex = 1 + skipRows;

            // 用于记录实体和行号的映射关系（用于重复检查）
            var entityRowMap = new Dictionary<RawDataEntity, int>();

            // 处理数据行
            for (int rowIndex = startRowIndex; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);
                if (row == null)
                    continue;

                var entity = new RawDataEntity();
                // 设置行号（Excel行号，从1开始，rowIndex是Excel行索引，所以行号是rowIndex+1）
                entity.SortCode = rowIndex + 1;
                var errors = new List<string>();

                try
                {
                    // 读取基础字段
                    entity.DetectionDate = GetCellValue<DateTime?>(row, headerIndexes, "日期");
                    entity.FurnaceNo = GetCellValue<string>(row, headerIndexes, "炉号");
                    entity.Width = GetCellValue<decimal?>(row, headerIndexes, "宽度");
                    entity.CoilWeight = GetCellValue<decimal?>(row, headerIndexes, "带材重量");
                    // 读取AA列（索引26）和AB列（索引27）
                    entity.BreakCount = GetCellValueByIndex<int?>(row, 26);
                    entity.SingleCoilWeight = GetCellValueByIndex<decimal?>(row, 27);

                    // 动态读取所有检测数据列（使用JSON字段存储，支持任意数量的检测列）
                    // 检测数据列1-22（固定22列）
                    for (int colIndex = 1; colIndex <= 22; colIndex++)
                    {
                        decimal? detectionValue = null;

                        // 尝试多种表头格式：数字、检测+数字、列+数字等
                        var possibleHeaders = new[]
                        {
                            colIndex.ToString(),
                            $"检测{colIndex}",
                            $"列{colIndex}",
                            $"第{colIndex}列",
                            $"检测列{colIndex}",
                        };

                        foreach (var header in possibleHeaders)
                        {
                            if (headerIndexes.ContainsKey(header))
                            {
                                detectionValue = GetCellValue<decimal?>(row, headerIndexes, header);
                                if (detectionValue.HasValue)
                                {
                                    // 使用反射设置对应的Detection属性
                                    var propName = $"Detection{colIndex}";
                                    var prop = typeof(RawDataEntity).GetProperty(propName);
                                    prop?.SetValue(entity, detectionValue.Value);
                                    break;
                                }
                            }
                        }
                    }

                    // 校验必填字段
                    if (entity.DetectionDate == null)
                        errors.Add("日期不能为空");
                    if (string.IsNullOrWhiteSpace(entity.FurnaceNo))
                        errors.Add("炉号不能为空");
                    if (entity.Width == null)
                        errors.Add("宽度不能为空");
                    if (entity.CoilWeight == null)
                        errors.Add("带材重量不能为空");

                    // 解析炉号（使用FurnaceNoHelper）
                    if (!string.IsNullOrWhiteSpace(entity.FurnaceNo))
                    {
                        var parseResult = FurnaceNoHelper.ParseFurnaceNo(entity.FurnaceNo);
                        if (!parseResult.Success)
                        {
                            errors.Add($"炉号格式错误：{parseResult.ErrorMessage}");
                            entity.IsValidData = 0;
                        }
                        else
                        {
                            entity.LineNo = parseResult.LineNoNumeric;
                            entity.Shift = parseResult.Shift;
                            entity.ShiftNumeric = parseResult.ShiftNumeric;
                            entity.FurnaceBatchNo = parseResult.FurnaceNoNumeric;
                            entity.CoilNo = parseResult.CoilNoNumeric;
                            entity.SubcoilNo = parseResult.SubcoilNoNumeric;
                            entity.FeatureSuffix = parseResult.FeatureSuffix;

                            // 生产日期（ProdDate）：优先使用从原始炉号（FurnaceNo）中解析出的日期
                            // 如果炉号解析失败，使用检测日期（DetectionDate，从Excel"日期"列读取）作为后备
                            if (parseResult.ProdDate.HasValue)
                            {
                                entity.ProdDate = parseResult.ProdDate;
                            }
                            else if (entity.DetectionDate.HasValue)
                            {
                                entity.ProdDate = entity.DetectionDate;
                            }

                            // 计算格式化炉号（格式：[产线数字][班次汉字][8位日期]-[炉次号]-[卷号]-[分卷号]）
                            var furnaceNoObj = FurnaceNo.Parse(entity.FurnaceNo);
                            if (furnaceNoObj.IsValid)
                            {
                                entity.FurnaceNoFormatted = furnaceNoObj.GetFurnaceNo();
                            }

                            // 判断是否为有效数据（符合炉号解析规则）
                            entity.IsValidData = parseResult.Success ? 1 : 0;

                            // 特性汉字匹配（如果有特性汉字）
                            if (!string.IsNullOrWhiteSpace(parseResult.FeatureSuffix))
                            {
                                var matchedFeatureIds = await MatchAppearanceFeatures(
                                    parseResult.FeatureSuffix
                                );
                                if (matchedFeatureIds.Count > 0)
                                {
                                    entity.AppearanceFeatureIds = SerializeFeatureIds(
                                        matchedFeatureIds
                                    );
                                }
                            }
                        }
                    }
                    else
                    {
                        entity.IsValidData = 0;
                    }

                    // 识别产品规格
                    if (errors.Count == 0)
                    {
                        var spec = IdentifyProductSpec(entity, productSpecs);
                        if (spec != null)
                        {
                            entity.ProductSpecId = spec.Id;
                            entity.ProductSpecCode = spec.Code;
                            entity.ProductSpecName = spec.Name;
                            entity.DetectionColumns = spec.DetectionColumns; // 保存检测列信息
                        }
                        else
                        {
                            // 获取有效检测列信息用于错误提示（从detection1-detection22字段读取）
                            var detectionProps = new[]
                            {
                                entity.Detection1,
                                entity.Detection2,
                                entity.Detection3,
                                entity.Detection4,
                                entity.Detection5,
                                entity.Detection6,
                                entity.Detection7,
                                entity.Detection8,
                                entity.Detection9,
                                entity.Detection10,
                                entity.Detection11,
                                entity.Detection12,
                                entity.Detection13,
                                entity.Detection14,
                                entity.Detection15,
                                entity.Detection16,
                                entity.Detection17,
                                entity.Detection18,
                                entity.Detection19,
                                entity.Detection20,
                                entity.Detection21,
                                entity.Detection22,
                            };

                            var validColumns = new List<int>();
                            for (int i = 0; i < detectionProps.Length; i++)
                            {
                                if (detectionProps[i].HasValue)
                                {
                                    validColumns.Add(i + 1);
                                }
                            }

                            if (validColumns.Count > 0)
                            {
                                int maxIndex = validColumns.Max();
                                int minIndex = validColumns.Min();

                                // 计算连续列数
                                int continuousCount = 0;
                                for (int i = 1; i <= maxIndex; i++)
                                {
                                    if (validColumns.Contains(i))
                                    {
                                        continuousCount = i;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                // 检查是否有缺失的列（不连续）
                                bool isContinuous = continuousCount == maxIndex && minIndex == 1;

                                if (isContinuous)
                                {
                                    errors.Add(
                                        $"无法识别产品规格，当前数据有效检测列为1-{maxIndex}。请检查产品定义是否包含检测列配置\"{maxIndex}\"。"
                                    );
                                }
                                else
                                {
                                    errors.Add(
                                        $"无法识别产品规格，当前数据有效检测列不连续（最小：{minIndex}，最大：{maxIndex}，连续列数：{continuousCount}）。请检查数据完整性。"
                                    );
                                }
                            }
                            else
                            {
                                errors.Add("无法识别产品规格，当前数据没有有效的检测列数据。");
                            }
                        }
                    }

                    // 如果有错误，记录错误信息
                    if (errors.Count > 0)
                    {
                        entity.ImportStatus = 1;
                        entity.ImportError = string.Join("; ", errors);
                        errorDetails.Add(
                            new RawDataImportErrorDetail
                            {
                                RowIndex = rowIndex + 1,
                                ErrorMessage = entity.ImportError,
                                RawData = GetRowData(row, headerIndexes),
                            }
                        );
                    }
                    else
                    {
                        entity.ImportStatus = 0;
                        entity.Creator();
                        entity.LastModifyUserId = entity.CreatorUserId;
                        entity.LastModifyTime = entity.CreatorTime;
                        successEntities.Add(entity);
                        // 记录实体和行号的映射关系
                        entityRowMap[entity] = rowIndex;
                    }
                }
                catch (Exception ex)
                {
                    entity.ImportStatus = 1;
                    entity.ImportError = $"处理异常：{ex.Message}";
                    errorDetails.Add(
                        new RawDataImportErrorDetail
                        {
                            RowIndex = rowIndex + 1,
                            ErrorMessage = entity.ImportError,
                            RawData = GetRowData(row, headerIndexes),
                        }
                    );
                }
            }

            // 检查炉号重复（格式：[产线数字][班次汉字][8位日期]-[炉次号]）
            CheckDuplicateFurnaceNoForImport(
                successEntities,
                errorDetails,
                headerIndexes,
                sheet,
                entityRowMap
            );

            // 批量插入成功的数据
            if (successEntities.Count > 0)
            {
                // 注意：SortCode已经在读取Excel时设置为Excel行号，不需要重新设置
                // 如果需要按业务规则排序，可以在这里排序，但SortCode保持为Excel行号
                await _repository.AsInsertable(successEntities).ExecuteCommandAsync();
            }

            result.SuccessCount = successEntities.Count;
            result.FailCount = errorDetails.Count;
            result.ErrorDetails = errorDetails;

            // 生成错误报告
            if (errorDetails.Count > 0)
            {
                var errorReport = GenerateErrorReport(errorDetails, input.FileName);
                result.ErrorReport = Convert.ToBase64String(errorReport);
                result.ErrorReportFileName = $"错误报告_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            }

            // 更新并保存日志
            log.SuccessCount = successEntities.Count;
            log.FailCount = errorDetails.Count;
            log.Status =
                successEntities.Count > 0 || errorDetails.Count == 0 ? "success" : "partial";
            if (successEntities.Count == 0 && errorDetails.Count > 0)
                log.Status = "failed";

            await _logRepository.InsertAsync(log);
        }
        catch (Exception ex)
        {
            // 保存失败日志
            log.Status = "failed";
            await _logRepository.InsertAsync(log);
            throw Oops.Oh($"导入失败：{ex.Message}");
        }

        return result;
    }

    /// <inheritdoc />
    [HttpGet("import-log")]
    public async Task<List<RawDataImportLogListOutput>> GetImportLogList(
        [FromQuery] PageInputBase input
    )
    {
        var data = await _logRepository
            .AsSugarClient()
            .Queryable<RawDataImportLogEntity, UserEntity>(
                (l, u) => new JoinQueryInfos(JoinType.Left, l.CreatorUserId == u.Id)
            )
            .OrderByDescending((l, u) => l.ImportTime)
            .Select(
                (l, u) =>
                    new RawDataImportLogListOutput
                    {
                        Id = l.Id,
                        FileName = l.FileName,
                        TotalRows = l.TotalRows,
                        SuccessCount = l.SuccessCount,
                        FailCount = l.FailCount,
                        Status = l.Status,
                        ImportTime = l.ImportTime,
                        CreatorUserId = l.CreatorUserId,
                        CreatorTime = l.CreatorTime,
                        OperatorName = SqlFunc.MergeString(u.RealName, "/", u.Account),
                    }
            )
            .ToPagedListAsync(input.CurrentPage, input.PageSize);

        return data.list.ToList();
    }

    /// <inheritdoc />
    [HttpGet("")]
    public async Task<List<RawDataListOutput>> GetList([FromQuery] RawDataListQuery input)
    {
        int? parsedLineNo = null;
        if (!string.IsNullOrEmpty(input.LineNo) && int.TryParse(input.LineNo, out var val))
        {
            parsedLineNo = val;
        }

        var data = await _repository
            .AsSugarClient()
            .Queryable<RawDataEntity, UserEntity>(
                (r, u) => new JoinQueryInfos(JoinType.Left, r.CreatorUserId == u.Id)
            )
            .Where((r, u) => r.DeleteMark == null)
            .WhereIF(
                !string.IsNullOrEmpty(input.Keyword),
                (r, u) =>
                    r.FurnaceNo.Contains(input.Keyword)
                    || SqlFunc.ToString(r.LineNo).Contains(input.Keyword)
                    || r.ProductSpecName.Contains(input.Keyword)
            )
            .WhereIF(input.StartDate.HasValue, (r, u) => r.ProdDate >= input.StartDate)
            .WhereIF(input.EndDate.HasValue, (r, u) => r.ProdDate <= input.EndDate)
            .WhereIF(
                !string.IsNullOrEmpty(input.ProductSpecId),
                (r, u) => r.ProductSpecId == input.ProductSpecId
            )
            .WhereIF(parsedLineNo.HasValue, (r, u) => r.LineNo == parsedLineNo)
            // 有效数据筛选：1-有效，0-无效，-1或null-全部
            .WhereIF(
                input.IsValidData.HasValue && input.IsValidData >= 0,
                (r, u) => r.IsValidData == input.IsValidData
            )
            // 录入日期范围筛选
            .WhereIF(
                input.CreatorTimeStart.HasValue,
                (r, u) => r.CreatorTime >= input.CreatorTimeStart
            )
            .WhereIF(
                input.CreatorTimeEnd.HasValue,
                (r, u) => r.CreatorTime <= input.CreatorTimeEnd.Value.AddDays(1).AddSeconds(-1)
            )
            .Select(
                (r, u) =>
                    new RawDataListOutput
                    {
                        Id = r.Id,
                        ProdDate = r.ProdDate,
                        DetectionDate = r.DetectionDate,
                        FurnaceNo = r.FurnaceNo,
                        LineNo = r.LineNo,
                        Shift = r.Shift,
                        FurnaceBatchNo = r.FurnaceBatchNo,
                        CoilNo = r.CoilNo,
                        SubcoilNo = r.SubcoilNo,
                        FeatureSuffix = r.FeatureSuffix,
                        Width = r.Width,
                        CoilWeight = r.CoilWeight,
                        BreakCount = r.BreakCount,
                        SingleCoilWeight = r.SingleCoilWeight,
                        ProductSpecId = r.ProductSpecId,
                        ProductSpecCode = r.ProductSpecCode,
                        ProductSpecName = r.ProductSpecName,
                        DetectionColumns = r.DetectionColumns,
                        // 检测列1-22
                        Detection1 = r.Detection1,
                        Detection2 = r.Detection2,
                        Detection3 = r.Detection3,
                        Detection4 = r.Detection4,
                        Detection5 = r.Detection5,
                        Detection6 = r.Detection6,
                        Detection7 = r.Detection7,
                        Detection8 = r.Detection8,
                        Detection9 = r.Detection9,
                        Detection10 = r.Detection10,
                        Detection11 = r.Detection11,
                        Detection12 = r.Detection12,
                        Detection13 = r.Detection13,
                        Detection14 = r.Detection14,
                        Detection15 = r.Detection15,
                        Detection16 = r.Detection16,
                        Detection17 = r.Detection17,
                        Detection18 = r.Detection18,
                        Detection19 = r.Detection19,
                        Detection20 = r.Detection20,
                        Detection21 = r.Detection21,
                        Detection22 = r.Detection22,
                        IsValidData = r.IsValidData,
                        ImportError = r.ImportError,
                        ImportStatus = r.ImportStatus,
                        SortCode = r.SortCode,
                        CreatorUserId = r.CreatorUserId,
                        CreatorTime = r.CreatorTime,
                        CreatorUserName = SqlFunc.MergeString(u.RealName, "/", u.Account),
                        ProdDateStr = r.ProdDate.HasValue
                            ? r.ProdDate.Value.ToString("yyyy/MM/dd")
                            : string.Empty,
                        DetectionDateStr = r.DetectionDate.HasValue
                            ? r.DetectionDate.Value.ToString("yyyy/MM/dd")
                            : string.Empty,
                    }
            )
            .ToListAsync();

        // 计算炉号字段（格式：[产线数字][班次汉字][8位日期]-[炉次号]-[卷号]-[分卷号]）
        foreach (var item in data)
        {
            try
            {
                // 使用FurnaceNo.GetFurnaceNo()方法生成格式化的炉号
                var furnaceNoObj = FurnaceNo.Parse(item.FurnaceNo);
                if (furnaceNoObj.IsValid)
                {
                    item.FurnaceNoFormatted = furnaceNoObj.GetFurnaceNo();
                }
                else
                {
                    // 如果解析失败，尝试从已有字段构建
                    if (
                        item.ProdDate.HasValue
                        && item.LineNo.HasValue
                        && !string.IsNullOrEmpty(item.Shift)
                        && item.FurnaceBatchNo.HasValue
                        && item.CoilNo.HasValue
                        && item.SubcoilNo.HasValue
                    )
                    {
                        var dateStr = item.ProdDate.Value.ToString("yyyyMMdd");
                        item.FurnaceNoFormatted =
                            $"{item.LineNo}{item.Shift}{dateStr}-{item.FurnaceBatchNo}-{item.CoilNo}-{item.SubcoilNo}";
                    }
                    else
                    {
                        item.FurnaceNoFormatted = item.FurnaceNo ?? string.Empty;
                    }
                }
            }
            catch
            {
                // 如果计算失败，使用原始炉号
                item.FurnaceNoFormatted = item.FurnaceNo ?? string.Empty;
            }
        }

        // 自定义排序
        if (!string.IsNullOrEmpty(input.SortRules))
        {
            try
            {
                var sortRules = JsonConvert.DeserializeObject<List<SortRule>>(input.SortRules);
                if (sortRules != null && sortRules.Count > 0)
                {
                    data = ApplyMultiFieldSort(data, sortRules);
                }
                else
                {
                    // 默认排序
                    data = ApplyDefaultSort(data);
                }
            }
            catch
            {
                // JSON解析失败，使用默认排序
                data = ApplyDefaultSort(data);
            }
        }
        else
        {
            // 默认排序
            data = ApplyDefaultSort(data);
        }
        return data;
    }

    /// <inheritdoc />
    [HttpGet("{id}")]
    public async Task<RawDataInfoOutput> GetInfo(string id)
    {
        var entity = await _repository.GetFirstAsync(t => t.Id == id && t.DeleteMark == null);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);
        return entity.Adapt<RawDataInfoOutput>();
    }

    /// <inheritdoc />
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _repository.GetFirstAsync(t => t.Id == id && t.DeleteMark == null);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);

        entity.Delete();
        var isOk = await _repository
            .AsUpdateable(entity)
            .UpdateColumns(it => new
            {
                it.DeleteMark,
                it.DeleteTime,
                it.DeleteUserId,
            })
            .ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);
    }

    #region 私有方法

    /// <summary>
    /// 获取表头索引.
    /// </summary>
    private Dictionary<string, int> GetHeaderIndexes(IRow headerRow)
    {
        var indexes = new Dictionary<string, int>();
        for (int i = 0; i < headerRow.LastCellNum; i++)
        {
            var cell = headerRow.GetCell(i);
            if (cell != null)
            {
                var headerName = cell.ToString()?.Trim();
                if (!string.IsNullOrWhiteSpace(headerName))
                {
                    indexes[headerName] = i;
                }
            }
        }
        return indexes;
    }

    /// <summary>
    /// 获取单元格值.
    /// </summary>
    private T GetCellValue<T>(IRow row, Dictionary<string, int> headerIndexes, string headerName)
    {
        if (!headerIndexes.ContainsKey(headerName))
            return default(T);

        var cellIndex = headerIndexes[headerName];
        var cell = row.GetCell(cellIndex);
        if (cell == null)
            return default(T);

        try
        {
            if (typeof(T) == typeof(DateTime?) || typeof(T) == typeof(DateTime))
            {
                if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
                {
                    return (T)(object)cell.DateCellValue;
                }
                if (DateTime.TryParse(cell.ToString(), out var date))
                {
                    return (T)(object)date;
                }
                return default(T);
            }

            if (typeof(T) == typeof(decimal?) || typeof(T) == typeof(decimal))
            {
                if (cell.CellType == CellType.Numeric)
                {
                    return (T)(object)(decimal)cell.NumericCellValue;
                }
                if (decimal.TryParse(cell.ToString(), out var dec))
                {
                    return (T)(object)dec;
                }
                return default(T);
            }

            if (typeof(T) == typeof(string))
            {
                return (T)(object)(cell.ToString()?.Trim() ?? string.Empty);
            }

            return (T)Convert.ChangeType(cell.ToString(), typeof(T));
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// 按列索引获取单元格值.
    /// </summary>
    private T GetCellValueByIndex<T>(IRow row, int columnIndex)
    {
        if (row == null || columnIndex < 0)
            return default(T);

        var cell = row.GetCell(columnIndex);
        if (cell == null)
            return default(T);

        try
        {
            if (typeof(T) == typeof(int?) || typeof(T) == typeof(int))
            {
                if (cell.CellType == CellType.Numeric)
                {
                    return (T)(object)(int)cell.NumericCellValue;
                }
                if (int.TryParse(cell.ToString(), out var intVal))
                {
                    return (T)(object)intVal;
                }
                return default(T);
            }

            if (typeof(T) == typeof(decimal?) || typeof(T) == typeof(decimal))
            {
                if (cell.CellType == CellType.Numeric)
                {
                    return (T)(object)(decimal)cell.NumericCellValue;
                }
                if (decimal.TryParse(cell.ToString(), out var dec))
                {
                    return (T)(object)dec;
                }
                return default(T);
            }

            if (typeof(T) == typeof(DateTime?) || typeof(T) == typeof(DateTime))
            {
                if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
                {
                    return (T)(object)cell.DateCellValue;
                }
                if (DateTime.TryParse(cell.ToString(), out var date))
                {
                    return (T)(object)date;
                }
                return default(T);
            }

            if (typeof(T) == typeof(string))
            {
                return (T)(object)(cell.ToString()?.Trim() ?? string.Empty);
            }

            return (T)Convert.ChangeType(cell.ToString(), typeof(T));
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// 检查炉号重复（用于预览）.
    /// 炉号格式：[产线数字][班次汉字][8位日期]-[炉次号]
    /// 只检查符合规则的有效数据，标记为重复但不直接设为失败，让用户选择保留哪条
    /// </summary>
    private void CheckDuplicateFurnaceNo(List<RawDataPreviewItem> items)
    {
        // 构建标准炉号字典：标准炉号 -> 行号列表
        var furnaceNoDict = new Dictionary<string, List<int>>();

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            // 只检查解析成功的有效数据（符合规则的数据）
            if (
                item.IsValidData == 1
                && item.LineNo.HasValue
                && !string.IsNullOrWhiteSpace(item.Shift)
                && item.ProdDate.HasValue
                && item.FurnaceBatchNo.HasValue
            )
            {
                // 构建标准炉号：[产线数字][班次汉字][8位日期]-[炉次号]
                var dateStr = item.ProdDate.Value.ToString("yyyyMMdd");
                var standardFurnaceNo = $"{item.LineNo}{item.Shift}{dateStr}-{item.FurnaceBatchNo}";
                item.StandardFurnaceNo = standardFurnaceNo;

                if (!furnaceNoDict.ContainsKey(standardFurnaceNo))
                {
                    furnaceNoDict[standardFurnaceNo] = new List<int>();
                }
                furnaceNoDict[standardFurnaceNo].Add(i);
            }
        }

        // 检查重复并标记（不直接设为失败，让用户选择保留哪条）
        foreach (var kvp in furnaceNoDict)
        {
            if (kvp.Value.Count > 1)
            {
                // 有重复的炉号，标记为重复状态
                var duplicateRowNumbers = string.Join(
                    "、",
                    kvp.Value.Select(idx => $"第{items[idx].SortCode}行")
                );
                var warningMessage =
                    $"炉号重复：标准炉号 {kvp.Key} 在以下行出现重复：{duplicateRowNumbers}，请选择保留哪条数据";

                foreach (var rowIndex in kvp.Value)
                {
                    var item = items[rowIndex];
                    // 标记为重复，但不直接设为失败
                    item.IsDuplicateInFile = true;
                    // 如果状态还是success，改为duplicate
                    if (item.Status == "success")
                    {
                        item.Status = "duplicate";
                    }
                    // 追加警告信息
                    if (string.IsNullOrWhiteSpace(item.ErrorMessage))
                    {
                        item.ErrorMessage = warningMessage;
                    }
                    else
                    {
                        item.ErrorMessage = $"{item.ErrorMessage}; {warningMessage}";
                    }
                }
            }
        }
    }

    /// <summary>
    /// 检查数据库中已存在的炉号（用于预览）.
    /// 如果本次Excel导入的炉号在数据库中已经存在，标记为将被忽略
    /// </summary>
    private async Task CheckExistingFurnaceNoInDatabase(List<RawDataPreviewItem> items)
    {
        // 只检查符合规则的有效数据
        var validItems = items
            .Where(item =>
                item.IsValidData == 1
                && item.LineNo.HasValue
                && !string.IsNullOrWhiteSpace(item.Shift)
                && item.ProdDate.HasValue
                && item.FurnaceBatchNo.HasValue
                && !string.IsNullOrWhiteSpace(item.StandardFurnaceNo)
            )
            .ToList();

        if (validItems.Count == 0)
            return;

        // 构建标准炉号列表
        var standardFurnaceNos = validItems
            .Select(item => item.StandardFurnaceNo)
            .Distinct()
            .ToList();

        // 查询数据库中已存在的标准炉号
        // 标准炉号格式：[产线数字][班次汉字][8位日期]-[炉次号]
        var existingFurnaceNos = new HashSet<string>();

        // 查询数据库中所有有效数据，构建标准炉号并检查
        var dbEntities = await _repository
            .AsQueryable()
            .Where(e =>
                e.IsValidData == 1
                && e.LineNo.HasValue
                && !string.IsNullOrWhiteSpace(e.Shift)
                && e.ProdDate.HasValue
                && e.FurnaceBatchNo.HasValue
            )
            .Select(e => new
            {
                e.LineNo,
                e.Shift,
                e.ProdDate,
                e.FurnaceBatchNo,
            })
            .ToListAsync();

        foreach (var entity in dbEntities)
        {
            var dateStr = entity.ProdDate.Value.ToString("yyyyMMdd");
            var standardFurnaceNo =
                $"{entity.LineNo}{entity.Shift}{dateStr}-{entity.FurnaceBatchNo}";
            if (standardFurnaceNos.Contains(standardFurnaceNo))
            {
                existingFurnaceNos.Add(standardFurnaceNo);
            }
        }

        // 标记数据库中已存在的炉号
        foreach (var item in validItems)
        {
            if (existingFurnaceNos.Contains(item.StandardFurnaceNo))
            {
                item.ExistsInDatabase = true;
                // 如果状态还是success或duplicate，改为exists_in_db
                if (item.Status == "success" || item.Status == "duplicate")
                {
                    item.Status = "exists_in_db";
                }
                // 追加提示信息
                var infoMessage =
                    $"炉号 {item.StandardFurnaceNo} 在数据库中已存在，将被忽略，不会保存到数据库";
                if (string.IsNullOrWhiteSpace(item.ErrorMessage))
                {
                    item.ErrorMessage = infoMessage;
                }
                else
                {
                    item.ErrorMessage = $"{item.ErrorMessage}; {infoMessage}";
                }
            }
        }
    }

    /// <summary>
    /// 检查炉号重复（用于导入）.
    /// 炉号格式：[产线数字][班次汉字][8位日期]-[炉次号]
    /// </summary>
    private void CheckDuplicateFurnaceNoForImport(
        List<RawDataEntity> successEntities,
        List<RawDataImportErrorDetail> errorEntities,
        Dictionary<string, int> headerIndexes,
        ISheet sheet,
        Dictionary<RawDataEntity, int> entityRowMap
    )
    {
        // 构建标准炉号字典：标准炉号 -> 实体列表（包含行号信息）
        var furnaceNoDict = new Dictionary<string, List<(RawDataEntity Entity, int RowIndex)>>();

        foreach (var entity in successEntities)
        {
            // 只检查解析成功的有效数据
            if (
                entity.IsValidData == 1
                && entity.LineNo.HasValue
                && !string.IsNullOrWhiteSpace(entity.Shift)
                && entity.ProdDate.HasValue
                && entity.FurnaceBatchNo.HasValue
            )
            {
                // 构建标准炉号：[产线数字][班次汉字][8位日期]-[炉次号]
                var dateStr = entity.ProdDate.Value.ToString("yyyyMMdd");
                var standardFurnaceNo =
                    $"{entity.LineNo}{entity.Shift}{dateStr}-{entity.FurnaceBatchNo}";

                if (!furnaceNoDict.ContainsKey(standardFurnaceNo))
                {
                    furnaceNoDict[standardFurnaceNo] = new List<(RawDataEntity, int)>();
                }
                // 从映射中获取行号
                var rowIndex = entityRowMap.ContainsKey(entity) ? entityRowMap[entity] : 0;
                furnaceNoDict[standardFurnaceNo].Add((entity, rowIndex));
            }
        }

        // 检查重复并设置错误信息
        var entitiesToRemove = new List<RawDataEntity>();
        foreach (var kvp in furnaceNoDict)
        {
            if (kvp.Value.Count > 1)
            {
                // 有重复的炉号，为所有重复的实体设置错误信息
                var duplicateItems = kvp.Value;
                var rowNumbers = string.Join(
                    "、",
                    duplicateItems.Select(item => $"第{item.RowIndex + 1}行")
                ); // +1是因为Excel行号从1开始
                var errorMessage =
                    $"炉号重复：标准炉号 {kvp.Key} 在以下行出现重复：{rowNumbers}，请修改后重新导入";

                foreach (var (entity, rowIndex) in duplicateItems)
                {
                    entity.ImportStatus = 1;
                    entity.IsValidData = 0;
                    // 如果已有错误信息，追加；否则设置新错误信息
                    if (string.IsNullOrWhiteSpace(entity.ImportError))
                    {
                        entity.ImportError = errorMessage;
                    }
                    else
                    {
                        entity.ImportError = $"{entity.ImportError}; {errorMessage}";
                    }

                    // 标记为需要移除
                    entitiesToRemove.Add(entity);

                    // 添加到错误详情列表
                    var row = sheet?.GetRow(rowIndex);
                    errorEntities.Add(
                        new RawDataImportErrorDetail
                        {
                            RowIndex = rowIndex + 1, // Excel行号从1开始
                            ErrorMessage = entity.ImportError,
                            RawData =
                                row != null
                                    ? GetRowData(row, headerIndexes)
                                    : new Dictionary<string, object>
                                    {
                                        { "FurnaceNo", entity.FurnaceNo ?? "" },
                                        {
                                            "ProdDate",
                                            entity.ProdDate?.ToString("yyyy-MM-dd") ?? ""
                                        },
                                    },
                        }
                    );
                }
            }
        }

        // 从成功列表移除重复的实体
        foreach (var entity in entitiesToRemove)
        {
            successEntities.Remove(entity);
        }
    }

    /// <summary>
    /// 获取行数据.
    /// </summary>
    private Dictionary<string, object> GetRowData(IRow row, Dictionary<string, int> headerIndexes)
    {
        var data = new Dictionary<string, object>();
        foreach (var kvp in headerIndexes)
        {
            var cell = row.GetCell(kvp.Value);
            if (cell == null)
            {
                data[kvp.Key] = null;
                continue;
            }

            switch (cell.CellType)
            {
                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(cell))
                    {
                        data[kvp.Key] = cell.DateCellValue;
                    }
                    else
                    {
                        data[kvp.Key] = cell.NumericCellValue;
                    }
                    break;
                case CellType.Boolean:
                    data[kvp.Key] = cell.BooleanCellValue;
                    break;
                case CellType.Formula:
                    // 对于公式，尝试获取计算后的值
                    try
                    {
                        if (cell.CachedFormulaResultType == CellType.Numeric)
                        {
                            if (DateUtil.IsCellDateFormatted(cell))
                                data[kvp.Key] = cell.DateCellValue;
                            else
                                data[kvp.Key] = cell.NumericCellValue;
                        }
                        else if (cell.CachedFormulaResultType == CellType.Boolean)
                        {
                            data[kvp.Key] = cell.BooleanCellValue;
                        }
                        else
                        {
                            data[kvp.Key] = cell.StringCellValue;
                        }
                    }
                    catch
                    {
                        data[kvp.Key] = cell.ToString();
                    }
                    break;
                default:
                    data[kvp.Key] = cell.ToString()?.Trim();
                    break;
            }
        }
        return data;
    }

    /// <summary>
    /// 匹配外观特性（特性汉字匹配）.
    /// 调用外观特性规则匹配器进行1:n匹配.
    /// </summary>
    /// <param name="featureSuffix">特性汉字（从炉号解析）</param>
    /// <returns>匹配到的特性ID列表</returns>
    private async Task<List<string>> MatchAppearanceFeatures(string featureSuffix)
    {
        if (string.IsNullOrWhiteSpace(featureSuffix))
            return new List<string>();

        try
        {
            // 1. 获取所有外观特性
            var allFeatures = (await _appearanceFeatureService.GetList(new AppearanceFeatureListQuery()))
                .Cast<AppearanceFeatureEntity>()
                .Where(f => !string.IsNullOrEmpty(f.Name) && !string.IsNullOrEmpty(f.CategoryId))
                .ToList();

            if (allFeatures.Count == 0)
                return new List<string>();

            // 2. 获取所有大类和等级，建立ID到名称的映射
            var allCategories = await _categoryService.GetAllCategories();
            var categoryIdToName = allCategories.ToDictionary(c => c.Id, c => c.Name);

            var allFeatureLevels = (await _featureLevelService.GetList(new AppearanceFeatureLevelListQuery()))
                .Cast<AppearanceFeatureLevelEntity>()
                .ToList();
            var featureLevelIdToName = allFeatureLevels.ToDictionary(s => s.Id, s => s.Name);

            // 3. 获取启用的严重程度等级列表（用于程度词识别）
            var enabledLevels = await _featureLevelService.GetEnabledLevels();
            var degreeWords =
                enabledLevels?.Select(l => l.Name).Where(n => !string.IsNullOrEmpty(n)).ToList()
                ?? new List<string>();

            if (degreeWords.Count == 0)
            {
                // 如果没有启用的特性等级，仍然尝试匹配（不使用程度词）
                degreeWords = new List<string>();
            }

            // 4. 调用规则匹配器进行匹配
            var matchResults = _featureRuleMatcher.Match(
                featureSuffix,
                allFeatures,
                degreeWords,
                categoryIdToName,
                featureLevelIdToName
            );

            // 5. 提取匹配到的特性ID列表（1:n关系）
            var featureIds = matchResults
                .Where(r => r.Feature != null && !string.IsNullOrEmpty(r.Feature.Id))
                .Select(r => r.Feature.Id)
                .Distinct()
                .ToList();

            return featureIds;
        }
        catch (Exception ex)
        {
            // 匹配失败时返回空列表，不抛出异常，避免影响导入流程
            // 可以在日志中记录错误
            Console.WriteLine($"[MatchAppearanceFeatures] 特性匹配失败: {ex.Message}");
            return new List<string>();
        }
    }

    /// <summary>
    /// 序列化特性ID列表为JSON字符串.
    /// </summary>
    private string SerializeFeatureIds(List<string> featureIds)
    {
        if (featureIds == null || featureIds.Count == 0)
            return null;

        return JsonConvert.SerializeObject(featureIds);
    }

    /// <summary>
    /// 反序列化JSON字符串为特性ID列表.
    /// </summary>
    private List<string> DeserializeFeatureIds(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<string>();

        try
        {
            return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// 识别产品规格.
    /// 根据原始数据中实际存在的有效数据列去匹配产品规格配置。
    /// 例如：如果产品规格120配置的检测列是13，那么数据的有效列应该是1-13（第1-13列有值，第14列开始为空）。
    /// 判断逻辑：根据产品规格配置的检测列，检查数据的有效列是否正好匹配（从1到检测列都有值，检测列+1开始为空）。
    /// 使用JSON字段进行匹配（支持动态列数）.
    /// </summary>
    private ProductSpecEntity IdentifyProductSpec(
        RawDataEntity entity,
        List<ProductSpecEntity> productSpecs
    )
    {
        // 1. 从detection1-detection22字段获取检测数据
        var detectionData = new Dictionary<int, decimal?>();
        var detectionProps = new[]
        {
            entity.Detection1,
            entity.Detection2,
            entity.Detection3,
            entity.Detection4,
            entity.Detection5,
            entity.Detection6,
            entity.Detection7,
            entity.Detection8,
            entity.Detection9,
            entity.Detection10,
            entity.Detection11,
            entity.Detection12,
            entity.Detection13,
            entity.Detection14,
            entity.Detection15,
            entity.Detection16,
            entity.Detection17,
            entity.Detection18,
            entity.Detection19,
            entity.Detection20,
            entity.Detection21,
            entity.Detection22,
        };

        for (int i = 0; i < detectionProps.Length; i++)
        {
            if (detectionProps[i].HasValue)
            {
                detectionData[i + 1] = detectionProps[i].Value;
            }
        }

        // 2. 获取当前行数据中所有有效（非空）的列索引集合
        var validColumns = detectionData
            .Where(kvp => kvp.Value.HasValue)
            .Select(kvp => kvp.Key)
            .ToHashSet();

        if (validColumns.Count == 0)
        {
            return null; // 没有有效检测数据，无法匹配
        }

        // 3. 获取连续的有效检测列数量（从1开始连续到最后一个有效列）
        int maxValidIndex = validColumns.Max();
        int continuousCount = 0;
        for (int i = 1; i <= maxValidIndex; i++)
        {
            if (validColumns.Contains(i))
            {
                continuousCount = i;
            }
            else
            {
                break;
            }
        }

        // 3. 遍历规格进行匹配
        foreach (var spec in productSpecs)
        {
            // DetectionColumns 已经从可空类型调整为非空 int，
            // 约定：小于等于 0 表示“未配置检测列”
            if (spec.DetectionColumns <= 0)
                continue;

            // 解析规格配置的检测列（现在是 int 类型，直接转换为列表）
            var specColumns = new List<int> { spec.DetectionColumns };

            if (specColumns.Count == 0)
                continue;

            // 4. 对每个配置的检测列进行匹配判断
            foreach (var specColumn in specColumns)
            {
                // 判断数据的有效列是否正好是1到specColumn（即第1到specColumn列都有值，specColumn+1开始为空）
                // 例如：如果specColumn=13，那么应该检查：
                // - 第1-13列都有值（都在validColumns中）
                // - 第14列开始为空（不在validColumns中，或者maxValidIndex <= specColumn）

                bool isMatch = true;

                // 检查1到specColumn列是否都有值
                for (int i = 1; i <= specColumn; i++)
                {
                    if (!validColumns.Contains(i))
                    {
                        isMatch = false;
                        break;
                    }
                }

                // 如果1到specColumn列都有值，还需要检查specColumn+1列开始是否为空
                // 即最大有效列索引应该等于specColumn
                if (isMatch && maxValidIndex != specColumn)
                {
                    isMatch = false;
                }

                if (isMatch)
                {
                    return spec;
                }
            }
        }

        // 如果没有匹配到，返回 null，由外部决定错误信息
        return null;
    }

    /// <summary>
    /// 生成错误报告Excel.
    /// </summary>
    private byte[] GenerateErrorReport(
        List<RawDataImportErrorDetail> errorDetails,
        string originalFileName
    )
    {
        IWorkbook workbook = new XSSFWorkbook();
        var sheet = workbook.CreateSheet("错误报告");

        // 创建表头
        var headerRow = sheet.CreateRow(0);
        headerRow.CreateCell(0).SetCellValue("行号");
        headerRow.CreateCell(1).SetCellValue("错误信息");

        // 添加数据列（从第一个错误详情中获取所有列名）
        if (errorDetails.Count > 0 && errorDetails[0].RawData.Count > 0)
        {
            int colIndex = 2;
            foreach (var key in errorDetails[0].RawData.Keys)
            {
                headerRow.CreateCell(colIndex).SetCellValue(key);
                colIndex++;
            }
        }

        // 添加错误数据
        for (int i = 0; i < errorDetails.Count; i++)
        {
            var row = sheet.CreateRow(i + 1);
            var detail = errorDetails[i];

            row.CreateCell(0).SetCellValue(detail.RowIndex);
            row.CreateCell(1).SetCellValue(detail.ErrorMessage);

            int colIndex = 2;
            foreach (var kvp in detail.RawData)
            {
                var cell = row.CreateCell(colIndex);
                var value = kvp.Value?.ToString() ?? string.Empty;
                cell.SetCellValue(value);
                colIndex++;
            }
        }

        // 转换为字节数组
        using var stream = new MemoryStream();
        workbook.Write(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// 应用多字段排序.
    /// </summary>
    private List<RawDataListOutput> ApplyMultiFieldSort(
        List<RawDataListOutput> data,
        List<SortRule> sortRules
    )
    {
        if (sortRules == null || sortRules.Count == 0)
            return ApplyDefaultSort(data);

        IOrderedEnumerable<RawDataListOutput> orderedData = null;

        for (int i = 0; i < sortRules.Count; i++)
        {
            var rule = sortRules[i];
            var isDesc = rule.Order?.ToLower() == "desc";

            if (i == 0)
            {
                // 第一个排序字段
                orderedData = ApplySingleFieldSort(data, rule.Field, isDesc);
            }
            else
            {
                // 后续的ThenBy
                orderedData = ApplyThenBy(orderedData, rule.Field, isDesc);
            }
        }

        return orderedData?.ToList() ?? data.ToList();
    }

    /// <summary>
    /// 应用单字段排序（首次排序）.
    /// </summary>
    private IOrderedEnumerable<RawDataListOutput> ApplySingleFieldSort(
        IEnumerable<RawDataListOutput> data,
        string field,
        bool isDesc
    )
    {
        switch (field.ToLower())
        {
            case "proddate":
                return isDesc
                    ? data.OrderByDescending(t => t.ProdDate ?? DateTime.MinValue)
                    : data.OrderBy(t => t.ProdDate ?? DateTime.MinValue);

            case "furnaceno":
            case "furnacenoparsed":
            case "furnacebatchno":
                return isDesc
                    ? data.OrderByDescending(t => t.FurnaceBatchNo ?? int.MaxValue)
                    : data.OrderBy(t => t.FurnaceBatchNo ?? int.MaxValue);

            case "coilno":
                return isDesc
                    ? data.OrderByDescending(t => t.CoilNo ?? decimal.MaxValue)
                    : data.OrderBy(t => t.CoilNo ?? decimal.MaxValue);

            case "subcoilno":
                return isDesc
                    ? data.OrderByDescending(t => t.SubcoilNo ?? decimal.MaxValue)
                    : data.OrderBy(t => t.SubcoilNo ?? decimal.MaxValue);

            case "lineno":
                return isDesc
                    ? data.OrderByDescending(t => t.LineNo ?? int.MaxValue)
                    : data.OrderBy(t => t.LineNo ?? int.MaxValue);

            case "shift":
                return isDesc
                    ? data.OrderByDescending(t => t.ShiftNumeric ?? int.MaxValue)
                    : data.OrderBy(t => t.ShiftNumeric ?? int.MaxValue);

            case "productspecname":
                return isDesc
                    ? data.OrderByDescending(t => t.ProductSpecName ?? string.Empty)
                    : data.OrderBy(t => t.ProductSpecName ?? string.Empty);

            case "creatortime":
                return isDesc
                    ? data.OrderByDescending(t => t.CreatorTime ?? DateTime.MinValue)
                    : data.OrderBy(t => t.CreatorTime ?? DateTime.MinValue);

            default:
                // 不支持的字段，按ID排序
                return data.OrderBy(t => t.Id);
        }
    }

    /// <summary>
    /// 应用ThenBy排序.
    /// </summary>
    private IOrderedEnumerable<RawDataListOutput> ApplyThenBy(
        IOrderedEnumerable<RawDataListOutput> orderedData,
        string field,
        bool isDesc
    )
    {
        switch (field.ToLower())
        {
            case "proddate":
                return isDesc
                    ? orderedData.ThenByDescending(t => t.ProdDate ?? DateTime.MinValue)
                    : orderedData.ThenBy(t => t.ProdDate ?? DateTime.MinValue);

            case "furnaceno":
            case "furnacenoparsed":
            case "furnacebatchno":
                return isDesc
                    ? orderedData.ThenByDescending(t => t.FurnaceBatchNo ?? int.MaxValue)
                    : orderedData.ThenBy(t => t.FurnaceBatchNo ?? int.MaxValue);

            case "coilno":
                return isDesc
                    ? orderedData.ThenByDescending(t => t.CoilNo ?? decimal.MaxValue)
                    : orderedData.ThenBy(t => t.CoilNo ?? decimal.MaxValue);

            case "subcoilno":
                return isDesc
                    ? orderedData.ThenByDescending(t => t.SubcoilNo ?? decimal.MaxValue)
                    : orderedData.ThenBy(t => t.SubcoilNo ?? decimal.MaxValue);

            case "lineno":
                return isDesc
                    ? orderedData.ThenByDescending(t => t.LineNo ?? int.MaxValue)
                    : orderedData.ThenBy(t => t.LineNo ?? int.MaxValue);

            case "shift":
                return isDesc
                    ? orderedData.ThenByDescending(t => t.ShiftNumeric ?? int.MaxValue)
                    : orderedData.ThenBy(t => t.ShiftNumeric ?? int.MaxValue);

            case "productspecname":
                return isDesc
                    ? orderedData.ThenByDescending(t => t.ProductSpecName ?? string.Empty)
                    : orderedData.ThenBy(t => t.ProductSpecName ?? string.Empty);

            case "creatortime":
                return isDesc
                    ? orderedData.ThenByDescending(t => t.CreatorTime ?? DateTime.MinValue)
                    : orderedData.ThenBy(t => t.CreatorTime ?? DateTime.MinValue);

            default:
                // 不支持的字段，保持原顺序
                return orderedData;
        }
    }

    /// <summary>
    /// 应用默认排序.
    /// </summary>
    private List<RawDataListOutput> ApplyDefaultSort(List<RawDataListOutput> data)
    {
        return data.OrderBy(t => t.ProdDate ?? DateTime.MinValue)
            .ThenBy(t => t.FurnaceBatchNo ?? int.MaxValue)
            .ThenBy(t => t.CoilNo ?? decimal.MaxValue)
            .ThenBy(t => t.SubcoilNo ?? decimal.MaxValue)
            .ThenBy(t => t.LineNo ?? int.MaxValue)
            .ThenBy(t => t.ShiftNumeric ?? int.MaxValue)
            .ToList();
    }

    #endregion
}
