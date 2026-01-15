using Microsoft.AspNetCore.Mvc;
using Poxiao.DatabaseAccessor;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.Lab.Entity.Entity;
using Poxiao.Lab.Interfaces;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// 产品规格扩展属性服务.
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "product-spec-attributes", Order = 101)]
[Route("api/lab/product-spec-attributes")]
public class ProductSpecAttributeService : IProductSpecAttributeService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<ProductSpecAttributeEntity> _repository;

    public ProductSpecAttributeService(ISqlSugarRepository<ProductSpecAttributeEntity> repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    [HttpGet("by-product-spec/{productSpecId}")]
    public async Task<List<ProductSpecAttributeEntity>> GetAttributesByProductSpecId(string productSpecId)
    {
        return await _repository
            .AsQueryable()
            .Where(t => t.ProductSpecId == productSpecId && t.DeleteMark == null)
            .OrderBy(t => t.SortCode)
            .OrderBy(t => t.CreatorTime)
            .ToListAsync();
    }

    /// <inheritdoc />
    [HttpPost("save")]
    public async Task SaveAttributes(string productSpecId, [FromBody] List<ProductSpecAttributeEntity> attributes)
    {
        if (string.IsNullOrEmpty(productSpecId) || attributes == null)
            return;

        // 先删除该产品规格的所有现有属性（软删除）
        var existingAttributes = await _repository
            .AsQueryable()
            .Where(t => t.ProductSpecId == productSpecId && t.DeleteMark == null)
            .ToListAsync();

        foreach (var existing in existingAttributes)
        {
            existing.Delete();
            await _repository.AsUpdateable(existing)
                .UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId })
                .ExecuteCommandAsync();
        }

        // 插入新属性
        foreach (var attr in attributes)
        {
            attr.ProductSpecId = productSpecId;
            attr.Creator();
            attr.LastModifyUserId = attr.CreatorUserId;
            attr.LastModifyTime = attr.CreatorTime;
        }

        if (attributes.Count > 0)
        {
            await _repository.InsertRangeAsync(attributes);
        }
    }

    /// <inheritdoc />
    [HttpDelete("by-product-spec/{productSpecId}")]
    public async Task DeleteAttributesByProductSpecId(string productSpecId)
    {
        var attributes = await _repository
            .AsQueryable()
            .Where(t => t.ProductSpecId == productSpecId && t.DeleteMark == null)
            .ToListAsync();

        foreach (var attr in attributes)
        {
            attr.Delete();
            await _repository.AsUpdateable(attr)
                .UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId })
                .ExecuteCommandAsync();
        }
    }
}
