using Mapster;
using Microsoft.AspNetCore.Mvc;
using Poxiao.DatabaseAccessor;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.ReportConfig;
using Poxiao.Lab.Entity.Enum;
using SqlSugar;
using System.Text.Json;

namespace Poxiao.Lab.Service;

/// <summary>
/// 报表统计配置服务
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "report-config", Order = 221)]
[Route("api/lab/report-config")]
public class ReportConfigService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<ReportConfigEntity> _reportConfigRepository;
    private readonly ISqlSugarRepository<IntermediateDataJudgmentLevelEntity> _judgmentLevelRepository;

    public ReportConfigService(
        ISqlSugarRepository<ReportConfigEntity> reportConfigRepository,
        ISqlSugarRepository<IntermediateDataJudgmentLevelEntity> judgmentLevelRepository)
    {
        _reportConfigRepository = reportConfigRepository;
        _judgmentLevelRepository = judgmentLevelRepository;
    }

    /// <summary>
    /// 获取配置列表
    /// </summary>
    [HttpGet("")]
    public async Task<List<ReportConfigDto>> GetListAsync()
    {
        var configs = await _reportConfigRepository.AsQueryable()
            .Where(c => c.DeleteMark == null)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        // 如果没有配置，初始化默认配置
        if (!configs.Any())
        {
            await InitDefaultConfigAsync();
            configs = await _reportConfigRepository.AsQueryable()
                .Where(c => c.DeleteMark == null)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();
        }

        return configs.Select(c => new ReportConfigDto
        {
            Id = c.Id,
            Name = c.Name,
            LevelNames = string.IsNullOrEmpty(c.LevelNames)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(c.LevelNames),
            IsSystem = c.IsSystem,
            SortOrder = c.SortOrder,
            Description = c.Description,
            IsHeader = c.IsHeader,
            IsPercentage = c.IsPercentage,
            IsShowInReport = c.IsShowInReport,
            IsShowRatio = c.IsShowRatio,
            FormulaId = c.FormulaId
        }).ToList();
    }

    /// <summary>
    /// 添加配置
    /// </summary>
    [HttpPost("")]
    public async Task AddAsync([FromBody] ReportConfigInputDto input)
    {
        var entity = input.Adapt<ReportConfigEntity>();
        entity.LevelNames = JsonSerializer.Serialize(input.LevelNames);
        entity.Creator();
        await _reportConfigRepository.InsertAsync(entity);
    }

    /// <summary>
    /// 更新配置
    /// </summary>
    [HttpPut("")]
    public async Task UpdateAsync([FromBody] ReportConfigInputDto input)
    {
        var entity = await _reportConfigRepository.GetFirstAsync(c => c.Id == input.Id);
        if (entity == null)
        {
            throw new Exception("配置不存在");
        }

        entity.Name = input.Name;
        entity.LevelNames = JsonSerializer.Serialize(input.LevelNames);
        entity.SortOrder = input.SortOrder;
        entity.Description = input.Description;
        entity.IsHeader = input.IsHeader;
        entity.IsPercentage = input.IsPercentage;
        entity.IsShowInReport = input.IsShowInReport;
        entity.IsShowRatio = input.IsShowRatio;
        entity.FormulaId = input.FormulaId;
        entity.LastModify();

        await _reportConfigRepository.UpdateAsync(entity);
    }

    /// <summary>
    /// 删除配置
    /// </summary>
    [HttpDelete("{id}")]
    public async Task DeleteAsync(string id)
    {
        var entity = await _reportConfigRepository.GetFirstAsync(c => c.Id == id);
        if (entity == null)
        {
            throw new Exception("配置不存在");
        }

        if (entity.IsSystem)
        {
            // 系统默认配置不允许删除，但可以修改
            throw new Exception("系统默认配置不允许删除");
        }

        await _reportConfigRepository.DeleteAsync(entity);
    }

    /// <summary>
    /// 初始化默认配置
    /// </summary>
    private async Task InitDefaultConfigAsync()
    {
        await Task.CompletedTask;
    }
}
