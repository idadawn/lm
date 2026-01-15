namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标价值链规则服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-18.
/// </summary>
public class MetricCovRuleService : IMetricCovRuleService, ITransient
{
    /// <summary>
    /// 标签仓库.
    /// </summary>
    private readonly ISqlSugarRepository<MetricCovRuleEntity> _repository;

    /// <summary>
    /// 初始化一个<see cref="MetricCovRuleService"/>类型的新实例.
    /// </summary>
    public MetricCovRuleService(ISqlSugarRepository<MetricCovRuleEntity> repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<MetricCovRuleInfoOutput> GetAsync(string id)
    {
        var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
        var info = entity.Adapt<MetricCovRuleInfoOutput>();
        return info;
    }

    /// <inheritdoc />
    public async Task<List<MetricCovRuleListOutput>> GetListAsync(string covId)
    {
        var data = await _repository.AsQueryable()
            .Where(x => covId.Equals(x.CovId))
            .Select(x => new MetricCovRuleListOutput
            {
                Id = x.Id,
                CovId = x.CovId,
                Level = x.Level,
                Type = x.Type,
                Operators = x.Operators,
                Value = x.Value,
                MinValue = x.MinValue,
                MaxValue = x.MaxValue,
                Status = x.Status,
                CreatedTime = x.CreatedTime,
                CreatedUserid = x.CreatedUserId,
                LastModifiedTime = x.LastModifiedTime,
                LastModifiedUserid = x.LastModifiedUserId,
                TenantId = x.TenantId,
            })
            .MergeTable()
            .OrderBy(x => x.CreatedTime)
            .ToListAsync();
        return data;
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(MetricCovRuleCrInput input)
    {
        var entity = input.Adapt<MetricCovRuleEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        var count = await _repository.AsInsertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(x => x.Create()).ExecuteCommandAsync();
        return count;
    }

    /// <inheritdoc />
    public async Task<int> UpdateAsync(MetricCovRuleUpInput input)
    {
        var entity = input.Adapt<MetricCovRuleEntity>();
        var count = await _repository.AsUpdateable(entity)
            .IgnoreColumns(ignoreAllNullColumns: true)
            .CallEntityMethod(x => x.Update())
            .ExecuteCommandAsync();
        return count;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return true;
        var flag = await _repository.DeleteByIdAsync(id);
        return flag;
    }
}