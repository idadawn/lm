using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MiniExcelLibs;
using Newtonsoft.Json;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Core.Manager.Files;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Security;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Config;
using Poxiao.Lab.Entity.Dto.AppearanceFeature;
using Poxiao.Lab.Entity.Dto.RawData;
using Poxiao.Lab.Entity.Models;
using Poxiao.Lab.Entity.Enum;
using Poxiao.Lab.Helpers;
using Poxiao.Lab.Interfaces;
using Poxiao.Systems.Interfaces.Common;
using Poxiao.TaskQueue;
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
    private readonly ISqlSugarRepository<AppearanceFeatureLevelEntity> _featureLevelRepository;
    private readonly IFileService _fileService;
    private readonly IUserManager _userManager;
    private readonly IIntermediateDataService _intermediateDataService;
    private readonly IAppearanceFeatureService _appearanceFeatureService;
    private readonly ISqlSugarRepository<ProductSpecAttributeEntity> _productSpecAttributeRepository;
    private readonly IFileManager _fileManager;
    private readonly IRawDataValidationService _validationService;
    private readonly ISqlSugarRepository<ExcelImportTemplateEntity> _excelTemplateRepository;

    public RawDataImportSessionService(
        ISqlSugarRepository<RawDataImportSessionEntity> sessionRepository,
        ISqlSugarRepository<RawDataEntity> rawDataRepository,
        ISqlSugarRepository<ProductSpecEntity> productSpecRepository,
        ISqlSugarRepository<RawDataImportLogEntity> logRepository,
        ISqlSugarRepository<AppearanceFeatureEntity> featureRepository,
        ISqlSugarRepository<AppearanceFeatureCategoryEntity> featureCategoryRepository,
        ISqlSugarRepository<AppearanceFeatureLevelEntity> featureLevelRepository,
        IFileService fileService,
        IUserManager userManager,
        IIntermediateDataService intermediateDataService,
        IAppearanceFeatureService appearanceFeatureService,
        ISqlSugarRepository<ProductSpecAttributeEntity> productSpecAttributeRepository,
        IFileManager fileManager,
        IRawDataValidationService validationService,
        ISqlSugarRepository<ExcelImportTemplateEntity> excelTemplateRepository
    )
    {
        _sessionRepository = sessionRepository;
        _rawDataRepository = rawDataRepository;
        _productSpecRepository = productSpecRepository;
        _logRepository = logRepository;
        _featureRepository = featureRepository;
        _featureCategoryRepository = featureCategoryRepository;
        _featureLevelRepository = featureLevelRepository;
        _fileService = fileService;
        _userManager = userManager;
        _intermediateDataService = intermediateDataService;
        _appearanceFeatureService = appearanceFeatureService;
        _productSpecAttributeRepository = productSpecAttributeRepository;
        _fileManager = fileManager;
        _validationService = validationService;
        _excelTemplateRepository = excelTemplateRepository;
    }

    /// <inheritdoc />
    [HttpPost("create")]
    public async Task<string> Create([FromBody] RawDataImportSessionInput input)
    {
        byte[] fileBytes = null;
        string fileHash = null;
        string fileMd5 = null;
        if (!string.IsNullOrEmpty(input.FileData))
        {
            try
            {
                fileBytes = Convert.FromBase64String(input.FileData);
                fileHash = ComputeFileHashSha256(fileBytes);
                fileMd5 = ComputeFileHashMd5(fileBytes);
            }
            catch (Exception ex)
            {
                throw Oops.Oh($"文件数据格式错误: {ex.Message}");
            }

            if (!input.ForceUpload && !string.IsNullOrWhiteSpace(fileHash))
            {
                var existingLog = await FindDuplicateLogAsync(fileHash, fileMd5, input.FileName);
                var existingSession = await FindDuplicateSessionAsync(fileHash, fileMd5, input.FileName);

                if (existingLog != null || existingSession != null)
                {
                    var lastTime =
                        existingLog?.ImportTime
                        ?? existingSession?.CreatorTime
                        ?? DateTime.Now;
                    var message =
                        $"[DUPLICATE_UPLOAD] 文件已上传过，最近一次时间：{lastTime:yyyy-MM-dd HH:mm:ss}，是否继续上传？";
                    throw Oops.Oh(ErrorCode.COM1026, message);
                }
            }
        }

        var session = new RawDataImportSessionEntity
        {
            Id = Guid.NewGuid().ToString(),
            FileName = input.FileName,
            Status = "pending",
            CurrentStep = 0,
            CreatorUserId = _userManager.UserId,
            CreatorTime = DateTime.Now,
            SourceFileHash = fileHash,
            SourceFileMd5 = fileMd5,
        };

        // 如果提供了文件数据，保存文件
        if (!string.IsNullOrEmpty(input.FileData))
        {
            try
            {
                if (fileBytes == null)
                {
                    fileBytes = Convert.FromBase64String(input.FileData);
                }
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

        var templateConfig = await LoadRawDataTemplateConfigAsync();
        var entities = ParseExcel(fileBytes, input.FileName, skipRows, templateConfig);

        var fileHash = ComputeFileHashSha256(fileBytes);
        var fileMd5 = ComputeFileHashMd5(fileBytes);
        if (string.IsNullOrWhiteSpace(existingSession.SourceFileHash))
        {
            existingSession.SourceFileHash = fileHash;
        }
        if (string.IsNullOrWhiteSpace(existingSession.SourceFileMd5))
        {
            existingSession.SourceFileMd5 = fileMd5;
        }

        var validDataHash = ComputeValidDataHash(entities, templateConfig);
        existingSession.ValidDataHash = validDataHash;

        var lastLog = await FindDuplicateLogAsync(fileHash, fileMd5, existingSession.FileName);
        if (lastLog != null && !string.IsNullOrWhiteSpace(validDataHash))
        {
            var lastValidHash = await EnsureLogValidDataHashAsync(lastLog, templateConfig);
            if (!string.IsNullOrWhiteSpace(lastValidHash) && lastValidHash == validDataHash)
            {
                existingSession.Status = "completed";
                existingSession.TotalRows = entities.Count;
                existingSession.ValidDataRows = entities.Count(x => x.IsValidData == 1);
                existingSession.CurrentStep = 4;
                await _sessionRepository.UpdateAsync(existingSession);

                return new RawDataImportStep1Output
                {
                    ImportSessionId = sessionId,
                    TotalRows = existingSession.TotalRows ?? 0,
                    ValidDataRows = existingSession.ValidDataRows ?? 0,
                    PreviewData = new RawDataPreviewOutput
                    {
                        ParsedData = new List<RawDataPreviewItem>(),
                        SkippedRows = skipRows,
                    },
                    NoChanges = true,
                    NoChangesMessage = "有效数据未发生变化，已完成导入。",
                };
            }
        }

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
        session.SourceFileHash = existingSession.SourceFileHash;
        session.SourceFileMd5 = existingSession.SourceFileMd5;
        session.ValidDataHash = existingSession.ValidDataHash;
        await _sessionRepository.UpdateAsync(session);

        // 5. 转换为预览项并检查重复和数据库中已存在的炉号
        var previewItems = entities
            .Select(e =>
            {
                var item = e.Adapt<RawDataPreviewItem>();
                item.Status = e.ImportStatus == 0 ? "success" : "failed";
                item.ErrorMessage = e.ImportError;
                // 构建标准炉号（用于重复检查）
                if (
                    item.IsValidData == 1
                    && item.LineNo.HasValue
                    && !string.IsNullOrWhiteSpace(item.Shift)
                    && item.ProdDate.HasValue
                    && item.FurnaceBatchNo.HasValue
                )
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
        Console.WriteLine(
            $"[GetProductSpecMatches] 总数据: {entities.Count}, 有效数据: {validEntities.Count}"
        );

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

    private async Task<RawDataImportLogEntity> FindDuplicateLogAsync(
        string fileHash,
        string fileMd5,
        string fileName
    )
    {
        var existingLog = await _logRepository
            .AsQueryable()
            .Where(t =>
                (t.SourceFileHash == fileHash && fileHash != null && fileHash != "")
                || (t.SourceFileMd5 == fileMd5 && fileMd5 != null && fileMd5 != "")
            )
            .OrderByDescending(t => t.ImportTime)
            .FirstAsync();

        if (existingLog != null)
            return existingLog;

        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        var logsWithoutHash = await _logRepository
            .AsQueryable()
            .Where(t =>
                t.FileName == fileName
                && (
                    t.SourceFileHash == null
                    || t.SourceFileHash == ""
                    || t.SourceFileMd5 == null
                    || t.SourceFileMd5 == ""
                )
            )
            .OrderByDescending(t => t.ImportTime)
            .Take(5)
            .ToListAsync();

        foreach (var log in logsWithoutHash)
        {
            var (hash, md5) = TryComputeHashesFromSourceFile(log.SourceFileId);
            if (string.IsNullOrEmpty(hash) && string.IsNullOrEmpty(md5))
                continue;

            if (
                (!string.IsNullOrEmpty(hash) && hash == fileHash)
                || (!string.IsNullOrEmpty(md5) && md5 == fileMd5)
            )
            {
                log.SourceFileHash = fileHash;
                log.SourceFileMd5 = fileMd5;
                await _logRepository.UpdateAsync(log);
                return log;
            }
        }

        return null;
    }

    private async Task<RawDataImportSessionEntity> FindDuplicateSessionAsync(
        string fileHash,
        string fileMd5,
        string fileName
    )
    {
        var existingSession = await _sessionRepository
            .AsQueryable()
            .Where(t =>
                t.Status != "cancelled"
                && (
                    (t.SourceFileHash == fileHash && fileHash != null && fileHash != "")
                    || (t.SourceFileMd5 == fileMd5 && fileMd5 != null && fileMd5 != "")
                )
            )
            .OrderByDescending(t => t.CreatorTime)
            .FirstAsync();

        if (existingSession != null)
            return existingSession;

        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        var sessionsWithoutHash = await _sessionRepository
            .AsQueryable()
            .Where(t =>
                t.FileName == fileName
                && (
                    t.SourceFileHash == null
                    || t.SourceFileHash == ""
                    || t.SourceFileMd5 == null
                    || t.SourceFileMd5 == ""
                )
                && t.Status != "cancelled"
            )
            .OrderByDescending(t => t.CreatorTime)
            .Take(5)
            .ToListAsync();

        foreach (var session in sessionsWithoutHash)
        {
            var (hash, md5) = TryComputeHashesFromSourceFile(session.SourceFileId);
            if (string.IsNullOrEmpty(hash) && string.IsNullOrEmpty(md5))
                continue;

            if (
                (!string.IsNullOrEmpty(hash) && hash == fileHash)
                || (!string.IsNullOrEmpty(md5) && md5 == fileMd5)
            )
            {
                session.SourceFileHash = fileHash;
                session.SourceFileMd5 = fileMd5;
                await _sessionRepository.UpdateAsync(session);
                return session;
            }
        }

        return null;
    }

    private async Task<string> EnsureLogValidDataHashAsync(
        RawDataImportLogEntity log,
        ExcelTemplateConfig templateConfig
    )
    {
        if (log == null)
            return null;

        if (!string.IsNullOrWhiteSpace(log.ValidDataHash))
            return log.ValidDataHash;

        if (string.IsNullOrWhiteSpace(log.SourceFileId))
            return null;

        var hash = ComputeValidDataHashFromFile(log.SourceFileId, templateConfig);
        if (!string.IsNullOrWhiteSpace(hash))
        {
            log.ValidDataHash = hash;
            await _logRepository.UpdateAsync(log);
        }

        return hash;
    }

    private string ComputeValidDataHashFromFile(
        string sourceFileId,
        ExcelTemplateConfig templateConfig
    )
    {
        if (string.IsNullOrWhiteSpace(sourceFileId))
            return null;

        try
        {
            if (!File.Exists(sourceFileId))
                return null;

            var bytes = File.ReadAllBytes(sourceFileId);
            var entities = ParseExcel(bytes, Path.GetFileName(sourceFileId), 0, templateConfig);
            return ComputeValidDataHash(entities, templateConfig);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ComputeValidDataHashFromFile] Failed: {ex.Message}");
            return null;
        }
    }

    private (string Hash, string Md5) TryComputeHashesFromSourceFile(string sourceFileId)
    {
        if (string.IsNullOrWhiteSpace(sourceFileId))
            return (null, null);

        try
        {
            if (!File.Exists(sourceFileId))
                return (null, null);

            var bytes = File.ReadAllBytes(sourceFileId);
            return (ComputeFileHashSha256(bytes), ComputeFileHashMd5(bytes));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TryComputeHashesFromSourceFile] Failed: {ex.Message}");
            return (null, null);
        }
    }

    private static string ComputeFileHashSha256(byte[] fileBytes)
    {
        if (fileBytes == null || fileBytes.Length == 0)
            return null;

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(fileBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    private static string ComputeFileHashMd5(byte[] fileBytes)
    {
        if (fileBytes == null || fileBytes.Length == 0)
            return null;

        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(fileBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
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
            int? columns = null;

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
                    Console.WriteLine(
                        $"[UpdateDuplicateSelections] 更新数据ID: {item.RawDataId}, IsValidData: {oldIsValidData} -> {entity.IsValidData}"
                    );
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
                    if (
                        !string.IsNullOrWhiteSpace(entity.ImportError)
                        && entity.ImportError.Contains("重复数据，已选择保留其他数据")
                    )
                    {
                        entity.ImportError = entity
                            .ImportError.Replace("重复数据，已选择保留其他数据；", "")
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
                Console.WriteLine(
                    $"[UpdateDuplicateSelections] 警告：找不到数据ID: {item.RawDataId}"
                );
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
                Console.WriteLine(
                    $"[UpdateDuplicateSelections] 更新了 {input.Items.Count} 条数据，有效数据行数: {validCount}/{entities.Count}"
                );
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
        var featureDict = features
            .Where(f => !string.IsNullOrEmpty(f.Id))
            .GroupBy(f => f.Id)
            .ToDictionary(g => g.Key, g => g.First());
        var categories = await _featureCategoryRepository.GetListAsync();
        var levels = await _featureLevelRepository.GetListAsync(l => l.DeleteMark == null);
        var categoryNameDict = categories.ToDictionary(c => c.Id, c => c.Name);
        var levelNameDict = levels.ToDictionary(l => l.Id, l => l.Name);
        var categoryNameToId = categories
            .Where(c => !string.IsNullOrWhiteSpace(c.Name))
            .GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);
        var levelNameToId = levels
            .Where(l => !string.IsNullOrWhiteSpace(l.Name))
            .GroupBy(l => l.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

        bool needSave = false;
        if (unMatchedEntities.Count > 0)
        {
            // 执行自动匹配
            var batchInput = new BatchMatchInput
            {
                Items = unMatchedEntities
                    .Select(e => new MatchItemInput { Id = e.Id, Query = e.FeatureSuffix })
                    .ToList(),
            };
            var batchMatches = await _appearanceFeatureService.BatchMatch(batchInput);
            var matchGroups = batchMatches
                .GroupBy(m => m.Id)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var entity in unMatchedEntities)
            {
                if (!matchGroups.TryGetValue(entity.Id, out var group))
                {
                    continue;
                }

                var matchedIdSet = new HashSet<string>();
                foreach (var match in group)
                {
                    if (!match.IsPerfectMatch || string.IsNullOrWhiteSpace(match.FeatureName))
                    {
                        continue;
                    }

                    var featureId = ResolveFeatureId(
                        match,
                        features,
                        categoryNameToId,
                        levelNameToId
                    );
                    if (!string.IsNullOrWhiteSpace(featureId))
                    {
                        matchedIdSet.Add(featureId);
                    }
                }

                if (matchedIdSet.Count > 0)
                {
                    entity.AppearanceFeatureIds = JsonConvert.SerializeObject(matchedIdSet.ToList());
                    entity.MatchConfidence = 1.0;
                    needSave = true;
                }

                if (UpdateFeatureCategoryAndLevelFields(entity, featureDict))
                {
                    needSave = true;
                }
            }
        }

        foreach (var entity in validEntities)
        {
            if (!string.IsNullOrWhiteSpace(entity.AppearanceFeatureIds))
            {
                if (UpdateFeatureCategoryAndLevelFields(entity, featureDict))
                {
                    needSave = true;
                }
            }
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

                    if (!string.IsNullOrEmpty(entity.AppearanceFeatureCategoryIds))
                    {
                        item.AppearanceFeatureCategoryIds =
                            JsonConvert.DeserializeObject<List<string>>(
                                entity.AppearanceFeatureCategoryIds
                            );
                    }

                    if (!string.IsNullOrEmpty(entity.AppearanceFeatureLevelIds))
                    {
                        item.AppearanceFeatureLevelIds =
                            JsonConvert.DeserializeObject<List<string>>(
                                entity.AppearanceFeatureLevelIds
                            );
                    }

                    // 填充匹配详情
                    if (item.AppearanceFeatureIds != null && item.AppearanceFeatureIds.Any())
                    {
                        item.MatchDetails = item
                            .AppearanceFeatureIds.Select(id =>
                            {
                                var f = featureDict.TryGetValue(id, out var feature)
                                    ? feature
                                    : null;
                                return new FeatureMatchDetail
                                {
                                    FeatureId = id,
                                    FeatureName = f?.Name ?? id,
                                    Confidence = item.MatchConfidence?.ToString() ?? "1.0",
                                    CategoryId = f?.CategoryId,
                                    CategoryName = f?.CategoryId != null
                                        && categoryNameDict.TryGetValue(
                                            f.CategoryId,
                                            out var categoryName
                                        )
                                            ? categoryName
                                            : null,
                                    SeverityLevelId = f?.SeverityLevelId,
                                    SeverityLevelName = f?.SeverityLevelId != null
                                        && levelNameDict.TryGetValue(
                                            f.SeverityLevelId,
                                            out var levelName
                                        )
                                            ? levelName
                                            : null,
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
        var features = await _featureRepository.GetListAsync();
        var featureDict = features
            .Where(f => !string.IsNullOrEmpty(f.Id))
            .GroupBy(f => f.Id)
            .ToDictionary(g => g.Key, g => g.First());

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
                UpdateFeatureCategoryAndLevelFields(entity, featureDict);
            }
        }

        // 保存回JSON文件
        await SaveParsedDataToFile(sessionId, entities);
    }

    /// <summary>
    /// 根据匹配到的特性ID，填充特性大类和等级ID列表
    /// </summary>
    private bool UpdateFeatureCategoryAndLevelFields(
        RawDataEntity entity,
        Dictionary<string, AppearanceFeatureEntity> featureDict
    )
    {
        if (entity == null)
            return false;

        var originalCategoryIds = entity.AppearanceFeatureCategoryIds;
        var originalLevelIds = entity.AppearanceFeatureLevelIds;

        var featureIds = entity.AppearanceFeatureIdsList;
        if (featureIds == null || featureIds.Count == 0)
        {
            entity.AppearanceFeatureCategoryIds = null;
            entity.AppearanceFeatureLevelIds = null;
            return originalCategoryIds != entity.AppearanceFeatureCategoryIds
                || originalLevelIds != entity.AppearanceFeatureLevelIds;
        }

        var categoryIds = new List<string>();
        var levelIds = new List<string>();
        var categorySet = new HashSet<string>();
        var levelSet = new HashSet<string>();

        foreach (var id in featureIds)
        {
            if (string.IsNullOrEmpty(id) || !featureDict.TryGetValue(id, out var feature))
            {
                continue;
            }

            if (!string.IsNullOrEmpty(feature.CategoryId) && categorySet.Add(feature.CategoryId))
            {
                categoryIds.Add(feature.CategoryId);
            }

            if (
                !string.IsNullOrEmpty(feature.SeverityLevelId)
                && levelSet.Add(feature.SeverityLevelId)
            )
            {
                levelIds.Add(feature.SeverityLevelId);
            }
        }

        entity.AppearanceFeatureCategoryIds = categoryIds.Count > 0
            ? JsonConvert.SerializeObject(categoryIds)
            : null;
        entity.AppearanceFeatureLevelIds = levelIds.Count > 0
            ? JsonConvert.SerializeObject(levelIds)
            : null;

        return originalCategoryIds != entity.AppearanceFeatureCategoryIds
            || originalLevelIds != entity.AppearanceFeatureLevelIds;
    }

    /// <summary>
    /// 根据匹配结果名称解析特性ID
    /// </summary>
    private string ResolveFeatureId(
        MatchItemOutput match,
        List<AppearanceFeatureEntity> features,
        Dictionary<string, string> categoryNameToId,
        Dictionary<string, string> levelNameToId
    )
    {
        if (match == null || string.IsNullOrWhiteSpace(match.FeatureName))
        {
            return null;
        }

        string categoryId = null;
        if (
            !string.IsNullOrWhiteSpace(match.Category)
            && categoryNameToId.TryGetValue(match.Category, out var resolvedCategoryId)
        )
        {
            categoryId = resolvedCategoryId;
        }

        string levelId = null;
        if (
            !string.IsNullOrWhiteSpace(match.SeverityLevel)
            && levelNameToId.TryGetValue(match.SeverityLevel, out var resolvedLevelId)
        )
        {
            levelId = resolvedLevelId;
        }

        var feature = features.FirstOrDefault(f =>
            !string.IsNullOrWhiteSpace(f.Name)
            && f.Name.Equals(match.FeatureName, StringComparison.OrdinalIgnoreCase)
            && (string.IsNullOrWhiteSpace(categoryId) || f.CategoryId == categoryId)
            && (string.IsNullOrWhiteSpace(levelId) || f.SeverityLevelId == levelId)
        );

        return feature?.Id;
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

        var templateConfig = await LoadRawDataTemplateConfigAsync();

        // 1. 从JSON文件加载数据
        var allData = await LoadParsedDataFromFile(sessionId);
        var validData = allData.Where(t => t.IsValidData == 1).ToList();

        if (validData.Count == 0)
        {
            session.Status = "failed";
            await _sessionRepository.UpdateAsync(session);
            throw Oops.Oh("没有有效数据可以导入");
        }

        if (string.IsNullOrWhiteSpace(session.ValidDataHash))
        {
            session.ValidDataHash = ComputeValidDataHash(allData, templateConfig);
        }

        // 1.0 处理重复的炉号（保留第一条，移除其他）
        var validDataToCheck = validData
            .Where(t =>
                t.LineNo.HasValue
                && !string.IsNullOrWhiteSpace(t.Shift)
                && t.ProdDate.HasValue
                && t.FurnaceBatchNo.HasValue
            )
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
                    if (
                        !t.LineNo.HasValue
                        || string.IsNullOrWhiteSpace(t.Shift)
                        || !t.ProdDate.HasValue
                        || !t.FurnaceBatchNo.HasValue
                    )
                    {
                        return true; // 不符合规则的数据保留（会被后续逻辑处理）
                    }
                    // 如果是重复的炉号，只保留第一条
                    return keepIds.Contains(t.Id);
                })
                .ToList();
        }

        var compareLog = await FindDuplicateLogAsync(
            session.SourceFileHash,
            session.SourceFileMd5,
            session.FileName
        );
        var shouldUpdateExisting = false;
        if (compareLog != null && !string.IsNullOrWhiteSpace(session.ValidDataHash))
        {
            var compareHash = await EnsureLogValidDataHashAsync(compareLog, templateConfig);
            if (!string.IsNullOrWhiteSpace(compareHash) && compareHash != session.ValidDataHash)
            {
                shouldUpdateExisting = true;
            }
        }

        var existingRawDataMap = new Dictionary<string, RawDataEntity>();

        // 1.1 检查数据库中已存在的炉号，忽略这些数据（非更新模式）
        validDataToCheck = validData
            .Where(t =>
                t.LineNo.HasValue
                && !string.IsNullOrWhiteSpace(t.Shift)
                && t.ProdDate.HasValue
                && t.FurnaceBatchNo.HasValue
            )
            .ToList();

        if (validDataToCheck.Count > 0 && !shouldUpdateExisting)
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
                .Where(e =>
                    e.IsValidData == 1
                    && e.LineNo.HasValue
                    && !string.IsNullOrWhiteSpace(e.Shift)
                    && e.ProdDate.HasValue
                    && e.FurnaceBatchNo.HasValue
                )
                .Select(e => new {
                    e.LineNo,
                    e.Shift,
                    e.ProdDate,
                    e.FurnaceBatchNo,
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
                    "1" // 分卷号默认为1
                );
                var standardFurnaceNo = furnaceNoObj?.GetFurnaceNo();
                if (
                    !string.IsNullOrEmpty(standardFurnaceNo)
                    && standardFurnaceNos.Contains(standardFurnaceNo)
                )
                {
                    existingFurnaceNos.Add(standardFurnaceNo);
                }
            }

            // 从有效数据中移除数据库中已存在的炉号
            validData = validData
                .Where(t =>
                {
                    if (
                        !t.LineNo.HasValue
                        || string.IsNullOrWhiteSpace(t.Shift)
                        || !t.ProdDate.HasValue
                        || !t.FurnaceBatchNo.HasValue
                    )
                    {
                        return true; // 不符合规则的数据保留（会被后续逻辑处理）
                    }
                    var standardFurnaceNo = GetFurnaceNo(t);
                    return string.IsNullOrEmpty(standardFurnaceNo)
                        || !existingFurnaceNos.Contains(standardFurnaceNo);
                })
                .ToList();
        }

        if (validDataToCheck.Count > 0 && shouldUpdateExisting)
        {
            var standardFurnaceNos = validDataToCheck
                .Select(t => GetFurnaceNo(t))
                .Where(f => !string.IsNullOrEmpty(f))
                .Distinct()
                .ToList();

            if (standardFurnaceNos.Count > 0)
            {
                var dbEntities = await _rawDataRepository
                    .AsQueryable()
                    .Where(e =>
                        e.IsValidData == 1
                        && e.LineNo.HasValue
                        && !string.IsNullOrWhiteSpace(e.Shift)
                        && e.ProdDate.HasValue
                        && e.FurnaceBatchNo.HasValue
                    )
                    .Select(e => new RawDataEntity
                    {
                        Id = e.Id,
                        LineNo = e.LineNo,
                        Shift = e.Shift,
                        ProdDate = e.ProdDate,
                        FurnaceBatchNo = e.FurnaceBatchNo,
                        CoilNo = e.CoilNo,
                        SubcoilNo = e.SubcoilNo,
                        CreatorUserId = e.CreatorUserId,
                        CreatorTime = e.CreatorTime,
                    })
                    .ToListAsync();

                foreach (var entity in dbEntities)
                {
                    var furnaceNoObj = FurnaceNo.Build(
                        entity.LineNo.Value.ToString(),
                        entity.Shift,
                        entity.ProdDate,
                        entity.FurnaceBatchNo.Value.ToString(),
                        entity.CoilNo?.ToString() ?? "1",
                        entity.SubcoilNo?.ToString() ?? "1"
                    );

                    var standardFurnaceNo = furnaceNoObj?.GetFurnaceNo();
                    if (
                        !string.IsNullOrEmpty(standardFurnaceNo)
                        && standardFurnaceNos.Contains(standardFurnaceNo)
                        && !existingRawDataMap.ContainsKey(standardFurnaceNo)
                    )
                    {
                        existingRawDataMap[standardFurnaceNo] = entity;
                    }
                }
            }
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

        // 1.2 如果是更新模式，设置已有数据ID并准备更新列表
        var updateRawDataList = new List<RawDataEntity>();
        var updateIdSet = new HashSet<string>();
        if (shouldUpdateExisting && existingRawDataMap.Count > 0)
        {
            foreach (var entity in validData)
            {
                var standardFurnaceNo = GetFurnaceNo(entity);
                if (string.IsNullOrEmpty(standardFurnaceNo))
                    continue;

                if (existingRawDataMap.TryGetValue(standardFurnaceNo, out var existing))
                {
                    entity.Id = existing.Id;
                    entity.CreatorUserId = existing.CreatorUserId;
                    entity.CreatorTime = existing.CreatorTime;
                    entity.LastModifyUserId = _userManager.UserId;
                    entity.LastModifyTime = DateTime.Now;
                    updateRawDataList.Add(entity);
                    updateIdSet.Add(entity.Id);
                }
            }
        }

        // 2. 生成中间数据（在事务之前准备好所有数据）
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
                var intermediate = await _intermediateDataService.GenerateIntermediateDataAsync(
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

        // 3. 使用事务确保原始数据和中间数据的导入操作原子性（同时成功或同时失败）
        var db = _sessionRepository.AsSugarClient();
        try
        {
            await db.Ado.BeginTranAsync();

            // 3.1 将所有数据写入原始数据表（包括有效和无效数据）
            var insertRawDataList = allData
                .Where(t => !updateIdSet.Contains(t.Id))
                .ToList();

            if (insertRawDataList.Count > 0)
            {
                // 分批插入防止SQL太长
                var batches = insertRawDataList.Chunk(1000);
                foreach (var batch in batches)
                {
                    await _rawDataRepository.AsInsertable(batch.ToList()).ExecuteCommandAsync();
                }
            }

            if (updateRawDataList.Count > 0)
            {
                var updateBatches = updateRawDataList.Chunk(200);
                foreach (var batch in updateBatches)
                {
                    await db.Updateable(batch.ToList()).ExecuteCommandAsync();
                }
            }

            // 3.2 将中间数据写入中间数据表
            if (intermediateEntities.Count > 0)
            {
                var updateRawIds = updateRawDataList.Select(t => t.Id).Distinct().ToList();
                if (updateRawIds.Count > 0)
                {
                    await db
                        .Deleteable<IntermediateDataEntity>()
                        .In(t => t.RawDataId, updateRawIds)
                        .ExecuteCommandAsync();
                }

                await db.Insertable(intermediateEntities).ExecuteCommandAsync();
            }

            await db.Ado.CommitTranAsync();
        }
        catch (Exception ex)
        {
            await db.Ado.RollbackTranAsync();
            session.Status = "failed";
            await _sessionRepository.UpdateAsync(session);
            throw Oops.Oh($"导入失败，数据已回滚: {ex.Message}");
        }

        if (intermediateEntities.Count > 0)
        {
            // 异步触发公式计算任务（根据批次ID后台执行，不阻塞导入流程）
            TaskQueued.Enqueue(
                (serviceProvider) =>
                {
                    try
                    {
                        var intermediateDataService =
                            serviceProvider.GetRequiredService<IntermediateDataService>();
                        // 使用 Task.Run 异步执行，避免阻塞任务队列
                        Task.Run(async () =>
                        {
                            try
                            {
                                // 根据批次ID批量计算公式
                                var result =
                                    await intermediateDataService.BatchCalculateFormulasByBatchIdInternalAsync(
                                        sessionId
                                    );
                                // 可以在这里记录计算结果或发送通知
                                Console.WriteLine(
                                    $"批次 {sessionId} 公式计算完成 - 成功: {result?.SuccessCount}, 失败: {result?.FailedCount}"
                                );
                                if (result?.FailedCount > 0)
                                {
                                    Console.WriteLine(
                                        $"批次 {sessionId} 公式计算失败详情: {string.Join("; ", result.Errors.Select(e => $"{e.FurnaceNo}: {e.ErrorMessage}"))}"
                                    );
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(
                                    $"批次 {sessionId} 后台公式计算任务执行失败: {ex.Message}"
                                );
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"创建批次 {sessionId} 公式计算任务失败: {ex.Message}");
                    }
                },
                delay: 1000 // 延迟1秒执行，确保数据已完全保存
            );
        }

        // 3. Mark Session Complete
        session.Status = "completed";
        session.TotalRows = allData.Count; // 更新总数据行数
        session.ValidDataRows = validData.Count; // 更新有效数据行数
        session.ValidDataHash = ComputeValidDataHash(validData, templateConfig);
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
            SourceFileHash = session.SourceFileHash,
            SourceFileMd5 = session.SourceFileMd5,
            ValidDataHash = session.ValidDataHash,
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
        if (
            entity == null
            || !entity.LineNo.HasValue
            || string.IsNullOrWhiteSpace(entity.Shift)
            || !entity.ProdDate.HasValue
            || !entity.FurnaceBatchNo.HasValue
        )
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
        if (
            item == null
            || !item.LineNo.HasValue
            || string.IsNullOrWhiteSpace(item.Shift)
            || !item.ProdDate.HasValue
            || !item.FurnaceBatchNo.HasValue
        )
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

    private List<RawDataEntity> ParseExcel(
        byte[] fileBytes,
        string fileName,
        int skipRows,
        ExcelTemplateConfig templateConfig
    )
    {
        var entities = new List<RawDataEntity>();
        templateConfig ??= BuildDefaultRawDataTemplateConfig();

        // 先获取所有列名，用于模板映射与检测列识别
        List<string> columns;
        using (var streamForColumns = new MemoryStream(fileBytes))
        {
            columns = MiniExcelLibs.MiniExcel.GetColumns(streamForColumns, useHeaderRow: true).ToList();
        }

        var fieldHeaderMap = BuildFieldHeaderMap(templateConfig, columns);
        var requiredFields = GetRequiredFields(templateConfig);
        var fieldLabelMap = templateConfig.FieldMappings
            .Where(m => !string.IsNullOrWhiteSpace(m.Field))
            .ToDictionary(m => m.Field, m => m.Label, StringComparer.OrdinalIgnoreCase);
        var detectionHeaderMap = BuildDetectionHeaderMap(templateConfig, columns);

        // 读取数据行
        using var stream = new MemoryStream(fileBytes);
        var rows = stream.Query(useHeaderRow: true).Cast<IDictionary<string, object>>().ToList();

        var productSpecs = _productSpecRepository.GetList();

        for (int i = 0; i < rows.Count; i++)
        {
            if (i < skipRows)
                continue;

            var row = rows[i];
            var entity = new RawDataEntity();

            // 根据模板字段映射赋值
            foreach (var mapping in templateConfig.FieldMappings)
            {
                if (string.IsNullOrWhiteSpace(mapping.Field))
                    continue;
                if (!fieldHeaderMap.TryGetValue(mapping.Field, out var headerName))
                    continue;

                TrySetEntityValue(entity, row, headerName, mapping);
            }

            // 补充断头数、单卷重量（兼容列索引方式）
            ApplyFallbackColumns(entity, row, columns);

            // 检测数据列（根据模板配置识别）
            foreach (var kvp in detectionHeaderMap)
            {
                var colIndex = kvp.Key;
                var headerName = kvp.Value;
                if (!row.ContainsKey(headerName))
                    continue;
                var detectionValue = GetValue<decimal?>(row, headerName);
                if (!detectionValue.HasValue)
                    continue;
                var propName = $"Detection{colIndex}";
                var prop = typeof(RawDataEntity).GetProperty(propName);
                if (prop != null && prop.GetValue(entity) == null)
                {
                    prop.SetValue(entity, detectionValue.Value);
                }
            }

            // 必填字段校验
            var missingFields = GetMissingRequiredFields(entity, requiredFields, fieldLabelMap);
            if (missingFields.Count > 0)
            {
                entity.ImportStatus = 1;
                entity.ImportError = $"缺少必填字段: {string.Join("、", missingFields)}";
                entity.IsValidData = 0;
                entities.Add(entity);
                continue;
            }

            // 炉号解析
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
                    if (furnaceNoObj.ProdDate.HasValue)
                    {
                        entity.ProdDate = furnaceNoObj.ProdDate;
                    }
                    else if (entity.DetectionDate.HasValue)
                    {
                        entity.ProdDate = entity.DetectionDate;
                    }
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

    private async Task<ExcelTemplateConfig> LoadRawDataTemplateConfigAsync()
    {
        var template = await _excelTemplateRepository.GetFirstAsync(t =>
            t.TemplateCode == ExcelImportTemplateCode.RawDataImport.ToString()
            && (t.DeleteMark == 0 || t.DeleteMark == null)
        );

        if (template == null || string.IsNullOrWhiteSpace(template.ConfigJson))
            return BuildDefaultRawDataTemplateConfig();

        var config = TryParseTemplateConfig(template.ConfigJson);
        return config ?? BuildDefaultRawDataTemplateConfig();
    }

    private ExcelTemplateConfig TryParseTemplateConfig(string configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
            return null;

        try
        {
            var json = configJson.Trim();
            if (json.StartsWith("\"") && json.EndsWith("\""))
            {
                try
                {
                    json = System.Text.Json.JsonSerializer.Deserialize<string>(json);
                }
                catch
                {
                    // ignore
                }
            }

            return System.Text.Json.JsonSerializer.Deserialize<ExcelTemplateConfig>(json);
        }
        catch
        {
            return null;
        }
    }

    private ExcelTemplateConfig BuildDefaultRawDataTemplateConfig()
    {
        var config = new ExcelTemplateConfig
        {
            Version = "1.0",
            Description = "检测数据默认模板",
            DetectionColumns = new DetectionColumnConfig
            {
                MinColumn = 1,
                MaxColumn = 100,
                Patterns = new List<string>
                {
                    "{col}",
                    "检测{col}",
                    "列{col}",
                    "第{col}列",
                    "检测列{col}",
                },
            },
        };

        var mappings = new List<TemplateColumnMapping>();
        var props = typeof(RawDataEntity).GetProperties();
        foreach (var prop in props)
        {
            var importAttr =
                prop.GetCustomAttributes(
                        typeof(Poxiao.Lab.Entity.Attributes.ExcelImportColumnAttribute),
                        false
                    )
                    .FirstOrDefault() as Poxiao.Lab.Entity.Attributes.ExcelImportColumnAttribute;
            if (importAttr == null || !importAttr.IsImportField)
                continue;

            var dataType = "string";
            var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            if (
                underlyingType == typeof(decimal)
                || underlyingType == typeof(double)
                || underlyingType == typeof(float)
            )
                dataType = "decimal";
            else if (underlyingType == typeof(int) || underlyingType == typeof(long))
                dataType = "int";
            else if (underlyingType == typeof(DateTime))
                dataType = "datetime";

            mappings.Add(
                new TemplateColumnMapping
                {
                    Field = prop.Name,
                    Label = importAttr.Name,
                    ExcelColumnNames = new List<string> { importAttr.Name },
                    DataType = dataType,
                    Required = false,
                }
            );
        }

        config.FieldMappings = mappings.OrderBy(m => m.Field).ToList();
        return config;
    }

    private Dictionary<string, string> BuildFieldHeaderMap(
        ExcelTemplateConfig templateConfig,
        List<string> columns
    )
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (templateConfig?.FieldMappings == null || columns == null)
            return map;

        var columnMap = columns
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToDictionary(c => c.Trim(), c => c, StringComparer.OrdinalIgnoreCase);

        foreach (var mapping in templateConfig.FieldMappings)
        {
            if (string.IsNullOrWhiteSpace(mapping.Field))
                continue;

            string headerName = null;

            if (!string.IsNullOrWhiteSpace(mapping.ExcelColumnIndex))
            {
                var idx = ExcelColumnIndexToNumber(mapping.ExcelColumnIndex);
                if (idx >= 0 && idx < columns.Count)
                {
                    headerName = columns[idx];
                }
            }

            if (headerName == null && mapping.ExcelColumnNames != null)
            {
                foreach (var name in mapping.ExcelColumnNames)
                {
                    if (string.IsNullOrWhiteSpace(name))
                        continue;
                    if (columnMap.TryGetValue(name.Trim(), out var matched))
                    {
                        headerName = matched;
                        break;
                    }
                }
            }

            if (headerName == null && !string.IsNullOrWhiteSpace(mapping.Label))
            {
                if (columnMap.TryGetValue(mapping.Label.Trim(), out var matched))
                    headerName = matched;
            }

            if (!string.IsNullOrWhiteSpace(headerName))
                map[mapping.Field] = headerName;
        }

        return map;
    }

    private Dictionary<int, string> BuildDetectionHeaderMap(
        ExcelTemplateConfig templateConfig,
        List<string> columns
    )
    {
        var result = new Dictionary<int, string>();
        if (columns == null || columns.Count == 0)
            return result;

        var detectionConfig = templateConfig?.DetectionColumns ?? new DetectionColumnConfig();
        foreach (var header in columns)
        {
            if (string.IsNullOrWhiteSpace(header))
                continue;

            if (!IsDetectionHeaderMatch(header, detectionConfig))
                continue;

            var match = Regex.Match(header, @"\d+");
            if (match.Success && int.TryParse(match.Value, out var colNum))
            {
                if (colNum >= 1 && colNum <= 22 && !result.ContainsKey(colNum))
                    result[colNum] = header;
            }
        }

        return result;
    }

    private bool IsDetectionHeaderMatch(string header, DetectionColumnConfig config)
    {
        var name = header?.Trim() ?? "";
        if (string.IsNullOrEmpty(name))
            return false;

        if (config?.Patterns != null && config.Patterns.Count > 0)
        {
            foreach (var pattern in config.Patterns)
            {
                if (string.IsNullOrWhiteSpace(pattern))
                    continue;
                try
                {
                    var regexPattern = pattern.Replace("{col}", @"\d+");
                    var regex = new Regex($"^{regexPattern}$", RegexOptions.IgnoreCase);
                    if (regex.IsMatch(name))
                        return true;
                }
                catch
                {
                    var simplePattern = pattern.Replace("{col}", "");
                    if (name.Contains(simplePattern, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            return false;
        }

        return name.Contains("检测", StringComparison.OrdinalIgnoreCase);
    }

    private HashSet<string> GetRequiredFields(ExcelTemplateConfig templateConfig)
    {
        var required = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (templateConfig?.FieldMappings != null)
        {
            foreach (var mapping in templateConfig.FieldMappings)
            {
                if (mapping.Required && !string.IsNullOrWhiteSpace(mapping.Field))
                    required.Add(mapping.Field);
            }
        }

        if (templateConfig?.Validation?.RequiredFields != null)
        {
            foreach (var field in templateConfig.Validation.RequiredFields)
            {
                if (!string.IsNullOrWhiteSpace(field))
                    required.Add(field);
            }
        }

        return required;
    }

    private List<string> GetMissingRequiredFields(
        RawDataEntity entity,
        HashSet<string> requiredFields,
        Dictionary<string, string> fieldLabelMap
    )
    {
        var missing = new List<string>();
        if (entity == null || requiredFields == null || requiredFields.Count == 0)
            return missing;

        foreach (var field in requiredFields)
        {
            var prop = typeof(RawDataEntity).GetProperty(field);
            if (prop == null)
                continue;

            var value = prop.GetValue(entity);
            var isMissing =
                value == null
                || (value is string s && string.IsNullOrWhiteSpace(s))
                || (value is DateTime dt && dt == default)
                || (value is decimal d && d == default && !prop.PropertyType.IsGenericType);

            if (isMissing)
            {
                if (fieldLabelMap != null && fieldLabelMap.TryGetValue(field, out var label))
                {
                    missing.Add(label);
                }
                else
                {
                    missing.Add(field);
                }
            }
        }

        return missing;
    }

    private void TrySetEntityValue(
        RawDataEntity entity,
        IDictionary<string, object> row,
        string headerName,
        TemplateColumnMapping mapping
    )
    {
        if (entity == null || mapping == null || string.IsNullOrWhiteSpace(mapping.Field))
            return;
        if (!row.ContainsKey(headerName))
            return;

        var prop = typeof(RawDataEntity).GetProperty(mapping.Field);
        if (prop == null)
            return;

        var rawValue = row[headerName];
        if (rawValue == null || string.IsNullOrWhiteSpace(rawValue.ToString()))
        {
            if (!string.IsNullOrWhiteSpace(mapping.DefaultValue))
            {
                rawValue = mapping.DefaultValue;
            }
            else
            {
                return;
            }
        }

        var converted = ConvertValue(rawValue, mapping.DataType, prop.PropertyType);
        if (converted != null)
            prop.SetValue(entity, converted);
    }

    private object ConvertValue(object value, string dataType, Type targetType)
    {
        try
        {
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            var text = value?.ToString();
            if (string.Equals(dataType, "datetime", StringComparison.OrdinalIgnoreCase))
            {
                if (value is double d)
                    return DateTime.FromOADate(d);
                if (DateTime.TryParse(text, out var dt))
                    return dt;
                return null;
            }

            if (string.Equals(dataType, "decimal", StringComparison.OrdinalIgnoreCase))
            {
                if (decimal.TryParse(text, out var dec))
                    return Convert.ChangeType(dec, underlyingType);
                return null;
            }

            if (string.Equals(dataType, "int", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(text, out var num))
                    return Convert.ChangeType(num, underlyingType);
                return null;
            }

            if (underlyingType == typeof(DateTime))
            {
                if (value is double od)
                    return DateTime.FromOADate(od);
                if (DateTime.TryParse(text, out var dt))
                    return dt;
            }

            return Convert.ChangeType(value, underlyingType);
        }
        catch
        {
            return null;
        }
    }

    private void ApplyFallbackColumns(
        RawDataEntity entity,
        IDictionary<string, object> row,
        List<string> columns
    )
    {
        if (entity == null || row == null || columns == null)
            return;

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

            if (!entity.BreakCount.HasValue && columns.Count > 26)
            {
                var colName = columns[26];
                if (row.ContainsKey(colName))
                    entity.BreakCount = GetValue<int?>(row, colName);
            }
        }

        if (!entity.SingleCoilWeight.HasValue)
        {
            var singleCoilWeightHeaders = new[]
            {
                "单卷重量",
                "单卷重",
                "SingleCoilWeight",
                "AB",
            };
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

            if (!entity.SingleCoilWeight.HasValue && columns.Count > 27)
            {
                var colName = columns[27];
                if (row.ContainsKey(colName))
                    entity.SingleCoilWeight = GetValue<decimal?>(row, colName);
            }
        }
    }

    private int ExcelColumnIndexToNumber(string columnIndex)
    {
        if (string.IsNullOrWhiteSpace(columnIndex))
            return -1;

        int number = 0;
        foreach (char c in columnIndex.Trim().ToUpperInvariant())
        {
            if (c < 'A' || c > 'Z')
                return -1;
            number = number * 26 + (c - 'A' + 1);
        }

        return number - 1;
    }

    private string ComputeValidDataHash(
        List<RawDataEntity> entities,
        ExcelTemplateConfig templateConfig
    )
    {
        if (entities == null || entities.Count == 0)
            return null;

        var validEntities = entities.Where(t => t.IsValidData == 1).ToList();
        if (validEntities.Count == 0)
            return null;

        var fingerprints = validEntities
            .Select(e => ComputeValidRowFingerprint(e, templateConfig))
            .Where(v => !string.IsNullOrEmpty(v))
            .OrderBy(v => v)
            .ToList();

        var combined = string.Join("|", fingerprints);
        return ComputeFileHashSha256(Encoding.UTF8.GetBytes(combined));
    }

    private string ComputeValidRowFingerprint(RawDataEntity entity, ExcelTemplateConfig templateConfig)
    {
        if (entity == null)
            return null;

        var fields = new List<string>();
        if (templateConfig?.FieldMappings != null && templateConfig.FieldMappings.Count > 0)
        {
            foreach (var mapping in templateConfig.FieldMappings)
            {
                if (string.IsNullOrWhiteSpace(mapping.Field))
                    continue;
                fields.Add(mapping.Field);
            }
        }

        for (int i = 1; i <= 22; i++)
        {
            fields.Add($"Detection{i}");
        }

        var distinctFields = fields.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(f => f).ToList();
        var parts = new List<string>();
        foreach (var field in distinctFields)
        {
            var prop = typeof(RawDataEntity).GetProperty(field);
            if (prop == null)
                continue;
            var value = prop.GetValue(entity);
            var normalized = NormalizeValue(value);
            parts.Add($"{field}={normalized}");
        }

        var raw = string.Join("|", parts);
        return ComputeFileHashSha256(Encoding.UTF8.GetBytes(raw));
    }

    private string NormalizeValue(object value)
    {
        if (value == null)
            return "";
        if (value is DateTime dt)
            return dt.ToString("yyyy-MM-dd");
        if (value is decimal dec)
            return dec.ToString("G29");
        if (value is double dbl)
            return dbl.ToString("G29");
        if (value is float flt)
            return flt.ToString("G29");
        return value.ToString().Trim();
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
            if (
                item.IsValidData == 1
                && item.LineNo.HasValue
                && !string.IsNullOrWhiteSpace(item.Shift)
                && item.ProdDate.HasValue
                && item.FurnaceBatchNo.HasValue
            )
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
    /// 检查数据库中已存在的炉号（用于预览项）.
    /// 如果本次Excel导入的炉号在数据库中已经存在，标记为将被忽略
    /// </summary>
    private async Task CheckExistingFurnaceNoInDatabaseForPreview(List<RawDataPreviewItem> items)
    {
        // 只检查符合规则的有效数据
        var validItems = items
            .Where(item =>
                item.IsValidData == 1
                && item.LineNo.HasValue
                && !string.IsNullOrWhiteSpace(item.Shift)
                && item.ProdDate.HasValue
                && item.FurnaceBatchNo.HasValue
            )
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
            .Where(e =>
                e.IsValidData == 1
                && e.LineNo.HasValue
                && !string.IsNullOrWhiteSpace(e.Shift)
                && e.ProdDate.HasValue
                && e.FurnaceBatchNo.HasValue
            )
            .Select(e => new {
                e.LineNo,
                e.Shift,
                e.ProdDate,
                e.FurnaceBatchNo,
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
                "1" // 分卷号默认为1
            );

            var standardFurnaceNo = furnaceNoObj?.GetFurnaceNo();
            if (
                !string.IsNullOrEmpty(standardFurnaceNo)
                && standardFurnaceNos.Contains(standardFurnaceNo)
            )
            {
                existingFurnaceNos.Add(standardFurnaceNo);
            }
        }

        // 标记数据库中已存在的炉号
        foreach (var item in validItems)
        {
            var standardFurnaceNo = GetFurnaceNo(item);
            if (
                !string.IsNullOrEmpty(standardFurnaceNo)
                && existingFurnaceNos.Contains(standardFurnaceNo)
            )
            {
                item.ExistsInDatabase = true;
                // 如果状态还是success或duplicate，改为exists_in_db
                if (item.Status == "success" || item.Status == "duplicate")
                {
                    item.Status = "exists_in_db";
                }
                // 追加提示信息
                var infoMessage =
                    $"炉号 {standardFurnaceNo} 在数据库中已存在，将被忽略，不会保存到数据库";
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
            if (skipCount < 0)
                skipCount = 0;

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

        // 4. 遍历规格进行匹配
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
            string fileHash = null;
            string fileMd5 = null;
            try
            {
                fileBytes = Convert.FromBase64String(input.FileData);
                fileHash = ComputeFileHashSha256(fileBytes);
                fileMd5 = ComputeFileHashMd5(fileBytes);
            }
            catch (Exception ex)
            {
                throw Oops.Oh($"文件数据格式错误: {ex.Message}");
            }

            // 2. 解析Excel数据
            var templateConfig = await LoadRawDataTemplateConfigAsync();
            var entities = ParseExcel(fileBytes, input.FileName, skipRows: 0, templateConfig);
            var validDataHash = ComputeValidDataHash(entities, templateConfig);
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
                SourceFileHash = fileHash,
                SourceFileMd5 = fileMd5,
                ValidDataHash = validDataHash,
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
                var intermediate = await _intermediateDataService.GenerateIntermediateDataAsync(
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
