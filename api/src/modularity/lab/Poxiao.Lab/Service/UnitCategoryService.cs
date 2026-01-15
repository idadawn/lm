using Mapster;
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
/// 单位维度服务.
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "unit-category", Order = 201)]
[Route("api/lab/unit-category")]
public class UnitCategoryService : IUnitCategoryService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<UnitCategoryEntity> _repository;

    public UnitCategoryService(ISqlSugarRepository<UnitCategoryEntity> repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    [HttpGet("")]
    public async Task<List<UnitCategoryDto>> GetList()
    {
        var list = await _repository.GetListAsync(u => u.DeleteMark == 0 || u.DeleteMark == null);
        return list
            .OrderBy(u => u.SortCode ?? 0)
            .Select(u => u.Adapt<UnitCategoryDto>())
            .ToList();
    }

    /// <inheritdoc />
    [HttpGet("{id}")]
    public async Task<UnitCategoryDto> GetInfo(string id)
    {
        var entity = await _repository.GetFirstAsync(u => u.Id == id && (u.DeleteMark == 0 || u.DeleteMark == null));
        if (entity == null)
            throw Oops.Oh("单位维度不存在");

        return entity.Adapt<UnitCategoryDto>();
    }

    /// <inheritdoc />
    [HttpPost]
    public async Task Create([FromBody] UnitCategoryInput input)
    {
        // 检查编码是否已存在
        var exists = await _repository
            .AsQueryable()
            .Where(u => u.Code == input.Code && (u.DeleteMark == 0 || u.DeleteMark == null))
            .AnyAsync();
        if (exists)
            throw Oops.Oh($"编码 '{input.Code}' 已存在");

        var entity = input.Adapt<UnitCategoryEntity>();
        entity.Creator();

        var isOk = await _repository.InsertAsync(entity);
        if (!isOk)
            throw Oops.Oh("创建失败");
    }

    /// <inheritdoc />
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] UnitCategoryInput input)
    {
        var entity = await _repository.GetFirstAsync(u => u.Id == id && (u.DeleteMark == 0 || u.DeleteMark == null));
        if (entity == null)
            throw Oops.Oh("单位维度不存在");

        // 检查编码是否已被其他记录使用
        var exists = await _repository
            .AsQueryable()
            .Where(u => u.Code == input.Code && u.Id != id && (u.DeleteMark == 0 || u.DeleteMark == null))
            .AnyAsync();
        if (exists)
            throw Oops.Oh($"编码 '{input.Code}' 已被其他记录使用");

        entity.Name = input.Name;
        entity.Code = input.Code;
        entity.Description = input.Description;
        entity.SortCode = input.SortCode;
        entity.LastModify();

        var isOk = await _repository.UpdateAsync(entity);
        if (!isOk)
            throw Oops.Oh("更新失败");
    }

    /// <inheritdoc />
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _repository.GetFirstAsync(u => u.Id == id && (u.DeleteMark == 0 || u.DeleteMark == null));
        if (entity == null)
            throw Oops.Oh("单位维度不存在");

        // 检查是否有关联的单位定义
        // 注意：这里需要检查 UnitDefinition 表，但由于是跨模块调用，暂时只做软删除
        entity.Delete();

        var isOk = await _repository.UpdateAsync(entity);
        if (!isOk)
            throw Oops.Oh("删除失败");
    }
}
