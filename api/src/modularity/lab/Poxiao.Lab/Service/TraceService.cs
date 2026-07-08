using Microsoft.AspNetCore.Mvc;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.Infrastructure.Filter;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.Trace;
using Poxiao.Lab.Entity.Models;
using Poxiao.Lab.Interfaces;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// 炉号追溯聚合查询服务.
/// 场景：扫码枪扫描炉号二维码（内容=炉号纯文本，可能带尾部K或特性汉字）后，
/// 一次性返回该炉号在原始数据/中间数据/磁性数据/单片数据四张表中的全链路数据。
/// 只读查询，无写操作.
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "trace", Order = 210)]
[Route("api/lab/trace")]
public class TraceService : ITraceService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<RawDataEntity> _rawDataRepository;
    private readonly ISqlSugarRepository<IntermediateDataEntity> _intermediateDataRepository;
    private readonly ISqlSugarRepository<MagneticRawDataEntity> _magneticRawDataRepository;
    private readonly ISqlSugarRepository<SingleSheetRawDataEntity> _singleSheetRawDataRepository;

    public TraceService(
        ISqlSugarRepository<RawDataEntity> rawDataRepository,
        ISqlSugarRepository<IntermediateDataEntity> intermediateDataRepository,
        ISqlSugarRepository<MagneticRawDataEntity> magneticRawDataRepository,
        ISqlSugarRepository<SingleSheetRawDataEntity> singleSheetRawDataRepository
    )
    {
        _rawDataRepository = rawDataRepository;
        _intermediateDataRepository = intermediateDataRepository;
        _magneticRawDataRepository = magneticRawDataRepository;
        _singleSheetRawDataRepository = singleSheetRawDataRepository;
    }

    /// <inheritdoc />
    [HttpGet("")]
    public async Task<TraceOutput> Trace([FromQuery] string code)
    {
        var output = new TraceOutput { InputCode = code };

        // 扫码场景要温和：空码不抛异常，直接返回未命中
        if (string.IsNullOrWhiteSpace(code))
        {
            output.Matched = false;
            return output;
        }

        var (norm, batchNo) = NormalizeFurnaceNo(code);
        output.NormalizedFurnaceNo = norm;
        output.NormalizedBatchNo = batchNo;

        // 磁性/单片表存的炉号是批次级（如"1甲20251101-1"，无卷号/分卷号），
        // 扫完整三段炉号时等值匹配会漏——用 基准炉号+批次号 的 IN 列表兜住两种粒度
        var magneticKeys = new List<string> { norm };
        if (!string.IsNullOrEmpty(batchNo) && batchNo != norm)
        {
            magneticKeys.Add(batchNo);
        }

        // RAW/INTERMEDIATE 匹配列 = FurnaceNoFormatted（基准炉号，含卷号/分卷号）
        output.RawData = await _rawDataRepository
            .AsQueryable()
            .Where(t => t.FurnaceNoFormatted == norm)
            .OrderByDescending(t => t.CreatorTime)
            .FirstAsync();

        output.Intermediate = await _intermediateDataRepository
            .AsQueryable()
            .Where(t => t.FurnaceNoFormatted == norm)
            .OrderByDescending(t => t.CreatorTime)
            .FirstAsync();

        // MAGNETIC/SINGLE_SHEET 匹配列 = FurnaceNo（去K后的批次级炉号）
        output.MagneticRecords = await _magneticRawDataRepository
            .AsQueryable()
            .Where(t => magneticKeys.Contains(t.FurnaceNo))
            .OrderBy(t => t.IsScratched)
            .OrderByDescending(t => t.DetectionTime)
            .ToListAsync();

        output.SingleSheetRecords = await _singleSheetRawDataRepository
            .AsQueryable()
            .Where(t => magneticKeys.Contains(t.FurnaceNo))
            .OrderBy(t => t.IsScratched)
            .OrderByDescending(t => t.DetectionTime)
            .ToListAsync();

        output.Matched =
            output.RawData != null
            || output.Intermediate != null
            || output.MagneticRecords.Count > 0
            || output.SingleSheetRecords.Count > 0;

        return output;
    }

    /// <summary>
    /// 归一化炉号：去除首尾空白后剥离尾部K（环样/单片格式如"1甲20251101-1K"，
    /// 参照 MagneticDataImportSessionService.ParseMagneticFurnaceNo），
    /// 再尝试 FurnaceNo.TryParse 得到不含工艺标记的基准炉号；
    /// 解析失败（如仅有炉次号、缺卷号/分卷号的环样/单片格式）则兜底为大写化后的原文，
    /// 以匹配磁性/单片表中未经完整解析的批次级炉号（FurnaceNo字段）.
    /// </summary>
    /// <param name="code">扫码枪原始输入.</param>
    /// <returns>（基准炉号匹配键, 批次级炉号——解析失败为null）.</returns>
    private static (string Norm, string BatchNo) NormalizeFurnaceNo(string code)
    {
        var trimmed = code.Trim();

        if (trimmed.EndsWith("K", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed.Substring(0, trimmed.Length - 1).TrimEnd();
        }

        var parsed = FurnaceNo.TryParse(trimmed);
        if (parsed?.IsValid == true)
        {
            return (parsed.GetFurnaceNo(), parsed.GetBatchNo());
        }

        return (trimmed.ToUpperInvariant(), null);
    }
}
