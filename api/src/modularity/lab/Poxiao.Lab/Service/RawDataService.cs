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
using Poxiao.Lab.Entity.Dto.RawData;
using Poxiao.Lab.Entity.Entity;
using Poxiao.Lab.Helpers;
using Poxiao.Lab.Interfaces;
using Poxiao.Systems.Entitys.Permission;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// 原始数据服务.
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "raw-data", Order = 200)]
[Route("api/lab/raw-data")]
public class RawDataService : IRawDataService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<RawDataEntity> _repository;
    private readonly ISqlSugarRepository<ProductSpecEntity> _productSpecRepository;
    private readonly ISqlSugarRepository<RawDataImportLogEntity> _logRepository;
    private readonly IUserManager _userManager;
    private readonly ISqlSugarRepository<AppearanceFeatureEntity> _appearanceFeatureRepository;
    private readonly ISqlSugarRepository<AppearanceFeatureCategoryEntity> _categoryRepository;
    private readonly ISqlSugarRepository<AppearanceFeatureLevelEntity> _featureLevelRepository;
    private readonly AppearanceFeatureRuleMatcher _featureRuleMatcher;
    private readonly IAppearanceFeatureLevelService _featureLevelService;

    public RawDataService(
        ISqlSugarRepository<RawDataEntity> repository,
        ISqlSugarRepository<ProductSpecEntity> productSpecRepository,
        ISqlSugarRepository<RawDataImportLogEntity> logRepository,
        IUserManager userManager,
        ISqlSugarRepository<AppearanceFeatureEntity> appearanceFeatureRepository,
        ISqlSugarRepository<AppearanceFeatureCategoryEntity> categoryRepository,
        ISqlSugarRepository<AppearanceFeatureLevelEntity> featureLevelRepository,
        AppearanceFeatureRuleMatcher featureRuleMatcher,
        IAppearanceFeatureLevelService featureLevelService
    )
    {
        _repository = repository;
        _productSpecRepository = productSpecRepository;
        _logRepository = logRepository;
        _userManager = userManager;
        _appearanceFeatureRepository = appearanceFeatureRepository;
        _categoryRepository = categoryRepository;
        _featureLevelRepository = featureLevelRepository;
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
            var productSpecs = await _productSpecRepository
                .AsQueryable()
                .Where(t => t.DeleteMark == null)
                .ToListAsync();

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
                var errors = new List<string>();

                try
                {
                    // 读取基础字段
                    item.ProdDate = GetCellValue<DateTime?>(row, headerIndexes, "日期");
                    item.FurnaceNo = GetCellValue<string>(row, headerIndexes, "炉号");
                    item.Width = GetCellValue<decimal?>(row, headerIndexes, "宽度");
                    item.CoilWeight = GetCellValue<decimal?>(row, headerIndexes, "带材重量");

                    // 动态读取所有检测数据列（支持扩展，不限制为22列）
                    // 使用JSON字段存储，支持任意数量的检测列
                    var detectionData = new Dictionary<int, decimal?>();

                    // 从Excel中读取检测数据列（尝试读取到100列，实际可能更少）
                    for (int colIndex = 1; colIndex <= 100; colIndex++)
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
                                    detectionData[colIndex] = detectionValue.Value;
                                    break;
                                }
                            }
                        }

                        // 如果连续多列都没有值，停止读取（假设检测列是连续的）
                        if (!detectionValue.HasValue && detectionData.Count > 0)
                        {
                            // 如果已经有数据，但当前列没有值，可能是到了末尾
                            // 继续检查下一列，如果下一列也没有值，则停止
                            bool nextColumnHasValue = false;
                            foreach (
                                var nextHeader in new[]
                                {
                                    (colIndex + 1).ToString(),
                                    $"检测{colIndex + 1}",
                                    $"列{colIndex + 1}",
                                    $"第{colIndex + 1}列",
                                    $"检测列{colIndex + 1}",
                                }
                            )
                            {
                                if (headerIndexes.ContainsKey(nextHeader))
                                {
                                    var nextValue = GetCellValue<decimal?>(
                                        row,
                                        headerIndexes,
                                        nextHeader
                                    );
                                    if (nextValue.HasValue)
                                    {
                                        nextColumnHasValue = true;
                                        break;
                                    }
                                }
                            }

                            if (!nextColumnHasValue)
                            {
                                break; // 连续两列都没有值，停止读取
                            }
                        }
                    }

                    // 将检测数据保存到JSON字段
                    if (detectionData.Count > 0)
                    {
                        item.DetectionData = DetectionDataConverter.ToJson(detectionData);
                    }

                    // 校验必填字段
                    if (item.ProdDate == null)
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
                            item.FurnaceNoParsed = parseResult.FurnaceNoNumeric;
                            item.CoilNo = parseResult.CoilNoNumeric;
                            item.SubcoilNo = parseResult.SubcoilNoNumeric;
                            item.FeatureSuffix = parseResult.FeatureSuffix;

                            if (parseResult.ProdDate.HasValue && item.ProdDate == null)
                            {
                                item.ProdDate = parseResult.ProdDate;
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
                            // 获取有效检测列信息用于错误提示（使用JSON字段）
                            var detectionValues = DetectionDataConverter.FromJson(
                                item.DetectionData
                            );
                            var validColumns = detectionValues
                                .Where(kvp => kvp.Value.HasValue)
                                .Select(kvp => kvp.Key)
                                .OrderBy(k => k)
                                .ToList();

                            if (validColumns.Count > 0)
                            {
                                int maxIndex = validColumns.Max();
                                int minIndex = validColumns.Min();
                                int continuousCount =
                                    DetectionDataConverter.GetContinuousColumnCount(
                                        item.DetectionData
                                    );

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
            var productSpecs = await _productSpecRepository
                .AsQueryable()
                .Where(t => t.DeleteMark == null)
                .ToListAsync();

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

            // 处理数据行
            for (int rowIndex = startRowIndex; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);
                if (row == null)
                    continue;

                var entity = new RawDataEntity();
                var errors = new List<string>();

                try
                {
                    // 读取基础字段
                    entity.ProdDate = GetCellValue<DateTime?>(row, headerIndexes, "日期");
                    entity.FurnaceNo = GetCellValue<string>(row, headerIndexes, "炉号");
                    entity.Width = GetCellValue<decimal?>(row, headerIndexes, "宽度");
                    entity.CoilWeight = GetCellValue<decimal?>(row, headerIndexes, "带材重量");

                    // 动态读取所有检测数据列（使用JSON字段存储，支持任意数量的检测列）
                    var detectionData = new Dictionary<int, decimal?>();

                    // 从Excel中读取检测数据列（尝试读取到100列，实际可能更少）
                    for (int colIndex = 1; colIndex <= 100; colIndex++)
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
                                    detectionData[colIndex] = detectionValue.Value;
                                    break;
                                }
                            }
                        }

                        // 如果连续多列都没有值，停止读取（假设检测列是连续的）
                        if (!detectionValue.HasValue && detectionData.Count > 0)
                        {
                            // 如果已经有数据，但当前列没有值，可能是到了末尾
                            // 继续检查下一列，如果下一列也没有值，则停止
                            bool nextColumnHasValue = false;
                            foreach (
                                var nextHeader in new[]
                                {
                                    (colIndex + 1).ToString(),
                                    $"检测{colIndex + 1}",
                                    $"列{colIndex + 1}",
                                    $"第{colIndex + 1}列",
                                    $"检测列{colIndex + 1}",
                                }
                            )
                            {
                                if (headerIndexes.ContainsKey(nextHeader))
                                {
                                    var nextValue = GetCellValue<decimal?>(
                                        row,
                                        headerIndexes,
                                        nextHeader
                                    );
                                    if (nextValue.HasValue)
                                    {
                                        nextColumnHasValue = true;
                                        break;
                                    }
                                }
                            }

                            if (!nextColumnHasValue)
                            {
                                break; // 连续两列都没有值，停止读取
                            }
                        }
                    }

                    // 将检测数据保存到JSON字段
                    if (detectionData.Count > 0)
                    {
                        entity.DetectionData = DetectionDataConverter.ToJson(detectionData);
                    }

                    // 校验必填字段
                    if (entity.ProdDate == null)
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
                            entity.FurnaceNoParsed = parseResult.FurnaceNoNumeric;
                            entity.CoilNo = parseResult.CoilNoNumeric;
                            entity.SubcoilNo = parseResult.SubcoilNoNumeric;
                            entity.FeatureSuffix = parseResult.FeatureSuffix;

                            // 如果解析出的日期与输入的日期不一致，使用解析出的日期
                            if (parseResult.ProdDate.HasValue && entity.ProdDate == null)
                            {
                                entity.ProdDate = parseResult.ProdDate;
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
                            // 获取有效检测列信息用于错误提示（使用JSON字段）
                            var detectionValues = DetectionDataConverter.FromJson(
                                entity.DetectionData
                            );
                            var validColumns = detectionValues
                                .Where(kvp => kvp.Value.HasValue)
                                .Select(kvp => kvp.Key)
                                .OrderBy(k => k)
                                .ToList();

                            if (validColumns.Count > 0)
                            {
                                int maxIndex = validColumns.Max();
                                int minIndex = validColumns.Min();
                                int continuousCount =
                                    DetectionDataConverter.GetContinuousColumnCount(
                                        entity.DetectionData
                                    );

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

            // 批量插入成功的数据
            if (successEntities.Count > 0)
            {
                // 按排序规则排序
                successEntities = successEntities
                    .OrderBy(e => e.ProdDate)
                    .ThenBy(e => e.FurnaceNoParsed ?? int.MaxValue)
                    .ThenBy(e => e.CoilNo ?? int.MaxValue)
                    .ThenBy(e => e.SubcoilNo ?? int.MaxValue)
                    .ThenBy(e => e.LineNo ?? int.MaxValue)
                    .ThenBy(e => e.ShiftNumeric ?? int.MaxValue)
                    .ToList();

                // 设置排序码
                for (int i = 0; i < successEntities.Count; i++)
                {
                    successEntities[i].SortCode = i + 1;
                }

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
                        FurnaceNo = r.FurnaceNo,
                        LineNo = r.LineNo,
                        Shift = r.Shift,
                        FurnaceNoParsed = r.FurnaceNoParsed,
                        CoilNo = r.CoilNo,
                        SubcoilNo = r.SubcoilNo,
                        FeatureSuffix = r.FeatureSuffix,
                        Width = r.Width,
                        CoilWeight = r.CoilWeight,
                        ProductSpecId = r.ProductSpecId,
                        ProductSpecCode = r.ProductSpecCode,
                        ProductSpecName = r.ProductSpecName,
                        DetectionColumns = r.DetectionColumns,
                        DetectionData = r.DetectionData,
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
                    }
            )
            .ToListAsync();

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
            var allFeatures = await _appearanceFeatureRepository
                .AsQueryable()
                .Where(f =>
                    f.DeleteMark == null
                    && !string.IsNullOrEmpty(f.Name)
                    && !string.IsNullOrEmpty(f.CategoryId)
                )
                .ToListAsync();

            if (allFeatures.Count == 0)
                return new List<string>();

            // 2. 获取所有大类和等级，建立ID到名称的映射
            var allCategories = await _categoryRepository
                .AsQueryable()
                .Where(c => c.DeleteMark == null)
                .ToListAsync();
            var categoryIdToName = allCategories.ToDictionary(c => c.Id, c => c.Name);

            var allFeatureLevels = await _featureLevelRepository
                .AsQueryable()
                .Where(s => s.DeleteMark == null)
                .ToListAsync();
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
        // 1. 从JSON字段获取检测数据
        var detectionData = string.IsNullOrWhiteSpace(entity.DetectionData)
            ? new Dictionary<int, decimal?>()
            : DetectionDataConverter.FromJson(entity.DetectionData);

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
        int continuousCount = DetectionDataConverter.GetContinuousColumnCount(entity.DetectionData);
        int maxValidIndex = validColumns.Max();

        // 3. 遍历规格进行匹配
        foreach (var spec in productSpecs)
        {
            if (string.IsNullOrWhiteSpace(spec.DetectionColumns))
                continue;

            // 解析规格配置的检测列（如 "13" 或 "13,15,18,22"）
            var specColumns = spec
                .DetectionColumns.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s.Trim(), out var i) ? i : 0)
                .Where(i => i > 0)
                .OrderBy(i => i) // 按升序排序，优先匹配较小的检测列
                .ToList();

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
                return isDesc
                    ? data.OrderByDescending(t => t.FurnaceNoParsed ?? int.MaxValue)
                    : data.OrderBy(t => t.FurnaceNoParsed ?? int.MaxValue);

            case "coilno":
                return isDesc
                    ? data.OrderByDescending(t => t.CoilNo ?? int.MaxValue)
                    : data.OrderBy(t => t.CoilNo ?? int.MaxValue);

            case "subcoilno":
                return isDesc
                    ? data.OrderByDescending(t => t.SubcoilNo ?? int.MaxValue)
                    : data.OrderBy(t => t.SubcoilNo ?? int.MaxValue);

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
                return isDesc
                    ? orderedData.ThenByDescending(t => t.FurnaceNoParsed ?? int.MaxValue)
                    : orderedData.ThenBy(t => t.FurnaceNoParsed ?? int.MaxValue);

            case "coilno":
                return isDesc
                    ? orderedData.ThenByDescending(t => t.CoilNo ?? int.MaxValue)
                    : orderedData.ThenBy(t => t.CoilNo ?? int.MaxValue);

            case "subcoilno":
                return isDesc
                    ? orderedData.ThenByDescending(t => t.SubcoilNo ?? int.MaxValue)
                    : orderedData.ThenBy(t => t.SubcoilNo ?? int.MaxValue);

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
            .ThenBy(t => t.FurnaceNoParsed ?? int.MaxValue)
            .ThenBy(t => t.CoilNo ?? int.MaxValue)
            .ThenBy(t => t.SubcoilNo ?? int.MaxValue)
            .ThenBy(t => t.LineNo ?? int.MaxValue)
            .ThenBy(t => t.ShiftNumeric ?? int.MaxValue)
            .ToList();
    }

    #endregion
}
