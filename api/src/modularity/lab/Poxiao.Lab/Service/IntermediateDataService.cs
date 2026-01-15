using System.Text.Json;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Filter;
using Poxiao.Lab.Entity.Dto.IntermediateData;
using Poxiao.Lab.Entity.Dto.RawData;
using Poxiao.Lab.Entity.Entity;
using Poxiao.Lab.Entity.Extensions;
using Poxiao.Lab.Helpers;
using Poxiao.Lab.Interfaces;
using Poxiao.Systems.Entitys.Permission;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// 中间数据服务.
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "intermediate-data", Order = 199)]
[Route("api/lab/intermediate-data")]
public class IntermediateDataService : IIntermediateDataService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<IntermediateDataEntity> _repository;
    private readonly ISqlSugarRepository<RawDataEntity> _rawDataRepository;
    private readonly ISqlSugarRepository<ProductSpecEntity> _productSpecRepository;
    private readonly ISqlSugarRepository<ProductSpecAttributeEntity> _productSpecAttributeRepository;
    private readonly ISqlSugarRepository<UserEntity> _userRepository;
    private readonly IUserManager _userManager;
    private readonly ProductSpecVersionService _versionService;

    public IntermediateDataService(
        ISqlSugarRepository<IntermediateDataEntity> repository,
        ISqlSugarRepository<RawDataEntity> rawDataRepository,
        ISqlSugarRepository<ProductSpecAttributeEntity> productSpecAttributeRepository,
        ISqlSugarRepository<ProductSpecEntity> productSpecRepository,
        ISqlSugarRepository<UserEntity> userRepository,
        IUserManager userManager,
        ProductSpecVersionService versionService
    )
    {
        _repository = repository;
        _rawDataRepository = rawDataRepository;
        _productSpecRepository = productSpecRepository;
        _productSpecAttributeRepository = productSpecAttributeRepository;
        _userRepository = userRepository;
        _userManager = userManager;
        _versionService = versionService;
    }

    /// <inheritdoc />
    [HttpGet("list")]
    public async Task<dynamic> GetList([FromQuery] IntermediateDataListQuery input)
    {
        var query = _repository.AsQueryable().Where(t => t.DeleteMark == null);

        // 按产品规格筛选
        if (!string.IsNullOrEmpty(input.ProductSpecId))
        {
            query = query.Where(t => t.ProductSpecId == input.ProductSpecId);
        }

        // 关键词搜索
        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            query = query.Where(t =>
                t.FurnaceNo.Contains(input.Keyword)
                || t.LineNo.Contains(input.Keyword)
                || t.SprayNo.Contains(input.Keyword)
            );
        }

        // 日期月份筛选
        if (!string.IsNullOrWhiteSpace(input.DateMonth))
        {
            query = query.Where(t => t.DateMonth == input.DateMonth);
        }

        // 日期范围筛选
        if (input.StartDate.HasValue)
        {
            query = query.Where(t => t.ProdDate >= input.StartDate.Value);
        }
        if (input.EndDate.HasValue)
        {
            var endDate = input.EndDate.Value.AddDays(1);
            query = query.Where(t => t.ProdDate < endDate);
        }

        // 产线筛选
        if (!string.IsNullOrWhiteSpace(input.LineNo))
        {
            query = query.Where(t => t.LineNo == input.LineNo);
        }

        // 排序
        if (!string.IsNullOrWhiteSpace(input.SortRules))
        {
            try
            {
                var sortRules = JsonConvert.DeserializeObject<List<SortRule>>(input.SortRules);
                if (sortRules != null && sortRules.Count > 0)
                {
                    foreach (var rule in sortRules)
                    {
                        var isAsc = rule.Order?.ToLower() == "asc";
                        query = query.OrderByIF(true, $"{rule.Field} {(isAsc ? "asc" : "desc")}");
                    }
                }
            }
            catch
            {
                // 排序解析失败，使用默认排序
                query = query.OrderBy(t => t.ProdDate).OrderBy(t => t.FurnaceNoParsed);
            }
        }
        else
        {
            query = query.OrderBy(t => t.ProdDate).OrderBy(t => t.FurnaceNoParsed);
        }

        // 分页查询
        var result = await query.ToPagedListAsync(input.CurrentPage, input.PageSize);

        // 获取创建者姓名
        var userIds = result
            .list.Select(t => t.CreatorUserId)
            .Where(id => !string.IsNullOrEmpty(id))
            .Distinct()
            .ToList();
        var users = await _userRepository
            .AsQueryable()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.RealName })
            .ToListAsync();
        var userDict = users.ToDictionary(u => u.Id, u => u.RealName);

        // 转换输出
        var outputList = result
            .list.Select(item =>
            {
                var output = item.Adapt<IntermediateDataListOutput>();
                output.CreatorUserName = userDict.GetValueOrDefault(item.CreatorUserId ?? "", "");
                output.ProdDateStr = item.ProdDate?.ToString("yyyy/M/d") ?? "";

                // 构建带厚分布列表
                output.ThicknessDistList = BuildThicknessDistList(item);

                // 构建叠片系数分布列表
                output.LaminationDistList = BuildLaminationDistList(item);

                return output;
            })
            .ToList();

        return new { list = outputList, pagination = result.pagination };
    }

    /// <inheritdoc />
    [HttpGet("{id}")]
    public async Task<IntermediateDataInfoOutput> GetInfo(string id)
    {
        var entity = await _repository
            .AsQueryable()
            .Where(t => t.Id == id && t.DeleteMark == null)
            .FirstAsync();

        if (entity == null)
        {
            throw Oops.Oh("数据不存在");
        }

        var output = entity.Adapt<IntermediateDataInfoOutput>();

        // 获取创建者姓名
        if (!string.IsNullOrEmpty(entity.CreatorUserId))
        {
            var user = await _userRepository
                .AsQueryable()
                .Where(u => u.Id == entity.CreatorUserId)
                .FirstAsync();
            output.CreatorUserName = user?.RealName ?? "";
        }

        // 构建带厚分布列表
        output.ThicknessDistList = BuildThicknessDistList(entity);

        // 构建叠片系数分布列表
        output.LaminationDistList = BuildLaminationDistList(entity);

        return output;
    }

    /// <inheritdoc />
    [HttpPost("generate")]
    public async Task<IntermediateDataGenerateOutput> Generate(
        [FromBody] IntermediateDataGenerateInput input
    )
    {
        var output = new IntermediateDataGenerateOutput();

        // 获取产品规格
        if (string.IsNullOrEmpty(input.ProductSpecId))
        {
            throw Oops.Oh("请选择产品规格");
        }

        var productSpec = await _productSpecRepository
            .AsQueryable()
            .Where(t => t.Id == input.ProductSpecId && t.DeleteMark == null)
            .FirstAsync();

        if (productSpec == null)
        {
            throw Oops.Oh("产品规格不存在");
        }

        // 查询符合条件的原始数据
        var rawDataQuery = _rawDataRepository
            .AsQueryable()
            .Where(t => t.DeleteMark == null && t.ProductSpecId == input.ProductSpecId);

        if (input.StartDate.HasValue)
        {
            rawDataQuery = rawDataQuery.Where(t => t.ProdDate >= input.StartDate.Value);
        }
        if (input.EndDate.HasValue)
        {
            var endDate = input.EndDate.Value.AddDays(1);
            rawDataQuery = rawDataQuery.Where(t => t.ProdDate < endDate);
        }

        var rawDataList = await rawDataQuery.ToListAsync();

        if (rawDataList.Count == 0)
        {
            throw Oops.Oh("没有找到符合条件的原始数据");
        }

        // 获取已存在的中间数据ID
        var existingIds = new HashSet<string>();
        if (!input.ForceRegenerate)
        {
            var existingData = await _repository
                .AsQueryable()
                .Where(t => t.DeleteMark == null && t.ProductSpecId == input.ProductSpecId)
                .Select(t => t.RawDataId)
                .ToListAsync();
            existingIds = existingData.ToHashSet();
        }

        var currentUserId = _userManager.UserId;
        var currentUserName = _userManager.RealName;

        // 解析检测列配置
        var detectionColumns = ParseDetectionColumns(productSpec.DetectionColumns);

        // 获取指定版本的扩展属性
        var usedVersion = input.ProductSpecVersion; // 如果为null，GetAttributesByVersionAsync会获取当前版本
        var attributes = await _versionService.GetAttributesByVersionAsync(
            productSpec.Id,
            usedVersion
        );

        // 如果没有指定版本，获取到的attributes使用的是当前版本，我们需要知道具体是哪个版本
        if (!usedVersion.HasValue && attributes.Count > 0)
        {
            usedVersion = attributes.First().Version;
        }
        else if (!usedVersion.HasValue)
        {
            usedVersion = 1;
        }

        var lengthAttr = attributes.FirstOrDefault(a => a.AttributeKey == "length");
        var layersAttr = attributes.FirstOrDefault(a => a.AttributeKey == "layers");
        var densityAttr = attributes.FirstOrDefault(a => a.AttributeKey == "density");

        var length =
            lengthAttr != null
            && !string.IsNullOrEmpty(lengthAttr.AttributeValue)
            && decimal.TryParse(lengthAttr.AttributeValue, out var len)
                ? len
                : 4m;
        var layers =
            layersAttr != null
            && !string.IsNullOrEmpty(layersAttr.AttributeValue)
            && int.TryParse(layersAttr.AttributeValue, out var lay)
                ? lay
                : 20;
        var density =
            densityAttr != null
            && !string.IsNullOrEmpty(densityAttr.AttributeValue)
            && decimal.TryParse(densityAttr.AttributeValue, out var den)
                ? den
                : 7.25m;

        foreach (var rawData in rawDataList)
        {
            try
            {
                // 检查是否已存在
                if (!input.ForceRegenerate && existingIds.Contains(rawData.Id))
                {
                    output.SkippedCount++;
                    continue;
                }

                // 生成中间数据
                var intermediateData = GenerateIntermediateData(
                    rawData,
                    productSpec,
                    detectionColumns,
                    layers,
                    length,
                    density,
                    usedVersion
                );
                intermediateData.CreatorUserId = currentUserId;

                // 如果是强制重新生成，先删除已有数据
                if (input.ForceRegenerate)
                {
                    await _repository
                        .AsDeleteable()
                        .Where(t => t.RawDataId == rawData.Id)
                        .ExecuteCommandAsync();
                }

                await _repository.InsertAsync(intermediateData);
                output.SuccessCount++;
            }
            catch (Exception ex)
            {
                output.FailedCount++;
                output.Errors.Add($"炉号 {rawData.FurnaceNo} 生成失败: {ex.Message}");
            }
        }

        return output;
    }

    /// <inheritdoc />
    [HttpPut("performance")]
    public async Task UpdatePerformance([FromBody] IntermediateDataPerfUpdateInput input)
    {
        var entity = await _repository
            .AsQueryable()
            .Where(t => t.Id == input.Id && t.DeleteMark == null)
            .FirstAsync();

        if (entity == null)
        {
            throw Oops.Oh("数据不存在");
        }

        entity.PerfSsPower = input.PerfSsPower;
        entity.PerfPsLoss = input.PerfPsLoss;
        entity.PerfHc = input.PerfHc;
        entity.PerfAfterSsPower = input.PerfAfterSsPower;
        entity.PerfAfterPsLoss = input.PerfAfterPsLoss;
        entity.PerfAfterHc = input.PerfAfterHc;
        entity.PerfJudgeName = input.PerfJudgeName;
        entity.PerfEditorId = _userManager.UserId;
        entity.PerfEditorName = _userManager.RealName;
        entity.PerfEditTime = DateTime.Now;
        entity.LastModifyUserId = _userManager.UserId;
        entity.LastModifyTime = DateTime.Now;

        await _repository.UpdateAsync(entity);
    }

    /// <inheritdoc />
    [HttpPut("appearance")]
    public async Task UpdateAppearance([FromBody] IntermediateDataAppearUpdateInput input)
    {
        var entity = await _repository
            .AsQueryable()
            .Where(t => t.Id == input.Id && t.DeleteMark == null)
            .FirstAsync();

        if (entity == null)
        {
            throw Oops.Oh("数据不存在");
        }

        entity.Toughness = input.Toughness;
        entity.FishScale = input.FishScale;
        entity.MidSi = input.MidSi;
        entity.MidB = input.MidB;
        entity.LeftPattern = input.LeftPattern;
        entity.MidPattern = input.MidPattern;
        entity.RightPattern = input.RightPattern;
        entity.BreakCount = input.BreakCount;
        entity.CoilWeightKg = input.CoilWeightKg;
        entity.AppearJudgeName = input.AppearJudgeName;
        entity.AppearEditorId = _userManager.UserId;
        entity.AppearEditorName = _userManager.RealName;
        entity.AppearEditTime = DateTime.Now;
        entity.LastModifyUserId = _userManager.UserId;
        entity.LastModifyTime = DateTime.Now;

        await _repository.UpdateAsync(entity);
    }

    /// <inheritdoc />
    [HttpPut("base-info")]
    public async Task UpdateBaseInfo([FromBody] IntermediateDataBaseUpdateInput input)
    {
        var entity = await _repository
            .AsQueryable()
            .Where(t => t.Id == input.Id && t.DeleteMark == null)
            .FirstAsync();

        if (entity == null)
        {
            throw Oops.Oh("数据不存在");
        }

        entity.DateMonth = input.DateMonth;
        entity.MagneticResult = input.MagneticResult;
        entity.ThicknessResult = input.ThicknessResult;
        entity.LaminationResult = input.LaminationResult;
        entity.Remark = input.Remark;
        entity.LastModifyUserId = _userManager.UserId;
        entity.LastModifyTime = DateTime.Now;

        await _repository.UpdateAsync(entity);
    }

    /// <inheritdoc />
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _repository
            .AsQueryable()
            .Where(t => t.Id == id && t.DeleteMark == null)
            .FirstAsync();

        if (entity == null)
        {
            throw Oops.Oh("数据不存在");
        }

        entity.DeleteMark = 1;
        entity.DeleteUserId = _userManager.UserId;
        entity.DeleteTime = DateTime.Now;

        await _repository.UpdateAsync(entity);
    }

    /// <inheritdoc />
    [HttpDelete("batch")]
    public async Task BatchDelete([FromBody] List<string> ids)
    {
        if (ids == null || ids.Count == 0)
        {
            throw Oops.Oh("请选择要删除的数据");
        }

        await _repository
            .AsUpdateable()
            .SetColumns(t => t.DeleteMark == 1)
            .SetColumns(t => t.DeleteUserId == _userManager.UserId)
            .SetColumns(t => t.DeleteTime == DateTime.Now)
            .Where(t => ids.Contains(t.Id) && t.DeleteMark == null)
            .ExecuteCommandAsync();
    }

    /// <inheritdoc />
    [HttpGet("product-spec-options")]
    public async Task<List<ProductSpecOption>> GetProductSpecOptions()
    {
        var specs = await _productSpecRepository
            .AsQueryable()
            .Where(t => t.DeleteMark == null)
            .OrderBy(t => t.SortCode)
            .ToListAsync();

        var result = new List<ProductSpecOption>();

        foreach (var s in specs)
        {
            // 获取当前版本的属性
            var attrs = await _versionService.GetAttributesByVersionAsync(s.Id);
            var lengthAttr = attrs.FirstOrDefault(a => a.AttributeKey == "length");
            var layersAttr = attrs.FirstOrDefault(a => a.AttributeKey == "layers");

            result.Add(
                new ProductSpecOption
                {
                    Id = s.Id,
                    Code = s.Code,
                    Name = s.Name,
                    DetectionColumns = s.DetectionColumns,
                    Length =
                        lengthAttr != null
                        && !string.IsNullOrEmpty(lengthAttr.AttributeValue)
                        && decimal.TryParse(lengthAttr.AttributeValue, out var len)
                            ? len
                            : null,
                    Layers =
                        layersAttr != null
                        && !string.IsNullOrEmpty(layersAttr.AttributeValue)
                        && int.TryParse(layersAttr.AttributeValue, out var lay)
                            ? lay
                            : null,
                }
            );
        }

        return result;
    }

    #region 私有方法

    /// <summary>
    /// 解析检测列配置.
    /// </summary>
    /// <summary>
    /// 解析检测列配置.
    /// </summary>
    public List<int> ParseDetectionColumns(string detectionColumnsStr)
    {
        if (string.IsNullOrWhiteSpace(detectionColumnsStr))
        {
            return new List<int>();
        }

        return detectionColumnsStr
            .Split(',')
            .Select(s => int.TryParse(s.Trim(), out var n) ? n : 0)
            .Where(n => n > 0)
            .OrderBy(n => n)
            .ToList();
    }

    /// <summary>
    /// 从原始数据生成中间数据.
    /// </summary>
    public IntermediateDataEntity GenerateIntermediateData(
        RawDataEntity rawData,
        ProductSpecEntity productSpec,
        List<int> detectionColumns,
        int layers,
        decimal length,
        decimal density,
        int? specVersion
    )
    {
        var entity = new IntermediateDataEntity
        {
            Id = Guid.NewGuid().ToString(),
            RawDataId = rawData.Id,
            ProdDate = rawData.ProdDate,
            DateMonth = rawData.ProdDate?.ToString("yyyy-MM") ?? "",
            // 去掉特性汉字后的炉号
            FurnaceNo = FurnaceNoHelper.RemoveFeatureSuffix(
                rawData.FurnaceNo,
                rawData.FeatureSuffix
            ),
            LineNo = rawData.LineNo?.ToString(),
            Shift = rawData.Shift?.ToString(),
            FurnaceNoParsed = rawData.FurnaceNoParsed?.ToString(),
            CoilNo = rawData.CoilNo?.ToString(),
            SubcoilNo = rawData.SubcoilNo?.ToString(),
            ProductSpecId = productSpec.Id,
            ProductSpecName = productSpec.Name,
            ProductSpecVersion = specVersion,
            DetectionColumns = productSpec.DetectionColumns,
            CreatorTime = DateTime.Now,
        };

        // 计算喷次
        entity.SprayNo =
            $"{rawData.LineNo}-{rawData.Shift}-{rawData.ProdDate?.ToString("yyyyMMdd")}-{rawData.FurnaceNoParsed}";

        // 四米带材重量（原始数据带材重量）
        entity.FourMeterWeight = rawData.CoilWeight;

        // 一米带材重量 = 四米带材重量 / 长度
        if (rawData.CoilWeight.HasValue && length > 0)
        {
            entity.OneMeterWeight = Math.Round(rawData.CoilWeight.Value / length, 2);
        }

        // 带宽
        entity.StripWidth = rawData.Width.HasValue ? Math.Round(rawData.Width.Value, 2) : null;

        // 获取检测列数据并计算带厚
        var detectionValues = GetDetectionValues(rawData, detectionColumns);
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

        // 叠片系数分布（原始检测列数据）
        for (int i = 0; i < detectionValues.Count; i++)
        {
            SetLaminationDistValue(entity, i + 1, detectionValues[i]);
        }

        // 检测头尾数据异常（差值>1.5标红）
        var validThicknessValues = thicknessValues
            .Where(v => v.HasValue)
            .Select(v => v.Value)
            .ToList();
        if (validThicknessValues.Count > 2)
        {
            // 检查第一个值
            if (thicknessValues[0].HasValue && thicknessValues[1].HasValue)
            {
                if (Math.Abs(thicknessValues[0].Value - thicknessValues[1].Value) > 1.5m)
                {
                    abnormalIndexes.Add(1);
                }
            }
            // 检查最后一个值
            var lastIndex = thicknessValues.Count - 1;
            if (thicknessValues[lastIndex].HasValue && thicknessValues[lastIndex - 1].HasValue)
            {
                if (
                    Math.Abs(
                        thicknessValues[lastIndex].Value - thicknessValues[lastIndex - 1].Value
                    ) > 1.5m
                )
                {
                    abnormalIndexes.Add(lastIndex + 1);
                }
            }
        }
        entity.ThicknessAbnormal =
            abnormalIndexes.Count > 0 ? JsonConvert.SerializeObject(abnormalIndexes) : null;

        // 计算带厚范围、极差、平均厚度
        if (validThicknessValues.Count > 0)
        {
            entity.ThicknessMin = Math.Round(validThicknessValues.Min(), 2);
            entity.ThicknessMax = Math.Round(validThicknessValues.Max(), 2);
            entity.ThicknessRange = $"{entity.ThicknessMin}～{entity.ThicknessMax}";
            entity.ThicknessDiff = Math.Round(
                entity.ThicknessMax.Value - entity.ThicknessMin.Value,
                1
            );
            entity.AvgThickness = Math.Round(validThicknessValues.Average(), 2);
        }

        // 最大厚度（原始检测列最大值）
        var validDetectionValues = detectionValues
            .Where(v => v.HasValue)
            .Select(v => v.Value)
            .ToList();
        if (validDetectionValues.Count > 0)
        {
            entity.MaxThicknessRaw = Math.Round(validDetectionValues.Max(), 2);
            // 最大平均厚度 = 最大值 / 层数
            if (layers > 0)
            {
                entity.MaxAvgThickness = Math.Round(entity.MaxThicknessRaw.Value / layers, 2);
            }
        }

        // 密度计算: 一米带材重量(g) / (带宽(mm) * (平均厚度/10))
        if (
            entity.OneMeterWeight.HasValue
            && entity.StripWidth.HasValue
            && entity.AvgThickness.HasValue
            && entity.AvgThickness.Value > 0
        )
        {
            var divisor = entity.StripWidth.Value * (entity.AvgThickness.Value / 10m);
            if (divisor > 0)
            {
                entity.Density = Math.Round(entity.OneMeterWeight.Value / divisor, 2);
            }
        }

        // 叠片系数计算: 四米带材重量(g) / (带宽(mm) * 400 * 平均厚度 * 7.25 * 0.0000001)
        // 注：400对应产品规格长度(4m = 400cm)
        if (
            entity.FourMeterWeight.HasValue
            && entity.StripWidth.HasValue
            && entity.AvgThickness.HasValue
            && entity.AvgThickness.Value > 0
        )
        {
            var lengthCm = length * 100; // 转换为厘米
            var divisor =
                entity.StripWidth.Value
                * lengthCm
                * entity.AvgThickness.Value
                * density
                * 0.0000001m;
            if (divisor > 0)
            {
                entity.LaminationFactor = Math.Round(entity.FourMeterWeight.Value / divisor, 2);
            }
        }

        // 带型计算：判断中间段相对于两侧段的位置是否脱节
        // 公式：IF(AVERAGE(中间段)<AVERAGE(两侧段), MIN(中间段)-MAX(两侧段), MAX(中间段)-MIN(两侧段))
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

                if (midAvg < sideAvg)
                {
                    entity.StripType = Math.Round(midSection.Min() - bothSides.Max(), 2);
                }
                else
                {
                    entity.StripType = Math.Round(midSection.Max() - bothSides.Min(), 2);
                }
            }
        }

        // 分段类型：根据卷号判断前端/中端/后端
        if (rawData.CoilNo.HasValue)
        {
            var coilNoInt = rawData.CoilNo.Value;
            // 假设一个炉号有多个卷，根据卷号判断分段
            // 这里简化处理，实际逻辑可能需要根据业务调整
            entity.SegmentType = coilNoInt <= 1 ? "前端" : (coilNoInt >= 3 ? "后端" : "中端");
        }

        // 外观特性（从炉号解析的特性描述，原始特性汉字）
        entity.AppearanceFeature = rawData.FeatureSuffix;

        // 特性ID列表（从原始数据复制，如果原始数据有匹配结果）
        if (!string.IsNullOrWhiteSpace(rawData.AppearanceFeatureIds))
        {
            entity.AppearanceFeatureIds = rawData.AppearanceFeatureIds;
        }

        return entity;
    }

    /// <summary>
    /// 获取原始数据中指定检测列的值（优先使用JSON字段）.
    /// </summary>
    private List<decimal?> GetDetectionValues(RawDataEntity rawData, List<int> detectionColumns)
    {
        var values = new List<decimal?>();

        // 优先从JSON字段读取
        var detectionData = string.IsNullOrWhiteSpace(rawData.DetectionData)
            ? new Dictionary<int, decimal?>()
            : DetectionDataConverter.FromJson(rawData.DetectionData);

        foreach (var col in detectionColumns)
        {
            if (detectionData.ContainsKey(col) && detectionData[col].HasValue)
            {
                values.Add(detectionData[col].Value);
            }
            else
            {
                values.Add(null);
            }
        }

        return values;
    }

    /// <summary>
    /// 设置带厚值.
    /// </summary>
    private void SetThicknessValue(IntermediateDataEntity entity, int index, decimal? value)
    {
        var propName = $"Thickness{index}";
        var prop = typeof(IntermediateDataEntity).GetProperty(propName);
        if (prop != null)
        {
            prop.SetValue(entity, value);
        }
    }

    /// <summary>
    /// 设置叠片系数分布值.
    /// </summary>
    private void SetLaminationDistValue(IntermediateDataEntity entity, int index, decimal? value)
    {
        var propName = $"LaminationDist{index}";
        var prop = typeof(IntermediateDataEntity).GetProperty(propName);
        if (prop != null)
        {
            prop.SetValue(entity, value);
        }
    }

    /// <summary>
    /// 构建带厚分布列表.
    /// </summary>
    private List<ThicknessDistItem> BuildThicknessDistList(IntermediateDataEntity entity)
    {
        var list = new List<ThicknessDistItem>();
        var abnormalIndexes = new HashSet<int>();

        if (!string.IsNullOrEmpty(entity.ThicknessAbnormal))
        {
            try
            {
                var indexes = JsonConvert.DeserializeObject<List<int>>(entity.ThicknessAbnormal);
                if (indexes != null)
                {
                    abnormalIndexes = indexes.ToHashSet();
                }
            }
            catch { }
        }

        for (int i = 1; i <= 10; i++)
        {
            var propName = $"Thickness{i}";
            var prop = typeof(IntermediateDataEntity).GetProperty(propName);
            if (prop != null)
            {
                var value = prop.GetValue(entity) as decimal?;
                if (value.HasValue)
                {
                    list.Add(
                        new ThicknessDistItem
                        {
                            Index = i,
                            Value = value,
                            IsAbnormal = abnormalIndexes.Contains(i),
                        }
                    );
                }
            }
        }

        return list;
    }

    /// <summary>
    /// 构建叠片系数分布列表.
    /// </summary>
    private List<LaminationDistItem> BuildLaminationDistList(IntermediateDataEntity entity)
    {
        var list = new List<LaminationDistItem>();

        for (int i = 1; i <= 10; i++)
        {
            var propName = $"LaminationDist{i}";
            var prop = typeof(IntermediateDataEntity).GetProperty(propName);
            if (prop != null)
            {
                var value = prop.GetValue(entity) as decimal?;
                if (value.HasValue)
                {
                    list.Add(new LaminationDistItem { Index = i, Value = value });
                }
            }
        }

        return list;
    }

    #endregion
}
