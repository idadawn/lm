using System.Text;
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
using Poxiao.Infrastructure.Core.Manager.Files;
using Poxiao.Infrastructure.Security;
using Poxiao.Lab.Entity.Dto.AppearanceFeature;
using Poxiao.Lab.Entity.Dto.RawData;
using Poxiao.Lab.Entity.Entity;
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
            ImportStrategy = input.ImportStrategy,
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
        if (!string.IsNullOrEmpty(input.ImportStrategy))
            existingSession.ImportStrategy = input.ImportStrategy;
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

        // 获取策略相关信息（如增量导入需要跳过的行数）
        int skipRows = 0;
        if (input.ImportStrategy == "incremental")
        {
            var lastLog = await _logRepository
                .AsQueryable()
                .Where(t => t.FileName == input.FileName && t.Status == "success")
                .OrderByDescending(t => t.ImportTime)
                .FirstAsync();
            if (lastLog != null)
            {
                skipRows = lastLog.TotalRows;
                // Consistency Check
                if (!string.IsNullOrWhiteSpace(lastLog.LastRowsHash))
                {
                    var (isConsistent, msg) = CompareLastRows(fileBytes, input.FileName, lastLog);
                    if (!isConsistent)
                    {
                        throw Oops.Oh($"增量导入一致性校验失败: {msg}。建议使用覆盖导入。");
                    }
                }
            }
        }

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
        string parsedDataFile = "";
        if (entities.Count > 0)
        {
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

            // 将数据序列化为JSON文件保存
            try
            {
                parsedDataFile = $"{basePath}/{sessionId}_parsed.json";
                var jsonData = JsonConvert.SerializeObject(entities, Formatting.None);
                await File.WriteAllTextAsync(parsedDataFile, jsonData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UploadAndParse] Save parsed data failed: {ex.Message}");
                throw Oops.Oh($"保存解析数据失败: {ex.Message}");
            }
        }

        // 4. 更新会话（不再保存到数据库，只保存到JSON文件）
        var session = await _sessionRepository.GetFirstAsync(t => t.Id == sessionId);
        session.TotalRows = entities.Count;
        session.ValidDataRows = entities.Count(x => x.IsValidData == 1);
        session.CurrentStep = 1;
        session.SourceFileId = sourceFileId;
        session.ParsedDataFile = parsedDataFile;
        await _sessionRepository.UpdateAsync(session);

        // 5. 返回结果（返回全部数据，前端分页展示）
        var previewOutput = new RawDataPreviewOutput
        {
            ParsedData = entities
                .Select(e =>
                {
                    var item = e.Adapt<RawDataPreviewItem>();
                    item.Status = e.ImportStatus == 0 ? "success" : "failed";
                    item.ErrorMessage = e.ImportError;
                    return item;
                })
                .ToList(),
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

        return validEntities
            .Select(t => new RawDataProductSpecMatchOutput
            {
                RawDataId = t.Id,
                FurnaceNo = t.FurnaceNo,
                ProdDate = t.ProdDate,
                Width = t.Width,
                CoilWeight = t.CoilWeight,
                DetectionColumns = t.DetectionColumns,
                ProductSpecId = t.ProductSpecId,
                ProductSpecName = t.ProductSpecName,
                ProductSpecCode = t.ProductSpecCode,
                MatchStatus = t.ProductSpecId == null ? "unmatched" : "matched",
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
            throw Oops.Oh("解析数据文件不存在，请重新解析");

        if (!File.Exists(session.ParsedDataFile))
            throw Oops.Oh("解析数据文件已丢失，请重新解析");

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

    private List<RawDataEntity> ParseExcel(byte[] fileBytes, string fileName, int skipRows)
    {
        var entities = new List<RawDataEntity>();
        using var stream = new MemoryStream(fileBytes);
        IWorkbook workbook = fileName.EndsWith(".xlsx")
            ? new XSSFWorkbook(stream)
            : new HSSFWorkbook(stream);
        var sheet = workbook.GetSheetAt(0);

        var headerRow = sheet.GetRow(0);
        if (headerRow == null)
            throw Oops.Oh("缺少表头");
        var headerIndexes = GetHeaderIndexes(headerRow);

        // Cache Product Specs for matching
        var productSpecs = _productSpecRepository.GetList();

        int startRow = 1 + skipRows;

        for (int i = startRow; i <= sheet.LastRowNum; i++)
        {
            var row = sheet.GetRow(i);
            if (row == null)
                continue;

            var entity = new RawDataEntity();
            entity.ProdDate = GetCellValue<DateTime?>(row, headerIndexes, "日期");
            entity.FurnaceNo = GetCellValue<string>(row, headerIndexes, "炉号");
            entity.Width = GetCellValue<decimal?>(row, headerIndexes, "宽度");
            entity.CoilWeight = GetCellValue<decimal?>(row, headerIndexes, "带材重量");

            // Dynamic Detection Columns
            var detectionData = new Dictionary<int, decimal?>();
            for (int colIndex = 1; colIndex <= 100; colIndex++)
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
                    if (headerIndexes.ContainsKey(h))
                    {
                        detectionValue = GetCellValue<decimal?>(row, headerIndexes, h);
                        if (detectionValue.HasValue)
                        {
                            detectionData[colIndex] = detectionValue.Value;
                            break;
                        }
                    }
                }

                // If no value found for this column, check checking ahead to stop
                if (!detectionValue.HasValue && detectionData.Count > 0)
                {
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
                            var nextValue = GetCellValue<decimal?>(row, headerIndexes, nextHeader);
                            if (nextValue.HasValue)
                            {
                                nextColumnHasValue = true;
                                break;
                            }
                        }
                    }
                    if (!nextColumnHasValue)
                        break;
                }
            }

            if (detectionData.Count > 0)
            {
                entity.DetectionData = DetectionDataConverter.ToJson(detectionData);
            }

            // Furnace Parse using Helper
            if (!string.IsNullOrWhiteSpace(entity.FurnaceNo))
            {
                var parseResult = FurnaceNoHelper.ParseFurnaceNo(entity.FurnaceNo);
                if (parseResult.Success)
                {
                    entity.LineNo = parseResult.LineNoNumeric;
                    entity.Shift = parseResult.Shift;
                    entity.ShiftNumeric = parseResult.ShiftNumeric;
                    entity.FurnaceNoParsed = parseResult.FurnaceNoNumeric;
                    entity.CoilNo = parseResult.CoilNoNumeric;
                    entity.SubcoilNo = parseResult.SubcoilNoNumeric;
                    entity.FeatureSuffix = parseResult.FeatureSuffix;
                    if (parseResult.ProdDate.HasValue && entity.ProdDate == null)
                        entity.ProdDate = parseResult.ProdDate;

                    entity.IsValidData = 1;
                }
                else
                {
                    entity.ImportStatus = 1;
                    entity.ImportError = "炉号解析失败: " + parseResult.ErrorMessage;
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

        return entities;
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
            IWorkbook workbook = fileName.EndsWith(".xlsx")
                ? new XSSFWorkbook(stream)
                : new HSSFWorkbook(stream);
            var sheet = workbook.GetSheetAt(0);

            var headerRow = sheet.GetRow(0);
            if (headerRow == null)
                return (false, "缺少表头");
            var headerIndexes = GetHeaderIndexes(headerRow);

            // 读取上次导入的最后一行（用于一致性校验）
            // Excel 第0行是表头，数据从第1行开始
            // 如果上次 TotalRows = 194，则读取第194行
            int lastRowIndex = lastLog.TotalRows; // Excel 的第 TotalRows 行（数据行，不含表头）
            if (lastRowIndex < 1 || lastRowIndex > sheet.LastRowNum)
            {
                return (false, $"无法读取上次导入的最后一行（行号: {lastRowIndex}）");
            }

            var row = sheet.GetRow(lastRowIndex);
            if (row == null)
            {
                return (false, $"上次导入的最后一行为空（行号: {lastRowIndex}）");
            }

            var entity = new RawDataEntity();
            entity.ProdDate = GetCellValue<DateTime?>(row, headerIndexes, "日期");
            entity.FurnaceNo = GetCellValue<string>(row, headerIndexes, "炉号");
            entity.Width = GetCellValue<decimal?>(row, headerIndexes, "宽度");
            entity.CoilWeight = GetCellValue<decimal?>(row, headerIndexes, "带材重量");

            var currentHash = GenerateLastRowHash(entity);

            if (currentHash == lastLog.LastRowsHash)
            {
                return (true, null);
            }
            else
            {
                return (
                    false,
                    $"文件可能被修改过（上次导入的第{lastRowIndex}行数据与当前文件不一致）"
                );
            }
        }
        catch (Exception ex)
        {
            return (false, $"数据一致性校验失败: {ex.Message}");
        }
    }

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
}
