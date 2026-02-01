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
/// 产品规格公共属性服务.
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "product-spec-public-attributes", Order = 102)]
[Route("api/lab/product-spec-public-attributes")]
public class ProductSpecPublicAttributeService : IProductSpecPublicAttributeService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<ProductSpecPublicAttributeEntity> _repository;
    private readonly ISqlSugarRepository<ProductSpecEntity> _productSpecRepository;
    private readonly ISqlSugarRepository<ProductSpecAttributeEntity> _attributeRepository;

    public ProductSpecPublicAttributeService(
        ISqlSugarRepository<ProductSpecPublicAttributeEntity> repository,
        ISqlSugarRepository<ProductSpecEntity> productSpecRepository,
        ISqlSugarRepository<ProductSpecAttributeEntity> attributeRepository)
    {
        _repository = repository;
        _productSpecRepository = productSpecRepository;
        _attributeRepository = attributeRepository;
    }

    /// <inheritdoc />
    [HttpGet("")]
    public async Task<List<ProductSpecPublicAttributeEntity>> GetPublicAttributes()
    {
        return await _repository
            .AsQueryable()
            .Where(t => t.DeleteMark == null)
            .OrderBy(t => t.SortCode)
            .OrderBy(t => t.CreatorTime)
            .ToListAsync();
    }

    /// <inheritdoc />
    [HttpPost("")]
    public async Task Create([FromBody] ProductSpecPublicAttributeEntity entity)
    {
        // 检查属性键名是否已存在
        var exists = await _repository
            .AsQueryable()
            .Where(t => t.AttributeKey == entity.AttributeKey && t.DeleteMark == null)
            .AnyAsync();

        if (exists)
            throw Oops.Oh(ErrorCode.COM1003, $"属性键名 {entity.AttributeKey} 已存在");

        // 如果排序码未设置，自动生成
        if (entity.SortCode == null || entity.SortCode == 0)
        {
            entity.SortCode = await GetNextSortCodeAsync();
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

        // 如果是新建的公共属性，应用到所有现有产品
        await ApplyToAllProducts(entity.Id);
    }

    /// <inheritdoc />
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] ProductSpecPublicAttributeEntity entity)
    {
        var existing = await _repository.GetFirstAsync(t => t.Id == id && t.DeleteMark == null);
        if (existing == null)
            throw Oops.Oh(ErrorCode.COM1005);

        // 如果属性键名改变，检查新键名是否已存在
        if (existing.AttributeKey != entity.AttributeKey)
        {
            var keyExists = await _repository
                .AsQueryable()
                .Where(t => t.AttributeKey == entity.AttributeKey && t.Id != id && t.DeleteMark == null)
                .AnyAsync();

            if (keyExists)
                throw Oops.Oh(ErrorCode.COM1003, $"属性键名 {entity.AttributeKey} 已存在");
        }

        // 如果排序码未设置或为0，自动生成
        if (entity.SortCode == null || entity.SortCode == 0)
        {
            entity.SortCode = await GetNextSortCodeAsync();
        }

        entity.Id = id;
        entity.LastModify();

        var isOk = await _repository
            .AsUpdateable(entity)
            .IgnoreColumns(ignoreAllNullColumns: true)
            .ExecuteCommandHasChangeAsync();

        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);
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
    [HttpPost("apply-to-all/{publicAttributeId}")]
    public async Task ApplyToAllProducts(string publicAttributeId)
    {
        var publicAttr = await _repository.GetFirstAsync(t => t.Id == publicAttributeId && t.DeleteMark == null);
        if (publicAttr == null)
            throw Oops.Oh(ErrorCode.COM1005);

        // 获取所有产品规格
        var productSpecs = await _productSpecRepository
            .AsQueryable()
            .Where(t => t.DeleteMark == null)
            .ToListAsync();

        foreach (var productSpec in productSpecs)
        {
            // 检查该产品是否已有此属性（通过属性键名）
            var exists = await _attributeRepository
                .AsQueryable()
                .Where(t => t.ProductSpecId == productSpec.Id
                    && t.AttributeKey == publicAttr.AttributeKey
                    && t.DeleteMark == null)
                .AnyAsync();

            if (!exists)
            {
                // 创建属性实例
                var attribute = new ProductSpecAttributeEntity
                {
                    ProductSpecId = productSpec.Id,
                    AttributeName = publicAttr.AttributeName,
                    AttributeKey = publicAttr.AttributeKey,
                    ValueType = publicAttr.ValueType,
                    AttributeValue = publicAttr.DefaultValue,
                    Unit = publicAttr.Unit,
                    Precision = publicAttr.Precision,
                    SortCode = publicAttr.SortCode
                };

                attribute.Creator();
                attribute.LastModifyUserId = attribute.CreatorUserId;
                attribute.LastModifyTime = attribute.CreatorTime;

                await _attributeRepository.InsertAsync(attribute);
            }
        }
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
