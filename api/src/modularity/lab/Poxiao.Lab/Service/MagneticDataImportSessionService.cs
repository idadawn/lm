using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Logging;
using Poxiao.Infrastructure.Core.Manager.Files;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Config;
using Poxiao.Lab.Entity.Dto.IntermediateData;
using Poxiao.Lab.Entity.Dto.MagneticData;
using Poxiao.Lab.Entity.Enum;
using Poxiao.Lab.Helpers;
using Poxiao.Lab.Interfaces;
using Poxiao.Systems.Interfaces.Common;
using SqlSugar;
using System.Text.RegularExpressions;
using static Poxiao.Lab.Helpers.FurnaceNoHelper;

namespace Poxiao.Lab.Service;

/// <summary>
/// 磁性数据导入会话服务
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "magnetic-data-import-session", Order = 202)]
[Route("api/lab/magnetic-data-import-session")]
public class MagneticDataImportSessionService
    : IMagneticDataImportSessionService,
        IDynamicApiController,
        ITransient
{
    private readonly ISqlSugarRepository<MagneticDataImportSessionEntity> _sessionRepository;
    private readonly ISqlSugarRepository<IntermediateDataEntity> _intermediateDataRepository;
    private readonly ISqlSugarRepository<MagneticRawDataEntity> _magneticRawDataRepository;
    private readonly ISqlSugarRepository<ExcelImportTemplateEntity> _templateRepository;
    private readonly IFileService _fileService;
    private readonly IUserManager _userManager;
    private readonly IFileManager _fileManager;
    private readonly CalcTaskPublisher _calcTaskPublisher;

    public MagneticDataImportSessionService(
        ISqlSugarRepository<MagneticDataImportSessionEntity> sessionRepository,
        ISqlSugarRepository<IntermediateDataEntity> intermediateDataRepository,
        ISqlSugarRepository<MagneticRawDataEntity> magneticRawDataRepository,
        ISqlSugarRepository<ExcelImportTemplateEntity> templateRepository,
        IFileService fileService,
        IUserManager userManager,
        IFileManager fileManager,
        CalcTaskPublisher calcTaskPublisher
    )
    {
        _sessionRepository = sessionRepository;
        _intermediateDataRepository = intermediateDataRepository;
        _magneticRawDataRepository = magneticRawDataRepository;
        _templateRepository = templateRepository;
        _fileService = fileService;
        _userManager = userManager;
        _fileManager = fileManager;
        _calcTaskPublisher = calcTaskPublisher;
    }

    /// <inheritdoc />
    [HttpPost("create")]
    public async Task<string> Create([FromBody] MagneticDataImportSessionInput input)
    {
        if (input == null)
            throw Oops.Oh("请求体不能为空");
        if (string.IsNullOrWhiteSpace(input.FileName))
            throw Oops.Oh("文件名不能为空");

        string? creatorUserId = null;
        try
        {
            creatorUserId = _userManager.UserId;
        }
        catch
        {
            // 未登录或上下文无用户时允许为空，表字段可空
        }

        var session = new MagneticDataImportSessionEntity
        {
            Id = Guid.NewGuid().ToString(),
            FileName = input.FileName.Trim(),
            Status = "pending",
            CurrentStep = 0,
            CreatorUserId = creatorUserId,
            CreatorTime = DateTime.Now,
        };

        // 如果提供了文件数据，保存文件
        if (!string.IsNullOrEmpty(input.FileData))
        {
            try
            {
                var fileBytes = Convert.FromBase64String(input.FileData);
                var saveFileName = $"{DateTime.Now:yyyyMMddHHmmss}_{input.FileName.Trim()}";
                var basePath = _fileService.GetPathByType("MagneticData");
                if (string.IsNullOrEmpty(basePath))
                    basePath = _fileService.GetPathByType("");

                if (!string.IsNullOrEmpty(basePath))
                {
                    using var stream = new MemoryStream(fileBytes);
                    await _fileManager.UploadFileByType(stream, basePath, saveFileName);
                    session.SourceFileId = $"{basePath}/{saveFileName}";
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Create] File upload failed: {ex.Message}");
                // 文件保存失败不影响会话创建
            }
        }

        try
        {
            await _sessionRepository.InsertAsync(session);
            return session.Id;
        }
        catch (Exception ex)
        {
            Log.Error($"[Create] Insert session failed: {ex.Message}");
            throw Oops.Oh("创建导入会话失败：" + ex.Message);
        }
    }

    /// <inheritdoc />
    [HttpGet("{id}")]
    public async Task<MagneticDataImportSessionEntity> Get(string id)
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
    [HttpPost("{sessionId}/step1/upload-and-parse")]
    public async Task<MagneticDataImportStep1Output> UploadAndParse(
        string sessionId,
        [FromBody] MagneticDataImportSessionInput input
    )
    {
        // 1. 获取已存在的会话
        if (string.IsNullOrEmpty(sessionId))
            throw Oops.Oh("导入会话ID不能为空");

        var existingSession = await _sessionRepository.GetFirstAsync(t => t.Id == sessionId);
        if (existingSession == null)
            throw Oops.Oh("导入会话不存在");

        // 更新会话信息
        if (!string.IsNullOrEmpty(input.FileName))
            existingSession.FileName = input.FileName;
        existingSession.Status = "in_progress";
        await _sessionRepository.UpdateAsync(existingSession);

        // 2. 读取已保存的文件
        byte[] fileBytes;
        if (!string.IsNullOrEmpty(input.FileData))
        {
            // 如果请求中有文件数据，使用请求中的
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

        // 3. 获取模板配置
        ExcelTemplateConfig templateConfig = null;
        var template = await _templateRepository.GetFirstAsync(t =>
            t.TemplateCode == ExcelImportTemplateCode.MagneticDataImport.ToString()
            && (t.DeleteMark == 0 || t.DeleteMark == null)
        );

        if (template != null && !string.IsNullOrWhiteSpace(template.ConfigJson))
        {
            try
            {
                var json = template.ConfigJson;
                // 处理双重序列化
                if (json.StartsWith("\"") && json.EndsWith("\""))
                {
                    try
                    {
                        json = System.Text.Json.JsonSerializer.Deserialize<string>(json);
                    }
                    catch
                    {
                        // 忽略，当作普通JSON处理
                    }
                }

                templateConfig = System.Text.Json.JsonSerializer.Deserialize<ExcelTemplateConfig>(
                    json
                );
            }
            catch (Exception ex)
            {
                Log.Error($"[UploadAndParse] 解析模板配置失败: {ex.Message}");
                // 继续使用默认配置
            }
        }

        // 4. 解析Excel文件
        var parsedData = ParseExcel(
            fileBytes,
            input.FileName ?? existingSession.FileName,
            templateConfig
        );

        // 4. 保存解析后的数据到临时文件
        var tempFilePath = await SaveParsedDataToFile(sessionId, parsedData);
        existingSession.ParsedDataFile = tempFilePath;
        existingSession.TotalRows = parsedData.Count;
        existingSession.ValidDataRows = parsedData.Count(t => t.IsValid);
        existingSession.CurrentStep = 1;
        await _sessionRepository.UpdateAsync(existingSession);

        // 5. 返回解析结果
        return new MagneticDataImportStep1Output
        {
            ImportSessionId = sessionId,
            ParsedData = parsedData,
            TotalRows = parsedData.Count,
            ValidDataRows = parsedData.Count(t => t.IsValid),
            Errors = parsedData.Where(t => !t.IsValid).Select(t => t.ErrorMessage).ToList(),
        };
    }

    /// <inheritdoc />
    [HttpGet("{sessionId}/review")]
    public async Task<MagneticDataReviewOutput> GetReviewData(string sessionId)
    {
        var session = await _sessionRepository.GetFirstAsync(t => t.Id == sessionId);
        if (session == null)
            throw Oops.Oh("导入会话不存在");

        // 加载解析后的数据
        var parsedData = await LoadParsedDataFromFile(sessionId);
        var validData = parsedData.Where(t => t.IsValid).ToList();

        // 按炉号分组（不区分是否刻痕），每个炉号只选择一条最优数据
        var dataByFurnaceNo = validData.GroupBy(t => t.FurnaceNo).ToList();

        // 为每个炉号选择最优数据并标记
        foreach (var group in dataByFurnaceNo)
        {
            var items = group.ToList();
            MagneticDataImportItem bestItem;
            if (items.Count > 1)
            {
                bestItem = SelectBestData(items);
            }
            else
            {
                bestItem = items[0];
            }

            // 标记最优数据
            foreach (var item in items)
            {
                item.IsBest = item == bestItem;
            }
        }

        return new MagneticDataReviewOutput
        {
            Session = session,
            TotalRows = parsedData.Count,
            ValidDataRows = validData.Count,
            UpdatedRows = 0, // 将在完成导入时计算
            SkippedRows = 0, // 将在完成导入时计算
            Errors = parsedData.Where(t => !t.IsValid).Select(t => t.ErrorMessage).ToList(),
            ValidData = validData,
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
        var validData = allData.Where(t => t.IsValid).ToList();

        if (validData.Count == 0)
        {
            session.Status = "failed";
            await _sessionRepository.UpdateAsync(session);
            throw Oops.Oh("没有有效数据可以导入");
        }

        // 2. 准备原始数据实体
        var rawDataEntities = new List<MagneticRawDataEntity>();
        foreach (var item in allData)
        {
            var rawDataEntity = new MagneticRawDataEntity
            {
                Id = Guid.NewGuid().ToString(),
                OriginalFurnaceNo = item.OriginalFurnaceNo,
                FurnaceNo = item.FurnaceNo,
                IsScratched = item.IsScratched ? 1 : 0,
                PsLoss = item.PsLoss,
                SsPower = item.SsPower,
                Hc = item.Hc,
                DetectionTime = item.DetectionTime,
                RowIndex = item.RowIndex,
                IsValid = item.IsValid ? 1 : 0,
                ErrorMessage = item.ErrorMessage,
                ImportSessionId = sessionId,
                CreatorUserId = _userManager.UserId,
                CreatorTime = DateTime.Now,
                SortCode = item.RowIndex,
            };

            // 解析炉号信息
            var parseResult = ParseMagneticFurnaceNo(item.FurnaceNo, out bool isScratched);
            if (parseResult.Success)
            {
                rawDataEntity.LineNo = parseResult.LineNoNumeric;
                rawDataEntity.Shift = parseResult.Shift;
                rawDataEntity.ShiftNumeric = parseResult.ShiftNumeric;
                rawDataEntity.FurnaceBatchNo = parseResult.FurnaceNoNumeric;
                rawDataEntity.CoilNo = parseResult.CoilNoNumeric;
                rawDataEntity.SubcoilNo = parseResult.SubcoilNoNumeric;
                rawDataEntity.ProdDate = parseResult.ProdDate;
                rawDataEntity.FurnaceNoParsed = parseResult.FurnaceNo;
            }

            rawDataEntities.Add(rawDataEntity);
        }

        // 3. 按炉号分组，构建 MQ 消息载荷（不再逐条查询中间数据表，由 Worker 端通过炉号查找）
        var dataByFurnaceNo = validData.GroupBy(t => t.FurnaceNo).ToList();
        var magneticJudgeItems = new List<MagneticDataPayload>();

        foreach (var group in dataByFurnaceNo)
        {
            var items = group.ToList();
            MagneticDataImportItem bestItem = items.Count > 1 ? SelectBestData(items) : items[0];

            // 构建磁性数据载荷（包含炉号，Worker 端用炉号查找中间数据记录）
            var payload = new MagneticDataPayload
            {
                FurnaceNo = bestItem.FurnaceNo,
                OriginalFurnaceNo = bestItem.OriginalFurnaceNo,
                IsScratched = bestItem.IsScratched,
                SsPower = bestItem.SsPower,
                PsLoss = bestItem.PsLoss,
                Hc = bestItem.Hc,
                DetectionTime = bestItem.DetectionTime,
                EditorId = _userManager.UserId,
                EditorName = _userManager.RealName,
            };

            magneticJudgeItems.Add(payload);
        }

        // 4. 事务：只写入磁性原始数据（不更新中间表）
        var db = _sessionRepository.AsSugarClient();

        try
        {
            await db.Ado.BeginTranAsync();

            if (rawDataEntities.Count > 0)
            {
                // 先删除已存在的同名炉号数据，防止重复堆积
                var furnaceNos = rawDataEntities
                    .Select(x => x.OriginalFurnaceNo)
                    .Distinct()
                    .ToList();
                if (furnaceNos.Count > 0)
                {
                    var deleteBatches = furnaceNos.Chunk(1000);
                    foreach (var batch in deleteBatches)
                    {
                        var batchList = batch.ToList();
                        await _magneticRawDataRepository.DeleteAsync(t =>
                            batchList.Contains(t.OriginalFurnaceNo)
                        );
                    }
                }

                // 分批插入防止SQL太长
                var batches = rawDataEntities.Chunk(1000);
                foreach (var batch in batches)
                {
                    await _magneticRawDataRepository
                        .AsInsertable(batch.ToList())
                        .ExecuteCommandAsync();
                }
            }

            await db.Ado.CommitTranAsync();
        }
        catch (Exception ex)
        {
            await db.Ado.RollbackTranAsync();
            throw Oops.Oh($"导入失败，数据已回滚: {ex.Message}");
        }

        // 5. 更新会话状态
        session.Status = magneticJudgeItems.Count == 0 ? "failed" : "completed";
        session.CurrentStep = 2;
        await _sessionRepository.UpdateAsync(session);

        // 6. 发布 per-item 磁性更新+判定任务到 MQ（Worker 端通过炉号查找中间数据、更新磁性字段、执行判定）
        if (magneticJudgeItems.Count > 0)
        {
            var batchId = Guid.NewGuid().ToString();
            var tenantId = _userManager?.TenantId ?? "global";
            var userId = _userManager?.UserId ?? string.Empty;
            _calcTaskPublisher.PublishMagneticJudgeItems(batchId, magneticJudgeItems, tenantId, userId);
        }
        else
        {
            throw Oops.Oh("没有有效的磁性数据可以处理");
        }
    }

    /// <inheritdoc />
    [HttpDelete("{sessionId}")]
    public async Task CancelImport(string sessionId)
    {
        try
        {
            var session = await _sessionRepository.GetFirstAsync(t => t.Id == sessionId);
            if (session == null)
                return;

            // 删除临时文件
            if (!string.IsNullOrEmpty(session.ParsedDataFile))
            {
                try
                {
                    if (File.Exists(session.ParsedDataFile))
                    {
                        File.Delete(session.ParsedDataFile);
                    }
                }
                catch
                {
                    // 忽略删除失败
                }
            }

            // 删除会话
            await _sessionRepository.DeleteAsync(session);
        }
        catch (Exception ex)
        {
            throw Oops.Oh($"取消导入失败: {ex.Message}");
        }
    }

    // ================= Private Methods =================

    /// <summary>
    /// 解析Excel文件，根据模板配置读取列
    /// </summary>
    private List<MagneticDataImportItem> ParseExcel(
        byte[] fileBytes,
        string fileName,
        ExcelTemplateConfig templateConfig = null
    )
    {
        var items = new List<MagneticDataImportItem>();
        using var stream = new MemoryStream(fileBytes);
        IWorkbook workbook = fileName.EndsWith(".xlsx")
            ? new XSSFWorkbook(stream)
            : new HSSFWorkbook(stream);
        var sheet = workbook.GetSheetAt(0);

        // 解析表头，建立列名到列索引的映射
        var headerRow = sheet.GetRow(0);
        var columnNameToIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (headerRow != null)
        {
            for (int i = 0; i <= headerRow.LastCellNum; i++)
            {
                var cell = headerRow.GetCell(i);
                if (cell != null)
                {
                    var cellValue = cell.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(cellValue))
                    {
                        // 支持多个列名映射到同一列索引
                        if (!columnNameToIndex.ContainsKey(cellValue))
                        {
                            columnNameToIndex[cellValue] = i;
                        }
                    }
                }
            }
        }

        // 根据模板配置获取列索引，如果没有配置则使用默认值
        int colOriginalFurnaceNo = GetColumnIndex(
            templateConfig,
            "OriginalFurnaceNo",
            columnNameToIndex,
            "B",
            1
        );
        int colPsLoss = GetColumnIndex(templateConfig, "PsLoss", columnNameToIndex, "H", 7);
        int colSsPower = GetColumnIndex(templateConfig, "SsPower", columnNameToIndex, "I", 8);
        int colHc = GetColumnIndex(templateConfig, "Hc", columnNameToIndex, "F", 5);
        int colDetectionTime = GetColumnIndex(
            templateConfig,
            "DetectionTime",
            columnNameToIndex,
            "P",
            15
        );

        // 从第2行开始读取（第1行是表头）
        for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
        {
            var row = sheet.GetRow(rowIndex);
            if (row == null)
                continue;

            var item = new MagneticDataImportItem
            {
                RowIndex = rowIndex + 1, // Excel行号（从1开始）
            };

            try
            {
                // 读取原始炉号
                item.OriginalFurnaceNo = GetCellValue<string>(row, colOriginalFurnaceNo);
                if (string.IsNullOrWhiteSpace(item.OriginalFurnaceNo))
                {
                    item.IsValid = false;
                    item.ErrorMessage = "第" + item.RowIndex + "行：炉号为空";
                    items.Add(item);
                    continue;
                }

                // 解析炉号，检测是否带K
                var parseResult = ParseMagneticFurnaceNo(
                    item.OriginalFurnaceNo,
                    out bool isScratched
                );
                if (!parseResult.Success)
                {
                    item.IsValid = false;
                    item.ErrorMessage =
                        "第" + item.RowIndex + "行：炉号解析失败 - " + parseResult.ErrorMessage;
                    items.Add(item);
                    continue;
                }

                item.FurnaceNo = parseResult.FurnaceNo;
                item.IsScratched = isScratched;

                // 读取Ps铁损
                item.PsLoss = GetCellValue<decimal?>(row, colPsLoss);

                // 读取Ss激磁功率
                item.SsPower = GetCellValue<decimal?>(row, colSsPower);

                // 读取Hc
                item.Hc = GetCellValue<decimal?>(row, colHc);

                // 读取检测时间
                item.DetectionTime = GetCellValue<DateTime?>(row, colDetectionTime);

                // 验证至少有一个有效值
                if (!item.PsLoss.HasValue && !item.SsPower.HasValue && !item.Hc.HasValue)
                {
                    item.IsValid = false;
                    item.ErrorMessage = "第" + item.RowIndex + "行：Ps铁损、Ss激磁功率、Hc都为空";
                    items.Add(item);
                    continue;
                }

                item.IsValid = true;
            }
            catch (Exception ex)
            {
                item.IsValid = false;
                item.ErrorMessage = "第" + item.RowIndex + "行：解析失败 - " + ex.Message;
            }

            items.Add(item);
        }

        return items;
    }

    /// <summary>
    /// 根据模板配置获取列索引
    /// </summary>
    private int GetColumnIndex(
        ExcelTemplateConfig templateConfig,
        string fieldName,
        Dictionary<string, int> columnNameToIndex,
        string defaultColumnLetter,
        int defaultColumnIndex
    )
    {
        // 如果模板配置存在，尝试从配置中获取
        if (templateConfig?.FieldMappings != null)
        {
            var mapping = templateConfig.FieldMappings.FirstOrDefault(m => m.Field == fieldName);
            if (mapping != null)
            {
                // 优先使用 ExcelColumnIndex（列字母，如 "H"）
                if (!string.IsNullOrEmpty(mapping.ExcelColumnIndex))
                {
                    var index = ConvertColumnLetterToIndex(mapping.ExcelColumnIndex);
                    if (index >= 0)
                    {
                        return index;
                    }
                }

                // 其次尝试使用 ExcelColumnNames（列名）
                if (mapping.ExcelColumnNames != null && mapping.ExcelColumnNames.Count > 0)
                {
                    foreach (var columnName in mapping.ExcelColumnNames)
                    {
                        if (string.IsNullOrEmpty(columnName))
                            continue;

                        // 精确匹配
                        if (columnNameToIndex.ContainsKey(columnName))
                        {
                            return columnNameToIndex[columnName];
                        }

                        // 部分匹配：支持列名中包含列字母的情况，如 "Ps铁损(H)" 匹配 "Ps铁损"
                        var matchedKey = columnNameToIndex.Keys.FirstOrDefault(k =>
                            k.Contains(columnName, StringComparison.OrdinalIgnoreCase)
                            || columnName.Contains(k, StringComparison.OrdinalIgnoreCase)
                        );
                        if (matchedKey != null)
                        {
                            return columnNameToIndex[matchedKey];
                        }
                    }
                }
            }
        }

        // 如果模板配置不存在或未找到，使用默认值
        return defaultColumnIndex;
    }

    /// <summary>
    /// 将Excel列字母（如 "A", "B", "H", "AA"）转换为列索引（从0开始）
    /// </summary>
    private int ConvertColumnLetterToIndex(string columnLetter)
    {
        if (string.IsNullOrEmpty(columnLetter))
            return -1;

        columnLetter = columnLetter.Trim().ToUpper();
        int index = 0;
        for (int i = 0; i < columnLetter.Length; i++)
        {
            char c = columnLetter[i];
            if (c < 'A' || c > 'Z')
                return -1;
            index = (index * 26) + (c - 'A' + 1);
        }
        return index - 1; // 转换为从0开始的索引
    }

    /// <summary>
    /// 解析磁性数据炉号
    /// 格式：(产线数字)(班次汉字)(8位日期)-(炉号)(是否刻痕K)
    /// 例如：1甲20251101-1, 1甲20251101-1K
    /// 也支持包含卷号和分卷号的格式，但只提取前面的部分：1丙20260110-1-1-1 -> 1丙20260110-1
    /// </summary>
    private FurnaceNoParseResult ParseMagneticFurnaceNo(string furnaceNo, out bool isScratched)
    {
        var result = new FurnaceNoParseResult();

        if (string.IsNullOrWhiteSpace(furnaceNo))
        {
            result.ErrorMessage = "炉号为空";
            isScratched = false;
            return result;
        }

        // 检测是否带K（不区分大小写）
        isScratched = furnaceNo.EndsWith("K", StringComparison.OrdinalIgnoreCase);

        furnaceNo = furnaceNo.Trim();

        if (isScratched)
        {
            furnaceNo = furnaceNo.Substring(0, furnaceNo.Length - 1).TrimEnd();
        }

        var match = FurnaceNoHelper.ParseFurnaceNo(furnaceNo);

        if (!match.Success)
        {
            result.ErrorMessage = "炉号格式不符合规则";
            return result;
        }

        try
        {
            result.LineNo = match.LineNo; // 产线
            result.Shift = match.Shift; // 班次
            var dateStr = match.ProdDate; // 日期字符串

            // 解析数字字段（参考原始数据表）
            if (int.TryParse(result.LineNo, out var lineNoNum))
            {
                result.LineNoNumeric = lineNoNum;
            }

            // 磁性数据炉号格式不包含卷号和分卷号，设置为null
            result.CoilNo = null;
            result.SubcoilNo = null;
            result.FurnaceNo = furnaceNo;
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"解析失败：{ex.Message}";
        }

        return result;
    }

    /// <summary>
    /// 选择最优数据（多条数据时，取值最小优先级H、I、F，最后按检测时间）
    /// </summary>
    private MagneticDataImportItem SelectBestData(List<MagneticDataImportItem> items)
    {
        if (items.Count == 0)
            throw new ArgumentException("数据列表为空");
        if (items.Count == 1)
            return items[0];

        // 优先级：H(Ps铁损)、I(Ss激磁功率)、F(Hc)
        // 值最小为最优数据
        // 如果以上数据还无法确定一条数据，以最后的检测时间作为最好的数据

        // 1. 按H(Ps铁损)排序，值小的优先
        var sortedByH = items.Where(t => t.PsLoss.HasValue).OrderBy(t => t.PsLoss.Value).ToList();

        if (sortedByH.Count > 0)
        {
            var minH = sortedByH[0].PsLoss.Value;
            var candidates = sortedByH.Where(t => t.PsLoss.Value == minH).ToList();

            if (candidates.Count == 1)
                return candidates[0];

            // 2. 如果H相同，按I(Ss激磁功率)排序
            var sortedByI = candidates
                .Where(t => t.SsPower.HasValue)
                .OrderBy(t => t.SsPower.Value)
                .ToList();

            if (sortedByI.Count > 0)
            {
                var minI = sortedByI[0].SsPower.Value;
                var candidates2 = sortedByI.Where(t => t.SsPower.Value == minI).ToList();

                if (candidates2.Count == 1)
                    return candidates2[0];

                // 3. 如果I也相同，按F(Hc)排序
                var sortedByF = candidates2
                    .Where(t => t.Hc.HasValue)
                    .OrderBy(t => t.Hc.Value)
                    .ToList();

                if (sortedByF.Count > 0)
                {
                    var minF = sortedByF[0].Hc.Value;
                    var candidates3 = sortedByF.Where(t => t.Hc.Value == minF).ToList();

                    if (candidates3.Count == 1)
                        return candidates3[0];

                    // 4. 如果F也相同，按检测时间排序（最新的优先）
                    return candidates3
                        .OrderByDescending(t => t.DetectionTime ?? DateTime.MinValue)
                        .First();
                }
                else
                {
                    // 没有Hc值，按检测时间排序
                    return candidates2
                        .OrderByDescending(t => t.DetectionTime ?? DateTime.MinValue)
                        .First();
                }
            }
            else
            {
                // 没有SsPower值，按检测时间排序
                return candidates
                    .OrderByDescending(t => t.DetectionTime ?? DateTime.MinValue)
                    .First();
            }
        }

        // 如果没有PsLoss值，尝试按SsPower排序
        var sortedByIOnly = items
            .Where(t => t.SsPower.HasValue)
            .OrderBy(t => t.SsPower.Value)
            .ToList();

        if (sortedByIOnly.Count > 0)
        {
            var minI = sortedByIOnly[0].SsPower.Value;
            var candidates = sortedByIOnly.Where(t => t.SsPower.Value == minI).ToList();

            if (candidates.Count == 1)
                return candidates[0];

            // 按检测时间排序
            return candidates.OrderByDescending(t => t.DetectionTime ?? DateTime.MinValue).First();
        }

        // 如果没有SsPower值，尝试按Hc排序
        var sortedByFOnly = items.Where(t => t.Hc.HasValue).OrderBy(t => t.Hc.Value).ToList();

        if (sortedByFOnly.Count > 0)
        {
            var minF = sortedByFOnly[0].Hc.Value;
            var candidates = sortedByFOnly.Where(t => t.Hc.Value == minF).ToList();

            if (candidates.Count == 1)
                return candidates[0];

            // 按检测时间排序
            return candidates.OrderByDescending(t => t.DetectionTime ?? DateTime.MinValue).First();
        }

        // 如果都没有值，按检测时间排序
        return items.OrderByDescending(t => t.DetectionTime ?? DateTime.MinValue).First();
    }

    /// <summary>
    /// 按列索引获取单元格值
    /// </summary>
    private T GetCellValue<T>(IRow row, int columnIndex)
    {
        if (row == null || columnIndex < 0)
            return default(T);

        var cell = row.GetCell(columnIndex);
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
    /// 保存解析后的数据到临时文件
    /// </summary>
    private async Task<string> SaveParsedDataToFile(
        string sessionId,
        List<MagneticDataImportItem> data
    )
    {
        var basePath = _fileService.GetPathByType("TemporaryFile");
        if (string.IsNullOrEmpty(basePath))
            basePath = Path.Combine(Directory.GetCurrentDirectory(), "resources", "TemporaryFile");

        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }

        var fileName = $"{sessionId}_parsed.json";
        var filePath = Path.Combine(basePath, fileName);

        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        await File.WriteAllTextAsync(filePath, json, System.Text.Encoding.UTF8);

        return filePath;
    }

    /// <summary>
    /// 从临时文件加载解析后的数据
    /// </summary>
    private async Task<List<MagneticDataImportItem>> LoadParsedDataFromFile(string sessionId)
    {
        var session = await _sessionRepository.GetFirstAsync(t => t.Id == sessionId);
        if (session == null || string.IsNullOrEmpty(session.ParsedDataFile))
            throw Oops.Oh("解析数据文件不存在");

        if (!File.Exists(session.ParsedDataFile))
            throw Oops.Oh("解析数据文件不存在");

        var json = await File.ReadAllTextAsync(session.ParsedDataFile, System.Text.Encoding.UTF8);
        return JsonConvert.DeserializeObject<List<MagneticDataImportItem>>(json)
            ?? new List<MagneticDataImportItem>();
    }

    /// <summary>
    /// 将班次汉字转换为数字（用于排序）.
    /// 甲=1, 乙=2, 丙=3
    /// </summary>
    /// <param name="shift">班次汉字</param>
    /// <returns>班次数字，如果无法识别则返回null</returns>
    private static int? ConvertShiftToNumeric(string shift)
    {
        if (string.IsNullOrWhiteSpace(shift))
            return null;

        // 移除所有空白字符后判断
        var shiftTrimmed = shift.Trim();

        if (shiftTrimmed.Contains("甲"))
            return 1;
        if (shiftTrimmed.Contains("乙"))
            return 2;
        if (shiftTrimmed.Contains("丙"))
            return 3;

        // 如果无法识别，返回null
        return null;
    }
}
