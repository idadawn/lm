using Mapster;
using Microsoft.AspNetCore.Mvc;
using Poxiao.DatabaseAccessor;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Enums;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.ProductSpec;
using Poxiao.Lab.Entity.Extensions;
using Poxiao.Lab.Extensions;
using Poxiao.Lab.Interfaces;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// 产品规格服务.
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "product-specs", Order = 100)]
[Route("api/lab/product-specs")]
public class ProductSpecService : IProductSpecService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<ProductSpecEntity> _repository;
    private readonly ISqlSugarRepository<ProductSpecAttributeEntity> _attributeRepository;
    private readonly ISqlSugarRepository<ProductSpecPublicAttributeEntity> _publicAttributeRepository;
    private readonly ProductSpecVersionService _versionService;

    public ProductSpecService(
        ISqlSugarRepository<ProductSpecEntity> repository,
        ISqlSugarRepository<ProductSpecAttributeEntity> attributeRepository,
        ISqlSugarRepository<ProductSpecPublicAttributeEntity> publicAttributeRepository,
        ProductSpecVersionService versionService
    )
    {
        _repository = repository;
        _attributeRepository = attributeRepository;
        _publicAttributeRepository = publicAttributeRepository;
        _versionService = versionService;
    }

    /// <inheritdoc />
    [HttpGet("")]
    public async Task<List<ProductSpecListOutput>> GetList([FromQuery] ProductSpecListQuery input)
    {
        var data = await _repository
            .AsQueryable()
            .WhereIF(
                !string.IsNullOrEmpty(input.Keyword),
                t => t.Name.Contains(input.Keyword) || t.Code.Contains(input.Keyword)
            )
            .Where(t => t.DeleteMark == null)
            .OrderBy(t => t.SortCode)
            .ToListAsync();

        var result = data.Adapt<List<ProductSpecListOutput>>();

        // 批量加载扩展属性（只加载当前版本的属性）
        var specIds = result.Select(s => s.Id).ToList();
        if (specIds.Count > 0)
        {
            // 获取每个产品规格的当前版本号
            var versionMap = new Dictionary<string, int>();
            foreach (var specId in specIds)
            {
                try
                {
                    var currentVersion = await _versionService.GetCurrentVersionAsync(specId);
                    versionMap[specId] = currentVersion;
                }
                catch
                {
                    // 如果没有版本信息，默认使用版本1
                    versionMap[specId] = 1;
                }
            }

            // 只加载当前版本的属性
            var allAttributes = new List<ProductSpecAttributeEntity>();
            foreach (var kvp in versionMap)
            {
                var specId = kvp.Key;
                var version = kvp.Value;
                var attributes = await _attributeRepository
                    .AsQueryable()
                    .Where(t => t.ProductSpecId == specId && t.Version == version && t.DeleteMark == null)
                    .OrderBy(t => t.SortCode)
                    .ToListAsync();
                allAttributes.AddRange(attributes);
            }

            foreach (var spec in result)
            {
                if (versionMap.TryGetValue(spec.Id, out var version))
                {
                    spec.Attributes = allAttributes
                        .Where(a => a.ProductSpecId == spec.Id && a.Version == version)
                        .ToList();
                }
                else
                {
                    spec.Attributes = new List<ProductSpecAttributeEntity>();
                }
            }
        }

        return result;
    }

    /// <inheritdoc />
    [HttpGet("{id}")]
    public async Task<ProductSpecInfoOutput> GetInfo(string id)
    {
        var entity = await _repository.GetFirstAsync(t => t.Id == id && t.DeleteMark == null);
        var output = entity.Adapt<ProductSpecInfoOutput>();

        // 加载当前版本的扩展属性
        var currentVersion = await _versionService.GetCurrentVersionAsync(id);
        var attributes = await _versionService.GetAttributesByVersionAsync(id, currentVersion);

        // 将扩展属性添加到输出对象（用于前端展示）
        output.Attributes = attributes;

        return output;
    }

    /// <inheritdoc />
    [HttpPost("")]
    [UnitOfWork]
    public async Task Create([FromBody] ProductSpecCrInput input)
    {
        var entity = input.Adapt<ProductSpecEntity>();
        var attributes = input.Attributes ?? new List<ProductSpecAttributeEntity>();

        // 检查规格代码是否已存在
        var exists = await _repository
            .AsQueryable()
            .Where(t => t.Code == entity.Code && t.DeleteMark == null)
            .AnyAsync();
        if (exists)
            throw Oops.Oh(ErrorCode.COM1004, $"规格代码 '{entity.Code}' 已存在，不能重复添加");

        // 添加公共属性（不再硬编码核心属性）
        await AddPublicAttributes(attributes);

        // 验证计算属性
        var validationResult = ValidateAttributes(attributes);
        if (!validationResult.IsValid)
            throw Oops.Oh(ErrorCode.COM1003, validationResult.GetErrorMessage());

        entity.Creator();
        entity.LastModifyUserId = entity.CreatorUserId;
        entity.LastModifyTime = entity.CreatorTime;

        var isOk = await _repository
            .AsInsertable(entity)
            .IgnoreColumns(ignoreNullColumn: true)
            .ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);

        // 保存扩展属性
        if (attributes.Count > 0)
        {
            foreach (var attr in attributes)
            {
                attr.ProductSpecId = entity.Id;
                attr.Version = 1; // 初始版本为1
                attr.VersionCreateTime = entity.CreatorTime;
                attr.Creator();
                attr.LastModifyUserId = attr.CreatorUserId;
                attr.LastModifyTime = attr.CreatorTime;
            }
            await _attributeRepository.InsertRangeAsync(attributes);

            // 创建初始版本快照（版本1）
            await _versionService.CreateInitialVersionAsync(entity.Id, "初始版本");
        }
    }

    /// <inheritdoc />
    [HttpPut("{id}")]
    [UnitOfWork]
    public async Task Update(string id, [FromBody] ProductSpecUpInput input)
    {
        var entity = input.Adapt<ProductSpecEntity>();
        var attributes = input.Attributes ?? new List<ProductSpecAttributeEntity>();

        // 检查规格代码是否已被其他记录使用
        var exists = await _repository
            .AsQueryable()
            .Where(t => t.Code == entity.Code && t.Id != id && t.DeleteMark == null)
            .AnyAsync();
        if (exists)
            throw Oops.Oh(
                ErrorCode.COM1003,
                $"规格代码 '{entity.Code}' 已被其他产品规格使用，不能重复"
            );

        entity.LastModify();

        // 添加公共属性（如果还没有，不再硬编码核心属性）
        await AddPublicAttributes(attributes);

        // 验证计算属性
        var validationResult = ValidateAttributes(attributes);
        if (!validationResult.IsValid)
            throw Oops.Oh(ErrorCode.COM1003, validationResult.GetErrorMessage());

        var isOk = await _repository
            .AsUpdateable(entity)
            .IgnoreColumns(ignoreAllNullColumns: true)
            .ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);

        // 根据用户选择决定是否创建新版本
        if (input.CreateNewVersion == true)
        {
            // 用户选择创建新版本
            var versionDescription = input.VersionDescription?.Trim();
            if (string.IsNullOrEmpty(versionDescription))
            {
                versionDescription = $"扩展信息更新：{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            }

            var newVersion = await _versionService.CreateNewVersionAsync(id, versionDescription);

            // 插入新版本的属性
            if (attributes.Count > 0)
            {
                foreach (var attr in attributes)
                {
                    attr.ProductSpecId = id;
                    attr.Version = newVersion;
                    attr.VersionCreateTime = DateTime.Now;
                    attr.Creator();
                    attr.LastModifyUserId = attr.CreatorUserId;
                    attr.LastModifyTime = attr.CreatorTime;
                }
                await _attributeRepository.InsertRangeAsync(attributes);
            }
        }
        // 如果用户不选择创建新版本，不更新属性（保持当前版本不变）
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

        // 同时删除扩展属性
        var attributes = await _attributeRepository
            .AsQueryable()
            .Where(t => t.ProductSpecId == id && t.DeleteMark == null)
            .ToListAsync();

        foreach (var attr in attributes)
        {
            attr.Delete();
            await _attributeRepository
                .AsUpdateable(attr)
                .UpdateColumns(it => new
                {
                    it.DeleteMark,
                    it.DeleteTime,
                    it.DeleteUserId,
                })
                .ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 确保核心属性存在
    /// </summary>
    private void EnsureCoreAttributes(List<ProductSpecAttributeEntity> attributes)
    {
        var coreKeys = new[] { "length", "layers", "density" };
        var coreNames = new[] { "长度", "层数", "密度" };
        var coreValues = new[] { "4", "20", "7.25" };
        var coreUnits = new[] { "m", null, null };
        var coreTypes = new[] { "decimal", "int", "decimal" };
        var corePrecisions = new[] { 2, 0, 2 };

        for (int i = 0; i < coreKeys.Length; i++)
        {
            if (!attributes.Any(a => a.AttributeKey == coreKeys[i]))
            {
                attributes.Add(
                    new ProductSpecAttributeEntity
                    {
                        AttributeName = coreNames[i],
                        AttributeKey = coreKeys[i],
                        ValueType = coreTypes[i],
                        AttributeValue = coreValues[i],
                        Unit = coreUnits[i],
                        Precision = corePrecisions[i],
                        SortCode = i + 1,
                    }
                );
            }
        }
    }

    /// <summary>
    /// 添加公共属性到属性列表
    /// </summary>
    private async Task AddPublicAttributes(List<ProductSpecAttributeEntity> attributes)
    {
        // 获取所有公共属性
        var publicAttributes = await _publicAttributeRepository
            .AsQueryable()
            .Where(t => t.DeleteMark == null)
            .OrderBy(t => t.SortCode)
            .ToListAsync();

        foreach (var publicAttr in publicAttributes)
        {
            // 检查是否已存在（通过属性键名）
            if (!attributes.Any(a => a.AttributeKey == publicAttr.AttributeKey))
            {
                attributes.Add(
                    new ProductSpecAttributeEntity
                    {
                        AttributeName = publicAttr.AttributeName,
                        AttributeKey = publicAttr.AttributeKey,
                        ValueType = publicAttr.ValueType,
                        AttributeValue = publicAttr.DefaultValue,
                        Unit = publicAttr.Unit,
                        Precision = publicAttr.Precision,
                        SortCode = publicAttr.SortCode,
                    }
                );
            }
        }
    }

    /// <summary>
    /// 验证扩展属性
    /// </summary>
    private ValidationResult ValidateAttributes(List<ProductSpecAttributeEntity> attributes)
    {
        var result = new ValidationResult();
        // 目前不需要验证，因为已经移除了最小最大值
        return result;
    }

    /// <summary>
    /// 属性变更信息（已废弃，现在由用户手动决定是否创建新版本）
    /// </summary>
    [Obsolete("现在由用户手动决定是否创建新版本，此方法已不再使用")]
    private class AttributeChangeInfo
    {
        public bool HasChanges { get; set; }
        public string ChangeDescription { get; set; }
    }

    /// <summary>
    /// 检查扩展属性是否有真正的变更（已废弃，现在由用户手动决定是否创建新版本）
    /// 版本变更触发条件：
    /// 1. 属性值（AttributeValue）发生变化 - 影响计算
    /// 2. 属性单位（Unit）发生变化 - 影响计算
    /// 3. 属性精度（Precision）发生变化 - 影响计算
    /// 4. 新增或删除属性 - 影响计算
    ///
    /// 不触发版本变更的情况：
    /// 1. 只修改属性名称（AttributeName）- 不影响计算
    /// 2. 只修改排序码（SortCode）- 不影响计算
    /// 3. 属性值、单位、精度完全相同 - 无实际变更
    /// </summary>
    [Obsolete("现在由用户手动决定是否创建新版本，此方法已不再使用")]
    private async Task<AttributeChangeInfo> CheckAttributeChanges(
        string productSpecId,
        List<ProductSpecAttributeEntity> newAttributes
    )
    {
        // 获取当前版本的属性
        var currentAttributes = await _versionService.GetAttributesByVersionAsync(productSpecId);

        var changeInfo = new AttributeChangeInfo { HasChanges = false };
        var changes = new List<string>();

        // 如果数量不同，肯定有变更
        if (currentAttributes.Count != newAttributes.Count)
        {
            changeInfo.HasChanges = true;
            if (newAttributes.Count > currentAttributes.Count)
                changes.Add($"新增 {newAttributes.Count - currentAttributes.Count} 个属性");
            else
                changes.Add($"删除 {currentAttributes.Count - newAttributes.Count} 个属性");
        }

        // 创建当前属性的字典，方便查找
        var currentAttrDict = currentAttributes.ToDictionary(a => a.AttributeKey, a => a);
        var newAttrDict = newAttributes.ToDictionary(a => a.AttributeKey, a => a);

        // 检查新增的属性
        foreach (var newAttr in newAttributes)
        {
            if (!currentAttrDict.ContainsKey(newAttr.AttributeKey))
            {
                changeInfo.HasChanges = true;
                changes.Add($"新增属性：{newAttr.AttributeName}({newAttr.AttributeKey})");
            }
        }

        // 检查删除的属性
        foreach (var currentAttr in currentAttributes)
        {
            if (!newAttrDict.ContainsKey(currentAttr.AttributeKey))
            {
                changeInfo.HasChanges = true;
                changes.Add($"删除属性：{currentAttr.AttributeName}({currentAttr.AttributeKey})");
            }
        }

        // 比较每个属性的关键字段（影响计算的字段）
        foreach (var newAttr in newAttributes)
        {
            if (!currentAttrDict.TryGetValue(newAttr.AttributeKey, out var currentAttr))
                continue; // 已在上面的新增检查中处理

            // 比较属性值（影响计算）
            var currentValue = (currentAttr.AttributeValue ?? "").Trim();
            var newValue = (newAttr.AttributeValue ?? "").Trim();
            if (currentValue != newValue)
            {
                changeInfo.HasChanges = true;
                changes.Add($"{newAttr.AttributeName}：{currentValue} → {newValue}");
            }

            // 比较单位（影响计算）
            var currentUnit = (currentAttr.Unit ?? "").Trim();
            var newUnit = (newAttr.Unit ?? "").Trim();
            if (currentUnit != newUnit)
            {
                changeInfo.HasChanges = true;
                changes.Add($"{newAttr.AttributeName}单位：{currentUnit} → {newUnit}");
            }

            // 比较精度（影响计算）
            if (currentAttr.Precision != newAttr.Precision)
            {
                changeInfo.HasChanges = true;
                changes.Add(
                    $"{newAttr.AttributeName}精度：{currentAttr.Precision} → {newAttr.Precision}"
                );
            }

            // 注意：属性名称（AttributeName）和排序码（SortCode）的变化不影响计算，不触发版本变更
        }

        if (changeInfo.HasChanges && changes.Count > 0)
        {
            changeInfo.ChangeDescription = string.Join("；", changes);
        }

        return changeInfo;
    }
}

/// <summary>
/// 验证结果
/// </summary>
public class ValidationResult
{
    private readonly List<string> _errors = new();

    public bool IsValid => _errors.Count == 0;
    public IReadOnlyList<string> Errors => _errors.AsReadOnly();

    public void AddError(string error)
    {
        _errors.Add(error);
    }

    public string GetErrorMessage()
    {
        return string.Join("; ", _errors);
    }
}
