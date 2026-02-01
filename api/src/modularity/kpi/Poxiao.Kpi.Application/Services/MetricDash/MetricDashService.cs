namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标仪表板服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-18.
/// </summary>
public class MetricDashService : IMetricDashService, ITransient
{
    /// <summary>
    /// 标签仓库.
    /// </summary>
    private readonly ISqlSugarRepository<MetricDashEntity> _repository;

    /// <summary>
    /// 指标服务
    /// </summary>
    private readonly IMetricInfoService _metricInfoService;

    /// <summary>
    /// 初始化一个<see cref="MetricDashService"/>类型的新实例.
    /// </summary>
    public MetricDashService(ISqlSugarRepository<MetricDashEntity> repository, IMetricInfoService metricInfoService)
    {
        _repository = repository;
        _metricInfoService = metricInfoService;
    }

    /// <inheritdoc />
    public async Task<MetricDashInfoOutput> GetAsync(string id)
    {
        var entity = await _repository.AsQueryable().FirstAsync(it => it.GotId.Equals(id));
        var info = entity.Adapt<MetricDashInfoOutput>();
        return info;
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(MetricDashCrInput input)
    {
        var entity = await _repository.AsQueryable().FirstAsync(it => it.GotId.Equals(input.GotId));
        if (entity != null)
        {
            var update = input.Adapt<MetricDashUpInput>();
            update.Id = entity.Id;
            return await UpdateAsync(update);
        }
        entity = input.Adapt<MetricDashEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        var count = await _repository.AsInsertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(x => x.Create()).ExecuteCommandAsync();
        return count;
    }

    /// <summary>
    /// 更新指标仪表板.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<int> UpdateAsync(MetricDashUpInput input)
    {
        var entity = input.Adapt<MetricDashEntity>();
        var count = await _repository.AsUpdateable(entity)
            .IgnoreColumns(ignoreAllNullColumns: true)
            .CallEntityMethod(x => x.Update())
            .ExecuteCommandAsync();
        return count;
    }

    public async Task<bool> IsMetricCanDragToDashAsync(MetricDashDragInput input)
    {
        var isFlag = false;
        var dims = await _metricInfoService.GetDimensionsAsync(new MetricInfoDimQryCrInput()
        {
            MetricIds = input.Metrics
        });

        var currentMetric = await _metricInfoService.GetAsync(input.CurrentMetricId);
        return isFlag;
    }
}