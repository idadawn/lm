using Mapster;
using Microsoft.AspNetCore.Mvc;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.Unit;
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
    private readonly ISqlSugarRepository<UnitDefinitionEntity> _unitRepository;

    public UnitCategoryService(
        ISqlSugarRepository<UnitCategoryEntity> repository,
        ISqlSugarRepository<UnitDefinitionEntity> unitRepository
    )
    {
        _repository = repository;
        _unitRepository = unitRepository;
    }

    /// <inheritdoc />
    [HttpGet("")]
    public async Task<List<UnitCategoryDto>> GetList()
    {
        var list = await _repository.GetListAsync(u => u.DeleteMark == 0 || u.DeleteMark == null);
        return list.OrderBy(u => u.SortCode ?? 0).Select(u => u.Adapt<UnitCategoryDto>()).ToList();
    }

    /// <inheritdoc />
    [HttpGet("{id}")]
    public async Task<UnitCategoryDto> GetInfo(string id)
    {
        var entity = await _repository.GetFirstAsync(u =>
            u.Id == id && (u.DeleteMark == 0 || u.DeleteMark == null)
        );
        if (entity == null)
            throw Oops.Bah("单位维度不存在");

        return entity.Adapt<UnitCategoryDto>();
    }

    /// <inheritdoc />
    [HttpPost]
    public async Task Create([FromBody] UnitCategoryInput input)
    {
        // 检查编码是否已存在（忽略大小写）
        var code = input.Code?.Trim();
        if (string.IsNullOrWhiteSpace(code))
            throw Oops.Bah("编码不能为空");

        var codeUpper = code.ToUpperInvariant();

        // 查询未删除的记录，检查是否存在相同编码
        var existingCodes = await _repository
            .AsQueryable()
            .Where(u => u.Code != null && (u.DeleteMark == 0 || u.DeleteMark == null))
            .Select(u => u.Code)
            .ToListAsync();

        var exists = existingCodes.Any(c =>
            !string.IsNullOrEmpty(c) && c.Trim().ToUpperInvariant() == codeUpper
        );

        if (exists)
        {
            throw Oops.Bah($"编码 '{code}' 已存在");
        }

        var entity = input.Adapt<UnitCategoryEntity>();
        entity.Code = code; // 使用 trim 后的编码
        entity.Creator();

        var isOk = await _repository.InsertAsync(entity);
        if (!isOk)
            throw Oops.Bah("创建失败");
    }

    /// <inheritdoc />
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] UnitCategoryInput input)
    {
        var entity = await _repository.GetFirstAsync(u =>
            u.Id == id && (u.DeleteMark == 0 || u.DeleteMark == null)
        );
        if (entity == null)
            throw Oops.Bah("单位维度不存在");

        // 检查编码是否已被其他记录使用（排除已删除的记录，忽略大小写）
        var code = input.Code?.Trim();
        if (string.IsNullOrWhiteSpace(code))
            throw Oops.Bah("编码不能为空");

        // 查询所有未删除的其他记录，在内存中比较（忽略大小写）
        var existingCodes = await _repository
            .AsQueryable()
            .Where(u => u.Id != id && (u.DeleteMark == 0 || u.DeleteMark == null))
            .Select(u => u.Code)
            .ToListAsync();

        var codeUpper = code.ToUpperInvariant();
        var exists = existingCodes.Any(c =>
            !string.IsNullOrEmpty(c) && c.Trim().ToUpperInvariant() == codeUpper
        );
        if (exists)
            throw Oops.Bah($"编码 '{code}' 已被其他记录使用");

        entity.Name = input.Name;
        entity.Code = code; // 使用 trim 后的编码
        entity.Description = input.Description;
        entity.SortCode = input.SortCode;
        entity.LastModify();

        var isOk = await _repository.UpdateAsync(entity);
        if (!isOk)
            throw Oops.Bah("更新失败");
    }

    /// <inheritdoc />
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _repository.GetFirstAsync(u =>
            u.Id == id && (u.DeleteMark == 0 || u.DeleteMark == null)
        );
        if (entity == null)
            throw Oops.Bah("单位维度不存在");

        // 检查该维度下是否有单位定义
        var unitCount = await _unitRepository
            .AsQueryable()
            .Where(u => u.CategoryId == id && (u.DeleteMark == 0 || u.DeleteMark == null))
            .CountAsync();

        if (unitCount > 0)
        {
            throw Oops.Bah(
                $"无法删除，该单位维度下存在 {unitCount} 个单位定义，请先删除或转移这些单位后再尝试删除维度");
        }

        entity.Delete();

        var isOk = await _repository.UpdateAsync(entity);
        if (!isOk)
            throw Oops.Bah("删除失败");
    }
}
