using Poxiao.Lab.Entity.Dto.Unit;

namespace Poxiao.Lab.Interfaces;

/// <summary>
/// 单位换算服务接口.
/// </summary>
public interface IUnitConversionService
{
    /// <summary>
    /// 单位换算.
    /// </summary>
    /// <param name="value">原始数值.</param>
    /// <param name="fromUnitId">源单位 ID.</param>
    /// <param name="toUnitId">目标单位 ID.</param>
    /// <returns>换算后的数值.</returns>
    Task<decimal> ConvertAsync(decimal value, string fromUnitId, string toUnitId);

    /// <summary>
    /// 获取所有单位维度列表.
    /// </summary>
    /// <returns>单位维度列表.</returns>
    Task<List<UnitCategoryDto>> GetCategoriesAsync();

    /// <summary>
    /// 根据维度 ID 获取单位列表.
    /// </summary>
    /// <param name="categoryId">维度 ID.</param>
    /// <returns>单位列表.</returns>
    Task<List<UnitDefinitionDto>> GetUnitsByCategoryAsync(string categoryId);

    /// <summary>
    /// 获取所有单位列表（按维度分组）.
    /// </summary>
    /// <returns>按维度分组的单位列表.</returns>
    Task<Dictionary<string, List<UnitDefinitionDto>>> GetAllUnitsGroupedByCategoryAsync();
}
