using Newtonsoft.Json;
using Poxiao.DependencyInjection;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Enums;
using Poxiao.Lab.Entity.Models;
using SqlSugar;
using System.Globalization;

namespace Poxiao.Lab.Helpers;

/// <summary>
/// 中间数据生成器 — 将原始数据转换为中间数据实体。
/// 纯计算逻辑，不依赖 ICacheManager / IUserManager / IHttpContextAccessor，
/// 可在 API 进程和独立 Worker 进程中共用。
/// </summary>
public class IntermediateDataGenerator : ITransient
{
    private readonly ISqlSugarRepository<IntermediateDataFormulaCalcLogEntity> _calcLogRepository;

    public IntermediateDataGenerator(
        ISqlSugarRepository<IntermediateDataFormulaCalcLogEntity> calcLogRepository)
    {
        _calcLogRepository = calcLogRepository;
    }

    /// <summary>
    /// 解析检测列数（1~N 的整数列表）。
    /// </summary>
    public static List<int> ParseDetectionColumns(int? detectionColumnsCount)
    {
        if (!detectionColumnsCount.HasValue || detectionColumnsCount.Value <= 0)
            return new List<int>();

        return Enumerable.Range(1, detectionColumnsCount.Value).ToList();
    }

    /// <summary>
    /// 创建中间数据骨架（仅复制基础字段，不做任何计算）。
    /// 用于 API 端在事务中与原始数据一起写入，保证一致性。
    /// </summary>
    public static IntermediateDataEntity CreateSkeleton(
        RawDataEntity rawData,
        ProductSpecEntity productSpec,
        int layers,
        decimal length,
        decimal density,
        int? specVersion,
        string batchId)
    {
        var entity = new IntermediateDataEntity
        {
            Id = Guid.NewGuid().ToString(),
            RawDataId = rawData.Id,
            // 日期相关
            DetectionDate = rawData.DetectionDate,
            ProdDate = rawData.ProdDate,
            // 炉号相关
            FurnaceNo = rawData.FurnaceNo,
            FurnaceNoFormatted = rawData.FurnaceNoFormatted,
            // 基础信息
            LineNo = rawData.LineNo,
            Shift = rawData.Shift,
            ShiftNumeric = rawData.ShiftNumeric,
            FurnaceBatchNo = rawData.FurnaceBatchNo,
            CoilNo = rawData.CoilNo,
            SubcoilNo = rawData.SubcoilNo,
            FeatureSuffix = rawData.FeatureSuffix,
            // 产品规格信息
            ProductSpecId = productSpec.Id,
            ProductSpecCode = productSpec.Code,
            ProductSpecName = productSpec.Name,
            ProductSpecVersion = specVersion?.ToString(),
            DetectionColumns = productSpec.DetectionColumns,
            // 外观特性相关
            AppearanceFeatureIds = rawData.AppearanceFeatureIds,
            AppearanceFeatureCategoryIds = rawData.AppearanceFeatureCategoryIds,
            AppearanceFeatureLevelIds = rawData.AppearanceFeatureLevelIds,
            MatchConfidence = rawData.MatchConfidence,
            // 外观数据
            BreakCount = rawData.BreakCount,
            SingleCoilWeight = rawData.SingleCoilWeight,
            // 原始测量数据（Worker 计算时直接从这些字段读取，无需回查原始表）
            CoilWeight = rawData.CoilWeight,
            Width = rawData.Width.HasValue ? Math.Round(rawData.Width.Value, 2) : null,
            // 产品规格参数（Worker 计算时直接使用，无需回查产品规格表）
            ProductLength = length,
            ProductLayers = layers,
            ProductDensity = density,
            // 公式计算相关字段初始化
            BatchId = batchId,
            CalcStatus = IntermediateDataCalcStatus.PENDING,
            CalcStatusTime = DateTime.Now,
            CreatorTime = DateTime.Now,
            // 是否需要人工确认
            RequiresManualConfirm = rawData.MatchConfidence.HasValue && rawData.MatchConfidence.Value < 0.9,
        };

        // 复制检测列数据（Detection1-22），Worker 计算时直接读取
        CopyDetectionValues(entity, rawData);

        return entity;
    }

    /// <summary>
    /// 对已有的中间数据实体执行全部计算（带厚、密度、叠片系数、带型等）。
    /// Worker 从 DB 查出骨架数据后调用此方法，所有计算所需字段已在实体上。
    /// </summary>
    public async Task CalculateFromEntityAsync(IntermediateDataEntity entity)
    {
        var layers = entity.ProductLayers ?? 20;
        var length = entity.ProductLength ?? 4m;
        var density = entity.ProductDensity ?? 7.25m;
        var detectionColumns = ParseDetectionColumns(entity.DetectionColumns);

        // ── 炉号编号 ──
        var furnaceNoObj = FurnaceNo.Build(
            entity.LineNo?.ToString() ?? "1",
            entity.Shift?.ToString() ?? "甲",
            entity.ProdDate,
            entity.FurnaceBatchNo?.ToString() ?? "1",
            entity.CoilNo?.ToString() ?? "1",
            entity.SubcoilNo?.ToString() ?? "1",
            entity.FeatureSuffix);

        if (furnaceNoObj.IsValid)
        {
            entity.SprayNo = furnaceNoObj.GetSprayNo();
            entity.ShiftNo = furnaceNoObj.GetBatchNo();
        }
        else
        {
            entity.SprayNo = $"{entity.ProdDate?.ToString("yyyyMMdd")}-{entity.FurnaceBatchNo}";
            entity.ShiftNo = $"{entity.LineNo}{entity.Shift}{entity.ProdDate?.ToString("yyyyMMdd")}-{entity.FurnaceBatchNo}";
        }

        // ── 一米带材重量 ──
        if (entity.CoilWeight.HasValue && length > 0)
        {
            entity.OneMeterWeight = Math.Round(entity.CoilWeight.Value / length, 2);
        }

        // ── 带厚计算（从 Detection → Thickness）──
        var detectionValues = GetDetectionValuesFromEntity(entity, detectionColumns);
        var thicknessValues = new List<decimal?>();
        var abnormalIndexes = new List<int>();

        for (int i = 0; i < detectionValues.Count; i++)
        {
            if (detectionValues[i].HasValue && layers > 0)
            {
                var thickness = Math.Round(detectionValues[i].Value / layers, 2);
                thicknessValues.Add(thickness);
                SetThicknessValue(entity, i + 1, thickness);
            }
            else
            {
                thicknessValues.Add(null);
            }
        }

        // ── 叠片系数分布 ──
        for (int i = 0; i < detectionValues.Count; i++)
        {
            SetLaminationDistValue(entity, i + 1, detectionValues[i]);
        }

        // ── 头尾异常检测 ──
        var validThicknessValues = thicknessValues
            .Where(v => v.HasValue)
            .Select(v => v.Value)
            .ToList();

        if (validThicknessValues.Count > 2)
        {
            if (thicknessValues[0].HasValue && thicknessValues[1].HasValue)
            {
                if (Math.Abs(thicknessValues[0].Value - thicknessValues[1].Value) > 1.5m)
                    abnormalIndexes.Add(1);
            }

            var lastIndex = thicknessValues.Count - 1;
            if (thicknessValues[lastIndex].HasValue && thicknessValues[lastIndex - 1].HasValue)
            {
                if (Math.Abs(thicknessValues[lastIndex].Value - thicknessValues[lastIndex - 1].Value) > 1.5m)
                    abnormalIndexes.Add(lastIndex + 1);
            }
        }

        entity.ThicknessAbnormal = abnormalIndexes.Count > 0
            ? JsonConvert.SerializeObject(abnormalIndexes)
            : null;

        // ── 带厚范围、极差、平均厚度 ──
        if (validThicknessValues.Count > 0)
        {
            entity.ThicknessMin = Math.Round(validThicknessValues.Min(), 2);
            entity.ThicknessMax = Math.Round(validThicknessValues.Max(), 2);
            entity.ThicknessRange = $"{entity.ThicknessMin}～{entity.ThicknessMax}";
            entity.ThicknessDiff = Math.Round(entity.ThicknessMax.Value - entity.ThicknessMin.Value, 1);
            entity.AvgThickness = Math.Round(validThicknessValues.Average(), 2);
        }

        // ── 最大厚度 ──
        var validDetectionValues = detectionValues
            .Where(v => v.HasValue)
            .Select(v => v.Value)
            .ToList();

        if (validDetectionValues.Count > 0)
        {
            entity.MaxThicknessRaw = Math.Round(validDetectionValues.Max(), 2);
            if (layers > 0)
            {
                entity.MaxAvgThickness = Math.Round(entity.MaxThicknessRaw.Value / layers, 2);
            }
        }

        // ── 密度计算 ──
        if (entity.OneMeterWeight.HasValue
            && entity.Width.HasValue
            && entity.AvgThickness.HasValue
            && entity.AvgThickness.Value > 0)
        {
            var divisor = entity.Width.Value * (entity.AvgThickness.Value / 10m);
            if (divisor > 0)
            {
                entity.Density = Math.Round(entity.OneMeterWeight.Value / divisor, 2);
            }
        }

        // ── 叠片系数 ──
        var hasLaminationInputs = entity.CoilWeight.HasValue
            && entity.Width.HasValue
            && entity.AvgThickness.HasValue
            && entity.AvgThickness.Value > 0;

        decimal? laminationDivisor = null;
        if (hasLaminationInputs)
        {
            var lengthCm = length * 100;
            laminationDivisor = entity.Width.Value * lengthCm * entity.AvgThickness.Value * density * 0.0000001m;
            if (laminationDivisor > 0)
            {
                entity.LaminationFactor = Math.Round(entity.CoilWeight.Value / laminationDivisor.Value, 2);
            }
        }

        await TryInsertLaminationFactorLogAsync(entity, length, density, hasLaminationInputs, laminationDivisor);

        // ── 带型计算 ──
        if (validThicknessValues.Count >= 5)
        {
            var count = validThicknessValues.Count;
            var sideCount = count / 3;
            var midStart = sideCount;
            var midEnd = count - sideCount;

            var leftSide = validThicknessValues.Take(sideCount).ToList();
            var rightSide = validThicknessValues.Skip(count - sideCount).ToList();
            var midSection = validThicknessValues.Skip(midStart).Take(midEnd - midStart).ToList();
            var bothSides = leftSide.Concat(rightSide).ToList();

            if (midSection.Count > 0 && bothSides.Count > 0)
            {
                var midAvg = midSection.Average();
                var sideAvg = bothSides.Average();

                entity.StripType = midAvg < sideAvg
                    ? Math.Round(midSection.Min() - bothSides.Max(), 2)
                    : Math.Round(midSection.Max() - bothSides.Min(), 2);
            }
        }
    }

    /// <summary>
    /// 兼容方法：从原始数据一步生成完整中间数据（CreateSkeleton + CalculateFromEntityAsync）。
    /// </summary>
    public async Task<IntermediateDataEntity> GenerateAsync(
        RawDataEntity rawData,
        ProductSpecEntity productSpec,
        List<int> detectionColumns,
        int layers,
        decimal length,
        decimal density,
        int? specVersion,
        string batchId = null)
    {
        var entity = CreateSkeleton(rawData, productSpec, layers, length, density, specVersion, batchId);
        await CalculateFromEntityAsync(entity);
        return entity;
    }

    #region 私有辅助方法

    private static void CopyDetectionValues(IntermediateDataEntity entity, RawDataEntity rawData)
    {
        entity.Detection1 = rawData.Detection1;
        entity.Detection2 = rawData.Detection2;
        entity.Detection3 = rawData.Detection3;
        entity.Detection4 = rawData.Detection4;
        entity.Detection5 = rawData.Detection5;
        entity.Detection6 = rawData.Detection6;
        entity.Detection7 = rawData.Detection7;
        entity.Detection8 = rawData.Detection8;
        entity.Detection9 = rawData.Detection9;
        entity.Detection10 = rawData.Detection10;
        entity.Detection11 = rawData.Detection11;
        entity.Detection12 = rawData.Detection12;
        entity.Detection13 = rawData.Detection13;
        entity.Detection14 = rawData.Detection14;
        entity.Detection15 = rawData.Detection15;
        entity.Detection16 = rawData.Detection16;
        entity.Detection17 = rawData.Detection17;
        entity.Detection18 = rawData.Detection18;
        entity.Detection19 = rawData.Detection19;
        entity.Detection20 = rawData.Detection20;
        entity.Detection21 = rawData.Detection21;
        entity.Detection22 = rawData.Detection22;
    }

    /// <summary>
    /// 从中间数据实体本身读取检测列值（Worker 端使用，无需回查原始表）。
    /// </summary>
    private static List<decimal?> GetDetectionValuesFromEntity(IntermediateDataEntity entity, List<int> detectionColumns)
    {
        var values = new List<decimal?>();
        var detectionProps = new[]
        {
            entity.Detection1, entity.Detection2, entity.Detection3,
            entity.Detection4, entity.Detection5, entity.Detection6,
            entity.Detection7, entity.Detection8, entity.Detection9,
            entity.Detection10, entity.Detection11, entity.Detection12,
            entity.Detection13, entity.Detection14, entity.Detection15,
            entity.Detection16, entity.Detection17, entity.Detection18,
            entity.Detection19, entity.Detection20, entity.Detection21,
            entity.Detection22,
        };

        foreach (var col in detectionColumns)
        {
            values.Add(col >= 1 && col <= 22 ? detectionProps[col - 1] : null);
        }

        return values;
    }

    private static void SetThicknessValue(IntermediateDataEntity entity, int index, decimal? value)
    {
        var prop = typeof(IntermediateDataEntity).GetProperty($"Thickness{index}");
        prop?.SetValue(entity, value);
    }

    private static void SetLaminationDistValue(IntermediateDataEntity entity, int index, decimal? value)
    {
        var prop = typeof(IntermediateDataEntity).GetProperty($"LaminationDist{index}");
        prop?.SetValue(entity, value);
    }

    private async Task TryInsertLaminationFactorLogAsync(
        IntermediateDataEntity entity,
        decimal length,
        decimal density,
        bool hasInputs,
        decimal? divisor)
    {
        try
        {
            var errorType = hasInputs
                ? divisor.HasValue
                    ? divisor.Value > 0 ? "INFO" : "INVALID_DIVISOR"
                    : "INVALID_DIVISOR"
                : "SKIP";

            string FormatDecimal(decimal? value) =>
                value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : "null";

            var detail =
                "Formula=[CoilWeight] / ([ProductLength] * [Width] * [AvgThickness] * [ProductDensity]); "
                + $"CoilWeight={FormatDecimal(entity.CoilWeight)}; "
                + $"ProductLength={length.ToString(CultureInfo.InvariantCulture)}; "
                + $"Width={FormatDecimal(entity.Width)}; "
                + $"AvgThickness={FormatDecimal(entity.AvgThickness)}; "
                + $"ProductDensity={density.ToString(CultureInfo.InvariantCulture)}; "
                + $"LengthCm={(length * 100).ToString(CultureInfo.InvariantCulture)}; "
                + $"Divisor={FormatDecimal(divisor)}; "
                + $"Result={FormatDecimal(entity.LaminationFactor)}";

            var log = new IntermediateDataFormulaCalcLogEntity
            {
                BatchId = entity.BatchId,
                IntermediateDataId = entity.Id,
                ColumnName = "LaminationFactor",
                FormulaName = "叠片系数",
                FormulaType = "CALC",
                ErrorType = errorType,
                ErrorMessage = "叠片系数计算日志",
                ErrorDetail = detail,
            };

            await _calcLogRepository.InsertAsync(log);
        }
        catch
        {
            // 忽略日志写入异常
        }
    }

    #endregion
}
