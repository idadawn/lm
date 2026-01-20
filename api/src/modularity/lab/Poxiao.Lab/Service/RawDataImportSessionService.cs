using System.Text;
using System.Text.RegularExpressions;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;
using Newtonsoft.Json;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Core.Manager.Files;
using Poxiao.Infrastructure.Security;
using Poxiao.Lab.Entity.Dto.AppearanceFeature;
using Poxiao.Lab.Entity.Dto.RawData;
using Poxiao.Lab.Entity.Entity;
using Poxiao.Lab.Entity.Models;
using Poxiao.Lab.Helpers;
using Poxiao.Lab.Interfaces;
using Poxiao.Systems.Interfaces.Common;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// 原始数据导入会话服务
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "raw-data-import-session", Order = 201)]
[Route("api/lab/raw-data-import-session")]
public class RawDataImportSessionService
    : IRawDataImportSessionService,
        IDynamicApiController,
        ITransient
{
    private readonly ISqlSugarRepository<RawDataImportSessionEntity> _sessionRepository;
    private readonly ISqlSugarRepository<RawDataEntity> _rawDataRepository;
    private readonly ISqlSugarRepository<ProductSpecEntity> _productSpecRepository;
    private readonly ISqlSugarRepository<RawDataImportLogEntity> _logRepository;
    private readonly ISqlSugarRepository<AppearanceFeatureEntity> _featureRepository;
    private readonly ISqlSugarRepository<AppearanceFeatureCategoryEntity> _featureCategoryRepository;
    private readonly IAppearanceFeatureLevelService _levelService;
    private readonly IFileService _fileService;
    private readonly IUserManager _userManager;
    private readonly IIntermediateDataService _intermediateDataService;
    private readonly AppearanceFeatureRuleMatcher _featureRuleMatcher;
    private readonly ISqlSugarRepository<ProductSpecAttributeEntity> _productSpecAttributeRepository;
    private readonly IFileManager _fileManager;
    private readonly IRawDataValidationService _validationService;

    public RawDataImportSessionService(
        ISqlSugarRepository<RawDataImportSessionEntity> sessionRepository,
        ISqlSugarRepository<RawDataEntity> rawDataRepository,
        ISqlSugarRepository<ProductSpecEntity> productSpecRepository,
        ISqlSugarRepository<RawDataImportLogEntity> logRepository,
        ISqlSugarRepository<AppearanceFeatureEntity> featureRepository,
        ISqlSugarRepository<AppearanceFeatureCategoryEntity> featureCategoryRepository,
        IAppearanceFeatureLevelService levelService,
        IFileService fileService,
        IUserManager userManager,
        IIntermediateDataService intermediateDataService,
        AppearanceFeatureRuleMatcher featureRuleMatcher,
        ISqlSugarRepository<ProductSpecAttributeEntity> productSpecAttributeRepository,
        IFileManager fileManager,
        IRawDataValidationService validationService
    )
    {
        _sessionRepository = sessionRepository;
        _rawDataRepository = rawDataRepository;
        _productSpecRepository = productSpecRepository;
        _logRepository = logRepository;
        _featureRepository = featureRepository;
        _featureCategoryRepository = featureCategoryRepository;
        _levelService = levelService;
        _fileService = fileService;
        _userManager = userManager;
        _intermediateDataService = intermediateDataService;
        _featureRuleMatcher = featureRuleMatcher;
        _productSpecAttributeRepository = productSpecAttributeRepository;
        _fileManager = fileManager;
        _validationService = validationService;
    }

    /// <inheritdoc />
    [HttpPost("create")]
    public async Task<string> Create([FromBody] RawDataImportSessionInput input)
    {
        var session = new RawDataImportSessionEntity
        {
            Id = Guid.NewGuid().ToString(),
            FileName = input.FileName,
            // ImportStrategy 已废弃，固定为 "incremental" 以保持向后兼容性
            ImportStrategy = "incremental",
            Status = "pending",
            CurrentStep = 0,
            CreatorUserId = _userManager.UserId,
            CreatorTime = DateTime.Now,
        };

        // 如果提供了文件数据，保存文件
        if (!string.IsNullOrEmpty(input.FileData))
        {
            try
            {
                var fileBytes = Convert.FromBase64String(input.FileData);
                var saveFileName = $"{DateTime.Now:yyyyMMddHHmmss}_{input.FileName}";
                var basePath = _fileService.GetPathByType("RawData");
                if (string.IsNullOrEmpty(basePath))
                    basePath = _fileService.GetPathByType("");

                using var stream = new MemoryStream(fileBytes);
                await _fileManager.UploadFileByType(stream, basePath, saveFileName);
                session.SourceFileId = $"{basePath}/{saveFileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Create] File upload failed: {ex.Message}");
                // 文件保存失败不影响会话创建
            }
        }

        await _sessionRepository.InsertAsync(session);
        return session.Id;
    }

    /// <inheritdoc />
    [HttpGet("{id}")]
    public async Task<RawDataImportSessionEntity> Get(string id)
    {
        return await _sessionRepository.GetFirstAsync(t => t.Id == id);
    }

    /// <inheritdoc />
    [HttpPut("{id}/status")]
    public async Task UpdateStatus(string id, string status)
    {
        var session = await _sessionRepository.GetFirstAsync(t => t.Id == id);
        if (session == null)
            throw Oops.Oh("会话不存在");
        session.Status = status;
        await _sessionRepository.UpdateAsync(session);
    }

    /// <inheritdoc />
    [HttpPut("{id}/step")]
    public async Task UpdateStep(string id, int step)
    {
        var session = await _sessionRepository.GetFirstAsync(t => t.Id == id);
        if (session == null)
            throw Oops.Oh("会话不存在");
        session.CurrentStep = step;
        await _sessionRepository.UpdateAsync(session);
    }

    /// <inheritdoc />
    [HttpPost("step1/upload-and-parse")]
    public async Task<RawDataImportStep1Output> UploadAndParse(
        [FromBody] RawDataImportStep1Input input
    )
    {
        // 1. 获取已存在的会话
        if (string.IsNullOrEmpty(input.ImportSessionId))
            throw Oops.Oh("导入会话ID不能为空");

        var existingSession = await _sessionRepository.GetFirstAsync(t =>
            t.Id == input.ImportSessionId
        );
        if (existingSession == null)
            throw Oops.Oh("导入会话不存在");

        string sessionId = input.ImportSessionId;

        // 更新会话信息
        if (!string.IsNullOrEmpty(input.FileName))
            existingSession.FileName = input.FileName;
        // ImportStrategy 已废弃，不再更新
        existingSession.Status = "in_progress";
        await _sessionRepository.UpdateAsync(existingSession);

        // 2. 读取已保存的文件
        byte[] fileBytes;
        if (!string.IsNullOrEmpty(input.FileData))
        {
            // 如果请求中有文件数据，使用请求中的（兼容旧版本）
            fileBytes = Convert.FromBase64String(input.FileData);
        }
        else if (!string.IsNullOrEmpty(existingSession.SourceFileId))
        {
            // 从已保存的文件中读取
            try
            {
                fileBytes = await File.ReadAllBytesAsync(existingSession.SourceFileId);
            }
            catch (Exception ex)
            {
                throw Oops.Oh($"读取已保存的文件失败: {ex.Message}");
            }
        }
        else
        {
            throw Oops.Oh("文件数据不存在，请重新上传文件");
        }

        // 导入策略已废弃，不再支持增量导入，始终从第0行开始解析
        int skipRows = 0;

        // 保存文件
        var sourceFileId = "";
        var saveFileName = $"{DateTime.Now:yyyyMMddHHmmss}_{input.FileName}";
        var basePath = _fileService.GetPathByType("RawData");
        if (string.IsNullOrEmpty(basePath))
            basePath = _fileService.GetPathByType("");

        try
        {
            using var stream = new MemoryStream(fileBytes);
            await _fileManager.UploadFileByType(stream, basePath, saveFileName);
            // Use path + filename as ID for traceability
            sourceFileId = $"{basePath}/{saveFileName}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UploadAndParse] File upload failed: {ex.Message}");
            // Optional: throw if file saving is critical
            // throw Oops.Oh($"文件保存失败: {ex.Message}");
        }

        var entities = ParseExcel(fileBytes, input.FileName, skipRows);

        // 3. 将解析后的数据保存为JSON文件（不写入数据库，等待CompleteImport时才写入）
        // 为每条数据分配ID
        foreach (var entity in entities)
        {
            entity.Id = SnowflakeIdHelper.NextId();
            entity.ImportSessionId = sessionId;
            entity.CreatorUserId = _userManager.UserId;
            entity.CreatorTime = DateTime.Now;
            entity.LastModifyUserId = _userManager.UserId;
            entity.LastModifyTime = DateTime.Now;
        }

        // 将数据序列化为JSON文件保存（即使没有数据也要创建文件，确保后续步骤可以正常加载）
        string parsedDataFile = "";
        try
        {
            // 使用 Path.Combine 确保跨平台兼容性
            parsedDataFile = Path.Combine(basePath, $"{sessionId}_parsed.json");
            var jsonData = JsonConvert.SerializeObject(entities, Formatting.None);
            await File.WriteAllTextAsync(parsedDataFile, jsonData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UploadAndParse] Save parsed data failed: {ex.Message}");
            throw Oops.Oh($"保存解析数据失败: {ex.Message}");
        }

        // 4. 更新会话（不再保存到数据库，只保存到JSON文件）
        var session = await _sessionRepository.GetFirstAsync(t => t.Id == sessionId);
        session.TotalRows = entities.Count;
        session.ValidDataRows = entities.Count(x => x.IsValidData == 1);
        session.CurrentStep = 1;
        session.SourceFileId = sourceFileId;
        session.ParsedDataFile = parsedDataFile;
        await _sessionRepository.UpdateAsync(session);

        // 5. 转换为预览项并检查重复和数据库中已存在的炉号
        var previewItems = entities
            .Select(e =>
            {
                var item = e.Adapt<RawDataPreviewItem>();
                item.Status = e.ImportStatus == 0 ? "success" : "failed";
                item.ErrorMessage = e.ImportError;
                // 构建标准炉号（用于重复检查）
                if (item.IsValidData == 1 &&
                    item.LineNo.HasValue &&
                    !string.IsNullOrWhiteSpace(item.Shift) &&
                    item.ProdDate.HasValue &&
                    item.FurnaceBatchNo.HasValue)
                {
                    item.StandardFurnaceNo = GetFurnaceNo(item);
                }
                return item;
            })
            .ToList();

        // 检查重复的炉号（只检查符合规则的有效数据）
        CheckDuplicateFurnaceNoForPreview(previewItems);

        // 检查数据库中已存在的炉号
        await CheckExistingFurnaceNoInDatabaseForPreview(previewItems);

        // 6. 返回结果（返回全部数据，前端分页展示）
        var previewOutput = new RawDataPreviewOutput
        {
            ParsedData = previewItems,
            SkippedRows = skipRows,
        };

        return new RawDataImportStep1Output
        {
            ImportSessionId = sessionId,
            TotalRows = session.TotalRows ?? 0,
            ValidDataRows = session.ValidDataRows ?? 0,
            PreviewData = previewOutput,
        };
    }

    /// <inheritdoc />
    [HttpGet("{sessionId}/product-specs")]
    public async Task<List<RawDataProductSpecMatchOutput>> GetProductSpecMatches(string sessionId)
    {
        // 从JSON文件读取数据
        var entities = await LoadParsedDataFromFile(sessionId);

        // 只返回有效数据（IsValidData == 1）
        var validEntities = entities
            .Where(t => t.IsValidData == 1)
            .OrderBy(t => t.SortCode)
            .ToList();

        // 记录日志以便调试
        Console.WriteLine($"[GetProductSpecMatches] 总数据: {entities.Count}, 有效数据: {validEntities.Count}");

        return validEntities
            .Select(t => new RawDataProductSpecMatchOutput
            {
                RawDataId = t.Id,
                FurnaceNo = t.FurnaceNo,
                ProdDate = t.ProdDate,
                Width = t.Width,
                CoilWeight = t.CoilWeight,
                BreakCount = t.BreakCount,
                SingleCoilWeight = t.SingleCoilWeight,
                DetectionColumns = t.DetectionColumns,
                ProductSpecId = t.ProductSpecId,
                ProductSpecName = t.ProductSpecName,
                ProductSpecCode = t.ProductSpecCode,
                MatchStatus = t.ProductSpecId == null ? "unmatched" : "matched"
            })
            .ToList();
    }

    /// <summary>
    /// 从JSON文件加载解析后的数据
    /// </summary>
    private async Task<List<RawDataEntity>> LoadParsedDataFromFile(string sessionId)
    {
        var session = await _sessionRepository.GetFirstAsync(t => t.Id == sessionId);
        if (session == null)
            throw Oops.Oh("导入会话不存在");

        // 如果会话已完成，临时文件已被清理，返回友好提示
        if (session.Status == "completed" || session.Status == "cancelled")
        {
            throw Oops.Oh(
                $"导入会话已{GetStatusText(session.Status)}，无法加载数据。请创建新的导入会话。"
            );
        }

        if (string.IsNullOrEmpty(session.ParsedDataFile))
            return new List<RawDataEntity>(); // 文件可能还在处理中，返回空列表

        if (!File.Exists(session.ParsedDataFile))
            return new List<RawDataEntity>(); // 文件可能已被清理或移动，返回空列表

        var jsonData = await File.ReadAllTextAsync(session.ParsedDataFile);
        var entities = JsonConvert.DeserializeObject<List<RawDataEntity>>(jsonData);

        return entities ?? new List<RawDataEntity>();
    }

    /// <summary>
    /// 获取状态文本
    /// </summary>
    private string GetStatusText(string status)
    {
        return status switch
        {
            "completed" => "完成",
            "cancelled" => "取消",
            "failed" => "失败",
            _ => status,
        };
    }

    /// <summary>
    /// 保存解析后的数据到JSON文件
    /// </summary>
    private async Task SaveParsedDataToFile(string sessionId, List<RawDataEntity> entities)
    {
        var session = await _sessionRepository.GetFirstAsync(t => t.Id == sessionId);
        if (session == null)
            throw Oops.Oh("导入会话不存在");

        if (string.IsNullOrEmpty(session.ParsedDataFile))
            throw Oops.Oh("解析数据文件路径不存在");

        var jsonData = JsonConvert.SerializeObject(entities, Formatting.None);
        await File.WriteAllTextAsync(session.ParsedDataFile, jsonData);
    }

    /// <inheritdoc />
    [HttpPut("{sessionId}/product-specs")]
    public async Task UpdateProductSpecs(
        string sessionId,
        [FromBody] RawDataUpdateProductSpecsInput input
    )
    {
        // 确保 sessionId 匹配
        if (input.SessionId != sessionId)
        {
            input.SessionId = sessionId;
        }

        // 从JSON文件加载数据
        var entities = await LoadParsedDataFromFile(sessionId);
        var entityDict = entities.ToDictionary(e => e.Id, e => e);

        foreach (var item in input.Items)
        {
            // 获取产品规格详情
            string specCode = null;
            string specName = null;
            string columns = null;

            if (!string.IsNullOrEmpty(item.ProductSpecId))
            {
                var spec = await _productSpecRepository.GetByIdAsync(item.ProductSpecId);
                if (spec != null)
                {
                    specCode = spec.Code;
                    specName = spec.Name;
                    columns = spec.DetectionColumns;
                }
            }

            // 更新JSON中的数据
            if (entityDict.TryGetValue(item.RawDataId, out var entity))
            {
                entity.ProductSpecId = item.ProductSpecId;
                entity.ProductSpecCode = specCode;
                entity.ProductSpecName = specName;
                entity.DetectionColumns = columns;
            }
        }

        // 保存回JSON文件
        await SaveParsedDataToFile(sessionId, entities);
    }

    /// <summary>
    /// 更新重复数据的选择结果（将未选择的数据标记为无效）
    /// </summary>
    [HttpPut("{sessionId}/duplicate-selections")]
    public async Task UpdateDuplicateSelections(
        string sessionId,
        [FromBody] RawDataUpdateDuplicateSelectionsInput input
    )
    {
        // 从JSON文件加载数据
        var entities = await LoadParsedDataFromFile(sessionId);
        var entityDict = entities.ToDictionary(e => e.Id, e => e);

        // 检查输入是否有效
        if (input?.Items == null || input.Items.Count == 0)
        {
            // 没有更新项，直接返回
            Console.WriteLine($"[UpdateDuplicateSelections] 没有更新项");
            return;
        }

        Console.WriteLine($"[UpdateDuplicateSelections] 收到 {input.Items.Count} 条更新请求");

        bool needSave = false;
        int updatedCount = 0;
        foreach (var item in input.Items)
        {
            if (string.IsNullOrEmpty(item.RawDataId))
                continue;

            // 更新JSON中的数据
            if (entityDict.TryGetValue(item.RawDataId, out var entity))
            {
                var oldIsValidData = entity.IsValidData;
                // 更新 IsValidData 字段
                entity.IsValidData = item.IsValidData ? 1 : 0;
                
                // 如果值发生了变化，标记需要保存
                if (oldIsValidData != entity.IsValidData)
                {
                    needSave = true;
                    updatedCount++;
                    Console.WriteLine($"[UpdateDuplicateSelections] 更新数据ID: {item.RawDataId}, IsValidData: {oldIsValidData} -> {entity.IsValidData}");
                }
                
                // 如果被标记为无效，更新错误信息
                if (entity.IsValidData == 0)
                {
                    if (string.IsNullOrWhiteSpace(entity.ImportError))
                    {
                        entity.ImportError = "重复数据，已选择保留其他数据";
                        needSave = true;
                    }
                    else if (!entity.ImportError.Contains("重复数据，已选择保留其他数据"))
                    {
                        entity.ImportError = $"重复数据，已选择保留其他数据；{entity.ImportError}";
                        needSave = true;
                    }
                }
                else if (entity.IsValidData == 1)
                {
                    // 如果被标记为有效，移除"重复数据，已选择保留其他数据"的错误信息
                    if (!string.IsNullOrWhiteSpace(entity.ImportError) && 
                        entity.ImportError.Contains("重复数据，已选择保留其他数据"))
                    {
                        entity.ImportError = entity.ImportError
                            .Replace("重复数据，已选择保留其他数据；", "")
                            .Replace("重复数据，已选择保留其他数据", "")
                            .Trim();
                        if (string.IsNullOrWhiteSpace(entity.ImportError))
                        {
                            entity.ImportError = null;
                        }
                        needSave = true;
                    }
                }
            }
            else
            {
                Console.WriteLine($"[UpdateDuplicateSelections] 警告：找不到数据ID: {item.RawDataId}");
            }
        }

        // 如果有更新，保存回JSON文件并更新会话统计
        if (needSave)
        {
            await SaveParsedDataToFile(sessionId, entities);
            
            // 更新会话的有效数据行数
            var session = await _sessionRepository.GetFirstAsync(t => t.Id == sessionId);
            if (session != null)
            {
                var validCount = entities.Count(x => x.IsValidData == 1);
                session.ValidDataRows = validCount;
                await _sessionRepository.UpdateAsync(session);
                
                // 记录日志以便调试
                Console.WriteLine($"[UpdateDuplicateSelections] 更新了 {input.Items.Count} 条数据，有效数据行数: {validCount}/{entities.Count}");
            }
        }
        else
        {
            Console.WriteLine($"[UpdateDuplicateSelections] 没有需要保存的更新");
        }
    }

    /// <inheritdoc />
    [HttpGet("{sessionId}/features")]
    public async Task<List<RawDataFeatureMatchOutput>> GetFeatureMatches(string sessionId)
    {
        // 从JSON文件加载数据
        var allEntities = await LoadParsedDataFromFile(sessionId);
        var validEntities = allEntities.Where(t => t.IsValidData == 1).ToList();

        var unMatchedEntities = validEntities
            .Where(t =>
                t.AppearanceFeatureIds == null && t.FeatureSuffix != null && t.FeatureSuffix != ""
            )
            .ToList();

        var features = await _featureRepository.GetListAsync();

        bool needSave = false;
        if (unMatchedEntities.Count > 0)
        {
            // 执行自动匹配
            var levels = await _levelService.GetEnabledLevels();
            var categories = await _featureCategoryRepository.GetListAsync();

            var degreeWords = levels.Select(l => l.Name).ToList();
            var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);
            var levelMap = levels.ToDictionary(l => l.Id, l => l.Name);

            foreach (var entity in unMatchedEntities)
            {
                var matches = _featureRuleMatcher.Match(
                    entity.FeatureSuffix,
                    features,
                    degreeWords,
                    categoryMap,
                    levelMap
                );
                var matchedIds = matches.Select(m => m.Feature.Id).ToList();
                entity.AppearanceFeatureIds = JsonConvert.SerializeObject(matchedIds);
                // 存储最高置信度
                if (matches.Any())
                {
                    entity.MatchConfidence = matches.Max(m => m.Confidence);
                }
            }

            needSave = true;
        }

        // 保存回JSON文件
        if (needSave)
        {
            await SaveParsedDataToFile(sessionId, allEntities);
        }

        // 2. 返回结果
        var data = validEntities
            .Select(t => new RawDataFeatureMatchOutput
            {
                RawDataId = t.Id,
                FurnaceNo = t.FurnaceNo,
                FeatureSuffix = t.FeatureSuffix,
            })
            .ToList();

        // 填充 FeatureIds, MatchConfidence 和 MatchDetails
        var entityDict = validEntities.ToDictionary(e => e.Id, e => e);

        foreach (var item in data)
        {
            if (entityDict.TryGetValue(item.RawDataId, out var entity))
            {
                item.MatchConfidence = entity.MatchConfidence;

                if (!string.IsNullOrEmpty(entity.AppearanceFeatureIds))
                {
                    item.AppearanceFeatureIds = JsonConvert.DeserializeObject<List<string>>(
                        entity.AppearanceFeatureIds
                    );

                    // 填充匹配详情
                    if (item.AppearanceFeatureIds != null && item.AppearanceFeatureIds.Any())
                    {
                        item.MatchDetails = item
                            .AppearanceFeatureIds.Select(id =>
                            {
                                var f = features.FirstOrDefault(x => x.Id == id);
                                return new FeatureMatchDetail
                                {
                                    FeatureId = id,
                                    FeatureName = f?.Name ?? id,
                                    Confidence = item.MatchConfidence?.ToString() ?? "1.0",
                                };
                            })
                            .ToList();
                    }
                }
            }
        }

        return data;
    }

    /// <inheritdoc />
    [HttpPut("{sessionId}/features")]
    public async Task UpdateFeatures(string sessionId, [FromBody] RawDataUpdateFeaturesInput input)
    {
        // 从JSON文件加载数据
        var entities = await LoadParsedDataFromFile(sessionId);

        // 检查输入是否有效
        if (input?.Items == null || input.Items.Count == 0)
        {
            // 没有更新项，直接返回
            return;
        }

        var entityDict = entities.ToDictionary(e => e.Id, e => e);

        foreach (var item in input.Items)
        {
            if (string.IsNullOrEmpty(item.RawDataId))
                continue;

            var json = JsonConvert.SerializeObject(item.AppearanceFeatureIds ?? new List<string>());
            if (entityDict.TryGetValue(item.RawDataId, out var entity))
            {
                entity.AppearanceFeatureIds = json;
            }
        }

        // 保存回JSON文件
        await SaveParsedDataToFile(sessionId, entities);
    }

    /// <inheritdoc />
    [HttpGet("{sessionId}/review")]
    public async Task<RawDataReviewOutput> GetReviewData(string sessionId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
            throw Oops.Oh("导入会话不存在");

        // 如果会话已完成，返回已完成的信息（不加载临时文件）
        if (session.Status == "completed" || session.Status == "cancelled")
        {
            return new RawDataReviewOutput
            {
                Session = session,
                TotalRows = session.TotalRows ?? 0,
                ValidDataRows = session.ValidDataRows ?? 0,
                MatchedSpecRows = 0,
                MatchedFeatureRows = 0,
                MatchStatus = session.Status == "completed" ? "completed" : "cancelled",
                Errors = new List<string> { $"导入会话已{GetStatusText(session.Status)}" },
                PreviewIntermediateData = new List<RawDataEntity>(),
            };
        }

        // 检查是否已完成第一步（文件上传和解析）
        if (string.IsNullOrEmpty(session.ParsedDataFile))
        {
            return new RawDataReviewOutput
            {
                Session = session,
                TotalRows = 0,
                ValidDataRows = 0,
                MatchedSpecRows = 0,
                MatchedFeatureRows = 0,
                MatchStatus = "error",
                Errors = new List<string> { "请先完成第一步：文件上传和数据解析" },
                PreviewIntermediateData = new List<RawDataEntity>(),
            };
        }

        // 从JSON文件加载数据
        var allEntities = await LoadParsedDataFromFile(sessionId);

        var total = allEntities.Count;
        var validEntities = allEntities.Where(t => t.IsValidData == 1).ToList();
        var valid = validEntities.Count;
        var matchedSpec = allEntities.Count(t => !string.IsNullOrEmpty(t.ProductSpecId));
        var matchedFeature = allEntities.Count(t => !string.IsNullOrEmpty(t.AppearanceFeatureIds));

        // 预览中间数据
        var previewData = validEntities.Take(10).ToList();

        var errors = new List<string>();

        // Use Validation Service to check a sample or aggregate errors
        // Check for general integrity issues affecting completion

        if (matchedSpec < valid)
        {
            errors.Add(
                $"There are {valid - matchedSpec} rows with unidentified product specifications."
            );
        }

        // Check for integrity (e.g. missing spec id)
        var invalidSpecEntities = validEntities
            .Where(t => string.IsNullOrEmpty(t.ProductSpecId))
            .Take(5)
            .ToList();

        foreach (var entity in invalidSpecEntities)
        {
            var entityErrors = _validationService.ValidateIntegrity(entity);
            foreach (var err in entityErrors)
            {
                if (!errors.Contains(err))
                    errors.Add($"Row {entity.FurnaceNo}: {err}");
            }
        }

        // Also check if any features are missing if required? (Assuming optional for now)

        return new RawDataReviewOutput
        {
            Session = session,
            TotalRows = total,
            ValidDataRows = valid,
            MatchedSpecRows = matchedSpec,
            MatchedFeatureRows = matchedFeature,
            MatchStatus = matchedSpec == valid ? "ok" : "warning",
            Errors = errors,
            PreviewIntermediateData = previewData,
        };
    }

    /// <inheritdoc />
    [HttpPost("{sessionId}/complete")]
    public async Task CompleteImport(string sessionId)
    {
        // 0. 获取会话对象
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
            throw Oops.Oh("导入会话不存在");

        // 1. 从JSON文件加载数据
        var allData = await LoadParsedDataFromFile(sessionId);
        var validData = allData.Where(t => t.IsValidData == 1).ToList();

        if (validData.Count == 0)
        {
            session.Status = "failed";
            await _sessionRepository.UpdateAsync(session);
            throw Oops.Oh("没有有效数据可以导入");
        }

        // 1.0 处理重复的炉号（保留第一条，移除其他）
        var validDataToCheck = validData
            .Where(t => t.LineNo.HasValue && 
                       !string.IsNullOrWhiteSpace(t.Shift) && 
                       t.ProdDate.HasValue && 
                       t.FurnaceBatchNo.HasValue)
            .ToList();

        if (validDataToCheck.Count > 0)
        {
            // 按标准炉号分组，每组只保留第一条
            var groupedByFurnaceNo = validDataToCheck
                .GroupBy(t => GetFurnaceNo(t))
                .Where(g => !string.IsNullOrEmpty(g.Key))
                .ToList();

            var keepIds = new HashSet<string>();
            foreach (var group in groupedByFurnaceNo)
            {
                // 每组按SortCode排序，保留第一条（行号最小的）
                var first = group.OrderBy(t => t.SortCode).First();
                keepIds.Add(first.Id);
            }

            // 从有效数据中移除重复的炉号（保留第一条）
            validData = validData
                .Where(t =>
                {
                    if (!t.LineNo.HasValue || 
                        string.IsNullOrWhiteSpace(t.Shift) || 
                        !t.ProdDate.HasValue || 
                        !t.FurnaceBatchNo.HasValue)
                    {
                        return true; // 不符合规则的数据保留（会被后续逻辑处理）
                    }
                    // 如果是重复的炉号，只保留第一条
                    return keepIds.Contains(t.Id);
                })
                .ToList();
        }

        // 1.1 检查数据库中已存在的炉号，忽略这些数据
        validDataToCheck = validData
            .Where(t => t.LineNo.HasValue && 
                       !string.IsNullOrWhiteSpace(t.Shift) && 
                       t.ProdDate.HasValue && 
                       t.FurnaceBatchNo.HasValue)
            .ToList();

        if (validDataToCheck.Count > 0)
        {
            // 构建标准炉号列表
            var standardFurnaceNos = validDataToCheck
                .Select(t => GetFurnaceNo(t))
                .Where(f => !string.IsNullOrEmpty(f))
                .Distinct()
                .ToList();

            // 查询数据库中已存在的标准炉号
            var dbEntities = await _rawDataRepository
                .AsQueryable()
                .Where(e => e.IsValidData == 1 && 
                           e.LineNo.HasValue && 
                           !string.IsNullOrWhiteSpace(e.Shift) && 
                           e.ProdDate.HasValue && 
                           e.FurnaceBatchNo.HasValue)
                .Select(e => new
                {
                    e.LineNo,
                    e.Shift,
                    e.ProdDate,
                    e.FurnaceBatchNo
                })
                .ToListAsync();

            var existingFurnaceNos = new HashSet<string>();
            foreach (var entity in dbEntities)
            {
                var furnaceNoObj = FurnaceNo.Build(
                    entity.LineNo.Value.ToString(),
                    entity.Shift,
                    entity.ProdDate,
                    entity.FurnaceBatchNo.Value.ToString(),
                    "1", // 卷号默认为1
                    "1"  // 分卷号默认为1
                );
                var standardFurnaceNo = furnaceNoObj?.GetFurnaceNo();
                if (!string.IsNullOrEmpty(standardFurnaceNo) && standardFurnaceNos.Contains(standardFurnaceNo))
                {
                    existingFurnaceNos.Add(standardFurnaceNo);
                }
            }

            // 从有效数据中移除数据库中已存在的炉号
            validData = validData
                .Where(t =>
                {
                    if (!t.LineNo.HasValue || 
                        string.IsNullOrWhiteSpace(t.Shift) || 
                        !t.ProdDate.HasValue || 
                        !t.FurnaceBatchNo.HasValue)
                    {
                        return true; // 不符合规则的数据保留（会被后续逻辑处理）
                    }
                    var standardFurnaceNo = GetFurnaceNo(t);
                    return string.IsNullOrEmpty(standardFurnaceNo) || !existingFurnaceNos.Contains(standardFurnaceNo);
                })
                .ToList();
        }

        // 1.1 检查是否有未匹配产品规格的数据，尝试重新匹配
        var unmatchedSpecData = validData
            .Where(t => string.IsNullOrEmpty(t.ProductSpecId))
            .ToList();
        if (unmatchedSpecData.Count > 0)
        {
            var productSpecs = await _productSpecRepository.GetListAsync();
            foreach (var entity in unmatchedSpecData)
            {
                var spec = IdentifyProductSpec(entity, productSpecs);
                if (spec != null)
                {
                    entity.ProductSpecId = spec.Id;
                    entity.ProductSpecCode = spec.Code;
                    entity.ProductSpecName = spec.Name;
                    entity.DetectionColumns = spec.DetectionColumns;
                }
            }
        }

        // 2. 将所有数据写入原始数据表（包括有效和无效数据）
        if (allData.Count > 0)
        {
            // 分批插入防止SQL太长
            var batches = allData.Chunk(1000);
            foreach (var batch in batches)
            {
                await _rawDataRepository.AsInsertable(batch.ToList()).ExecuteCommandAsync();
            }
        }

        // 2. 生成中间数据
        var specs = await _productSpecRepository.GetListAsync();
        var intermediateEntities = new List<IntermediateDataEntity>();

        var dataBySpec = validData.GroupBy(t => t.ProductSpecId);

        foreach (var group in dataBySpec)
        {
            if (string.IsNullOrEmpty(group.Key))
                continue;

            var spec = specs.FirstOrDefault(s => s.Id == group.Key);
            if (spec == null)
                continue;

            // 查询产品扩展属性（长度、层数、密度）
            var attributes = await _productSpecAttributeRepository
                .AsQueryable()
                .Where(t => t.ProductSpecId == spec.Id && t.DeleteMark == null)
                .ToListAsync();

            decimal length = 4m;
            int layers = 20;
            decimal density = 7.25m;

            var lengthAttr = attributes.FirstOrDefault(a => a.AttributeKey == "length");
            if (
                lengthAttr != null
                && !string.IsNullOrEmpty(lengthAttr.AttributeValue)
                && decimal.TryParse(lengthAttr.AttributeValue, out var l)
            )
            {
                length = l;
            }

            var layersAttr = attributes.FirstOrDefault(a => a.AttributeKey == "layers");
            if (
                layersAttr != null
                && !string.IsNullOrEmpty(layersAttr.AttributeValue)
                && int.TryParse(layersAttr.AttributeValue, out var lay)
            )
            {
                layers = lay;
            }

            var densityAttr = attributes.FirstOrDefault(a => a.AttributeKey == "density");
            if (
                densityAttr != null
                && !string.IsNullOrEmpty(densityAttr.AttributeValue)
                && decimal.TryParse(densityAttr.AttributeValue, out var d)
            )
            {
                density = d;
            }

            var detectionColumns = _intermediateDataService.ParseDetectionColumns(
                spec.DetectionColumns
            );

            foreach (var raw in group)
            {
                var intermediate = _intermediateDataService.GenerateIntermediateData(
                    raw,
                    spec,
                    detectionColumns,
                    layers,
                    length,
                    density,
                    null
                );
                intermediate.CreatorUserId = _userManager.UserId;
                intermediateEntities.Add(intermediate);
            }
        }

        if (intermediateEntities.Count > 0)
        {
            // Use context from _sessionRepository to insert intermediate data if repository not injected
            await _sessionRepository
                .AsSugarClient()
                .Insertable(intermediateEntities)
                .ExecuteCommandAsync();
        }

        // 3. Mark Session Complete
        session.Status = "completed";
        session.TotalRows = allData.Count; // 更新总数据行数
        session.ValidDataRows = validData.Count; // 更新有效数据行数
        await _sessionRepository.UpdateAsync(session);

        // 4. Create Log
        // 获取本次导入时跳过的行数（用于计算累计行数）
        int previousSkipRows = 0;
        var lastLog = await _logRepository
            .AsQueryable()
            .Where(t => t.FileName == session.FileName && t.Status == "success")
            .OrderByDescending(t => t.ImportTime)
            .FirstAsync();
        if (lastLog != null)
        {
            previousSkipRows = lastLog.TotalRows;
        }

        var log = new RawDataImportLogEntity
        {
            FileName = session.FileName,
            TotalRows = previousSkipRows + allData.Count, // 累计的行数（上次跳过的 + 本次导入的）
            SuccessCount = validData.Count, // 有效数据行数（导入到中间数据表）
            FailCount = allData.Count - validData.Count, // 无效数据行数
            Status = "success",
            ImportTime = DateTime.Now,
            ImportSessionId = sessionId,
            SourceFileId = session.SourceFileId,
            ValidDataCount = validData.Count,
            LastRowsHash = GenerateLastRowHash(allData.LastOrDefault()),
            LastRowsCount = 1,
        };
        log.Creator();
        await _logRepository.InsertAsync(log);

        // 5. 清理临时JSON文件（保留 session 记录用于查询）
        try
        {
            if (
                !string.IsNullOrEmpty(session.ParsedDataFile) && File.Exists(session.ParsedDataFile)
            )
            {
                File.Delete(session.ParsedDataFile);
                session.ParsedDataFile = null;
            }
            // 更新 session 状态为已完成，但不删除（保留用于查询）
            session.Status = "completed";
            await _sessionRepository.UpdateAsync(session);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CompleteImport] Failed to cleanup: {ex.Message}");
            // 不影响主流程，但尝试标记 session 状态
            try
            {
                session.Status = "completed";
                await _sessionRepository.UpdateAsync(session);
            }
            catch
            {
                // 忽略更新失败
            }
        }
    }

    /// <inheritdoc />
    [HttpDelete("{sessionId}")]
    public async Task CancelImport(string sessionId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
        {
            // Session 不存在，直接返回（可能已经被清理）
            return;
        }

        try
        {
            // 清理临时JSON文件
            if (
                !string.IsNullOrEmpty(session.ParsedDataFile) && File.Exists(session.ParsedDataFile)
            )
            {
                File.Delete(session.ParsedDataFile);
            }

            // 清理源文件（可选，如果其他会话可能使用则保留）
            // if (!string.IsNullOrEmpty(session.SourceFileId) && File.Exists(session.SourceFileId))
            // {
            //     File.Delete(session.SourceFileId);
            // }

            // 删除 session
            await _sessionRepository.DeleteByIdAsync(sessionId);
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[CancelImport] Failed to cleanup session {sessionId}: {ex.Message}"
            );
            // 即使清理失败，也尝试标记为已取消
            try
            {
                session.Status = "cancelled";
                await _sessionRepository.UpdateAsync(session);
            }
            catch
            {
                // 忽略更新失败
            }
            throw Oops.Oh($"取消导入失败: {ex.Message}");
        }
    }

    // ================= Private Methods =================

    /// <summary>
    /// 从RawDataEntity获取标准炉号
    /// 格式：[产线数字][班次汉字][8位日期]-[炉次号]-[卷号]-[分卷号]
    /// </summary>
    private string GetFurnaceNo(RawDataEntity entity)
    {
        if (entity == null ||
            !entity.LineNo.HasValue ||
            string.IsNullOrWhiteSpace(entity.Shift) ||
            !entity.ProdDate.HasValue ||
            !entity.FurnaceBatchNo.HasValue)
        {
            return null;
        }

        var furnaceNoObj = FurnaceNo.Build(
            entity.LineNo.Value.ToString(),
            entity.Shift,
            entity.ProdDate,
            entity.FurnaceBatchNo.Value.ToString(),
            entity.CoilNo?.ToString() ?? "1",
            entity.SubcoilNo?.ToString() ?? "1"
        );

        return furnaceNoObj?.GetFurnaceNo();
    }

    /// <summary>
    /// 从RawDataPreviewItem获取标准炉号
    /// 格式：[产线数字][班次汉字][8位日期]-[炉次号]-[卷号]-[分卷号]
    /// </summary>
    private string GetFurnaceNo(RawDataPreviewItem item)
    {
        if (item == null ||
            !item.LineNo.HasValue ||
            string.IsNullOrWhiteSpace(item.Shift) ||
            !item.ProdDate.HasValue ||
            !item.FurnaceBatchNo.HasValue)
        {
            return null;
        }

        var furnaceNoObj = FurnaceNo.Build(
            item.LineNo.Value.ToString(),
            item.Shift,
            item.ProdDate,
            item.FurnaceBatchNo.Value.ToString(),
            item.CoilNo?.ToString() ?? "1",
            item.SubcoilNo?.ToString() ?? "1"
        );

        return furnaceNoObj?.GetFurnaceNo();
    }

    private List<RawDataEntity> ParseExcel(byte[] fileBytes, string fileName, int skipRows)
    {
        var entities = new List<RawDataEntity>();
        
        // 先获取所有列名，用于通过索引访问列（AA列=索引26，AB列=索引27）
        string breakCountColumnName = null;
        string singleCoilWeightColumnName = null;
        
        using (var streamForColumns = new MemoryStream(fileBytes))
        {
            var columns = MiniExcelLibs.MiniExcel.GetColumns(streamForColumns, useHeaderRow: true).ToList();
            
            // AA列是第27列（索引26），AB列是第28列（索引27）
            if (columns.Count > 26)
            {
                breakCountColumnName = columns[26]; // AA列
            }
            if (columns.Count > 27)
            {
                singleCoilWeightColumnName = columns[27]; // AB列
            }
        }
        
        // 使用新的流读取数据行
        using var stream = new MemoryStream(fileBytes);
        // Use HeaderRow = true to get key-value pairs
        var rows = stream.Query(useHeaderRow: true).Cast<IDictionary<string, object>>().ToList();

        // Cache Product Specs for matching
        var productSpecs = _productSpecRepository.GetList();

        foreach (var row in rows)
        {
            var entity = new RawDataEntity();
            entity.DetectionDate = GetValue<DateTime?>(row, "日期");
            entity.FurnaceNo = GetValue<string>(row, "炉号");
            entity.Width = GetValue<decimal?>(row, "宽度");
            entity.CoilWeight = GetValue<decimal?>(row, "带材重量");
            
            // 读取断头数：先尝试通过表头名称读取，如果失败则通过列索引读取
            if (!entity.BreakCount.HasValue)
            {
                var breakCountHeaders = new[] { "断头数", "断头", "BreakCount", "AA" };
                foreach (var header in breakCountHeaders)
                {
                    if (row.ContainsKey(header))
                    {
                        var value = GetValue<int?>(row, header);
                        if (value.HasValue)
                        {
                            entity.BreakCount = value;
                            break;
                        }
                    }
                }
            }
            
            // 如果通过表头名称读取失败，尝试通过列索引读取（AA列=索引26）
            if (!entity.BreakCount.HasValue && !string.IsNullOrEmpty(breakCountColumnName) && row.ContainsKey(breakCountColumnName))
            {
                entity.BreakCount = GetValue<int?>(row, breakCountColumnName);
            }
            
            // 读取单卷重量：先尝试通过表头名称读取，如果失败则通过列索引读取
            if (!entity.SingleCoilWeight.HasValue)
            {
                var singleCoilWeightHeaders = new[] { "单卷重量", "单卷重", "SingleCoilWeight", "AB" };
                foreach (var header in singleCoilWeightHeaders)
                {
                    if (row.ContainsKey(header))
                    {
                        var value = GetValue<decimal?>(row, header);
                        if (value.HasValue)
                        {
                            entity.SingleCoilWeight = value;
                            break;
                        }
                    }
                }
            }
            
            // 如果通过表头名称读取失败，尝试通过列索引读取（AB列=索引27）
            if (!entity.SingleCoilWeight.HasValue && !string.IsNullOrEmpty(singleCoilWeightColumnName) && row.ContainsKey(singleCoilWeightColumnName))
            {
                entity.SingleCoilWeight = GetValue<decimal?>(row, singleCoilWeightColumnName);
            }

            // 检测数据列1-22（固定22列）
            for (int colIndex = 1; colIndex <= 22; colIndex++)
            {
                decimal? detectionValue = null;
                var possibleHeaders = new[]
                {
                    colIndex.ToString(),
                    $"检测{colIndex}",
                    $"列{colIndex}",
                    $"第{colIndex}列",
                    $"检测列{colIndex}",
                };
                foreach (var h in possibleHeaders)
                {
                    if (row.ContainsKey(h))
                    {
                        detectionValue = GetValue<decimal?>(row, h);
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

            // Furnace Parse using Helper
            if (!string.IsNullOrWhiteSpace(entity.FurnaceNo))
            {
                var furnaceNoObj = FurnaceNo.Parse(entity.FurnaceNo);
                if (furnaceNoObj.IsValid)
                {
                    entity.LineNo = furnaceNoObj.LineNoNumeric;
                    entity.Shift = furnaceNoObj.Shift;
                    entity.ShiftNumeric = furnaceNoObj.ShiftNumeric;
                    entity.FurnaceBatchNo = furnaceNoObj.FurnaceBatchNoNumeric;
                    entity.CoilNo = furnaceNoObj.CoilNoNumeric;
                    entity.SubcoilNo = furnaceNoObj.SubcoilNoNumeric;
                    entity.FeatureSuffix = furnaceNoObj.FeatureSuffix;
                    // 生产日期（ProdDate）：优先使用从原始炉号（FurnaceNo）中解析出的日期
                    // 如果炉号解析失败，使用检测日期（DetectionDate，从Excel"日期"列读取）作为后备
                    if (furnaceNoObj.ProdDate.HasValue)
                    {
                        entity.ProdDate = furnaceNoObj.ProdDate;
                    }
                    else if (entity.DetectionDate.HasValue)
                    {
                        entity.ProdDate = entity.DetectionDate;
                    }

                    // 计算格式化炉号（格式：[产线数字][班次汉字][8位日期]-[炉次号]-[卷号]-[分卷号]）
                    entity.FurnaceNoFormatted = furnaceNoObj.GetFurnaceNo();

                    entity.IsValidData = 1;
                }
                else
                {
                    entity.ImportStatus = 1;
                    entity.ImportError = "炉号解析失败: " + furnaceNoObj.ErrorMessage;
                    entity.IsValidData = 0;
                }
            }
            else
            {
                entity.ImportStatus = 1;
                entity.ImportError = "炉号为空";
                entity.IsValidData = 0;
            }

            // Product Spec Match (Initial Guess)
            if (entity.IsValidData == 1)
            {
                var spec = IdentifyProductSpec(entity, productSpecs);
                if (spec != null)
                {
                    entity.ProductSpecId = spec.Id;
                    entity.ProductSpecCode = spec.Code;
                    entity.ProductSpecName = spec.Name;
                    entity.DetectionColumns = spec.DetectionColumns;
                }
            }

            entities.Add(entity);
        }

        CheckDuplicateFurnaceNo(entities);

        return entities;
    }

    private T GetValue<T>(IDictionary<string, object> row, string key)
    {
        if (
            !row.ContainsKey(key)
            || row[key] == null
            || string.IsNullOrWhiteSpace(row[key].ToString())
        )
            return default;

        var value = row[key];
        var targetType = typeof(T);
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            if (underlyingType == typeof(DateTime) && value is double d)
            {
                return (T)(object)DateTime.FromOADate(d);
            }
            return (T)Convert.ChangeType(value, underlyingType);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// 检查炉号重复.
    /// 炉号格式：[产线数字][班次汉字][8位日期]-[炉次号]
    /// 只检查符合规则的有效数据，标记为重复但不直接设为失败
    /// </summary>
    private void CheckDuplicateFurnaceNo(List<RawDataEntity> entities)
    {
        // 构建标准炉号字典：标准炉号 -> 行号列表
        var furnaceNoDict = new Dictionary<string, List<int>>();

        for (int i = 0; i < entities.Count; i++)
        {
            var entity = entities[i];
            // 只检查解析成功的有效数据（符合规则的数据）
            if (
                entity.IsValidData == 1
                && entity.LineNo.HasValue
                && !string.IsNullOrWhiteSpace(entity.Shift)
                && entity.ProdDate.HasValue
                && entity.FurnaceBatchNo.HasValue
            )
            {
                // 构建标准炉号：[产线数字][班次汉字][8位日期]-[炉次号]
                var standardFurnaceNo = GetFurnaceNo(entity);

                if (!string.IsNullOrEmpty(standardFurnaceNo))
                {
                    if (!furnaceNoDict.ContainsKey(standardFurnaceNo))
                    {
                        furnaceNoDict[standardFurnaceNo] = new List<int>();
                    }
                    furnaceNoDict[standardFurnaceNo].Add(i);
                }
            }
        }

        // 检查重复并标记（不直接设为失败，在CompleteImport时处理）
        foreach (var kvp in furnaceNoDict)
        {
            if (kvp.Value.Count > 1)
            {
                // 有重复的炉号，标记为重复状态
                var duplicateRowNumbers = string.Join(
                    "、",
                    kvp.Value.Select(idx => $"第{entities[idx].SortCode}行")
                );
                var warningMessage =
                    $"炉号重复：标准炉号 {kvp.Key} 在以下行出现重复：{duplicateRowNumbers}，将保留第一条数据";

                // 保留第一条，标记其他为重复
                for (int i = 1; i < kvp.Value.Count; i++)
                {
                    var rowIndex = kvp.Value[i];
                    var entity = entities[rowIndex];
                    // 标记为无效，但不改变IsValidData（在CompleteImport时处理）
                    entity.ImportStatus = 1;
                    // 追加警告信息
                    if (string.IsNullOrWhiteSpace(entity.ImportError))
                    {
                        entity.ImportError = warningMessage;
                    }
                    else
                    {
                        entity.ImportError = $"{entity.ImportError}; {warningMessage}";
                    }
                }
            }
        }
    }

    /// <summary>
    /// 检查炉号重复（用于预览项）.
    /// 炉号格式：[产线数字][班次汉字][8位日期]-[炉次号]
    /// 只检查符合规则的有效数据，标记为重复但不直接设为失败
    /// </summary>
    private void CheckDuplicateFurnaceNoForPreview(List<RawDataPreviewItem> items)
    {
        // 构建标准炉号字典：标准炉号 -> 行号列表
        var furnaceNoDict = new Dictionary<string, List<int>>();

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            // 只检查解析成功的有效数据（符合规则的数据）
            if (item.IsValidData == 1 &&
                item.LineNo.HasValue &&
                !string.IsNullOrWhiteSpace(item.Shift) &&
                item.ProdDate.HasValue &&
                item.FurnaceBatchNo.HasValue)
            {
                var standardFurnaceNo = GetFurnaceNo(item);
                if (!string.IsNullOrEmpty(standardFurnaceNo))
                {
                    if (!furnaceNoDict.ContainsKey(standardFurnaceNo))
                    {
                        furnaceNoDict[standardFurnaceNo] = new List<int>();
                    }
                    furnaceNoDict[standardFurnaceNo].Add(i);
                }
            }
        }

        // 检查重复并标记（不直接设为失败，让用户选择保留哪条）
        foreach (var kvp in furnaceNoDict)
        {
            if (kvp.Value.Count > 1)
            {
                // 有重复的炉号，标记为重复状态
                var duplicateRowNumbers = string.Join("、", kvp.Value.Select(idx => $"第{items[idx].SortCode}行"));
                var warningMessage = $"炉号重复：标准炉号 {kvp.Key} 在以下行出现重复：{duplicateRowNumbers}，请选择保留哪条数据";

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
    /// 检查数据库中已存在的炉号（用于预览项）.
    /// 如果本次Excel导入的炉号在数据库中已经存在，标记为将被忽略
    /// </summary>
    private async Task CheckExistingFurnaceNoInDatabaseForPreview(List<RawDataPreviewItem> items)
    {
        // 只检查符合规则的有效数据
        var validItems = items
            .Where(item => item.IsValidData == 1 &&
                          item.LineNo.HasValue &&
                          !string.IsNullOrWhiteSpace(item.Shift) &&
                          item.ProdDate.HasValue &&
                          item.FurnaceBatchNo.HasValue)
            .ToList();

        if (validItems.Count == 0)
            return;

        // 构建标准炉号列表
        var standardFurnaceNos = new List<string>();
        foreach (var item in validItems)
        {
            var standardFurnaceNo = GetFurnaceNo(item);
            if (!string.IsNullOrEmpty(standardFurnaceNo))
            {
                standardFurnaceNos.Add(standardFurnaceNo);
            }
        }

        if (standardFurnaceNos.Count == 0)
            return;

        standardFurnaceNos = standardFurnaceNos.Distinct().ToList();

        // 查询数据库中已存在的标准炉号
        var existingFurnaceNos = new HashSet<string>();

        // 查询数据库中所有有效数据，构建标准炉号并检查
        var dbEntities = await _rawDataRepository
            .AsQueryable()
            .Where(e => e.IsValidData == 1 &&
                       e.LineNo.HasValue &&
                       !string.IsNullOrWhiteSpace(e.Shift) &&
                       e.ProdDate.HasValue &&
                       e.FurnaceBatchNo.HasValue)
            .Select(e => new
            {
                e.LineNo,
                e.Shift,
                e.ProdDate,
                e.FurnaceBatchNo
            })
            .ToListAsync();

        foreach (var entity in dbEntities)
        {
            var furnaceNoObj = FurnaceNo.Build(
                entity.LineNo.Value.ToString(),
                entity.Shift,
                entity.ProdDate,
                entity.FurnaceBatchNo.Value.ToString(),
                "1", // 卷号默认为1
                "1"  // 分卷号默认为1
            );

            var standardFurnaceNo = furnaceNoObj?.GetFurnaceNo();
            if (!string.IsNullOrEmpty(standardFurnaceNo) && standardFurnaceNos.Contains(standardFurnaceNo))
            {
                existingFurnaceNos.Add(standardFurnaceNo);
            }
        }

        // 标记数据库中已存在的炉号
        foreach (var item in validItems)
        {
            var standardFurnaceNo = GetFurnaceNo(item);
            if (!string.IsNullOrEmpty(standardFurnaceNo) && existingFurnaceNos.Contains(standardFurnaceNo))
            {
                item.ExistsInDatabase = true;
                // 如果状态还是success或duplicate，改为exists_in_db
                if (item.Status == "success" || item.Status == "duplicate")
                {
                    item.Status = "exists_in_db";
                }
                // 追加提示信息
                var infoMessage = $"炉号 {standardFurnaceNo} 在数据库中已存在，将被忽略，不会保存到数据库";
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
    /// 生成单行数据的哈希值（用于增量导入一致性校验）.
    /// </summary>
    private string GenerateLastRowHash(RawDataEntity row)
    {
        if (row == null)
            return null;

        // Normalize decimal fields to ensure consistent hash regardless of trailing zeros (e.g., "100.00" -> "100")
        string widthStr = row.Width?.ToString("G29") ?? string.Empty;
        string weightStr = row.CoilWeight?.ToString("G29") ?? string.Empty;

        // 提取关键字段（日期、炉号、宽度、带材重量）
        var keyFields =
            $"日期:{row.ProdDate?.ToString("yyyy-MM-dd")}|炉号:{row.FurnaceNo}|宽度:{widthStr}|带材重量:{weightStr}";

        // 生成MD5哈希值
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(keyFields));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }

    private (bool IsConsistent, string Message) CompareLastRows(
        byte[] currentFileBytes,
        string fileName,
        RawDataImportLogEntity lastLog
    )
    {
        if (string.IsNullOrWhiteSpace(lastLog.LastRowsHash) || lastLog.LastRowsCount <= 0)
        {
            return (true, null);
        }

        try
        {
            using var stream = new MemoryStream(currentFileBytes);
            var rows = stream.Query(useHeaderRow: true).Cast<IDictionary<string, object>>();

            // Excel TotalRows is 1-based data count. Skip TotalRows - 1 to get the last data row.
            // If TotalRows=1, Skip(0).
            int skipCount = lastLog.TotalRows - 1;
            if (skipCount < 0) skipCount = 0;

            var row = rows.Skip(skipCount).FirstOrDefault();
            if (row == null)
            {
                return (false, $"无法读取上次导入的最后一行（行号: {lastLog.TotalRows}）");
            }

            var entity = new RawDataEntity();
            // Just use a dummy sort code or match logic
            entity.SortCode = lastLog.TotalRows;

            entity.DetectionDate = GetValue<DateTime?>(row, "日期");
            entity.FurnaceNo = GetValue<string>(row, "炉号");
            entity.Width = GetValue<decimal?>(row, "宽度");
            entity.CoilWeight = GetValue<decimal?>(row, "带材重量");

            // 生产日期（ProdDate）：优先使用从原始炉号（FurnaceNo）中解析出的日期
            // 如果炉号解析失败，使用检测日期（DetectionDate，从Excel"日期"列读取）作为后备
            if (!string.IsNullOrWhiteSpace(entity.FurnaceNo))
            {
                 var furnaceNoObj = FurnaceNo.Parse(entity.FurnaceNo);
                 if (furnaceNoObj.IsValid && furnaceNoObj.ProdDate.HasValue)
                 {
                     entity.ProdDate = furnaceNoObj.ProdDate;
                 }
                 else if (entity.DetectionDate.HasValue)
                 {
                     entity.ProdDate = entity.DetectionDate;
                 }
            }
            else if (entity.DetectionDate.HasValue)
            {
                entity.ProdDate = entity.DetectionDate;
            }

            var currentHash = GenerateLastRowHash(entity);

            if (currentHash == lastLog.LastRowsHash)
            {
                return (true, null);
            }
            else
            {
                return (
                    false,
                    $"文件可能被修改过（上次导入的第{lastLog.TotalRows}行数据与当前文件不一致）"
                );
            }
        }
        catch (Exception ex)
        {
            return (false, $"数据一致性校验失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 识别产品规格.
    /// 根据原始数据中实际存在的有效数据列去匹配产品规格配置。
    /// 例如：如果产品规格120配置的检测列是13，那么数据的有效列应该是1-13（第1-13列有值，第14列开始为空）。
    /// 判断逻辑：根据产品规格配置的检测列，检查数据的有效列是否正好匹配（从1到检测列都有值，检测列+1开始为空）。
    /// 使用detection1-detection22字段进行匹配（固定22列）.
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
            entity.Detection1, entity.Detection2, entity.Detection3, entity.Detection4,
            entity.Detection5, entity.Detection6, entity.Detection7, entity.Detection8,
            entity.Detection9, entity.Detection10, entity.Detection11, entity.Detection12,
            entity.Detection13, entity.Detection14, entity.Detection15, entity.Detection16,
            entity.Detection17, entity.Detection18, entity.Detection19, entity.Detection20,
            entity.Detection21, entity.Detection22
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

        // 4. 遍历规格进行匹配
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

            // 5. 对每个配置的检测列进行匹配判断
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
    /// 简化导入：直接上传、解析、保存，根据炉号去重
    /// </summary>
    [HttpPost("simple-import")]
    public async Task<SimpleImportOutput> SimpleImport([FromBody] SimpleImportInput input)
    {
        var output = new SimpleImportOutput();

        try
        {
            // 1. 解析Excel文件
            byte[] fileBytes;
            try
            {
                fileBytes = Convert.FromBase64String(input.FileData);
            }
            catch (Exception ex)
            {
                throw Oops.Oh($"文件数据格式错误: {ex.Message}");
            }

            // 2. 解析Excel数据
            var entities = ParseExcel(fileBytes, input.FileName, skipRows: 0);
            output.TotalRows = entities.Count;

            if (entities.Count == 0)
            {
                output.Errors.Add("Excel文件中没有有效数据");
                return output;
            }

            // 3. 获取数据库中已存在的炉号
            var existingFurnaceNos = new HashSet<string>();
            if (input.SkipExistingFurnaceNo)
            {
                var validEntities = entities
                    .Where(e => e.IsValidData == 1 && !string.IsNullOrEmpty(e.FurnaceNo))
                    .ToList();
                if (validEntities.Count > 0)
                {
                    var furnaceNos = validEntities
                        .Select(e => e.FurnaceNo.Trim())
                        .Distinct()
                        .ToList();
                    var existing = await _rawDataRepository
                        .AsQueryable()
                        .Where(e => furnaceNos.Contains(e.FurnaceNo))
                        .Select(e => e.FurnaceNo)
                        .ToListAsync();

                    existingFurnaceNos = new HashSet<string>(existing);
                    output.DuplicateFurnaceNos = existing.ToList();
                }
            }

            // 4. 过滤和保存数据
            var entitiesToSave = new List<RawDataEntity>();
            var skippedCount = 0;

            foreach (var entity in entities)
            {
                // 只保存有效数据
                if (entity.IsValidData != 1)
                {
                    output.FailRows++;
                    continue;
                }

                // 检查炉号是否已存在
                if (
                    !string.IsNullOrEmpty(entity.FurnaceNo)
                    && existingFurnaceNos.Contains(entity.FurnaceNo.Trim())
                )
                {
                    skippedCount++;
                    output.SkippedRows++;
                    continue;
                }

                // 设置创建信息
                entity.Id = SnowflakeIdHelper.NextId();
                entity.CreatorUserId = _userManager.UserId;
                entity.CreatorTime = DateTime.Now;
                entity.LastModifyUserId = _userManager.UserId;
                entity.LastModifyTime = DateTime.Now;

                entitiesToSave.Add(entity);
            }

            // 5. 批量保存到数据库
            if (entitiesToSave.Count > 0)
            {
                // 分批插入防止SQL太长
                var batches = entitiesToSave.Chunk(1000);
                foreach (var batch in batches)
                {
                    await _rawDataRepository.AsInsertable(batch.ToList()).ExecuteCommandAsync();
                }

                output.SuccessRows = entitiesToSave.Count;
            }

            // 6. 生成中间数据（如果需要）
            if (output.SuccessRows > 0)
            {
                await GenerateIntermediateDataForEntities(entitiesToSave);
            }

            // 7. 创建导入日志
            var log = new RawDataImportLogEntity
            {
                FileName = input.FileName,
                TotalRows = output.TotalRows,
                SuccessCount = output.SuccessRows,
                FailCount = output.FailRows,
                Status = output.FailRows == 0 ? "success" : "partial",
                ImportTime = DateTime.Now,
                ValidDataCount = output.SuccessRows,
            };
            log.Creator();
            await _logRepository.InsertAsync(log);

            return output;
        }
        catch (Exception ex)
        {
            output.Errors.Add($"导入失败: {ex.Message}");
            return output;
        }
    }

    /// <summary>
    /// 为原始数据生成中间数据
    /// </summary>
    private async Task GenerateIntermediateDataForEntities(List<RawDataEntity> entities)
    {
        var validEntities = entities.Where(t => t.IsValidData == 1).ToList();
        if (validEntities.Count == 0)
            return;

        var specs = await _productSpecRepository.GetListAsync();
        var intermediateEntities = new List<IntermediateDataEntity>();

        var dataBySpec = validEntities.GroupBy(t => t.ProductSpecId);

        foreach (var group in dataBySpec)
        {
            if (string.IsNullOrEmpty(group.Key))
                continue;

            var spec = specs.FirstOrDefault(s => s.Id == group.Key);
            if (spec == null)
                continue;

            // 查询产品扩展属性
            var attributes = await _productSpecAttributeRepository
                .AsQueryable()
                .Where(t => t.ProductSpecId == spec.Id && t.DeleteMark == null)
                .ToListAsync();

            decimal length = 4m;
            int layers = 20;
            decimal density = 7.25m;

            var lengthAttr = attributes.FirstOrDefault(a => a.AttributeKey == "length");
            if (
                lengthAttr != null
                && !string.IsNullOrEmpty(lengthAttr.AttributeValue)
                && decimal.TryParse(lengthAttr.AttributeValue, out var l)
            )
            {
                length = l;
            }

            var layersAttr = attributes.FirstOrDefault(a => a.AttributeKey == "layers");
            if (
                layersAttr != null
                && !string.IsNullOrEmpty(layersAttr.AttributeValue)
                && int.TryParse(layersAttr.AttributeValue, out var lay)
            )
            {
                layers = lay;
            }

            var densityAttr = attributes.FirstOrDefault(a => a.AttributeKey == "density");
            if (
                densityAttr != null
                && !string.IsNullOrEmpty(densityAttr.AttributeValue)
                && decimal.TryParse(densityAttr.AttributeValue, out var d)
            )
            {
                density = d;
            }

            var detectionColumns = _intermediateDataService.ParseDetectionColumns(
                spec.DetectionColumns
            );

            foreach (var raw in group)
            {
                var intermediate = _intermediateDataService.GenerateIntermediateData(
                    raw,
                    spec,
                    detectionColumns,
                    layers,
                    length,
                    density,
                    null
                );
                intermediate.CreatorUserId = _userManager.UserId;
                intermediateEntities.Add(intermediate);
            }
        }

        if (intermediateEntities.Count > 0)
        {
            await _sessionRepository
                .AsSugarClient()
                .Insertable(intermediateEntities)
                .ExecuteCommandAsync();
        }
    }
}
