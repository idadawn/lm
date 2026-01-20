using Mapster;
using Microsoft.AspNetCore.Mvc;
using Poxiao.DatabaseAccessor;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.Lab.Entity;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// 产品规格版本管理服务.
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "product-spec-versions", Order = 101)]
[Route("api/lab/product-spec-versions")]
public class ProductSpecVersionService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<ProductSpecVersionEntity> _versionRepository;
    private readonly ISqlSugarRepository<ProductSpecAttributeEntity> _attributeRepository;

    public ProductSpecVersionService(
        ISqlSugarRepository<ProductSpecVersionEntity> versionRepository,
        ISqlSugarRepository<ProductSpecAttributeEntity> attributeRepository
    )
    {
        _versionRepository = versionRepository;
        _attributeRepository = attributeRepository;
    }

    /// <summary>
    /// 获取产品规格的当前版本号.
    /// </summary>
    [HttpGet("current-version")]
    public async Task<int> GetCurrentVersionAsync([FromQuery] string productSpecId)
    {
        var currentVersion = await _versionRepository
            .AsQueryable()
            .Where(v =>
                v.ProductSpecId == productSpecId && v.IsCurrent == 1 && v.DeleteMark == null
            )
            .FirstAsync();

        if (currentVersion == null)
            return 1;
        return currentVersion.Version;
    }

    /// <summary>
    /// 创建初始版本（第一次创建产品规格时调用）.
    /// </summary>
    public async Task<int> CreateInitialVersionAsync(
        string productSpecId,
        string versionDescription = "初始版本"
    )
    {
        // 检查是否已有版本记录
        var existingVersion = await _versionRepository
            .AsQueryable()
            .Where(v => v.ProductSpecId == productSpecId && v.DeleteMark == null)
            .FirstAsync();

        if (existingVersion != null)
        {
            // 如果已有版本，返回当前版本号
            return existingVersion.Version;
        }

        // 创建版本1
        var initialVersionEntity = new ProductSpecVersionEntity
        {
            Id = Guid.NewGuid().ToString(),
            ProductSpecId = productSpecId,
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
    /// 创建新版本（当扩展信息修改时调用）.
    /// </summary>
    public async Task<int> CreateNewVersionAsync(
        string productSpecId,
        string versionDescription = null
    )
    {
        // 1. 获取当前版本号
        var currentVersion = await GetCurrentVersionAsync(productSpecId);
        
        // 如果没有版本记录（返回1表示默认值），创建初始版本
        var existingVersion = await _versionRepository
            .AsQueryable()
            .Where(v => v.ProductSpecId == productSpecId && v.IsCurrent == 1 && v.DeleteMark == null)
            .FirstAsync();
        
        if (existingVersion == null)
        {
            // 没有版本记录，创建初始版本1
            return await CreateInitialVersionAsync(productSpecId, versionDescription ?? "初始版本");
        }
        
        var newVersion = currentVersion + 1;

        // 2. 将当前版本标记为非当前版本
        await _versionRepository
            .AsUpdateable()
            .SetColumns(v => new ProductSpecVersionEntity { IsCurrent = 0 })
            .Where(v => v.ProductSpecId == productSpecId && v.IsCurrent == 1)
            .ExecuteCommandAsync();

        // 3. 创建新版本记录
        var newVersionEntity = new ProductSpecVersionEntity
        {
            Id = Guid.NewGuid().ToString(),
            ProductSpecId = productSpecId,
            Version = newVersion,
            VersionName = $"v{newVersion}.0",
            VersionDescription = versionDescription,
            IsCurrent = 1,
            CreatorTime = DateTime.Now,
        };
        newVersionEntity.Creator(); // 设置创建人信息
        await _versionRepository.InsertAsync(newVersionEntity);

        // 注意：不再自动复制属性，新版本的属性由用户在前端提交时提供
        // 这样可以避免重复数据，用户可以选择性地更新属性

        return newVersion;
    }

    /// <summary>
    /// 复制属性到新版本.
    /// </summary>
    private async Task CopyAttributesToNewVersionAsync(
        string productSpecId,
        int sourceVersion,
        int targetVersion
    )
    {
        var sourceAttributes = await _attributeRepository
            .AsQueryable()
            .Where(a =>
                a.ProductSpecId == productSpecId
                && a.Version == sourceVersion
                && a.DeleteMark == null
            )
            .ToListAsync();

        if (sourceAttributes.Count == 0)
            return;

        var newAttributes = sourceAttributes
            .Select(a =>
            {
                var newAttr = a.Adapt<ProductSpecAttributeEntity>();
                newAttr.Id = Guid.NewGuid().ToString();
                newAttr.Version = targetVersion;
                newAttr.VersionCreateTime = DateTime.Now;
                newAttr.CreatorTime = DateTime.Now;
                // 重置其他审计字段
                newAttr.LastModifyTime = null;
                newAttr.LastModifyUserId = null;
                newAttr.DeleteMark = null;
                newAttr.DeleteTime = null;
                newAttr.DeleteUserId = null;
                newAttr.Creator();
                return newAttr;
            })
            .ToList();

        await _attributeRepository.InsertRangeAsync(newAttributes);
    }

    /// <summary>
    /// 获取指定版本的扩展属性.
    /// </summary>
    [HttpGet("attributes-by-version")]
    public async Task<List<ProductSpecAttributeEntity>> GetAttributesByVersionAsync(
        [FromQuery] string productSpecId,
        [FromQuery] int? version = null
    )
    {
        // 如果没有指定版本，使用当前版本
        if (!version.HasValue)
        {
            version = await GetCurrentVersionAsync(productSpecId);
        }

        return await _attributeRepository
            .AsQueryable()
            .Where(a =>
                a.ProductSpecId == productSpecId
                && a.Version == version.Value
                && a.DeleteMark == null
            )
            .OrderBy(a => a.SortCode)
            .ToListAsync();
    }

    /// <summary>
    /// 获取所有版本列表.
    /// </summary>
    [HttpGet("version-list")]
    public async Task<List<ProductSpecVersionEntity>> GetVersionListAsync(
        [FromQuery] string productSpecId
    )
    {
        return await _versionRepository
            .AsQueryable()
            .Where(v => v.ProductSpecId == productSpecId && v.DeleteMark == null)
            .OrderByDescending(v => v.Version)
            .ToListAsync();
    }

    /// <summary>
    /// 对比两个版本的差异.
    /// </summary>
    [HttpGet("compare")]
    public async Task<object> CompareVersions(
        [FromQuery] string productSpecId,
        [FromQuery] int version1,
        [FromQuery] int version2
    )
    {
        var attrs1 = await GetAttributesByVersionAsync(productSpecId, version1);
        var attrs2 = await GetAttributesByVersionAsync(productSpecId, version2);

        // 对比逻辑：找出新增、删除、修改的属性
        var comparison = new
        {
            Version1 = version1,
            Version2 = version2,
            Added = attrs2
                .Where(a2 => !attrs1.Any(a1 => a1.AttributeKey == a2.AttributeKey))
                .ToList(),
            Removed = attrs1
                .Where(a1 => !attrs2.Any(a2 => a2.AttributeKey == a1.AttributeKey))
                .ToList(),
            Modified = attrs1
                .Where(a1 =>
                {
                    var a2 = attrs2.FirstOrDefault(a => a.AttributeKey == a1.AttributeKey);
                    return a2 != null && a1.AttributeValue != a2.AttributeValue;
                })
                .Select(a1 => new
                {
                    AttributeKey = a1.AttributeKey,
                    AttributeName = a1.AttributeName,
                    OldValue = a1.AttributeValue,
                    NewValue = attrs2
                        .First(a2 => a2.AttributeKey == a1.AttributeKey)
                        .AttributeValue,
                })
                .ToList(),
        };

        return comparison;
    }
}
