using System;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Poxiao.DatabaseAccessor;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Manager;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.IntermediateDataJudgmentLevel;
using Poxiao.Lab.Entity.Enum;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// 中间数据判定等级服务.
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "intermediate-data-judgment-level", Order = 211)]
[Route("api/lab/intermediate-data-judgment-level")]
public class IntermediateDataJudgmentLevelService
    : IDynamicApiController,
        ITransient
{
    private readonly ISqlSugarRepository<IntermediateDataJudgmentLevelEntity> _repository;
    private readonly ISqlSugarRepository<IntermediateDataFormulaEntity> _formulaRepository;
    private readonly ICacheManager _cacheManager;
    private readonly IUserManager _userManager;

    private const string CachePrefix = "LAB:IntermediateDataJudgmentLevel";

    public IntermediateDataJudgmentLevelService(
        ISqlSugarRepository<IntermediateDataJudgmentLevelEntity> repository,
        ISqlSugarRepository<IntermediateDataFormulaEntity> formulaRepository,
        ICacheManager cacheManager,
        IUserManager userManager
    )
    {
        _repository = repository;
        _formulaRepository = formulaRepository;
        _cacheManager = cacheManager;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取判定等级列表.
    /// </summary>
    /// <param name="input">查询参数</param>
    /// <returns>列表</returns>
    [HttpGet("")]
    public async Task<List<IntermediateDataJudgmentLevelDto>> GetListAsync(
        [FromQuery] IntermediateDataJudgmentLevelListInput input
    )
    {
        var cacheKey = BuildCacheKey(input.FormulaId);
        if (_cacheManager.Exists(cacheKey))
        {
            var cached = _cacheManager.Get<List<IntermediateDataJudgmentLevelDto>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }
        }

        var list = await _repository
            .AsQueryable()
            .Where(t => t.DeleteMark == null && t.FormulaId == input.FormulaId)
            .OrderBy(t => t.Priority) // 按优先级排序
            .OrderBy(t => t.CreatorTime) // 然后按创建时间
            .ToListAsync();

        var result = list.Adapt<List<IntermediateDataJudgmentLevelDto>>();

        // 如果没有默认项，增加系统默认兜底项 (虚拟数据，不存库，方便前端展示和逻辑判定)
        if (!result.Any(t => t.IsDefault))
        {
            result.Add(new IntermediateDataJudgmentLevelDto
            {
                Id = "sys_default_virtual_id",
                FormulaId = input.FormulaId,
                Code = "SYS_DEF",
                Name = "(空)",
                QualityStatus = "其他",
                Priority = 999,
                Color = "",
                IsStatistic = false,
                IsDefault = true,
                Description = "系统生成的兜底默认项。当您未配置任何默认等级时显示，用于承接无匹配结果的情况。手动配置默认项后此项将自动隐藏。"
            });
        }

        await _cacheManager.SetAsync(cacheKey, result, TimeSpan.FromHours(6));

        return result;
    }

    /// <summary>
    /// 获取单个判定等级.
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <returns>详情</returns>
    [HttpGet("{id}")]
    public async Task<IntermediateDataJudgmentLevelDto> GetByIdAsync(string id)
    {
        var entity = await _repository.GetFirstAsync(t => t.Id == id && t.DeleteMark == null);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);

        return entity.Adapt<IntermediateDataJudgmentLevelDto>();
    }

    /// <summary>
    /// 创建判定等级.
    /// </summary>
    /// <param name="input">输入参数</param>
    /// <returns>结果</returns>
    [HttpPost("")]
    public async Task<IntermediateDataJudgmentLevelDto> CreateAsync(
        [FromBody] IntermediateDataJudgmentLevelAddInput input
    )
    {
        // 验证公式是否存在
        var formula = await _formulaRepository.GetFirstAsync(t => t.Id == input.FormulaId && t.DeleteMark == null);
        if (formula == null)
            throw Oops.Oh(ErrorCode.COM1005, "关联的判定公式不存在");

        var entity = input.Adapt<IntermediateDataJudgmentLevelEntity>();
        
        // 自动生成 Priority (当前最大 + 1)
        var maxPriority = await _repository
            .AsQueryable()
            .Where(t => t.FormulaId == input.FormulaId && t.DeleteMark == null)
            .MaxAsync(t => t.Priority);
        entity.Priority = maxPriority + 1;

        // 自动生成 Code
        entity.Code = Guid.NewGuid().ToString("N")[..8].ToUpper(); // 生成8位短UUID作为Code
        entity.FormulaName = formula.FormulaName; // 冗余公式名称

        if (input.IsDefault)
        {
            // 如果设置为默认，取消当前公式下的其他默认项
            await _repository.UpdateAsync(
                t => new IntermediateDataJudgmentLevelEntity { IsDefault = false },
                t => t.FormulaId == input.FormulaId && t.IsDefault == true
            );
        }

        entity.Creator();
        
        var isOk = await _repository.InsertAsync(entity);
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1000);

        await ClearCacheAsync(input.FormulaId);

        return entity.Adapt<IntermediateDataJudgmentLevelDto>();
    }

    /// <summary>
    /// 更新排序.
    /// </summary>
    /// <param name="ids">ID列表（按顺序）</param>
    /// <returns></returns>
    [HttpPut("sort")]
    public async Task UpdateSortAsync([FromBody] List<string> ids)
    {
        if (ids == null || !ids.Any()) return;

        var entities = await _repository.AsQueryable().In(t => t.Id, ids).ToListAsync();
        if (!entities.Any()) return;

        var formulaId = entities.First().FormulaId;

        for (int i = 0; i < ids.Count; i++)
        {
            var id = ids[i];
            var entity = entities.FirstOrDefault(t => t.Id == id);
            if (entity != null)
            {
                entity.Priority = i + 1; // 重新分配优先级
                entity.LastModify();
            }
        }

        await _repository.UpdateRangeAsync(entities);
        await ClearCacheAsync(formulaId);
    }

    /// <summary>
    /// 更新判定等级.
    /// </summary>
    /// <param name="input">输入参数</param>
    /// <returns>结果</returns>
    [HttpPut("")]
    public async Task<IntermediateDataJudgmentLevelDto> UpdateAsync(
        [FromBody] IntermediateDataJudgmentLevelUpdateInput input
    )
    {
        var entity = await _repository.GetFirstAsync(t => t.Id == input.Id && t.DeleteMark == null);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);

        // 更新字段
        entity.Name = input.Name;
        entity.QualityStatus = input.QualityStatus;
        entity.Priority = input.Priority;
        entity.Color = input.Color;
        entity.IsStatistic = input.IsStatistic;
        entity.IsDefault = input.IsDefault;
        entity.Description = input.Description;
        entity.Condition = input.Condition;

        if (input.IsDefault)
        {
            // 如果设置为默认，取消当前公式下的其他默认项（排除自己）
            await _repository.UpdateAsync(
                t => new IntermediateDataJudgmentLevelEntity { IsDefault = false },
                t => t.FormulaId == input.FormulaId && t.IsDefault == true && t.Id != input.Id
            );
        }
        
        // 确保 FormulaId 不被篡改，或者如果允许修改则需要处理
        if (entity.FormulaId != input.FormulaId)
        {
             var formula = await _formulaRepository.GetFirstAsync(t => t.Id == input.FormulaId && t.DeleteMark == null);
             if (formula == null)
                 throw Oops.Oh(ErrorCode.COM1005, "关联的判定公式不存在");
             entity.FormulaId = input.FormulaId;
             entity.FormulaName = formula.FormulaName;
        }

        entity.LastModify();

        var isOk = await _repository.UpdateAsync(entity);
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);

        // 清除旧的和新的 FormulaId 的缓存（如果变更了）
        await ClearCacheAsync(entity.FormulaId);
        if (entity.FormulaId != input.FormulaId)
        {
             await ClearCacheAsync(input.FormulaId);
        }

        return entity.Adapt<IntermediateDataJudgmentLevelDto>();
    }

    /// <summary>
    /// 删除判定等级.
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task DeleteAsync(string id)
    {
        var entity = await _repository.GetFirstAsync(t => t.Id == id);
        if (entity == null) return; // 已经不存在了，直接返回成功

        var isOk = await _repository.DeleteAsync(t => t.Id == id);
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);

        await ClearCacheAsync(entity.FormulaId);
    }

    private string BuildCacheKey(string formulaId)
    {
        var tenantId = _userManager?.TenantId ?? "global";
        return $"{CachePrefix}:{tenantId}:{formulaId}";
    }

    private async Task ClearCacheAsync(string formulaId)
    {
        if (!string.IsNullOrEmpty(formulaId))
        {
            await _cacheManager.DelAsync(BuildCacheKey(formulaId));
        }
    }
}
