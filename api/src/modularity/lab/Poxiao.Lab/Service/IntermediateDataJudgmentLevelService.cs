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
using System;

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
    private readonly ISqlSugarRepository<ProductSpecEntity> _productSpecRepository;
    private readonly ICacheManager _cacheManager;
    private readonly IUserManager _userManager;

    private const string CachePrefix = "LAB:IntermediateDataJudgmentLevel";

    public IntermediateDataJudgmentLevelService(
        ISqlSugarRepository<IntermediateDataJudgmentLevelEntity> repository,
        ISqlSugarRepository<IntermediateDataFormulaEntity> formulaRepository,
        ISqlSugarRepository<ProductSpecEntity> productSpecRepository,
        ICacheManager cacheManager,
        IUserManager userManager
    )
    {
        _repository = repository;
        _formulaRepository = formulaRepository;
        _productSpecRepository = productSpecRepository;
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
        // 如果有 ProductSpecId 筛选条件，跳过缓存直接查询
        if (string.IsNullOrEmpty(input.ProductSpecId))
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
        }

        var list = await _repository
            .AsQueryable()
            .Where(t => t.DeleteMark == null && t.FormulaId == input.FormulaId)
            .WhereIF(!string.IsNullOrEmpty(input.ProductSpecId), t => t.ProductSpecId == input.ProductSpecId)
            .OrderBy(t => t.Priority) // 按优先级排序
            .OrderBy(t => t.CreatorTime) // 然后按创建时间
            .ToListAsync();

        var result = list.Adapt<List<IntermediateDataJudgmentLevelDto>>();

        // 已移除系统自动生成的默认兜底项 - 如果没有匹配等级，判定失败
        // if (!result.Any(t => t.IsDefault))
        // {
        //     result.Add(new IntermediateDataJudgmentLevelDto { ... });
        // }

        // 只有无筛选条件时才缓存
        if (string.IsNullOrEmpty(input.ProductSpecId))
        {
            await _cacheManager.SetAsync(BuildCacheKey(input.FormulaId), result, TimeSpan.FromHours(6));
        }

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

        // 处理产品规格
        if (!string.IsNullOrEmpty(input.ProductSpecId))
        {
            var productSpec = await _productSpecRepository.GetFirstAsync(t => t.Id == input.ProductSpecId && t.DeleteMark == null);
            if (productSpec != null)
            {
                entity.ProductSpecId = productSpec.Id;
                entity.ProductSpecName = productSpec.Name;
            }
        }

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
                entity.Priority = i + 1; // 从1开始重新分配优先级
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

        // 处理产品规格
        if (!string.IsNullOrEmpty(input.ProductSpecId) && entity.ProductSpecId != input.ProductSpecId)
        {
            var productSpec = await _productSpecRepository.GetFirstAsync(t => t.Id == input.ProductSpecId && t.DeleteMark == null);
            if (productSpec != null)
            {
                entity.ProductSpecId = productSpec.Id;
                entity.ProductSpecName = productSpec.Name;
            }
        }
        else if (string.IsNullOrEmpty(input.ProductSpecId))
        {
            entity.ProductSpecId = null;
            entity.ProductSpecName = null;
        }

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
    /// 批量复制等级到其他判定项目.
    /// </summary>
    /// <param name="input">批量复制参数</param>
    /// <returns>复制结果</returns>
            [HttpPost("batch-copy")]
    public async Task<object> BatchCopyAsync([FromBody] BatchCopyLevelsInput input)
    {
        // 1. 获取源判定项目下的等级
        var sourceLevelQuery = _repository
            .AsQueryable()
            .Where(t => t.FormulaId == input.SourceFormulaId && t.DeleteMark == null);

        // 如果指定了源产品规格，则只复制该规格下的等级
        if (!string.IsNullOrEmpty(input.SourceProductSpecId))
        {
            sourceLevelQuery = sourceLevelQuery.Where(t => t.ProductSpecId == input.SourceProductSpecId);
        }

        var sourceLevels = await sourceLevelQuery
            .OrderBy(t => t.Priority)
            .ToListAsync();

        if (!sourceLevels.Any())
            throw Oops.Oh(ErrorCode.COM1005, "源判定项目下没有符合条件的等级数据");

        int copiedCount = 0;
        int skippedCount = 0;
        int overwrittenCount = 0;

        // 2. 确定目标组合 (Formula, ProductSpec)
        // 如果 TargetProductSpecIds 为空，则保留源等级的 ProductSpecId (即 null 或 原有ID)
        // 如果 TargetProductSpecIds 不为空，则为每个 TargetFormula + TargetProductSpec 生成组合
        var targetSpecs = new List<(string Id, string Name)>();
        if (input.TargetProductSpecIds != null && input.TargetProductSpecIds.Any())
        {
            var specs = await _productSpecRepository.AsQueryable()
                .Where(t => input.TargetProductSpecIds.Contains(t.Id))
                .Select(t => new { t.Id, t.Name })
                .ToListAsync();
            targetSpecs = specs.Select(t => (t.Id, t.Name)).ToList();
        }

        foreach (var targetFormulaId in input.TargetFormulaIds)
        {
            // 验证目标判定项目
            var targetFormula = await _formulaRepository.GetFirstAsync(
                t => t.Id == targetFormulaId && t.DeleteMark == null);
            if (targetFormula == null) continue;

            // 获取该目标项目下已有的所有等级，用于查重
            var existingLevels = await _repository
                .AsQueryable()
                .Where(t => t.FormulaId == targetFormulaId && t.DeleteMark == null)
                .ToListAsync();

            var currentMaxPriority = existingLevels.Any() ? existingLevels.Max(t => t.Priority) : 0;

            // 内部函数：执行单个等级的复制/覆盖逻辑
            async Task ProcessCopy(IntermediateDataJudgmentLevelEntity source, string targetSpecId, string targetSpecName)
            {
                // 查找是否存在同名且同规格的等级
                var existingLevel = existingLevels.FirstOrDefault(t =>
                    t.Name == source.Name &&
                    t.ProductSpecId == targetSpecId // 比较目标规格ID
                );

                if (existingLevel != null)
                {
                    if (input.OverwriteExisting)
                    {
                        // 覆盖
                        existingLevel.Condition = source.Condition;
                        existingLevel.QualityStatus = source.QualityStatus;
                        existingLevel.Color = source.Color;
                        existingLevel.IsStatistic = source.IsStatistic;
                        existingLevel.IsDefault = source.IsDefault;
                        existingLevel.Description = source.Description;
                        existingLevel.LastModify();
                        await _repository.UpdateAsync(existingLevel);
                        overwrittenCount++;
                    }
                    else
                    {
                        skippedCount++;
                    }
                }
                else
                {
                    // 新增
                    var newEntity = new IntermediateDataJudgmentLevelEntity
                    {
                        FormulaId = targetFormulaId,
                        FormulaName = targetFormula.FormulaName,
                        ProductSpecId = targetSpecId,    // 使用目标规格ID
                        ProductSpecName = targetSpecName,// 使用目标规格名称
                        Code = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                        Name = source.Name,
                        QualityStatus = source.QualityStatus,
                        Priority = ++currentMaxPriority,
                        Color = source.Color,
                        IsStatistic = source.IsStatistic,
                        IsDefault = source.IsDefault,
                        Description = source.Description,
                        Condition = source.Condition,
                    };
                    newEntity.Creator();
                    await _repository.InsertAsync(newEntity);
                    copiedCount++;
                }
            }

            // 开始复制
            if (targetSpecs.Any())
            {
                // 模式A: 显式指定了目标规格 -> 笛卡尔积复制
                foreach (var spec in targetSpecs)
                {
                    foreach (var sourceLevel in sourceLevels)
                    {
                        await ProcessCopy(sourceLevel, spec.Id, spec.Name);
                    }
                }
            }
            else
            {
                // 模式B: 未指定目标规格 -> 保持源等级的规格信息 (通常用于同规格下的复制，或者不涉及规格的复制)
                // 注意：如果源数据有规格ID，且直接复制到另一个Formula下，可能那个Formula并不适用该规格ID
                // 但通常用法是：未指定目标规格时，我们认为用户希望"原样仅仅换个Formula"
                foreach (var sourceLevel in sourceLevels)
                {
                    await ProcessCopy(sourceLevel, sourceLevel.ProductSpecId, sourceLevel.ProductSpecName);
                }
            }

            await ClearCacheAsync(targetFormulaId);
        }

        return new { copiedCount, skippedCount, overwrittenCount };
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
