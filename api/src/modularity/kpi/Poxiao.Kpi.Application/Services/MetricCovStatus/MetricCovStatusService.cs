using NPOI.OpenXmlFormats.Vml.Office;

namespace Poxiao.Kpi.Application;

/// <summary>
/// 价值链状态服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-12-6.
/// </summary>
public class MetricCovStatusService : IMetricCovStatusService, ITransient
{
    /// <summary>
    /// 标签仓库.
    /// </summary>
    private readonly ISqlSugarRepository<MetricCovStatusEntity> _repository;

    /// <summary>
    /// 初始化一个<see cref="MetricCovStatusService"/>类型的新实例.
    /// </summary>
    public MetricCovStatusService(ISqlSugarRepository<MetricCovStatusEntity> repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<MetricCovStatusInfoOutput> GetAsync(string id)
    {
        var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
        var info = entity.Adapt<MetricCovStatusInfoOutput>();
        return info;
    }

    /// <inheritdoc />
    public async Task<PagedResultDto<MetricCovStatusListOutput>> GetListAsync(MetricCovStatusListQueryInput input)
    {
        var data = await _repository.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(input.Keyword), it => input.Keyword.Contains(it.Name))
            .Select(x => new MetricCovStatusListOutput
            {
                Id = x.Id,
                Name = x.Name,
                Color = x.Color,
                CreatedTime = x.CreatedTime,
                CreatedUserid = x.CreatedUserId,
                LastModifiedTime = x.LastModifiedTime,
                LastModifiedUserid = x.LastModifiedUserId,
                TenantId = x.TenantId,
            })
            .MergeTable()
            .OrderByIF(string.IsNullOrEmpty(input.Sidx), it => it.Id)
              .OrderByIF(!string.IsNullOrEmpty(input.Sidx), input.Sidx + " " + input.Sort)
              .ToPagedListAsync(input.CurrentPage, input.PageSize);
        return PagedResultDto<MetricCovStatusListOutput>.SqlSugarPageResult(data);
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(MetricCovStatusCrInput input)
    {
        var entity = input.Adapt<MetricCovStatusEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        var count = await _repository.AsInsertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(x => x.Create()).ExecuteCommandAsync();
        return count;
    }

    /// <inheritdoc />
    public async Task<int> UpdateAsync(MetricCovStatusUpInput input)
    {
        var entity = input.Adapt<MetricCovStatusEntity>();
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

    /// <inheritdoc />
    public async Task<List<MetricCovStatusOptionOutput>> GetOptionsAsync()
    {
        var list = await _repository.AsQueryable().Select(x => new MetricCovStatusOptionOutput
        {
            Id = x.Id,
            Name = x.Name,
            Color = x.Color,
        }).ToListAsync();
        return list;
    }
}