using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.EventBus;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Filter;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.IntermediateData;
using Poxiao.Lab.Entity.Dto.IntermediateDataFormula;
using Poxiao.Lab.Entity.Dto.RawData;
using Poxiao.Lab.Entity.Enums;
using Poxiao.Lab.EventBus;
using Poxiao.Lab.Entity.Extensions;
using Poxiao.Lab.Entity.Models;
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

    // 静态缓存属性名称映射 (LowerCase -> CamelCase)
    private static readonly Dictionary<string, string> _propertyCamelCaseMapping;

    static IntermediateDataService()
    {
        _propertyCamelCaseMapping = typeof(IntermediateDataListOutput)
            .GetProperties()
            .ToDictionary(
                p => p.Name.ToLowerInvariant(),
                p => char.ToLowerInvariant(p.Name[0]) + p.Name.Substring(1)
            );
    }
    private readonly ISqlSugarRepository<IntermediateDataFormulaCalcLogEntity> _calcLogRepository;
    private readonly IUserManager _userManager;
    private readonly ProductSpecVersionService _versionService;
    private readonly IIntermediateDataFormulaService _formulaService;
    private readonly IFormulaParser _formulaParser;
    private readonly IntermediateDataFormulaBatchCalculator _formulaBatchCalculator;
    private readonly IEventPublisher _eventPublisher;

    public IntermediateDataService(
        ISqlSugarRepository<IntermediateDataEntity> repository,
        ISqlSugarRepository<RawDataEntity> rawDataRepository,
        ISqlSugarRepository<ProductSpecAttributeEntity> productSpecAttributeRepository,
        ISqlSugarRepository<ProductSpecEntity> productSpecRepository,
        ISqlSugarRepository<UserEntity> userRepository,
        ISqlSugarRepository<IntermediateDataFormulaCalcLogEntity> calcLogRepository,
        IUserManager userManager,
        ProductSpecVersionService versionService,
        IIntermediateDataFormulaService formulaService,
        IFormulaParser formulaParser,
        IntermediateDataFormulaBatchCalculator formulaBatchCalculator,
        IEventPublisher eventPublisher
    )
    {
        _repository = repository;
        _rawDataRepository = rawDataRepository;
        _productSpecRepository = productSpecRepository;
        _productSpecAttributeRepository = productSpecAttributeRepository;
        _userRepository = userRepository;
        _calcLogRepository = calcLogRepository;
        _userManager = userManager;
        _versionService = versionService;
        _formulaService = formulaService;
        _formulaParser = formulaParser;
        _formulaBatchCalculator = formulaBatchCalculator;
        _eventPublisher = eventPublisher;
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

        // 按关键词筛选（炉号、产线等）
        if (!string.IsNullOrEmpty(input.Keyword))
        {
            query = query.Where(t =>
                (t.FurnaceNoFormatted != null && t.FurnaceNoFormatted.Contains(input.Keyword))
                || (t.LineNo.HasValue && t.LineNo.Value.ToString().Contains(input.Keyword))
            );
        }

        // 按生产日期范围筛选
        if (input.StartDate.HasValue)
        {
            query = query.Where(t => t.ProdDate >= input.StartDate.Value);
        }
        if (input.EndDate.HasValue)
        {
            var endDate = input.EndDate.Value.AddDays(1);
            query = query.Where(t => t.ProdDate < endDate);
        }

        // 按检测日期范围筛选
        if (input.DetectionStartDate.HasValue)
        {
            query = query.Where(t => t.DetectionDate >= input.DetectionStartDate.Value);
        }
        if (input.DetectionEndDate.HasValue)
        {
            var detectionEndDate = input.DetectionEndDate.Value.AddDays(1);
            query = query.Where(t => t.DetectionDate < detectionEndDate);
        }

        // 按产线筛选
        if (!string.IsNullOrEmpty(input.LineNo))
        {
            if (int.TryParse(input.LineNo, out var lineNoValue))
            {
                query = query.Where(t => t.LineNo == lineNoValue);
            }
            else
            {
                // 如果无法解析为整数，则返回空结果
                query = query.Where(t => false);
            }
        }

        // 先查询所有符合条件的数据（不排序，不分页）
        var allData = await query.ToListAsync();

        // 处理排序规则
        if (!string.IsNullOrEmpty(input.SortRules))
        {
            try
            {
                var sortRules = System.Text.Json.JsonSerializer.Deserialize<List<SortRule>>(
                    input.SortRules
                );
                if (sortRules != null && sortRules.Count > 0)
                {
                    allData = ApplySortRules(allData, sortRules);
                }
            }
            catch
            {
                // 如果排序规则解析失败，使用默认排序
                allData = ApplyDefaultSort(allData);
            }
        }
        else
        {
            // 没有排序规则，使用默认排序
            allData = ApplyDefaultSort(allData);
        }

        // 手动分页
        var total = allData.Count;
        var pageSize = input.PageSize > 0 ? input.PageSize : 10;
        var currentPage = input.CurrentPage > 0 ? input.CurrentPage : 1;
        var skip = (currentPage - 1) * pageSize;
        var pagedData = allData.Skip(skip).Take(pageSize).ToList();

        // 获取创建者姓名
        var userIds = pagedData
            .Select(t => t.CreatorUserId)
            .Where(id => !string.IsNullOrEmpty(id))
            .Distinct()
            .ToList();
        var users = await _userRepository
            .AsQueryable()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.RealName })
            .ToListAsync();
        var userDict = users.ToDictionary(u => u.Id, u => u.RealName);

        // 获取公式精度配置 - 只应用 SYSTEM 类型的公式精度
        var formulas = await _formulaService.GetListAsync();
        var precisionMap = formulas
            .Where(f => f.SourceType == "SYSTEM" && f.Precision.HasValue && !string.IsNullOrEmpty(f.ColumnName))
            .ToDictionary(
                f => f.ColumnName,
                f => f.Precision.Value,
                StringComparer.OrdinalIgnoreCase
            );

        // 转换输出
        var outputList = pagedData
            .Select(item =>
            {
                var output = item.Adapt<IntermediateDataListOutput>();
                output.CreatorUserName = userDict.GetValueOrDefault(item.CreatorUserId ?? "", "");
                output.ProdDateStr = item.ProdDate?.ToString("yyyy/MM/ddm") ?? "";
                output.DetectionDateStr = item.DetectionDate?.ToString("yyyy/MM/dd") ?? "";
                // 构建带厚分布列表
                output.ThicknessDistList = BuildThicknessDistList(item);

                // 构建叠片系数分布列表
                output.LaminationDistList = BuildLaminationDistList(item);

                // 转换为字典以支持字符串格式化
                // Mapster 默认支持对象到字典的转换
                var dict = output.Adapt<Dictionary<string, object>>();

                // 应用精度格式化
                ApplyPrecisionFormatting(dict, precisionMap);

                // 转换为 CamelCase (强制修正属性名为前端期望的格式)
                var camelDict = new Dictionary<string, object>(dict.Count);
                foreach (var kvp in dict)
                {
                    var key = kvp.Key;
                    var lowerKey = key.ToLowerInvariant();
                    
                    // 尝试从映射表中获取标准的 CamelCase 名称
                    if (_propertyCamelCaseMapping.TryGetValue(lowerKey, out var correctKey))
                    {
                        key = correctKey;
                    }
                    else if (!string.IsNullOrEmpty(key) && char.IsUpper(key[0]))
                    {
                        // Fallback: 如果不在映射表里，尝试简单的首字母小写
                        key = char.ToLowerInvariant(key[0]) + key.Substring(1);
                    }
                    camelDict[key] = kvp.Value;
                }

                return camelDict;
            })
            .ToList();

        return new
        {
            list = outputList,
            pagination = new
            {
                total = total,
                pageSize = pageSize,
                currentPage = currentPage,
            },
        };
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

    /// <summary>
    /// 计算日志分页列表.
    /// </summary>
    [HttpGet("calc-logs")]
    public async Task<dynamic> GetCalcLogs([FromQuery] IntermediateDataCalcLogQuery input)
    {
        var query = _calcLogRepository.AsQueryable().Where(t => t.DeleteMark == null);

        query = query.WhereIF(
            !string.IsNullOrWhiteSpace(input.BatchId),
            t => t.BatchId == input.BatchId
        );
        query = query.WhereIF(
            !string.IsNullOrWhiteSpace(input.IntermediateDataId),
            t => t.IntermediateDataId == input.IntermediateDataId
        );
        query = query.WhereIF(
            !string.IsNullOrWhiteSpace(input.ColumnName),
            t => t.ColumnName.Contains(input.ColumnName)
        );
        query = query.WhereIF(
            !string.IsNullOrWhiteSpace(input.FormulaType),
            t => t.FormulaType == input.FormulaType
        );
        query = query.WhereIF(
            !string.IsNullOrWhiteSpace(input.ErrorType),
            t => t.ErrorType == input.ErrorType
        );

        var page = await query
            .OrderBy(t => t.CreatorTime, OrderByType.Desc)
            .Select(t => new IntermediateDataCalcLogOutput
            {
                Id = t.Id,
                BatchId = t.BatchId,
                IntermediateDataId = t.IntermediateDataId,
                ColumnName = t.ColumnName,
                FormulaName = t.FormulaName,
                FormulaType = t.FormulaType,
                ErrorType = t.ErrorType,
                ErrorMessage = t.ErrorMessage,
                ErrorDetail = t.ErrorDetail,
                CreatorTime = t.CreatorTime
            })
            .ToPagedListAsync(input.CurrentPage, input.PageSize);

        return PageResult<IntermediateDataCalcLogOutput>.SqlSugarPageResult(page);
    }

    /// <inheritdoc />
    [HttpPut("performance")]
    [Microsoft.AspNetCore.Authorization.Authorize("lab:intermediateData:performance_edit")]
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

        // 只更新传入的非null字段
        if (input.PerfSsPower.HasValue)
        {
            entity.PerfSsPower = input.PerfSsPower;
        }
        if (input.PerfPsLoss.HasValue)
        {
            entity.PerfPsLoss = input.PerfPsLoss;
        }
        if (input.PerfHc.HasValue)
        {
            entity.PerfHc = input.PerfHc;
        }
        if (input.PerfAfterSsPower.HasValue)
        {
            entity.PerfAfterSsPower = input.PerfAfterSsPower;
        }
        if (input.PerfAfterPsLoss.HasValue)
        {
            entity.PerfAfterPsLoss = input.PerfAfterPsLoss;
        }
        if (input.PerfAfterHc.HasValue)
        {
            entity.PerfAfterHc = input.PerfAfterHc;
        }

        // 始终更新编辑人信息
        entity.PerfEditorId = _userManager.UserId;
        entity.PerfEditorName = _userManager.RealName;
        entity.PerfEditTime = DateTime.Now;
        entity.LastModifyUserId = _userManager.UserId;
        entity.LastModifyTime = DateTime.Now;

        await _repository.UpdateAsync(entity);
    }

    /// <inheritdoc />
    [HttpPut("appearance")]
    [Microsoft.AspNetCore.Authorization.Authorize("lab:intermediateData:appearance_edit")]
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

        entity.MagneticResult = input.MagneticResult;
        entity.ThicknessResult = input.ThicknessResult;
        entity.LaminationResult = input.LaminationResult;
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
    [HttpPost("recalculate")]
    public async Task<FormulaCalculationResult> Recalculate([FromBody] List<string> ids)
    {
        return await _formulaBatchCalculator.CalculateByIdsAsync(ids);
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
    /// 应用精度格式化到输出数据.
    /// </summary>
    /// <param name="output">输出对象</param>
    /// <param name="precisionMap">精度映射（columnName -> precision）</param>
    /// <summary>
    /// 应用精度格式化到输出数据 (转为字符串).
    /// </summary>
    /// <param name="output">输出字典</param>
    /// <param name="precisionMap">精度映射（columnName -> precision）</param>
    private void ApplyPrecisionFormatting(Dictionary<string, object> output, Dictionary<string, int> precisionMap)
    {
        if (precisionMap == null || precisionMap.Count == 0)
        {
            return;
        }

        // 获取精度的辅助函数，支持精确匹配和前缀匹配
        int? GetPrecision(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName)) return null;

            // 1. 精确匹配
            if (precisionMap.TryGetValue(fieldName, out var precision))
            {
                return precision;
            }

            // 2. 前缀匹配（如 Thickness1 匹配 Thickness）
            // 优化：只对符合特定模式的字段才使用正则
            if (char.IsLetter(fieldName[0]) && char.IsDigit(fieldName[fieldName.Length - 1]))
            {
                var match = System.Text.RegularExpressions.Regex.Match(fieldName, @"^([a-zA-Z]+)\d+$");
                if (match.Success)
                {
                    var prefix = match.Groups[1].Value;
                    if (precisionMap.TryGetValue(prefix, out var prefixPrecision))
                    {
                        return prefixPrecision;
                    }
                }
            }

            return null;
        }

        var keys = output.Keys.ToList();
        foreach (var key in keys)
        {
            var value = output[key];
            if (value == null) continue;

            // 检查 value 是否为数字类型
            if (value is decimal || value is double || value is float)
            {
                var precision = GetPrecision(key);
                if (precision.HasValue)
                {
                    if (value is decimal d) output[key] = d.ToString("F" + precision.Value);
                    else if (value is double db) output[key] = db.ToString("F" + precision.Value);
                    else if (value is float f) output[key] = f.ToString("F" + precision.Value);
                }
            }
        }
    }

    /// <summary>
    /// 解析检测列配置 (生成 1 到 N 的列表).
    /// </summary>
    public List<int> ParseDetectionColumns(int? detectionColumnsCount)
    {
        if (!detectionColumnsCount.HasValue || detectionColumnsCount.Value <= 0)
        {
            return new List<int>();
        }

        // 生成从 1 到 N 的整数列表
        return Enumerable.Range(1, detectionColumnsCount.Value).ToList();
    }

    /// <summary>
    /// 从原始数据生成中间数据.
    /// </summary>
    /// <param name="rawData">原始数据</param>
    /// <param name="productSpec">产品规格</param>
    /// <param name="detectionColumns">检测列</param>
    /// <param name="layers">层数</param>
    /// <param name="length">长度</param>
    /// <param name="density">密度</param>
    /// <param name="specVersion">规格版本</param>
    /// <param name="batchId">批次ID，用于后续异步公式计算</param>
    public async Task<IntermediateDataEntity> GenerateIntermediateDataAsync(
        RawDataEntity rawData,
        ProductSpecEntity productSpec,
        List<int> detectionColumns,
        int layers,
        decimal length,
        decimal density,
        int? specVersion,
        string batchId = null
    )
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
            // 产品规格参数直接写入中间数据表，参与后续计算
            ProductLength = length,
            ProductLayers = layers,
            ProductDensity = density,
            // 公式计算相关字段初始化
            BatchId = batchId,
            CalcStatus = IntermediateDataCalcStatus.PENDING,
            CalcStatusTime = DateTime.Now,
            CreatorTime = DateTime.Now,
        };

        // 使用FurnaceNo类生成各种编号
        var furnaceNoObj = FurnaceNo.Build(
            rawData.LineNo?.ToString() ?? "1",
            rawData.Shift?.ToString() ?? "甲",
            rawData.ProdDate,
            rawData.FurnaceBatchNo?.ToString() ?? "1",
            rawData.CoilNo?.ToString() ?? "1",
            rawData.SubcoilNo?.ToString() ?? "1",
            rawData.FeatureSuffix
        );

        if (furnaceNoObj.IsValid)
        {
            // 喷次：8位日期-炉号
            entity.SprayNo = furnaceNoObj.GetSprayNo();
            entity.ShiftNo = furnaceNoObj.GetBatchNo();
        }
        else
        {
            // 如果解析失败，使用简单格式
            entity.SprayNo = $"{rawData.ProdDate?.ToString("yyyyMMdd")}-{rawData.FurnaceBatchNo}";
            entity.ShiftNo =
                $"{rawData.LineNo}{rawData.Shift}{rawData.ProdDate?.ToString("yyyyMMdd")}-{rawData.FurnaceBatchNo}";
        }

        // 四米带材重量（原始数据带材重量）
        entity.CoilWeight = rawData.CoilWeight;

        // 一米带材重量 = 四米带材重量 / 长度
        if (rawData.CoilWeight.HasValue && length > 0)
        {
            entity.OneMeterWeight = Math.Round(rawData.CoilWeight.Value / length, 2);
        }

        // 带宽
        entity.Width = rawData.Width.HasValue ? Math.Round(rawData.Width.Value, 2) : null;

        // 赋值所有检测列数据（Detection1-22）
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
            && entity.Width.HasValue
            && entity.AvgThickness.HasValue
            && entity.AvgThickness.Value > 0
        )
        {
            var divisor = entity.Width.Value * (entity.AvgThickness.Value / 10m);
            if (divisor > 0)
            {
                entity.Density = Math.Round(entity.OneMeterWeight.Value / divisor, 2);
            }
        }

        // ��Ƭϵ������: ���״�������(g) / (����(mm) * 400 * ƽ����� * 7.25 * 0.0000001)
        // ע��400��Ӧ��Ʒ��񳤶�(4m = 400cm)
        var hasLaminationInputs =
            entity.CoilWeight.HasValue
            && entity.Width.HasValue
            && entity.AvgThickness.HasValue
            && entity.AvgThickness.Value > 0;
        decimal? laminationDivisor = null;
        if (hasLaminationInputs)
        {
            var lengthCm = length * 100; // ת��Ϊ����
            laminationDivisor =
                entity.Width.Value * lengthCm * entity.AvgThickness.Value * density * 0.0000001m;
            if (laminationDivisor > 0)
            {
                entity.LaminationFactor = Math.Round(
                    entity.CoilWeight.Value / laminationDivisor.Value,
                    2
                );
            }
        }

        await TryInsertLaminationFactorLogAsync(
            entity,
            length,
            density,
            hasLaminationInputs,
            laminationDivisor
        );

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
            var coilNoValue = rawData.CoilNo.Value;
        }

        // 特性ID列表（从原始数据复制，如果原始数据有匹配结果）
        if (!string.IsNullOrWhiteSpace(rawData.AppearanceFeatureIds))
        {
            entity.AppearanceFeatureIds = rawData.AppearanceFeatureIds;
        }
        if (!string.IsNullOrWhiteSpace(rawData.AppearanceFeatureCategoryIds))
        {
            entity.AppearanceFeatureCategoryIds = rawData.AppearanceFeatureCategoryIds;
        }
        if (!string.IsNullOrWhiteSpace(rawData.AppearanceFeatureLevelIds))
        {
            entity.AppearanceFeatureLevelIds = rawData.AppearanceFeatureLevelIds;
        }

        // 设置是否需要人工确认：如果特性匹配置信度 < 90%，则需要人工确认
        if (rawData.MatchConfidence.HasValue && rawData.MatchConfidence.Value < 0.9)
        {
            entity.RequiresManualConfirm = true;
        }
        else
        {
            entity.RequiresManualConfirm = false;
        }

        // 计算指标（在基础数据生成完成后）
        await CalculateIndicatorsAsync(entity, productSpec.Id);

        // 注意：公式计算已改为异步后台任务，不在生成时计算
        // 数据先保存到中间表，然后通过 BatchCalculateFormulasAsync 批量计算

        return entity;
    }

    /// <summary>
    /// 计算所有启用的指标（使用新的 Metric 系统）
    /// </summary>
    private async Task CalculateIndicatorsAsync(
        IntermediateDataEntity entity,
        string productSpecId
    ) { }

    /// <summary>
    /// 批量计算公式（用于后台任务）.
    /// 对指定的中间数据ID列表进行公式计算，支持错误记录和用户提示.
    /// </summary>
    /// <param name="intermediateDataIds">中间数据ID列表</param>
    /// <returns>计算结果统计</returns>
    [HttpPost("batch-calculate-formulas")]
    public async Task<FormulaCalculationResult> BatchCalculateFormulasAsync(
        [FromBody] List<string> intermediateDataIds
    )
    {
        return await _formulaBatchCalculator.CalculateByIdsAsync(intermediateDataIds);
    }

    /// <summary>
    /// 根据批次ID批量计算公式.
    /// </summary>
    /// <param name="batchId">批次ID</param>
    /// <returns>计算结果统计</returns>
    [HttpPost("batch-calculate-by-batch-id/{batchId}")]
    public async Task<FormulaCalculationResult> BatchCalculateFormulasByBatchIdAsync(string batchId)
    {
        return await _formulaBatchCalculator.CalculateByBatchAsync(batchId);
    }

    /// <summary>
    /// 同步计算CALC公式（不持久化）.
    /// </summary>
    /// <param name="entities">中间数据实体列表</param>
    public async System.Threading.Tasks.Task ApplyCalcFormulasForEntitiesAsync(
        List<IntermediateDataEntity> entities
    )
    {
        if (entities == null || entities.Count == 0)
        {
            return;
        }

        var (calcFormulas, _) = await GetEnabledFormulasAsync();
        if (calcFormulas.Count == 0)
        {
            return;
        }

        foreach (var entity in entities)
        {
            if (entity == null)
            {
                continue;
            }

            var contextData = ExtractContextDataFromEntity(entity);
            ApplyCalcFormulasToEntity(entity, calcFormulas, contextData);
        }
    }

    /// <summary>
    /// 获取批次计算状态.
    /// </summary>
    /// <param name="batchId">批次ID</param>
    /// <returns>批次计算状态统计</returns>
    [HttpGet("batch-status/{batchId}")]
    public async Task<BatchCalculationStatus> GetBatchCalculationStatusAsync(string batchId)
    {
        var entities = await _repository
            .AsQueryable()
            .Where(t => t.BatchId == batchId && t.DeleteMark == null)
            .Select(t => new { t.CalcStatus, t.CalcErrorMessage })
            .ToListAsync();

        return new BatchCalculationStatus
        {
            BatchId = batchId,
            TotalCount = entities.Count,
            PendingCount = entities.Count(e => e.CalcStatus == IntermediateDataCalcStatus.PENDING),
            ProcessingCount = entities.Count(e =>
                e.CalcStatus == IntermediateDataCalcStatus.PROCESSING
            ),
            SuccessCount = entities.Count(e => e.CalcStatus == IntermediateDataCalcStatus.SUCCESS),
            FailedCount = entities.Count(e => e.CalcStatus == IntermediateDataCalcStatus.FAILED),
        };
    }

    /// <summary>
    /// 根据批次ID批量计算公式（内部方法，用于后台任务）.
    /// </summary>
    /// <param name="batchId">批次ID</param>
    /// <returns>计算结果统计</returns>
    internal async Task<FormulaCalculationResult> BatchCalculateFormulasByBatchIdInternalAsync(
        string batchId
    )
    {
        // 根据批次ID查询待计算的中间数据
        var intermediateDataIds = await _repository
            .AsQueryable()
            .Where(t => t.BatchId == batchId && t.DeleteMark == null)
            .Select(t => t.Id)
            .ToListAsync();

        return await BatchCalculateFormulasInternalAsync(intermediateDataIds);
    }

    private async Task<(
        List<IntermediateDataFormulaDto> CalcFormulas,
        List<IntermediateDataFormulaDto> JudgeFormulas
    )> GetEnabledFormulasAsync()
    {
        var allFormulas = await _formulaService.GetListAsync();
        var enabledFormulas = allFormulas
            .Where(f => f.IsEnabled && f.TableName == "INTERMEDIATE_DATA")
            .ToList();

        var calcFormulas = enabledFormulas
            .Where(f => f.FormulaType == "CALC" && !string.IsNullOrWhiteSpace(f.Formula))
            .OrderBy(f => f.SortOrder)
            .ThenBy(f => f.CreatorTime)
            .ToList();

        var judgeFormulas = enabledFormulas
            .Where(f => f.FormulaType == "JUDGE" && !string.IsNullOrWhiteSpace(f.Formula))
            .OrderBy(f => f.SortOrder)
            .ThenBy(f => f.CreatorTime)
            .ToList();

        return (calcFormulas, judgeFormulas);
    }

    /// <summary>
    /// 批量计算公式内部实现（可被后台任务调用）.
    /// </summary>
    internal async Task<FormulaCalculationResult> BatchCalculateFormulasInternalAsync(
        List<string> intermediateDataIds
    )
    {
        var result = new FormulaCalculationResult
        {
            TotalCount = intermediateDataIds?.Count ?? 0,
            SuccessCount = 0,
            FailedCount = 0,
            Errors = new List<FormulaCalculationError>(),
        };

        if (intermediateDataIds == null || intermediateDataIds.Count == 0)
        {
            return result;
        }

        try
        {
            // 1. 从公式维护中获取所有启用的公式（计算+判定），只查询一次
            var (calcFormulas, judgeFormulas) = await GetEnabledFormulasAsync();

            if (calcFormulas.Count == 0 && judgeFormulas.Count == 0)
            {
                result.Message = "没有启用的公式";
                return result;
            }

            // 2. 批量查询中间数据
            var entities = await _repository
                .AsQueryable()
                .Where(t => intermediateDataIds.Contains(t.Id) && t.DeleteMark == null)
                .ToListAsync();

            if (entities.Count == 0)
            {
                result.Message = "未找到需要计算的中间数据";
                return result;
            }

            // 3. 逐条计算（可以后续优化为并行计算）
            foreach (var entity in entities)
            {
                try
                {
                    // 更新状态为计算中
                    entity.CalcStatus = IntermediateDataCalcStatus.PROCESSING;
                    entity.CalcStatusTime = DateTime.Now;
                    entity.CalcErrorMessage = null; // 清空之前的错误信息
                    await _repository
                        .AsUpdateable(entity)
                        .UpdateColumns(t => new
                        {
                            t.CalcStatus,
                            t.CalcStatusTime,
                            t.CalcErrorMessage,
                        })
                        .ExecuteCommandAsync();

                    // 执行公式计算
                    await CalculateFormulasForEntityAsync(entity, calcFormulas, judgeFormulas);

                    // 更新状态为成功
                    entity.CalcStatus = IntermediateDataCalcStatus.SUCCESS;
                    entity.CalcStatusTime = DateTime.Now;
                    entity.CalcErrorMessage = null;
                    await _repository
                        .AsUpdateable(entity)
                        .UpdateColumns(t => new
                        {
                            t.CalcStatus,
                            t.CalcStatusTime,
                            t.CalcErrorMessage,
                        })
                        .ExecuteCommandAsync();

                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    var errorMessage =
                        ex.Message.Length > 500 ? ex.Message.Substring(0, 500) : ex.Message;

                    // 更新状态为失败，并记录错误摘要
                    entity.CalcStatus = IntermediateDataCalcStatus.FAILED;
                    entity.CalcStatusTime = DateTime.Now;
                    entity.CalcErrorMessage = errorMessage;
                    await _repository
                        .AsUpdateable(entity)
                        .UpdateColumns(t => new
                        {
                            t.CalcStatus,
                            t.CalcStatusTime,
                            t.CalcErrorMessage,
                        })
                        .ExecuteCommandAsync();

                    result.Errors.Add(
                        new FormulaCalculationError
                        {
                            IntermediateDataId = entity.Id,
                            FurnaceNo = entity.FurnaceNo,
                            ErrorMessage = ex.Message,
                            ErrorDetail = ex.ToString(),
                        }
                    );
                }
            }

            result.Message =
                $"计算完成：成功 {result.SuccessCount} 条，失败 {result.FailedCount} 条";
        }
        catch (Exception ex)
        {
            result.Message = $"批量计算过程发生异常: {ex.Message}";
            result.Errors.Add(
                new FormulaCalculationError
                {
                    ErrorMessage = ex.Message,
                    ErrorDetail = ex.ToString(),
                }
            );
        }

        return result;
    }

    /// <summary>
    /// 对单个中间数据实体进行公式计算.
    /// </summary>
    private async Task CalculateFormulasForEntityAsync(
        IntermediateDataEntity entity,
        List<IntermediateDataFormulaDto> calcFormulas,
        List<IntermediateDataFormulaDto> judgeFormulas
    )
    {
        try
        {
            if (
                (calcFormulas == null || calcFormulas.Count == 0)
                && (judgeFormulas == null || judgeFormulas.Count == 0)
            )
            {
                return; // 没有公式，直接返回
            }

            // 1. 提取中间数据实体的上下文数据（用于公式计算）
            var contextData = ExtractContextDataFromEntity(entity);

            if (calcFormulas != null && calcFormulas.Count > 0)
            {
                ApplyCalcFormulasToEntity(entity, calcFormulas, contextData);
            }

            if (judgeFormulas != null && judgeFormulas.Count > 0)
            {
                ApplyJudgeFormulasToEntity(entity, judgeFormulas, contextData);
            }

            // 7. 保存更新后的实体到数据库
            entity.LastModifyTime = DateTime.Now;
            await _repository.UpdateAsync(entity);
        }
        catch (Exception ex)
        {
            // 重新抛出异常，以便上层记录错误
            throw;
        }
    }

    private void ApplyCalcFormulasToEntity(
        IntermediateDataEntity entity,
        List<IntermediateDataFormulaDto> calcFormulas,
        Dictionary<string, object> contextData
    )
    {
        // 按顺序计算每个计算公式
        foreach (var formulaDto in calcFormulas)
        {
            try
            {
                // 计算公式值
                decimal? calculatedValue = null;
                try
                {
                    // 根据公式类型选择上下文
                    object calcContext = contextData;
                    // 如果是范围公式，需要传入实体对象以便使用反射获取动态列
                    if (
                        formulaDto.Formula.Contains("RANGE(")
                        || formulaDto.Formula.Contains("DIFF_FIRST_LAST")
                    )
                    {
                        calcContext = entity;
                    }

                    calculatedValue = _formulaParser.Calculate(formulaDto.Formula, calcContext);
                }
                catch (Exception calcEx)
                {
                    // 计算失败，如果公式有默认值，使用默认值
                    if (!string.IsNullOrWhiteSpace(formulaDto.DefaultValue))
                    {
                        if (decimal.TryParse(formulaDto.DefaultValue, out var defaultVal))
                        {
                            calculatedValue = defaultVal;
                        }
                    }
                    // 如果没有默认值，抛出异常以便记录错误
                    if (!calculatedValue.HasValue)
                    {
                        throw new Exception(
                            $"公式计算失败 - 列名: {formulaDto.ColumnName}, 公式: {formulaDto.Formula}, 错误: {calcEx.Message}"
                        );
                    }
                }

                // 如果计算值为null，使用默认值（注意：公式维护中定义的默认值）
                if (
                    !calculatedValue.HasValue
                    && !string.IsNullOrWhiteSpace(formulaDto.DefaultValue)
                )
                {
                    if (decimal.TryParse(formulaDto.DefaultValue, out var defaultVal))
                    {
                        calculatedValue = defaultVal;
                    }
                }

                // 应用精度（公式维护中定义的精度）
                if (calculatedValue.HasValue && formulaDto.Precision.HasValue)
                {
                    calculatedValue = Math.Round(
                        calculatedValue.Value,
                        formulaDto.Precision.Value
                    );
                }

                // 将计算结果设置到中间数据实体的对应属性
                SetFormulaValueToEntity(entity, formulaDto.ColumnName, calculatedValue);

                // 更新上下文数据，以便后续公式可以使用已计算的值
                if (calculatedValue.HasValue)
                {
                    contextData[formulaDto.ColumnName] = calculatedValue.Value;
                }
            }
            catch (Exception ex)
            {
                // 公式计算失败，使用默认值（如果公式维护中定义了默认值）
                if (!string.IsNullOrWhiteSpace(formulaDto.DefaultValue))
                {
                    if (decimal.TryParse(formulaDto.DefaultValue, out var defaultVal))
                    {
                        // 应用精度
                        if (formulaDto.Precision.HasValue)
                        {
                            defaultVal = Math.Round(defaultVal, formulaDto.Precision.Value);
                        }
                        SetFormulaValueToEntity(entity, formulaDto.ColumnName, defaultVal);
                        contextData[formulaDto.ColumnName] = defaultVal;
                    }
                }
                else
                {
                    // 没有默认值，抛出异常以便上层记录错误
                    throw new Exception(
                        $"公式计算失败 - 列名: {formulaDto.ColumnName}, 公式: {formulaDto.Formula}, 错误: {ex.Message}"
                    );
                }
            }
        }
    }

    private void ApplyJudgeFormulasToEntity(
        IntermediateDataEntity entity,
        List<IntermediateDataFormulaDto> judgeFormulas,
        Dictionary<string, object> contextData
    )
    {
        foreach (var formulaDto in judgeFormulas)
        {
            try
            {
                var resultValue = EvaluateJudgeFormula(
                    formulaDto.Formula,
                    formulaDto.DefaultValue,
                    entity,
                    contextData
                );
                SetJudgeValueToEntity(entity, formulaDto.ColumnName, resultValue);
            }
            catch (Exception ex)
            {
                if (formulaDto.DefaultValue != null)
                {
                    SetJudgeValueToEntity(entity, formulaDto.ColumnName, formulaDto.DefaultValue);
                }
                else
                {
                    throw new Exception(
                        $"判定公式计算失败 - 列名: {formulaDto.ColumnName}, 错误: {ex.Message}"
                    );
                }
            }
        }
    }

    private string EvaluateJudgeFormula(
        string formulaJson,
        string defaultValue,
        IntermediateDataEntity entity,
        Dictionary<string, object> numericContext
    )
    {
        if (string.IsNullOrWhiteSpace(formulaJson))
        {
            return defaultValue;
        }

        try
        {
            var rulesToken = JArray.Parse(formulaJson);
            foreach (var token in rulesToken)
            {
                if (token.Type != JTokenType.Object)
                {
                    continue;
                }

                var ruleObject = (JObject)token;
                if (ruleObject["rootGroup"] != null)
                {
                    var rule = ruleObject.ToObject<SimpleJudgmentRule>();
                    if (rule != null && EvaluateSimpleRule(rule, entity, numericContext))
                    {
                        return rule.ResultValue;
                    }
                }
                else if (ruleObject["groups"] != null)
                {
                    var rule = ruleObject.ToObject<AdvancedJudgmentRule>();
                    if (rule != null && EvaluateAdvancedRule(rule, entity, numericContext))
                    {
                        return rule.ResultValue;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (defaultValue != null)
            {
                Console.WriteLine($"判定公式解析失败，使用默认值: {ex.Message}");
                return defaultValue;
            }

            throw new Exception($"判定公式解析失败: {ex.Message}", ex);
        }

        return defaultValue;
    }

    private bool EvaluateSimpleRule(
        SimpleJudgmentRule rule,
        IntermediateDataEntity entity,
        Dictionary<string, object> numericContext
    )
    {
        if (rule?.RootGroup == null)
        {
            return false;
        }

        return EvaluateSimpleGroup(rule.RootGroup, entity, numericContext);
    }

    private bool EvaluateSimpleGroup(
        SimpleConditionGroup group,
        IntermediateDataEntity entity,
        Dictionary<string, object> numericContext
    )
    {
        if (group?.Conditions == null || group.Conditions.Count == 0)
        {
            return false;
        }

        var isAnd = string.Equals(group.Logic, "AND", StringComparison.OrdinalIgnoreCase);
        return isAnd
            ? group.Conditions.All(c => EvaluateCondition(c.FieldId, c.Operator, c.Value, entity, numericContext))
            : group.Conditions.Any(c => EvaluateCondition(c.FieldId, c.Operator, c.Value, entity, numericContext));
    }

    private bool EvaluateAdvancedRule(
        AdvancedJudgmentRule rule,
        IntermediateDataEntity entity,
        Dictionary<string, object> numericContext
    )
    {
        if (rule?.Groups == null || rule.Groups.Count == 0)
        {
            return false;
        }

        return rule.Groups.All(group => EvaluateAdvancedGroup(group, entity, numericContext));
    }

    private bool EvaluateAdvancedGroup(
        AdvancedConditionGroup group,
        IntermediateDataEntity entity,
        Dictionary<string, object> numericContext
    )
    {
        if (group == null)
        {
            return false;
        }

        if (string.Equals(group.Mode, "nested", StringComparison.OrdinalIgnoreCase))
        {
            if (group.SubGroups == null || group.SubGroups.Count == 0)
            {
                return false;
            }

            var isAnd = string.Equals(group.Logic, "AND", StringComparison.OrdinalIgnoreCase);
            return isAnd
                ? group.SubGroups.All(sub => EvaluateSubGroup(sub, entity, numericContext))
                : group.SubGroups.Any(sub => EvaluateSubGroup(sub, entity, numericContext));
        }

        if (group.Conditions == null || group.Conditions.Count == 0)
        {
            return false;
        }

        var simpleAnd = string.Equals(group.Logic, "AND", StringComparison.OrdinalIgnoreCase);
        return simpleAnd
            ? group.Conditions.All(c => EvaluateCondition(c.LeftExpr, c.Operator, c.RightValue, entity, numericContext))
            : group.Conditions.Any(c => EvaluateCondition(c.LeftExpr, c.Operator, c.RightValue, entity, numericContext));
    }

    private bool EvaluateSubGroup(
        AdvancedSubConditionGroup subGroup,
        IntermediateDataEntity entity,
        Dictionary<string, object> numericContext
    )
    {
        if (subGroup?.Conditions == null || subGroup.Conditions.Count == 0)
        {
            return false;
        }

        var isAnd = string.Equals(subGroup.Logic, "AND", StringComparison.OrdinalIgnoreCase);
        return isAnd
            ? subGroup.Conditions.All(c => EvaluateCondition(c.LeftExpr, c.Operator, c.RightValue, entity, numericContext))
            : subGroup.Conditions.Any(c => EvaluateCondition(c.LeftExpr, c.Operator, c.RightValue, entity, numericContext));
    }

    private bool EvaluateCondition(
        string leftExpr,
        string operatorValue,
        string rightValue,
        IntermediateDataEntity entity,
        Dictionary<string, object> numericContext
    )
    {
        if (string.IsNullOrWhiteSpace(operatorValue))
        {
            return false;
        }

        var normalizedOp = operatorValue.Trim().ToUpperInvariant();
        if (normalizedOp == "!=")
        {
            normalizedOp = "<>";
        }

        var leftValue = ResolveLeftValue(leftExpr, entity, numericContext);

        if (normalizedOp == "IS_NULL")
        {
            return IsNullValue(leftValue);
        }

        if (normalizedOp == "NOT_NULL")
        {
            return !IsNullValue(leftValue);
        }

        if (leftValue == null)
        {
            return false;
        }

        if (leftValue is IEnumerable<string> listValue)
        {
            return CompareList(listValue, normalizedOp, rightValue);
        }

        if (leftValue is bool boolValue)
        {
            if (TryParseBool(rightValue, out var rightBool))
            {
                return normalizedOp switch
                {
                    "=" => boolValue == rightBool,
                    "<>" => boolValue != rightBool,
                    _ => false,
                };
            }
        }

        if (TryConvertToDecimal(leftValue, out var leftNumber) && TryConvertToDecimal(rightValue, out var rightNumber))
        {
            return normalizedOp switch
            {
                "=" => leftNumber == rightNumber,
                "<>" => leftNumber != rightNumber,
                ">" => leftNumber > rightNumber,
                ">=" => leftNumber >= rightNumber,
                "<" => leftNumber < rightNumber,
                "<=" => leftNumber <= rightNumber,
                _ => false,
            };
        }

        var leftStr = Convert.ToString(leftValue) ?? string.Empty;
        var rightStr = rightValue ?? string.Empty;

        return normalizedOp switch
        {
            "=" => string.Equals(leftStr, rightStr, StringComparison.OrdinalIgnoreCase),
            "<>" => !string.Equals(leftStr, rightStr, StringComparison.OrdinalIgnoreCase),
            _ => false,
        };
    }

    private object ResolveLeftValue(
        string leftExpr,
        IntermediateDataEntity entity,
        Dictionary<string, object> numericContext
    )
    {
        if (string.IsNullOrWhiteSpace(leftExpr))
        {
            return null;
        }

        var expr = leftExpr.Trim();
        if (IsFormulaExpression(expr))
        {
            try
            {
                return _formulaParser.Calculate(expr, numericContext);
            }
            catch
            {
                return null;
            }
        }

        var prop = typeof(IntermediateDataEntity).GetProperty(
            expr,
            System.Reflection.BindingFlags.IgnoreCase
                | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
        );
        if (prop != null)
        {
            var value = prop.GetValue(entity);
            return NormalizeJudgeValue(expr, value);
        }

        if (numericContext != null && numericContext.TryGetValue(expr, out var contextValue))
        {
            return contextValue;
        }

        return null;
    }

    private bool IsFormulaExpression(string expr)
    {
        if (string.IsNullOrWhiteSpace(expr))
        {
            return false;
        }

        return expr.Contains("RANGE(", StringComparison.OrdinalIgnoreCase)
            || expr.Contains("DIFF_FIRST_LAST", StringComparison.OrdinalIgnoreCase)
            || expr.Contains("[")
            || expr.Contains("]")
            || Regex.IsMatch(expr, @"[+\-*/()]");
    }

    private object NormalizeJudgeValue(string fieldName, object value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is string strValue)
        {
            if (IsJsonListField(fieldName))
            {
                try
                {
                    return JsonConvert.DeserializeObject<List<string>>(strValue) ?? new List<string>();
                }
                catch
                {
                    return new List<string>();
                }
            }

            return strValue;
        }

        return value;
    }

    private bool IsJsonListField(string fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            return false;
        }

        return string.Equals(fieldName, "AppearanceFeatureCategoryIds", StringComparison.OrdinalIgnoreCase)
            || string.Equals(fieldName, "AppearanceFeatureLevelIds", StringComparison.OrdinalIgnoreCase);
    }

    private bool CompareList(IEnumerable<string> listValue, string op, string rightValue)
    {
        var list = listValue?.Where(item => !string.IsNullOrWhiteSpace(item)).ToList() ?? new List<string>();
        var right = rightValue?.Trim() ?? string.Empty;

        if (op == "IS_NULL")
        {
            return list.Count == 0;
        }

        if (op == "NOT_NULL")
        {
            return list.Count > 0;
        }

        var contains = list.Any(item => string.Equals(item, right, StringComparison.OrdinalIgnoreCase));
        return op switch
        {
            "=" => contains,
            "<>" => !contains,
            _ => false,
        };
    }

    private bool IsNullValue(object value)
    {
        if (value == null)
        {
            return true;
        }

        if (value is string strValue)
        {
            return string.IsNullOrWhiteSpace(strValue);
        }

        if (value is IEnumerable<string> listValue)
        {
            return !listValue.Any();
        }

        return false;
    }

    private bool TryConvertToDecimal(object value, out decimal result)
    {
        result = 0m;
        if (value == null)
        {
            return false;
        }

        switch (value)
        {
            case decimal d:
                result = d;
                return true;
            case int i:
                result = i;
                return true;
            case long l:
                result = l;
                return true;
            case double db:
                result = Convert.ToDecimal(db);
                return true;
            case float f:
                result = Convert.ToDecimal(f);
                return true;
            case short s:
                result = s;
                return true;
            case byte b:
                result = b;
                return true;
            case string str:
                return decimal.TryParse(
                        str,
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out result
                    )
                    || decimal.TryParse(str, NumberStyles.Any, CultureInfo.CurrentCulture, out result);
        }

        try
        {
            result = Convert.ToDecimal(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool TryParseBool(string value, out bool result)
    {
        result = false;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim();
        if (bool.TryParse(normalized, out var boolValue))
        {
            result = boolValue;
            return true;
        }

        if (normalized == "1")
        {
            result = true;
            return true;
        }

        if (normalized == "0")
        {
            result = false;
            return true;
        }

        return false;
    }

    private void SetJudgeValueToEntity(
        IntermediateDataEntity entity,
        string columnName,
        string value
    )
    {
        if (string.IsNullOrWhiteSpace(columnName))
        {
            return;
        }

        var property = typeof(IntermediateDataEntity).GetProperty(columnName);
        if (property == null || !property.CanWrite)
        {
            Console.WriteLine($"未找到可写属性 - 属性名: {columnName}");
            return;
        }

        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        if (string.IsNullOrEmpty(value))
        {
            if (targetType == typeof(string))
            {
                property.SetValue(entity, value);
                return;
            }

            property.SetValue(entity, null);
            return;
        }

        try
        {
            object convertedValue = value;

            if (targetType == typeof(string))
            {
                convertedValue = value;
            }
            else if (targetType == typeof(int))
            {
                if (int.TryParse(value, out var intValue))
                {
                    convertedValue = intValue;
                }
                else
                {
                    Console.WriteLine($"无法将判定结果转换为 int: {value}");
                    return;
                }
            }
            else if (targetType == typeof(decimal))
            {
                if (decimal.TryParse(value, out var decimalValue))
                {
                    convertedValue = decimalValue;
                }
                else
                {
                    Console.WriteLine($"无法将判定结果转换为 decimal: {value}");
                    return;
                }
            }
            else if (targetType == typeof(bool))
            {
                if (TryParseBool(value, out var boolValue))
                {
                    convertedValue = boolValue;
                }
                else
                {
                    Console.WriteLine($"无法将判定结果转换为 bool: {value}");
                    return;
                }
            }
            else
            {
                convertedValue = Convert.ChangeType(value, targetType);
            }

            property.SetValue(entity, convertedValue);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"设置判定结果失败 - 列名: {columnName}, 错误: {ex.Message}");
        }
    }

    private class SimpleJudgmentRule
    {
        public string ResultValue { get; set; }
        public SimpleConditionGroup RootGroup { get; set; }
    }

    private class SimpleConditionGroup
    {
        public string Logic { get; set; }
        public List<SimpleCondition> Conditions { get; set; }
    }

    private class SimpleCondition
    {
        public string FieldId { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
    }

    private class AdvancedJudgmentRule
    {
        public string ResultValue { get; set; }
        public List<AdvancedConditionGroup> Groups { get; set; }
    }

    private class AdvancedConditionGroup
    {
        public string Mode { get; set; }
        public string Logic { get; set; }
        public List<AdvancedCondition> Conditions { get; set; }
        public List<AdvancedSubConditionGroup> SubGroups { get; set; }
    }

    private class AdvancedSubConditionGroup
    {
        public string Logic { get; set; }
        public List<AdvancedCondition> Conditions { get; set; }
    }

    private class AdvancedCondition
    {
        public string LeftExpr { get; set; }
        public string Operator { get; set; }
        public string RightValue { get; set; }
    }

    /// <summary>
    /// 将公式计算结果设置到中间数据实体的对应属性.
    /// </summary>
    private void SetFormulaValueToEntity(
        IntermediateDataEntity entity,
        string columnName,
        decimal? value
    )
    {
        if (string.IsNullOrWhiteSpace(columnName))
            return;

        // 使用反射设置属性值
        var property = typeof(IntermediateDataEntity).GetProperty(columnName);
        if (property != null && property.CanWrite)
        {
            try
            {
                // 处理可空类型
                var targetType = property.PropertyType;
                if (
                    targetType.IsGenericType
                    && targetType.GetGenericTypeDefinition() == typeof(Nullable<>)
                )
                {
                    var underlyingType = Nullable.GetUnderlyingType(targetType);
                    if (underlyingType != null && underlyingType == typeof(decimal))
                    {
                        property.SetValue(entity, value);
                        return;
                    }
                }
                else if (targetType == typeof(decimal))
                {
                    property.SetValue(entity, value ?? 0m);
                    return;
                }
                else if (targetType == typeof(decimal?))
                {
                    property.SetValue(entity, value);
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"设置属性值失败 - 属性名: {columnName}, 值: {value}, 错误: {ex.Message}"
                );
            }
        }
        else
        {
            Console.WriteLine(
                $"未找到可写属性 - 属性名: {columnName}，可能需要在 IntermediateDataEntity 中添加该属性"
            );
        }
    }

    /// <summary>
    /// 从中间数据实体提取上下文数据（用于公式计算）
    /// </summary>
    private Dictionary<string, object> ExtractContextDataFromEntity(IntermediateDataEntity entity)
    {
        var contextData = new Dictionary<string, object>();

        // 使用反射自动提取所有数值类型的属性
        var entityType = typeof(IntermediateDataEntity);
        var properties = entityType.GetProperties(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
        );

        foreach (var prop in properties)
        {
            // 跳过非数值类型和特殊属性
            if (
                prop.Name == "Id"
                || prop.Name == "RawDataId"
                || prop.Name == "CreatorUserId"
                || prop.Name == "CreatorTime"
                || prop.Name == "LastModifyUserId"
                || prop.Name == "LastModifyTime"
                || prop.Name == "DeleteMark"
                || prop.Name == "DeleteUserId"
                || prop.Name == "DeleteTime"
                || prop.Name == "TenantId"
                || prop.Name == "AppearanceFeatureIdsList" // 辅助属性
            )
            {
                continue;
            }

            var propType = prop.PropertyType;
            var underlyingType = Nullable.GetUnderlyingType(propType) ?? propType;

            // 只提取数值类型（decimal, int, double, float, long等）
            if (
                underlyingType == typeof(decimal)
                || underlyingType == typeof(int)
                || underlyingType == typeof(long)
                || underlyingType == typeof(double)
                || underlyingType == typeof(float)
                || underlyingType == typeof(short)
                || underlyingType == typeof(byte)
            )
            {
                var value = prop.GetValue(entity);
                if (value != null)
                {
                    // 转换为decimal以便公式计算
                    try
                    {
                        var decimalValue = Convert.ToDecimal(value);
                        contextData[prop.Name] = decimalValue;
                    }
                    catch
                    {
                        // 转换失败，跳过
                    }
                }
            }
        }

        // 添加常用简写变量（兼容性）
        if (!contextData.ContainsKey("Length") && contextData.ContainsKey("ProductLength"))
        {
            contextData["Length"] = contextData["ProductLength"];
        }
        if (!contextData.ContainsKey("Layers") && contextData.ContainsKey("ProductLayers"))
        {
            contextData["Layers"] = contextData["ProductLayers"];
        }
        if (!contextData.ContainsKey("Density"))
        {
            if (contextData.ContainsKey("Density"))
            {
                contextData["Density"] = contextData["Density"];
            }
            else if (contextData.ContainsKey("ProductDensity"))
            {
                contextData["Density"] = contextData["ProductDensity"];
            }
        }

        // 添加检测列数据（Detection1-Detection22）
        for (int i = 1; i <= 22; i++)
        {
            var propName = $"Detection{i}";
            var prop = entityType.GetProperty(propName);
            if (prop != null)
            {
                var value = prop.GetValue(entity);
                if (value != null)
                {
                    try
                    {
                        contextData[propName] = Convert.ToDecimal(value);
                    }
                    catch { }
                }
            }
        }

        // 添加带厚分布数据（Thickness1-Thickness22）
        for (int i = 1; i <= 22; i++)
        {
            var propName = $"Thickness{i}";
            var prop = entityType.GetProperty(propName);
            if (prop != null)
            {
                var value = prop.GetValue(entity);
                if (value != null)
                {
                    try
                    {
                        contextData[propName] = Convert.ToDecimal(value);
                    }
                    catch { }
                }
            }
        }

        // 添加叠片系数分布数据（LaminationDist1-LaminationDist22）
        for (int i = 1; i <= 22; i++)
        {
            var propName = $"LaminationDist{i}";
            var prop = entityType.GetProperty(propName);
            if (prop != null)
            {
                var value = prop.GetValue(entity);
                if (value != null)
                {
                    try
                    {
                        contextData[propName] = Convert.ToDecimal(value);
                    }
                    catch { }
                }
            }
        }

        // 添加检测列数量（用于TO运算符）
        if (entity.DetectionColumns.HasValue && entity.DetectionColumns.Value > 0)
        {
            contextData["DetectionColumns"] = entity.DetectionColumns.Value;
        }

        return contextData;
    }

    /// <summary>
    /// 如果值不为null，添加到字典
    /// </summary>
    private void AddValueIfNotNull(Dictionary<string, object> dict, string key, object value)
    {
        if (value != null)
        {
            dict[key] = value;
        }
    }

    /// <summary>
    /// 设置指标值到中间数据实体（使用反射）.
    /// </summary>
    private void SetIndicatorValue(
        IntermediateDataEntity entity,
        string indicatorCode,
        object value
    )
    {
        // 尝试直接设置属性（属性名与指标编码相同）
        var property = typeof(IntermediateDataEntity).GetProperty(indicatorCode);
        if (property != null && property.CanWrite)
        {
            // 转换类型
            if (value != null)
            {
                var targetType = property.PropertyType;
                if (
                    targetType.IsGenericType
                    && targetType.GetGenericTypeDefinition() == typeof(Nullable<>)
                )
                {
                    targetType = Nullable.GetUnderlyingType(targetType);
                }

                if (targetType != null)
                {
                    var convertedValue = Convert.ChangeType(value, targetType);
                    property.SetValue(entity, convertedValue);
                    return;
                }
            }
        }

        // 如果直接属性不存在，可以考虑使用扩展字段（JSON）存储
        // 这里暂时只支持直接属性映射
    }

    /// <summary>
    /// 获取原始数据中指定检测列的值（从detection1-detection22字段读取）.
    /// </summary>
    private List<decimal?> GetDetectionValues(RawDataEntity rawData, List<int> detectionColumns)
    {
        var values = new List<decimal?>();
        var detectionProps = new[]
        {
            rawData.Detection1,
            rawData.Detection2,
            rawData.Detection3,
            rawData.Detection4,
            rawData.Detection5,
            rawData.Detection6,
            rawData.Detection7,
            rawData.Detection8,
            rawData.Detection9,
            rawData.Detection10,
            rawData.Detection11,
            rawData.Detection12,
            rawData.Detection13,
            rawData.Detection14,
            rawData.Detection15,
            rawData.Detection16,
            rawData.Detection17,
            rawData.Detection18,
            rawData.Detection19,
            rawData.Detection20,
            rawData.Detection21,
            rawData.Detection22,
        };

        foreach (var col in detectionColumns)
        {
            if (col >= 1 && col <= 22)
            {
                values.Add(detectionProps[col - 1]);
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
                    ? divisor.Value > 0
                        ? "INFO"
                        : "INVALID_DIVISOR"
                    : "INVALID_DIVISOR"
                : "SKIP";

            string FormatDecimal(decimal? value) =>
                value.HasValue
                    ? value.Value.ToString(CultureInfo.InvariantCulture)
                    : "null";

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
            // 忽略日志写入异常，避免影响主流程
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

        for (int i = 1; i <= 22; i++)
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

        for (int i = 1; i <= 22; i++)
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

    #region 排序相关方法

    /// <summary>
    /// 应用排序规则.
    /// </summary>
    private List<IntermediateDataEntity> ApplySortRules(
        List<IntermediateDataEntity> data,
        List<SortRule> sortRules
    )
    {
        if (sortRules == null || sortRules.Count == 0)
            return ApplyDefaultSort(data);

        IOrderedEnumerable<IntermediateDataEntity> orderedData = null;

        for (int i = 0; i < sortRules.Count; i++)
        {
            var rule = sortRules[i];

            // 跳过字段为空的规则
            if (string.IsNullOrEmpty(rule.Field))
                continue;

            var isDesc = rule.Order?.ToLower() == "desc";

            if (i == 0)
            {
                // 第一个排序字段
                orderedData = ApplySingleFieldSort(data, rule.Field, isDesc);
            }
            else
            {
                // 后续的ThenBy
                if (orderedData != null)
                {
                    orderedData = ApplyThenBy(orderedData, rule.Field, isDesc);
                }
            }
        }

        return orderedData?.ToList() ?? data;
    }

    /// <summary>
    /// 应用单字段排序（首次排序）.
    /// </summary>
    private IOrderedEnumerable<IntermediateDataEntity> ApplySingleFieldSort(
        IEnumerable<IntermediateDataEntity> data,
        string field,
        bool isDesc
    )
    {
        if (string.IsNullOrEmpty(field))
        {
            // 如果字段为空，按ID排序
            return data.OrderBy(t => t.Id);
        }

        switch (field.ToLower())
        {
            case "proddate":
                return isDesc
                    ? data.OrderByDescending(t => t.ProdDate ?? DateTime.MinValue)
                    : data.OrderBy(t => t.ProdDate ?? DateTime.MinValue);

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
    private IOrderedEnumerable<IntermediateDataEntity> ApplyThenBy(
        IOrderedEnumerable<IntermediateDataEntity> orderedData,
        string field,
        bool isDesc
    )
    {
        if (string.IsNullOrEmpty(field))
        {
            // 如果字段为空，保持原顺序
            return orderedData;
        }

        switch (field.ToLower())
        {
            case "proddate":
                return isDesc
                    ? orderedData.ThenByDescending(t => t.ProdDate ?? DateTime.MinValue)
                    : orderedData.ThenBy(t => t.ProdDate ?? DateTime.MinValue);

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
    private List<IntermediateDataEntity> ApplyDefaultSort(List<IntermediateDataEntity> data)
    {
        return data.OrderBy(t => t.ProdDate ?? DateTime.MinValue)
            .ThenBy(t => t.FurnaceBatchNo ?? int.MaxValue)
            .ThenBy(t => t.CoilNo ?? decimal.MaxValue)
            .ThenBy(t => t.SubcoilNo ?? decimal.MaxValue)
            .ThenBy(t => t.LineNo ?? int.MaxValue)
            .ToList();
    }

    #endregion

    #endregion
}