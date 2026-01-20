using System.Text.RegularExpressions;
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
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.MagneticData;
using Poxiao.Lab.Helpers;
using Poxiao.Lab.Interfaces;
using Poxiao.Systems.Interfaces.Common;
using SqlSugar;

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
    private readonly IFileService _fileService;
    private readonly IUserManager _userManager;
    private readonly IFileManager _fileManager;

    public MagneticDataImportSessionService(
        ISqlSugarRepository<MagneticDataImportSessionEntity> sessionRepository,
        ISqlSugarRepository<IntermediateDataEntity> intermediateDataRepository,
        ISqlSugarRepository<MagneticRawDataEntity> magneticRawDataRepository,
        IFileService fileService,
        IUserManager userManager,
        IFileManager fileManager
    )
    {
        _sessionRepository = sessionRepository;
        _intermediateDataRepository = intermediateDataRepository;
        _magneticRawDataRepository = magneticRawDataRepository;
        _fileService = fileService;
        _userManager = userManager;
        _fileManager = fileManager;
    }

    /// <inheritdoc />
    [HttpPost("create")]
    public async Task<string> Create([FromBody] MagneticDataImportSessionInput input)
    {
        var session = new MagneticDataImportSessionEntity
        {
            Id = Guid.NewGuid().ToString(),
            FileName = input.FileName,
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
                var basePath = _fileService.GetPathByType("MagneticData");
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

        // 3. 解析Excel文件
        var parsedData = ParseExcel(fileBytes, input.FileName ?? existingSession.FileName);

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

        return new MagneticDataReviewOutput
        {
            Session = session,
            TotalRows = parsedData.Count,
            ValidDataRows = validData.Count,
            UpdatedRows = 0, // 将在完成导入时计算
            SkippedRows = 0, // 将在完成导入时计算
            Errors = parsedData.Where(t => !t.IsValid).Select(t => t.ErrorMessage).ToList(),
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

        // 2. 保存所有原始数据到磁性原始数据表（包括有效和无效数据）
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
            var parseResult = ParseMagneticFurnaceNo(item.FurnaceNo);
            if (parseResult.Success)
            {
                rawDataEntity.LineNo = parseResult.LineNoNumeric;
                rawDataEntity.Shift = parseResult.Shift;
                rawDataEntity.ShiftNumeric = parseResult.ShiftNumeric;
                rawDataEntity.FurnaceBatchNo = parseResult.FurnaceBatchNo;
                rawDataEntity.CoilNo = parseResult.CoilNo;
                rawDataEntity.SubcoilNo = parseResult.SubcoilNo;
                rawDataEntity.ProdDate = parseResult.ProdDate;
                rawDataEntity.FurnaceNoParsed = parseResult.FurnaceNo;
            }

            rawDataEntities.Add(rawDataEntity);
        }

        // 批量插入原始数据
        if (rawDataEntities.Count > 0)
        {
            var batches = rawDataEntities.Chunk(1000);
            foreach (var batch in batches)
            {
                await _magneticRawDataRepository.AsInsertable(batch.ToList()).ExecuteCommandAsync();
            }
        }

        // 3. 按炉号分组，处理多条数据的情况
        var dataByFurnaceNo = validData.GroupBy(t => t.FurnaceNo).ToList();

        int updatedRows = 0;
        int skippedRows = 0;
        var errors = new List<string>();

        foreach (var group in dataByFurnaceNo)
        {
            var furnaceNo = group.Key;
            var items = group.ToList();

            // 如果同一炉号有多条数据，需要选择最优数据
            MagneticDataImportItem bestItem;
            if (items.Count > 1)
            {
                bestItem = SelectBestData(items);
            }
            else
            {
                bestItem = items[0];
            }

            // 4. 根据是否带K，更新中间数据表的不同字段
            try
            {
                // 查找中间数据表中对应的记录
                // 炉号格式：(产线数字)(班次汉字)(8位日期)-(炉号)
                // 需要解析炉号，然后查找匹配的记录
                var parseResult = ParseMagneticFurnaceNo(bestItem.FurnaceNo);
                if (!parseResult.Success)
                {
                    errors.Add(
                        $"炉号 {bestItem.OriginalFurnaceNo} 解析失败: {parseResult.ErrorMessage}"
                    );
                    skippedRows++;
                    continue;
                }

                // 构建查询条件：产线、班次、日期、炉号
                var query = _intermediateDataRepository
                    .AsQueryable()
                    .Where(t => t.DeleteMark == null);

                if (parseResult.LineNoNumeric.HasValue)
                {
                    // 中间数据表的LineNo是INT，直接比较
                    query = query.Where(t => t.LineNo == parseResult.LineNoNumeric.Value);
                }

                if (!string.IsNullOrEmpty(parseResult.Shift))
                {
                    query = query.Where(t => t.Shift == parseResult.Shift);
                }

                if (parseResult.ProdDate.HasValue)
                {
                    query = query.Where(t =>
                        t.ProdDate.HasValue
                        && t.ProdDate.Value.Date == parseResult.ProdDate.Value.Date
                    );
                }

                if (parseResult.FurnaceBatchNo.HasValue)
                {
                    // 中间数据表的FurnaceBatchNo是INT，直接比较
                    query = query.Where(t => t.FurnaceBatchNo == parseResult.FurnaceBatchNo.Value);
                }

                var intermediateData = await query.FirstAsync();

                if (intermediateData == null)
                {
                    errors.Add($"未找到炉号 {bestItem.OriginalFurnaceNo} 对应的中间数据记录");
                    skippedRows++;
                    continue;
                }

                // 根据是否带K，更新不同的字段
                if (bestItem.IsScratched)
                {
                    // 带K：更新刻痕后性能字段
                    intermediateData.PerfAfterSsPower = bestItem.SsPower;
                    intermediateData.PerfAfterPsLoss = bestItem.PsLoss;
                    intermediateData.PerfAfterHc = bestItem.Hc;
                }
                else
                {
                    // 不带K：更新正常性能字段
                    intermediateData.PerfSsPower = bestItem.SsPower;
                    intermediateData.PerfPsLoss = bestItem.PsLoss;
                    intermediateData.PerfHc = bestItem.Hc;
                }

                // 更新检测时间（如果有）
                if (bestItem.DetectionTime.HasValue)
                {
                    // 注意：中间数据表可能没有检测时间字段，这里先不更新
                    // 如果需要，可以在中间数据表中添加检测时间字段
                }

                // 更新编辑信息
                intermediateData.PerfEditorId = _userManager.UserId;
                intermediateData.PerfEditorName = _userManager.RealName;
                intermediateData.PerfEditTime = DateTime.Now;
                intermediateData.LastModifyUserId = _userManager.UserId;
                intermediateData.LastModifyTime = DateTime.Now;

                await _intermediateDataRepository.UpdateAsync(intermediateData);
                updatedRows++;
            }
            catch (Exception ex)
            {
                errors.Add($"更新炉号 {bestItem.OriginalFurnaceNo} 失败: {ex.Message}");
                skippedRows++;
            }
        }

        // 5. 更新会话状态
        session.Status = errors.Count > 0 && updatedRows == 0 ? "failed" : "completed";
        session.CurrentStep = 2;
        await _sessionRepository.UpdateAsync(session);

        // 如果有错误但至少更新了一些数据，返回警告而不是错误
        if (errors.Count > 0)
        {
            if (updatedRows > 0)
            {
                // 部分成功，返回警告信息
                var errorSummary =
                    errors.Count <= 10
                        ? string.Join("; ", errors)
                        : string.Join("; ", errors.Take(10)) + $"... (共 {errors.Count} 条错误)";
                throw Oops.Oh(
                    $"导入完成，成功更新 {updatedRows} 条数据，但有 {errors.Count} 条错误: {errorSummary}"
                );
            }
            else
            {
                // 完全失败
                var errorSummary =
                    errors.Count <= 10
                        ? string.Join("; ", errors)
                        : string.Join("; ", errors.Take(10)) + $"... (共 {errors.Count} 条错误)";
                throw Oops.Oh($"导入失败: {errorSummary}");
            }
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
    /// 解析Excel文件，读取B、H、I、F、P列
    /// </summary>
    private List<MagneticDataImportItem> ParseExcel(byte[] fileBytes, string fileName)
    {
        var items = new List<MagneticDataImportItem>();
        using var stream = new MemoryStream(fileBytes);
        IWorkbook workbook = fileName.EndsWith(".xlsx")
            ? new XSSFWorkbook(stream)
            : new HSSFWorkbook(stream);
        var sheet = workbook.GetSheetAt(0);

        // Excel列索引：B=1, H=7, I=8, F=5, P=15
        int colB = 1; // 原始炉号
        int colH = 7; // Ps铁损
        int colI = 8; // Ss激磁功率
        int colF = 5; // Hc
        int colP = 15; // 检测时间

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
                // 读取B列：原始炉号
                item.OriginalFurnaceNo = GetCellValue<string>(row, colB);
                if (string.IsNullOrWhiteSpace(item.OriginalFurnaceNo))
                {
                    item.IsValid = false;
                    item.ErrorMessage = "第" + item.RowIndex + "行：炉号为空";
                    items.Add(item);
                    continue;
                }

                // 解析炉号，检测是否带K
                var parseResult = ParseMagneticFurnaceNo(item.OriginalFurnaceNo);
                if (!parseResult.Success)
                {
                    item.IsValid = false;
                    item.ErrorMessage =
                        "第" + item.RowIndex + "行：炉号解析失败 - " + parseResult.ErrorMessage;
                    items.Add(item);
                    continue;
                }

                item.FurnaceNo = parseResult.FurnaceNo;
                item.IsScratched = parseResult.IsScratched;

                // 读取H列：Ps铁损
                item.PsLoss = GetCellValue<decimal?>(row, colH);

                // 读取I列：Ss激磁功率
                item.SsPower = GetCellValue<decimal?>(row, colI);

                // 读取F列：Hc
                item.Hc = GetCellValue<decimal?>(row, colF);

                // 读取P列：检测时间
                item.DetectionTime = GetCellValue<DateTime?>(row, colP);

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
    /// 解析磁性数据炉号
    /// 格式：(产线数字)(班次汉字)(8位日期)-(炉号)(是否刻痕K)
    /// 例如：1甲20251101-1, 1甲20251101-1K
    /// </summary>
    private MagneticFurnaceNoParseResult ParseMagneticFurnaceNo(string furnaceNo)
    {
        var result = new MagneticFurnaceNoParseResult();

        if (string.IsNullOrWhiteSpace(furnaceNo))
        {
            result.ErrorMessage = "炉号为空";
            return result;
        }

        furnaceNo = furnaceNo.Trim();

        // 检测是否带K（不区分大小写）
        bool isScratched = furnaceNo.EndsWith("K", StringComparison.OrdinalIgnoreCase);
        if (isScratched)
        {
            furnaceNo = furnaceNo.Substring(0, furnaceNo.Length - 1).TrimEnd();
        }

        // 正则表达式：匹配 [产线数字][班次汉字][8位日期]-[炉号]
        var pattern = @"^(\d+)([^\d]+?)(\d{8})-(\d+)$";
        var match = Regex.Match(furnaceNo, pattern);

        if (!match.Success)
        {
            result.ErrorMessage = "炉号格式不符合规则";
            return result;
        }

        try
        {
            result.LineNo = match.Groups[1].Value; // 产线
            result.Shift = match.Groups[2].Value.Trim(); // 班次
            var dateStr = match.Groups[3].Value; // 日期字符串
            result.FurnaceNo = match.Groups[4].Value; // 炉号
            result.IsScratched = isScratched;

            // 解析日期
            if (
                DateTime.TryParseExact(
                    dateStr,
                    "yyyyMMdd",
                    null,
                    System.Globalization.DateTimeStyles.None,
                    out var date
                )
            )
            {
                result.ProdDate = date;
            }
            else
            {
                result.ErrorMessage = $"日期格式错误：无法解析日期 {dateStr}";
                return result;
            }

            // 解析数字字段（参考原始数据表）
            if (int.TryParse(result.LineNo, out var lineNoNum))
            {
                result.LineNoNumeric = lineNoNum;
            }

            // 班次转换为数字：甲=1, 乙=2, 丙=3
            result.ShiftNumeric = ConvertShiftToNumeric(result.Shift);

            if (int.TryParse(result.FurnaceNo, out var furnaceNoNum))
            {
                result.FurnaceBatchNo = furnaceNoNum;
            }

            // 磁性数据炉号格式不包含卷号和分卷号，设置为null
            result.CoilNo = null;
            result.SubcoilNo = null;

            result.Success = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"解析失败：{ex.Message}";
        }

        return result;
    }

    /// <summary>
    /// 磁性数据炉号解析结果
    /// </summary>
    private class MagneticFurnaceNoParseResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string LineNo { get; set; }
        public int? LineNoNumeric { get; set; }
        public string Shift { get; set; }
        public int? ShiftNumeric { get; set; }
        public DateTime? ProdDate { get; set; }
        public string FurnaceNo { get; set; }
        public int? FurnaceBatchNo { get; set; }
        public decimal? CoilNo { get; set; }
        public decimal? SubcoilNo { get; set; }
        public bool IsScratched { get; set; }
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
