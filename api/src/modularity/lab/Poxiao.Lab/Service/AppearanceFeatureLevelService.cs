using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Manager;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.AppearanceFeatureLevel;
using Poxiao.Lab.Interfaces;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// 外观特性等级服务.
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "appearance-feature-levels", Order = 102)]
[Route("api/lab/appearance-feature-levels")]
public class AppearanceFeatureLevelService
    : IAppearanceFeatureLevelService,
        IDynamicApiController,
        ITransient
{
    private readonly ISqlSugarRepository<AppearanceFeatureLevelEntity> _repository;
    private readonly ISqlSugarRepository<AppearanceFeatureEntity> _featureRepository;
    private readonly ICacheManager _cacheManager;
    private readonly IUserManager _userManager;

    private const string CachePrefix = "LAB:AppearanceFeatureLevel";

    public AppearanceFeatureLevelService(
        ISqlSugarRepository<AppearanceFeatureLevelEntity> repository,
        ISqlSugarRepository<AppearanceFeatureEntity> featureRepository,
        ICacheManager cacheManager,
        IUserManager userManager
    )
    {
        _repository = repository;
        _featureRepository = featureRepository;
        _cacheManager = cacheManager;
        _userManager = userManager;
    }

    private string GetCacheKey(string suffix)
    {
        var tenantId = _userManager?.TenantId ?? "global";
        return $"{CachePrefix}:{tenantId}:{suffix}";
    }

    /// <inheritdoc />
    [HttpGet("")]
    public async Task<List<AppearanceFeatureLevelListOutput>> GetList(
        [FromQuery] AppearanceFeatureLevelListQuery input
    )
    {
        var data = await _repository
            .AsQueryable()
            .WhereIF(
                !string.IsNullOrEmpty(input.Keyword),
                t =>
                    t.Name.Contains(input.Keyword)
                    || (
                        !string.IsNullOrEmpty(t.Description)
                        && t.Description.Contains(input.Keyword)
                    )
            )
            .WhereIF(input.Enabled.HasValue, t => t.Enabled == input.Enabled.Value)
            .Where(t => t.DeleteMark == null)
            .OrderBy(t => t.SortCode)
            .ToListAsync();

        // 如果SortCode相同，按Name排序（在内存中排序）
        data = data.OrderBy(x => x.SortCode ?? 0).ThenBy(x => x.Name).ToList();

        return data.Adapt<List<AppearanceFeatureLevelListOutput>>();
    }

    /// <inheritdoc />
    [HttpGet("{id}")]
    public async Task<AppearanceFeatureLevelInfoOutput> GetInfo(string id)
    {
        var cacheKey = GetCacheKey($"info:{id}");
        var cached = await _cacheManager.GetAsync<AppearanceFeatureLevelInfoOutput>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var entity = await _repository.GetFirstAsync(t => t.Id == id && t.DeleteMark == null);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);

        var result = entity.Adapt<AppearanceFeatureLevelInfoOutput>();
        await _cacheManager.SetAsync(cacheKey, result, TimeSpan.FromHours(6));
        return result;
    }

    /// <inheritdoc />
    [HttpPost("")]
    public async Task Create([FromBody] AppearanceFeatureLevelCrInput input)
    {
        // 检查名称是否重复（唯一性验证）
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw Oops.Bah("等级名称不能为空");
        }

        // 使用局部变量避免闭包问题，确保 SqlSugar 正确解析表达式
        var nameToCheck = input.Name;
        var exists = await _repository
            .AsQueryable()
            .Where(t => t.Name == nameToCheck && t.DeleteMark == null)
            .AnyAsync();

        if (exists)
        {
            throw Oops.Bah($"外观特性等级名称 '{input.Name}' 已存在，不能重复");
        }

        var entity = input.Adapt<AppearanceFeatureLevelEntity>();
        // 显式设置 IsDefault 和 Enabled 字段，确保布尔值正确设置
        entity.IsDefault = input.IsDefault;
        entity.Enabled = input.Enabled;

        // 先调用 Creator() 设置创建信息
        entity.Creator();
        entity.LastModifyUserId = entity.CreatorUserId;
        entity.LastModifyTime = entity.CreatorTime;

        // 如果设置为默认，需要将其他所有项设为非默认（批量更新）
        if (entity.IsDefault)
        {
            var currentUserId = entity.CreatorUserId;
            var currentTime = DateTime.Now;

            await _repository
                .AsSugarClient()
                .Updateable<AppearanceFeatureLevelEntity>()
                .SetColumns(it => new AppearanceFeatureLevelEntity
                {
                    IsDefault = false,
                    LastModifyTime = currentTime,
                    LastModifyUserId = currentUserId,
                })
                .Where(t => t.IsDefault == true && t.DeleteMark == null)
                .ExecuteCommandAsync();
        }

        var isOk = await _repository.AsInsertable(entity).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);

        await ClearCacheAsync();
    }

    /// <inheritdoc />
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] AppearanceFeatureLevelUpInput input)
    {
        // 检查名称是否重复（排除自身）
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw Oops.Bah("等级名称不能为空");
        }

        // 使用局部变量避免闭包问题，确保 SqlSugar 正确解析表达式
        var nameToCheck = input.Name;
        var currentId = id;
        var exists = await _repository
            .AsQueryable()
            .Where(t => t.Name == nameToCheck && t.Id != currentId && t.DeleteMark == null)
            .AnyAsync();

        if (exists)
        {
            throw Oops.Bah($"外观特性等级名称 '{input.Name}' 已存在，不能重复");
        }

        // 先获取原有实体，保留 CreatorUserId 等字段
        var existingEntity = await _repository.GetFirstAsync(t =>
            t.Id == id && t.DeleteMark == null
        );
        if (existingEntity == null)
            throw Oops.Oh(ErrorCode.COM1005);

        // 将输入数据映射到现有实体
        var entity = input.Adapt(existingEntity);
        // 显式设置 IsDefault 字段，确保布尔值正确更新
        entity.IsDefault = input.IsDefault;
        entity.Enabled = input.Enabled;

        // 先调用 LastModify() 设置修改信息
        entity.LastModify();

        // 如果设置为默认，需要将其他所有项设为非默认（排除当前项，批量更新）
        // 这样可以确保数据一致性，即使当前项已经是默认，也能处理数据不一致的情况
        if (input.IsDefault)
        {
            var currentUserId = entity.LastModifyUserId;
            var currentTime = entity.LastModifyTime ?? DateTime.Now;

            await _repository
                .AsSugarClient()
                .Updateable<AppearanceFeatureLevelEntity>()
                .SetColumns(it => new AppearanceFeatureLevelEntity
                {
                    IsDefault = false,
                    LastModifyTime = currentTime,
                    LastModifyUserId = currentUserId,
                })
                .Where(t => t.IsDefault == true && t.Id != currentId && t.DeleteMark == null)
                .ExecuteCommandAsync();
        }

        // 只更新指定字段，避免更新 CreatorUserId 等不应该更新的字段
        var isOk = await _repository
            .AsUpdateable(entity)
            .UpdateColumns(it => new
            {
                it.Name,
                it.Description,
                it.Enabled,
                it.IsDefault,
                it.SortCode,
                it.LastModifyTime,
                it.LastModifyUserId,
            })
            .ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);

        await ClearCacheAsync(id);
    }

    /// <inheritdoc />
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _repository.GetFirstAsync(t => t.Id == id && t.DeleteMark == null);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);

        // 检查是否被使用
        var isUsed = await _featureRepository
            .AsQueryable()
            .Where(t => t.SeverityLevelId == id && t.DeleteMark == null)
            .AnyAsync();

        if (isUsed)
        {
            throw Oops.Bah("该等级已被使用，请先解除引用");
        }

        entity.Delete();
        var isOk = await _repository
            .AsUpdateable(entity)
            .UpdateColumns(it => new
            {
                it.DeleteMark,
                it.DeleteTime,
                it.DeleteUserId,
            })
            .ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);

        await ClearCacheAsync(id);
    }

    /// <inheritdoc />
    [HttpGet("enabled")]
    public async Task<List<AppearanceFeatureLevelListOutput>> GetEnabledLevels()
    {
        var cacheKey = GetCacheKey("enabled:list");
        var cached = await _cacheManager.GetAsync<List<AppearanceFeatureLevelListOutput>>(cacheKey);
        if (cached != null && cached.Count > 0)
        {
            return cached;
        }

        var data = await _repository
            .AsQueryable()
            .Where(t => t.Enabled == true && t.DeleteMark == null)
            .OrderBy(t => t.SortCode)
            .ToListAsync();

        // 如果SortCode相同，按Name排序（在内存中排序）
        data = data.OrderBy(x => x.SortCode ?? 0).ThenBy(x => x.Name).ToList();

        var result = data.Adapt<List<AppearanceFeatureLevelListOutput>>();
        if (result.Count > 0)
        {
            await _cacheManager.SetAsync(cacheKey, result, TimeSpan.FromHours(6));
        }
        return result;
    }

    private async Task ClearCacheAsync(string? id = null)
    {
        // 清除已启用列表缓存
        await _cacheManager.DelAsync(GetCacheKey("enabled:list"));

        // 如果指定了ID，清除该记录的详情缓存
        if (!string.IsNullOrEmpty(id))
        {
            await _cacheManager.DelAsync(GetCacheKey($"info:{id}"));
        }
    }
}
