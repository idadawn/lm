using Microsoft.AspNetCore.Mvc;
using Poxiao.DatabaseAccessor;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Enums;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Interfaces;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// 公共维度服务.
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "public-dimensions", Order = 103)]
[Route("api/lab/public-dimensions")]
public class PublicDimensionService : IPublicDimensionService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<PublicDimensionEntity> _repository;
    private readonly ISqlSugarRepository<PublicDimensionVersionEntity> _versionRepository;
    private readonly ISqlSugarRepository<UnitDefinitionEntity> _unitRepository;

    public PublicDimensionService(
        ISqlSugarRepository<PublicDimensionEntity> repository,
        ISqlSugarRepository<PublicDimensionVersionEntity> versionRepository,
        ISqlSugarRepository<UnitDefinitionEntity> unitRepository)
    {
        _repository = repository;
        _versionRepository = versionRepository;
        _unitRepository = unitRepository;
    }

    /// <inheritdoc />
    [HttpGet("")]
    public async Task<List<PublicDimensionEntity>> GetPublicDimensions()
    {
        var list = await _repository
            .AsQueryable()
            .Where(t => t.DeleteMark == null)
            .OrderBy(t => t.SortCode)
            .OrderBy(t => t.CreatorTime)
            .ToListAsync();

        // 填充单位信息
        foreach (var item in list)
        {
            if (!string.IsNullOrEmpty(item.UnitId))
            {
                var unit = await _unitRepository.GetFirstAsync(u => u.Id == item.UnitId && u.DeleteMark == null);
                if (unit != null)
                {
                    item.Unit = unit.Symbol;
                }
            }
        }

        return list;
    }

    /// <inheritdoc />
    [HttpGet("{id}")]
    public async Task<PublicDimensionEntity> GetById(string id)
    {
        var entity = await _repository.GetFirstAsync(t => t.Id == id && t.DeleteMark == null);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);

        // 填充单位信息
        if (!string.IsNullOrEmpty(entity.UnitId))
        {
            var unit = await _unitRepository.GetFirstAsync(u => u.Id == entity.UnitId && u.DeleteMark == null);
            if (unit != null)
            {
                entity.Unit = unit.Symbol;
            }
        }

        return entity;
    }

    /// <inheritdoc />
    [HttpPost("")]
    public async Task Create([FromBody] PublicDimensionEntity entity)
    {
        // 检查维度键名是否已存在
        var exists = await _repository
            .AsQueryable()
            .Where(t => t.DimensionKey == entity.DimensionKey && t.DeleteMark == null)
            .AnyAsync();
        
        if (exists)
            throw Oops.Oh(ErrorCode.COM1003, $"维度键名 {entity.DimensionKey} 已存在");

        // 如果排序码未设置，自动生成
        if (entity.SortCode == null || entity.SortCode == 0)
        {
            entity.SortCode = await GetNextSortCodeAsync();
        }

        // 如果指定了单位ID，获取单位符号
        if (!string.IsNullOrEmpty(entity.UnitId))
        {
            var unit = await _unitRepository.GetFirstAsync(u => u.Id == entity.UnitId && u.DeleteMark == null);
            if (unit != null)
            {
                entity.Unit = unit.Symbol;
            }
        }

        entity.Creator();
        entity.LastModifyUserId = entity.CreatorUserId;
        entity.LastModifyTime = entity.CreatorTime;

        var isOk = await _repository
            .AsInsertable(entity)
            .IgnoreColumns(ignoreNullColumn: true)
            .ExecuteCommandAsync();
        
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);

        // 创建初始版本
        await CreateInitialVersionAsync(entity.Id, "初始版本");
    }

    /// <inheritdoc />
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] PublicDimensionEntity entity)
    {
        var existing = await _repository.GetFirstAsync(t => t.Id == id && t.DeleteMark == null);
        if (existing == null)
            throw Oops.Oh(ErrorCode.COM1005);

        // 如果维度键名改变，检查新键名是否已存在
        if (existing.DimensionKey != entity.DimensionKey)
        {
            var keyExists = await _repository
                .AsQueryable()
                .Where(t => t.DimensionKey == entity.DimensionKey && t.Id != id && t.DeleteMark == null)
                .AnyAsync();
            
            if (keyExists)
                throw Oops.Oh(ErrorCode.COM1003, $"维度键名 {entity.DimensionKey} 已存在");
        }

        // 如果排序码未设置或为0，自动生成
        if (entity.SortCode == null || entity.SortCode == 0)
        {
            entity.SortCode = await GetNextSortCodeAsync();
        }

        // 如果指定了单位ID，获取单位符号
        if (!string.IsNullOrEmpty(entity.UnitId))
        {
            var unit = await _unitRepository.GetFirstAsync(u => u.Id == entity.UnitId && u.DeleteMark == null);
            if (unit != null)
            {
                entity.Unit = unit.Symbol;
            }
        }

        // 检查是否需要创建新版本（如果关键信息发生变化）
        bool needNewVersion = existing.UnitId != entity.UnitId 
            || existing.Precision != entity.Precision 
            || existing.ValueType != entity.ValueType;

        entity.Id = id;
        entity.LastModify();

        var isOk = await _repository
            .AsUpdateable(entity)
            .IgnoreColumns(ignoreAllNullColumns: true)
            .ExecuteCommandHasChangeAsync();
        
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);

        // 如果需要，创建新版本
        if (needNewVersion)
        {
            await CreateNewVersionAsync(id, "维度信息更新");
        }
    }

    /// <inheritdoc />
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _repository.GetFirstAsync(t => t.Id == id && t.DeleteMark == null);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);

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
    }

    /// <inheritdoc />
    [HttpGet("current-version")]
    public async Task<int> GetCurrentVersionAsync(string dimensionId)
    {
        var currentVersion = await _versionRepository
            .AsQueryable()
            .Where(v =>
                v.DimensionId == dimensionId && v.IsCurrent == 1 && v.DeleteMark == null
            )
            .FirstAsync();

        if (currentVersion == null)
            return 1;
        return currentVersion.Version;
    }

    /// <inheritdoc />
    [HttpPost("create-version")]
    public async Task<int> CreateNewVersionAsync([FromQuery] string dimensionId, [FromQuery] string versionDescription = null)
    {
        // 1. 获取当前版本号
        var currentVersion = await GetCurrentVersionAsync(dimensionId);
        
        // 如果没有版本记录（返回1表示默认值），创建初始版本
        var existingVersion = await _versionRepository
            .AsQueryable()
            .Where(v => v.DimensionId == dimensionId && v.IsCurrent == 1 && v.DeleteMark == null)
            .FirstAsync();
        
        if (existingVersion == null)
        {
            // 没有版本记录，创建初始版本1
            return await CreateInitialVersionAsync(dimensionId, versionDescription ?? "初始版本");
        }
        
        var newVersion = currentVersion + 1;

        // 2. 将当前版本标记为非当前版本
        await _versionRepository
            .AsUpdateable()
            .SetColumns(v => new PublicDimensionVersionEntity { IsCurrent = 0 })
            .Where(v => v.DimensionId == dimensionId && v.IsCurrent == 1)
            .ExecuteCommandAsync();

        // 3. 创建新版本记录
        var newVersionEntity = new PublicDimensionVersionEntity
        {
            Id = Guid.NewGuid().ToString(),
            DimensionId = dimensionId,
            Version = newVersion,
            VersionName = $"v{newVersion}.0",
            VersionDescription = versionDescription,
            IsCurrent = 1,
            CreatorTime = DateTime.Now,
        };
        newVersionEntity.Creator();
        await _versionRepository.InsertAsync(newVersionEntity);

        return newVersion;
    }

    /// <inheritdoc />
    [HttpGet("version-list")]
    public async Task<List<PublicDimensionVersionEntity>> GetVersionListAsync(string dimensionId)
    {
        return await _versionRepository
            .AsQueryable()
            .Where(v => v.DimensionId == dimensionId && v.DeleteMark == null)
            .OrderByDescending(v => v.Version)
            .ToListAsync();
    }

    /// <summary>
    /// 创建初始版本（第一次创建维度时调用）.
    /// </summary>
    private async Task<int> CreateInitialVersionAsync(
        string dimensionId,
        string versionDescription = "初始版本"
    )
    {
        // 检查是否已有版本记录
        var existingVersion = await _versionRepository
            .AsQueryable()
            .Where(v => v.DimensionId == dimensionId && v.DeleteMark == null)
            .FirstAsync();

        if (existingVersion != null)
        {
            // 如果已有版本，返回当前版本号
            return existingVersion.Version;
        }

        // 创建版本1
        var initialVersionEntity = new PublicDimensionVersionEntity
        {
            Id = Guid.NewGuid().ToString(),
            DimensionId = dimensionId,
            Version = 1,
            VersionName = "v1.0",
            VersionDescription = versionDescription,
            IsCurrent = 1,
            CreatorTime = DateTime.Now,
        };
        initialVersionEntity.Creator();
        await _versionRepository.InsertAsync(initialVersionEntity);

        return 1;
    }

    /// <summary>
    /// 获取下一个可用的排序码
    /// </summary>
    private async Task<long> GetNextSortCodeAsync()
    {
        var maxSortCode = await _repository
            .AsQueryable()
            .Where(t => t.DeleteMark == null)
            .MaxAsync(t => t.SortCode);
        
        return (maxSortCode ?? 0) + 1;
    }
}
