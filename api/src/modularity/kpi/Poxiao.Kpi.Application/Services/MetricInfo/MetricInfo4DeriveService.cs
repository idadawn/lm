namespace Poxiao.Kpi.Application;

/// <summary>
/// 派生指标定义服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-12.
/// </summary>
public class MetricInfo4DeriveService : IMetricInfo4DeriveService, ITransient
{
    /// <summary>
    /// 仓库.
    /// </summary>
    private readonly ISqlSugarRepository<MetricInfoEntity> _repository;

    private readonly IMetricInfoService _metricInfoService;

    /// <summary>
    /// 初始化一个<see cref="MetricInfo4DeriveService"/>类型的新实例.
    /// </summary>
    public MetricInfo4DeriveService(ISqlSugarRepository<MetricInfoEntity> repository, IMetricInfoService metricInfoService)
    {
        _repository = repository;
        _metricInfoService = metricInfoService;
    }

    /// <inheritdoc />
    public async Task<MetricInfo4DeriveOutput> GetAsync(string id)
    {
        var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
        var info = entity.Adapt<MetricInfo4DeriveOutput>();
        return info;
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(MetricInfo4DeriveCrInput input)
    {
        if (await _metricInfoService.CheckNameAsync(input.Name))
            throw Oops.Oh(ErrorCode.K10010);
        if (await _metricInfoService.CheckCodeAsync(input.Code))
            throw Oops.Oh(ErrorCode.K10011);
        var entity = input.Adapt<MetricInfoEntity>();
        DealDeriveMetricAsync(input, entity);

        entity.Id = SnowflakeIdHelper.NextId();
        var count = await _repository.AsInsertable(entity).CallEntityMethod(x => x.Create()).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        return count;
    }

    /// <summary>
    /// 处理派生指标数据
    /// </summary>
    /// <param name="input"></param>
    /// <param name="entity"></param>
    private void DealDeriveMetricAsync(MetricInfo4DeriveCrInput input, MetricInfoEntity entity)
    {
        entity.ParentId = input.ParentId;
        switch (input.DeriveType)
        {
            case DeriveType.PTD:
                entity.Expression = $"PTD('{input.Column.field}')";
                break;
            case DeriveType.POP:
                entity.Expression = $"POP('{input.Column.field}')";
                break;
            case DeriveType.Cumulative:
                entity.Expression = $"Cumulative('{input.Column.field}')";
                break;
            case DeriveType.Moving:
                entity.Expression = $"Moving('{input.Column.field}')";
                break;
            case DeriveType.Difference:
                entity.Expression = $"Difference('{input.Column.field}')";
                break;
            case DeriveType.DifferenceRatio:
                entity.Expression = $"DifferenceRatio('{input.Column.field}')";
                break;
            case DeriveType.TotalRatio:
                entity.Expression = $"TotalRatio('{input.Column.field}')";
                break;
            case DeriveType.Ranking:
                entity.Expression = $"Ranking('{input.Column.field}')";
                break;
        }
    }

    /// <inheritdoc />
    public async Task<int> UpdateAsync(MetricInfo4DeriveUpInput input)
    {
        var entity = input.Adapt<MetricInfoEntity>();
        var count = await _repository.AsUpdateable(entity)
            .IgnoreColumns(ignoreAllNullColumns: true)
            .CallEntityMethod(x => x.Update())
            .ExecuteCommandAsync();
        return count;
    }

}