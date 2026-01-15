using Microsoft.AspNetCore.Mvc;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Lab.Entity.Dto.Unit;
using Poxiao.Lab.Entity.Entity;
using Poxiao.Lab.Interfaces;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// 单位换算服务.
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "unit-conversion", Order = 200)]
[Route("api/lab/unit")]
public class UnitConversionService : IUnitConversionService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<UnitDefinitionEntity> _unitRepository;
    private readonly ISqlSugarRepository<UnitCategoryEntity> _categoryRepository;

    public UnitConversionService(
        ISqlSugarRepository<UnitDefinitionEntity> unitRepository,
        ISqlSugarRepository<UnitCategoryEntity> categoryRepository)
    {
        _unitRepository = unitRepository;
        _categoryRepository = categoryRepository;
    }

    /// <summary>
    /// 单位换算（API 端点）.
    /// </summary>
    /// <param name="request">换算请求.</param>
    /// <returns>换算结果.</returns>
    [HttpPost("convert")]
    public async Task<UnitConversionResponseDto> ConvertAsync([FromBody] UnitConversionRequestDto request)
    {
        if (request == null)
            throw Oops.Oh("请求参数不能为空");

        var result = await ConvertAsync(request.Value, request.FromUnitId, request.ToUnitId);

        return new UnitConversionResponseDto
        {
            OriginalValue = request.Value,
            FromUnitId = request.FromUnitId,
            ToUnitId = request.ToUnitId,
            ConvertedValue = result
        };
    }

    /// <inheritdoc />
    public async Task<decimal> ConvertAsync(decimal value, string fromUnitId, string toUnitId)
    {
        if (string.IsNullOrWhiteSpace(fromUnitId))
            throw Oops.Oh("源单位 ID 不能为空");

        if (string.IsNullOrWhiteSpace(toUnitId))
            throw Oops.Oh("目标单位 ID 不能为空");

        // 获取源单位和目标单位
        var fromUnit = await _unitRepository.GetByIdAsync(fromUnitId);
        if (fromUnit == null)
            throw Oops.Oh($"源单位不存在：{fromUnitId}");

        var toUnit = await _unitRepository.GetByIdAsync(toUnitId);
        if (toUnit == null)
            throw Oops.Oh($"目标单位不存在：{toUnitId}");

        // 检查是否属于同一维度
        if (fromUnit.CategoryId != toUnit.CategoryId)
            throw Oops.Oh($"不能进行跨维度换算：{fromUnit.CategoryId} -> {toUnit.CategoryId}");

        // 换算算法：
        // 1. 将原始数值转换为基准单位：V_base = (V_source * ScaleToBase_from) + Offset_from
        // 2. 将基准单位转换为目标单位：V_target = (V_base - Offset_to) / ScaleToBase_to

        // 转换为基准单位
        var baseValue = (value * fromUnit.ScaleToBase) + fromUnit.Offset;

        // 转换为目标单位
        var targetValue = (baseValue - toUnit.Offset) / toUnit.ScaleToBase;

        return targetValue;
    }

    /// <inheritdoc />
    [HttpGet("categories")]
    public async Task<List<UnitCategoryDto>> GetCategoriesAsync()
    {
        var categories = await _categoryRepository.GetListAsync(u => u.DeleteMark == 0 || u.DeleteMark == null);
        return categories
            .OrderBy(u => u.SortCode ?? 0)
            .Select(u => new UnitCategoryDto
            {
                Id = u.Id,
                Name = u.Name,
                Code = u.Code,
                Description = u.Description
            })
            .ToList();
    }

    /// <inheritdoc />
    [HttpGet("units/{categoryId}")]
    public async Task<List<UnitDefinitionDto>> GetUnitsByCategoryAsync(string categoryId)
    {
        if (string.IsNullOrWhiteSpace(categoryId))
            throw Oops.Oh("维度 ID 不能为空");

        var units = await _unitRepository.GetListAsync(u => 
            u.CategoryId == categoryId && (u.DeleteMark == 0 || u.DeleteMark == null));

        return units
            .OrderBy(u => u.SortCode ?? 0)
            .ThenBy(u => u.IsBase == 1 ? 0 : 1) // 基准单位排在前面
            .Select(u => new UnitDefinitionDto
            {
                Id = u.Id,
                CategoryId = u.CategoryId,
                Name = u.Name,
                Symbol = u.Symbol,
                IsBase = u.IsBase == 1,
                ScaleToBase = u.ScaleToBase,
                Offset = u.Offset,
                Precision = u.Precision
            })
            .ToList();
    }

    /// <inheritdoc />
    [HttpGet("units/all")]
    public async Task<Dictionary<string, List<UnitDefinitionDto>>> GetAllUnitsGroupedByCategoryAsync()
    {
        var units = await _unitRepository.GetListAsync(u => u.DeleteMark == 0 || u.DeleteMark == null);
        var categories = await _categoryRepository.GetListAsync(u => u.DeleteMark == 0 || u.DeleteMark == null);

        var result = new Dictionary<string, List<UnitDefinitionDto>>();

        foreach (var category in categories.OrderBy(c => c.SortCode ?? 0))
        {
            var categoryUnits = units
                .Where(u => u.CategoryId == category.Id)
                .OrderBy(u => u.SortCode ?? 0)
                .ThenBy(u => u.IsBase == 1 ? 0 : 1)
                .Select(u => new UnitDefinitionDto
                {
                    Id = u.Id,
                    CategoryId = u.CategoryId,
                    Name = u.Name,
                    Symbol = u.Symbol,
                    IsBase = u.IsBase == 1,
                    ScaleToBase = u.ScaleToBase,
                    Offset = u.Offset,
                    Precision = u.Precision
                })
                .ToList();

            if (categoryUnits.Any())
            {
                result[category.Id] = categoryUnits;
            }
        }

        return result;
    }
}
