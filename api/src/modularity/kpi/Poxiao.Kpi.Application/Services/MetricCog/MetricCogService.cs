namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标图链服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-18.
/// </summary>
public class MetricCogService : IMetricCogService, ITransient
{
    /// <summary>
    /// 标签仓库.
    /// </summary>
    private readonly ISqlSugarRepository<MetricCogEntity> _repository;

    /// <summary>
    /// 初始化一个<see cref="MetricCogService"/>类型的新实例.
    /// </summary>
    public MetricCogService(ISqlSugarRepository<MetricCogEntity> repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<MetricCogInfoOutput> GetAsync(string id)
    {
        var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
        var info = entity.Adapt<MetricCogInfoOutput>();
        return info;
    }

    /// <inheritdoc />
    public async Task<dynamic> GetListAsync(MetricCogListQueryInput input)
    {
        var data = await _repository.AsQueryable()
            .WhereIF(input.IsShowDeleted != null, it => it.IsDeleted == input.IsShowDeleted)
            .WhereIF(!string.IsNullOrEmpty(input.MetricId), it => it.MetricId.Contains(input.MetricId))
            .Select(x => new MetricCogListOutput
            {
                Id = x.Id,
                MetricId = x.MetricId,
                ParentId = x.ParentId,
                ChainOfGraphIds = x.ChainOfGraphIds,
                CreatedTime = x.CreatedTime,
                CreatedUserid = x.CreatedUserId,
                LastModifiedTime = x.LastModifiedTime,
                LastModifiedUserid = x.LastModifiedUserId,
                DeleteTime = x.DeleteTime,
                DeleteUserid = x.DeleteUserId,
                TenantId = x.TenantId,
            })
            .MergeTable()
            .OrderByIF(string.IsNullOrEmpty(input.Sidx), it => it.Id)
              .OrderByIF(!string.IsNullOrEmpty(input.Sidx), input.Sidx + " " + input.Sort)
              .ToPagedListAsync(input.CurrentPage, input.PageSize);
        return PageResult<MetricCogListOutput>.SqlSugarPageResult(data);
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(MetricCogCrInput input)
    {
        var entity = input.Adapt<MetricCogEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        var count = await _repository.AsInsertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(x => x.Create()).ExecuteCommandAsync();
        return count;
    }

    /// <inheritdoc />
    public async Task<int> UpdateAsync(MetricCogUpInput input)
    {
        var entity = input.Adapt<MetricCogEntity>();
        var count = await _repository.AsUpdateable(entity)
            .IgnoreColumns(ignoreAllNullColumns: true)
            .CallEntityMethod(x => x.Update())
            .ExecuteCommandAsync();
        return count;
    }

    /// <inheritdoc />
    public async Task<int> DeleteAsync(string id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return 1;
        var count = await _repository.AsUpdateable(entity)
            .CallEntityMethod(x => x.Delete())
            .UpdateColumns(it => new
            {
                it.DeleteTime,
                it.DeleteUserId,
                it.IsDeleted
            })
            .ExecuteCommandAsync();
        return count;
    }
}