using Mapster;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.IntermediateDataFormula;
using Poxiao.Lab.Interfaces;
using SqlSugar;

namespace Poxiao.Lab.CalcWorker.Services;

/// <summary>
/// Worker 进程专用的中间数据公式服务实现。
/// 仅实现计算所需的 GetListAsync，其他接口在 Worker 中不会被调用。
/// </summary>
public class WorkerIntermediateDataFormulaService : IIntermediateDataFormulaService
{
    private readonly ISqlSugarRepository<IntermediateDataFormulaEntity> _repository;

    public WorkerIntermediateDataFormulaService(
        ISqlSugarRepository<IntermediateDataFormulaEntity> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 获取所有公式列表（供批量计算器使用）。
    /// </summary>
    public async Task<List<IntermediateDataFormulaDto>> GetListAsync()
    {
        // 注意：不能使用 Select<IntermediateDataFormulaDto>() 直接投影，
        // 因为 Entity 中 FormulaType 是枚举类型 (IntermediateDataFormulaType)，
        // 而 DTO 中是 string 类型。SqlSugar 投影时可能将枚举的整数值（如 1）
        // 映射为字符串 "1"，而非枚举名称 "CALC"，导致后续按 FormulaType 过滤时
        // 全部被过滤掉（NormalizeFormulaType("1") != "CALC"）。
        // 因此改为先查询 Entity，再手动映射为 DTO，确保 FormulaType 使用枚举名称。
        var entities = await _repository
            .AsQueryable()
            .Where(t => t.DeleteMark == null)
            .OrderBy(t => t.SortOrder)
            .OrderBy(t => t.CreatorTime)
            .ToListAsync();

        return entities.Select(entity =>
        {
            var dto = entity.Adapt<IntermediateDataFormulaDto>();
            // 显式将枚举转为名称字符串，与 IntermediateDataFormulaService.ToDto() 保持一致
            dto.FormulaType = entity.FormulaType.ToString();
            dto.SourceType = entity.SourceType;
            return dto;
        }).ToList();
    }

    #region 非 Worker 场景使用的接口 — 此处不支持

    public Task<IntermediateDataFormulaDto> GetByIdAsync(string id) =>
        throw new NotSupportedException("WorkerIntermediateDataFormulaService.GetByIdAsync 仅用于 Web API 场景，不在计算 Worker 中使用。");

    public Task<IntermediateDataFormulaDto> CreateAsync(IntermediateDataFormulaDto dto) =>
        throw new NotSupportedException("WorkerIntermediateDataFormulaService.CreateAsync 仅用于 Web API 场景，不在计算 Worker 中使用。");

    public Task<IntermediateDataFormulaDto> UpdateAsync(string id, IntermediateDataFormulaDto dto) =>
        throw new NotSupportedException("WorkerIntermediateDataFormulaService.UpdateAsync 仅用于 Web API 场景，不在计算 Worker 中使用。");

    public Task DeleteAsync(string id) =>
        throw new NotSupportedException("WorkerIntermediateDataFormulaService.DeleteAsync 仅用于 Web API 场景，不在计算 Worker 中使用。");

    public Task<IntermediateDataFormulaDto> UpdateFormulaAsync(string id, FormulaUpdateInput input) =>
        throw new NotSupportedException("WorkerIntermediateDataFormulaService.UpdateFormulaAsync 仅用于 Web API 场景，不在计算 Worker 中使用。");

    public Task<List<IntermediateDataColumnInfo>> GetAvailableColumnsAsync(bool includeHidden = false) =>
        throw new NotSupportedException("WorkerIntermediateDataFormulaService.GetAvailableColumnsAsync 仅用于 Web API 场景，不在计算 Worker 中使用。");

    public Task InitializeAsync() =>
        throw new NotSupportedException("WorkerIntermediateDataFormulaService.InitializeAsync 仅用于 Web API 场景，不在计算 Worker 中使用。");

    public Task<List<FormulaVariableSource>> GetVariableSourcesAsync() =>
        throw new NotSupportedException("WorkerIntermediateDataFormulaService.GetVariableSourcesAsync 仅用于 Web API 场景，不在计算 Worker 中使用。");

    public Task<FormulaValidationResult> ValidateFormulaAsync(FormulaValidationRequest request) =>
        throw new NotSupportedException("WorkerIntermediateDataFormulaService.ValidateFormulaAsync 仅用于 Web API 场景，不在计算 Worker 中使用。");

    #endregion
}

