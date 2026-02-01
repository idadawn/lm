using Mapster;
using Microsoft.AspNetCore.Mvc;
using Poxiao.DatabaseAccessor;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Manager;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.Unit;
using Poxiao.Lab.Interfaces;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// 单位定义服务.
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "unit-definition", Order = 202)]
[Route("api/lab/unit-definition")]
public class UnitDefinitionService : IUnitDefinitionService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<UnitDefinitionEntity> _repository;
    private readonly ISqlSugarRepository<UnitCategoryEntity> _categoryRepository;
    private readonly ICacheManager _cacheManager;
    private readonly IUserManager _userManager;

    /// <summary>
    /// 缓存键前缀
    /// </summary>
    private const string CachePrefix = "LAB:UnitDefinition";

    public UnitDefinitionService(
        ISqlSugarRepository<UnitDefinitionEntity> repository,
        ISqlSugarRepository<UnitCategoryEntity> categoryRepository,
        ICacheManager cacheManager,
        IUserManager userManager
    )
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _cacheManager = cacheManager;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取缓存键（带租户隔离）
    /// </summary>
    private string GetCacheKey(string suffix)
    {
        var tenantId = _userManager?.TenantId ?? "global";
        return $"{CachePrefix}:{tenantId}:{suffix}";
    }

    /// <inheritdoc />
    [HttpGet("")]
    public async Task<List<UnitDefinitionDto>> GetList([FromQuery] string? categoryId = null)
    {
        var cacheKey = GetCacheKey($"list:{categoryId ?? "all"}");

        // 尝试从缓存获取
        var cachedList = await _cacheManager.GetAsync<List<UnitDefinitionDto>>(cacheKey);
        if (cachedList != null && cachedList.Count > 0)
        {
            return cachedList;
        }

        // 从数据库获取
        var query = _repository.AsQueryable().Where(u => u.DeleteMark == 0 || u.DeleteMark == null);

        if (!string.IsNullOrWhiteSpace(categoryId))
        {
            query = query.Where(u => u.CategoryId == categoryId);
        }

        var list = await query.ToListAsync();
        var result = list.OrderBy(u => u.SortCode ?? 0)
            .ThenBy(u => u.IsBase == 1 ? 0 : 1)
            .Select(u => u.Adapt<UnitDefinitionDto>())
            .ToList();

        // 写入缓存（6小时过期）
        if (result.Count > 0)
        {
            await _cacheManager.SetAsync(cacheKey, result, TimeSpan.FromHours(6));
        }

        return result;
    }

    /// <inheritdoc />
    [HttpGet("{id}")]
    public async Task<UnitDefinitionDto> GetInfo(string id)
    {
        var cacheKey = GetCacheKey($"info:{id}");

        // 尝试从缓存获取
        var cached = await _cacheManager.GetAsync<UnitDefinitionDto>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var entity = await _repository.GetFirstAsync(u =>
            u.Id == id && (u.DeleteMark == 0 || u.DeleteMark == null)
        );
        if (entity == null)
            throw Oops.Oh("单位定义不存在");

        var result = entity.Adapt<UnitDefinitionDto>();

        // 写入缓存（6小时过期）
        await _cacheManager.SetAsync(cacheKey, result, TimeSpan.FromHours(6));

        return result;
    }

    /// <inheritdoc />
    [HttpPost]
    [UnitOfWork]
    public async Task Create([FromBody] UnitDefinitionInput input)
    {
        // 验证维度是否存在
        var category = await _categoryRepository.GetFirstAsync(c =>
            c.Id == input.CategoryId && (c.DeleteMark == 0 || c.DeleteMark == null)
        );
        if (category == null)
            throw Oops.Oh("单位维度不存在");

        // 如果设置为基准单位，检查该维度是否已有基准单位
        if (input.IsBase)
        {
            var existingBase = await _repository
                .AsQueryable()
                .Where(u =>
                    u.CategoryId == input.CategoryId
                    && u.IsBase == 1
                    && (u.DeleteMark == 0 || u.DeleteMark == null)
                )
                .AnyAsync();
            if (existingBase)
                throw Oops.Oh("该维度已存在基准单位，每个维度只能有一个基准单位");
        }

        var entity = input.Adapt<UnitDefinitionEntity>();
        entity.IsBase = input.IsBase ? 1 : 0;
        entity.Creator();

        var isOk = await _repository.InsertAsync(entity);
        if (!isOk)
            throw Oops.Oh("创建失败");

        // 清除缓存
        await ClearCacheAsync(input.CategoryId);
    }

    /// <inheritdoc />
    [HttpPut("{id}")]
    [UnitOfWork]
    public async Task Update(string id, [FromBody] UnitDefinitionInput input)
    {
        var entity = await _repository.GetFirstAsync(u =>
            u.Id == id && (u.DeleteMark == 0 || u.DeleteMark == null)
        );
        if (entity == null)
            throw Oops.Oh("单位定义不存在");

        // 验证维度是否存在
        var category = await _categoryRepository.GetFirstAsync(c =>
            c.Id == input.CategoryId && (c.DeleteMark == 0 || c.DeleteMark == null)
        );
        if (category == null)
            throw Oops.Oh("单位维度不存在");

        // 记录是否更换了基准单位
        bool isChangingBaseUnit = false;
        UnitDefinitionEntity oldBaseUnit = null;
        decimal newBaseUnitOldScale = 0; // 新基准单位原来的换算比例（相对于旧基准单位）

        // 如果设置为基准单位，需要检查目标维度是否已有其他基准单位
        if (input.IsBase)
        {
            // 检查目标维度（可能是新维度，如果修改了CategoryId）是否已有基准单位
            var existingBase = await _repository
                .AsQueryable()
                .Where(u =>
                    u.CategoryId == input.CategoryId
                    && u.Id != id
                    && u.IsBase == 1
                    && (u.DeleteMark == 0 || u.DeleteMark == null)
                )
                .FirstAsync();

            if (existingBase != null)
            {
                // 如果当前单位不是基准单位，但要将它设置为基准单位，这是更换基准单位的情况
                if (entity.IsBase != 1)
                {
                    isChangingBaseUnit = true;
                    oldBaseUnit = existingBase;

                    // 优先使用用户明确提供的换算因子（ConversionFactor）
                    // 其次使用 ScaleToBase（非0且非1时，假设用户在修正）
                    // 最后使用数据库中的原有比例
                    if (input.ConversionFactor.HasValue && input.ConversionFactor.Value != 0)
                    {
                        newBaseUnitOldScale = input.ConversionFactor.Value;
                    }
                    else if (input.ScaleToBase != 0 && input.ScaleToBase != 1)
                    {
                        newBaseUnitOldScale = input.ScaleToBase;
                    }
                    else
                    {
                        newBaseUnitOldScale = entity.ScaleToBase;
                    }
                }
                else
                {
                    var baseUnitName = existingBase.Name;
                    throw Oops.Oh(
                        $"该维度已存在基准单位「{baseUnitName}」，每个维度只能有一个基准单位。如需更改基准单位，请先将「{baseUnitName}」的基准单位设置取消"
                    );
                }
            }
        }

        // 如果当前单位是基准单位，但更新后不再是基准单位，需要检查原维度是否还有其他单位
        if (entity.IsBase == 1 && !input.IsBase)
        {
            // 检查原维度是否还有其他单位（除了当前单位）
            var otherUnitsInCategory = await _repository
                .AsQueryable()
                .Where(u =>
                    u.CategoryId == entity.CategoryId
                    && u.Id != id
                    && (u.DeleteMark == 0 || u.DeleteMark == null)
                )
                .AnyAsync();

            if (!otherUnitsInCategory)
            {
                throw Oops.Oh(
                    "该维度只有这一个单位，不能取消基准单位设置。每个维度必须有一个基准单位"
                );
            }
        }

        // 更新当前单位
        entity.CategoryId = input.CategoryId;
        entity.Name = input.Name;
        entity.Symbol = input.Symbol;
        entity.IsBase = input.IsBase ? 1 : 0;
        entity.ScaleToBase = input.IsBase ? 1.0m : input.ScaleToBase; // 基准单位的换算比例必须是1
        entity.Offset = input.Offset;
        entity.Precision = input.Precision;
        entity.SortCode = input.SortCode;
        entity.LastModify();

        var isOk = await _repository.UpdateAsync(entity);
        if (!isOk)
            throw Oops.Oh("更新失败");

        // 如果更换了基准单位，需要重新计算该维度下所有其他单位的换算比例
        if (isChangingBaseUnit && oldBaseUnit != null)
        {
            try
            {
                await RecalculateScaleToBaseForCategoryAsync(
                    input.CategoryId,
                    entity.Id,
                    oldBaseUnit.Id,
                    newBaseUnitOldScale
                );
            }
            catch (Exception ex)
            {
                // 如果重新计算失败，回滚当前单位的更新
                throw Oops.Oh($"更换基准单位时重新计算换算比例失败：{ex.Message}");
            }
        }

        // 清除缓存
        await ClearCacheAsync(input.CategoryId, id);
    }

    /// <inheritdoc />
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _repository.GetFirstAsync(u =>
            u.Id == id && (u.DeleteMark == 0 || u.DeleteMark == null)
        );
        if (entity == null)
            throw Oops.Oh("单位定义不存在");

        // 如果是基准单位，不允许删除
        if (entity.IsBase == 1)
            throw Oops.Oh("基准单位不能删除，请先设置其他单位为基准单位");

        entity.Delete();

        var isOk = await _repository.UpdateAsync(entity);
        if (!isOk)
            throw Oops.Oh("删除失败");

        // 清除缓存
        await ClearCacheAsync(entity.CategoryId, id);
    }

    /// <summary>
    /// 当基准单位更换时，重新计算该维度下所有其他单位的换算比例.
    /// </summary>
    /// <param name="categoryId">维度ID.</param>
    /// <param name="newBaseUnitId">新的基准单位ID.</param>
    /// <param name="oldBaseUnitId">旧的基准单位ID.</param>
    /// <param name="newBaseUnitOldScale">新基准单位原来的换算比例（相对于旧基准单位）.</param>
    private async Task RecalculateScaleToBaseForCategoryAsync(
        string categoryId,
        string newBaseUnitId,
        string oldBaseUnitId,
        decimal newBaseUnitOldScale
    )
    {
        // 获取新基准单位（此时它的 ScaleToBase 已经是 1）
        var newBaseUnit = await _repository.GetByIdAsync(newBaseUnitId);
        if (newBaseUnit == null || newBaseUnit.IsBase != 1)
            throw Oops.Oh($"新基准单位不存在或未正确设置为基准单位：{newBaseUnitId}");

        // 获取旧基准单位
        var oldBaseUnit = await _repository.GetByIdAsync(oldBaseUnitId);
        if (oldBaseUnit == null)
            throw Oops.Oh($"旧基准单位不存在：{oldBaseUnitId}");

        // 验证新基准单位原来的换算比例不能为0
        if (newBaseUnitOldScale == 0)
            throw Oops.Oh($"新基准单位原来的换算比例不能为0");

        // 计算新旧基准单位之间的换算关系
        // 新基准单位原来的 ScaleToBase = newBaseUnitOldScale（相对于旧基准单位）
        // 这意味着：1 新基准单位 = newBaseUnitOldScale 旧基准单位
        // 所以：1 旧基准单位 = 1/newBaseUnitOldScale 新基准单位
        decimal oldBaseToNewBaseRatio = 1.0m / newBaseUnitOldScale;

        // 获取该维度下所有其他单位（不包括新基准单位）
        var otherUnits = await _repository
            .AsQueryable()
            .Where(u =>
                u.CategoryId == categoryId
                && u.Id != newBaseUnitId
                && (u.DeleteMark == 0 || u.DeleteMark == null)
            )
            .ToListAsync();

        foreach (var unit in otherUnits)
        {
            // 计算该单位相对于新基准单位的换算比例
            // 步骤：
            // 1. 该单位相对于旧基准单位的比例：unit.ScaleToBase（旧值）
            // 2. 旧基准单位相对于新基准单位的比例：oldBaseToNewBaseRatio
            // 3. 该单位相对于新基准单位的比例 = unit.ScaleToBase * oldBaseToNewBaseRatio

            decimal newScaleToBase = unit.ScaleToBase * oldBaseToNewBaseRatio;

            // 更新该单位的换算比例
            unit.ScaleToBase = newScaleToBase;
            unit.LastModify();
            await _repository.UpdateAsync(unit);

            // 清除该单位的详情缓存
            await _cacheManager.DelAsync(GetCacheKey($"info:{unit.Id}"));
        }

        // 更新旧基准单位：它现在不再是基准单位，需要计算它相对于新基准单位的比例
        oldBaseUnit.IsBase = 0;
        oldBaseUnit.ScaleToBase = oldBaseToNewBaseRatio;
        oldBaseUnit.LastModify();
        await _repository.UpdateAsync(oldBaseUnit);

        // 清除旧基准单位的详情缓存
        await _cacheManager.DelAsync(GetCacheKey($"info:{oldBaseUnitId}"));
    }

    /// <summary>
    /// 清除缓存
    /// </summary>
    /// <param name="categoryId">维度ID，用于清除按维度过滤的列表缓存</param>
    /// <param name="id">可选的ID，用于清除特定记录的缓存</param>
    private async Task ClearCacheAsync(string? categoryId = null, string? id = null)
    {
        // 清除全部列表缓存
        await _cacheManager.DelAsync(GetCacheKey("list:all"));

        // 如果指定了维度ID，清除该维度的列表缓存
        if (!string.IsNullOrEmpty(categoryId))
        {
            await _cacheManager.DelAsync(GetCacheKey($"list:{categoryId}"));
        }

        // 如果指定了ID，清除该记录的详情缓存
        if (!string.IsNullOrEmpty(id))
        {
            await _cacheManager.DelAsync(GetCacheKey($"info:{id}"));
        }
    }
}
