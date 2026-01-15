namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标分级服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2024-1-5.
/// </summary>
public class MetricGradedService : IMetricGradedService, ITransient
{
    /// <summary>
    /// 标签仓库.
    /// </summary>
    private readonly ISqlSugarRepository<MetricGradedEntity> _repository;

    /// <summary>
    /// 初始化一个<see cref="MetricGradedService"/>类型的新实例.
    /// </summary>
    public MetricGradedService(ISqlSugarRepository<MetricGradedEntity> repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<MetricGradedInfoOutput> GetAsync(string id)
    {
        var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
        var info = entity.Adapt<MetricGradedInfoOutput>();
        return info;
    }

    /// <inheritdoc />
    public async Task<List<MetricGradedListOutput>> GetListAsync(string metricId)
    {
        var data = await _repository.AsQueryable()
            .Where(x => x.MetricId.Equals(metricId))
            .Select(x => new MetricGradedListOutput
            {
                Id = x.Id,
                MetricId = x.MetricId,
                Name = x.Name,
                Type = x.Type,
                RangType = x.RangType,
                Trend = x.Trend,
                Value = x.Value.ToString(),
                Status = x.Status,
                CreatedTime = x.CreatedTime,
                CreatedUserid = x.CreatedUserId,
                LastModifiedTime = x.LastModifiedTime,
                LastModifiedUserid = x.LastModifiedUserId,
                TenantId = x.TenantId,
            })
            .MergeTable()
            .OrderBy(x => x.CreatedTime)
            .OrderBy(x => x.Status)
            .ToListAsync();
        return data;
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(MetricGradedCrInput input)
    {
        var entity = input.Adapt<MetricGradedEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        var count = await _repository.AsInsertable(entity).IgnoreColumns(ignoreNullColumn: true)
            .CallEntityMethod(x => x.Create()).ExecuteCommandAsync();
        return count;
    }

    /// <inheritdoc />
    public async Task<int> UpdateAsync(MetricGradedUpInput input)
    {
        var entity = input.Adapt<MetricGradedEntity>();
        var count = await _repository.AsUpdateable(entity)
            .IgnoreColumns(ignoreAllNullColumns: true)
            .CallEntityMethod(x => x.Update())
            .ExecuteCommandAsync();
        return count;
    }

    /// <inheritdoc />
    public async Task<int> DeleteAsync(string id)
    {
        var count = await _repository.AsDeleteable().Where(it => it.Id == id).ExecuteCommandAsync();
        return count;
    }

    /// <inheritdoc />
    public async Task<List<MetricInfoGradeExtInput>> GetGradeExtInfoAsync(string metricId)
    {
        var data = await _repository.AsQueryable()
            .Where(x => x.MetricId.Equals(metricId))
            .OrderBy(x => x.CreatedTime)
            .Select(x => new MetricInfoGradeExtInput
            {
                Id = x.Id,
                MetricId = x.MetricId,
                Name = x.Name,
                DValue = x.Value,
                RangType = x.RangType,
                IsShow = true,
                StatusColor = x.StatusColor
            })
            .MergeTable()
            .ToListAsync();
        data.ForEach(x => x.Value = DealData(x.RangType, x.DValue));
        return data;
    }

    /// <summary>
    /// 处理数据显示.
    /// </summary>
    /// <param name="format">数据格式.</param>
    /// <param name="data">数据.</param>
    private string DealData(CovRuleValueType? format, decimal data)
    {
        if (format is null or CovRuleValueType.Value)
            return data.ToString("0.00");

        // 处理百分比.
        if (format == CovRuleValueType.Percent)
            // 处理小数点.
            data = Math.Round(data, 2, MidpointRounding.AwayFromZero);
        return $"{data}%";

    }
}