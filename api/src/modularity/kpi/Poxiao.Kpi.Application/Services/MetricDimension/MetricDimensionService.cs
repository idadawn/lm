namespace Poxiao.Kpi.Application;

/// <summary>
/// 公共维度服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-12-6.
/// </summary>
public class MetricDimensionService : IMetricDimensionService, ITransient
{
    /// <summary>
    /// 标签仓库.
    /// </summary>
    private readonly ISqlSugarRepository<MetricDimensionEntity> _repository;

    /// <summary>
    /// 数据操作
    /// </summary>
    private readonly IDbService _dbService;

    /// <summary>
    /// 初始化一个<see cref="MetricDimensionService"/>类型的新实例.
    /// </summary>
    public MetricDimensionService(ISqlSugarRepository<MetricDimensionEntity> repository, IDbService dbService)
    {
        _repository = repository;
        _dbService = dbService;
    }

    /// <inheritdoc />
    public async Task<MetricDimensionInfoOutput> GetAsync(string id)
    {
        var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
        var info = entity.Adapt<MetricDimensionInfoOutput>();
        return info;
    }

    /// <inheritdoc />
    public async Task<PagedResultDto<MetricDimensionListOutput>> GetListAsync(MetricDimensionListQueryInput input)
    {
        var data = await _repository.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(input.Keyword), it => input.Keyword.Contains(it.Name))
            .Select(x => new MetricDimensionListOutput
            {
                Id = x.Id,
                DateModelType = x.DateModelType,
                DataModelId = x.DataModelId,
                Name = x.Name,
                DataType = x.DataType,
                Column = x.Column,
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
        var pageList = PagedResultDto<MetricDimensionListOutput>.SqlSugarPageResult(data);
        return pageList;
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(MetricDimensionCrInput input)
    {
        var entity = input.Adapt<MetricDimensionEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        var count = await _repository.AsInsertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(x => x.Create()).ExecuteCommandAsync();
        return count;
    }

    /// <inheritdoc />
    public async Task<int> UpdateAsync(MetricDimensionUpInput input)
    {
        var entity = input.Adapt<MetricDimensionEntity>();
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
        var isOk = await _repository.DeleteByIdAsync(id);
        return isOk;
    }

    /// <inheritdoc />
    public async Task<List<MetricDimOptionsOutput>> GetOptionsAsync()
    {
        var list = await _repository.AsQueryable().Select(x => new MetricDimOptionsOutput
        {
            Id = x.Id,
            Name = x.Name,
        }).ToListAsync();
        return list;
    }

    /// <inheritdoc />
    public async Task<ModelDataListOutput> GetDimensionDataAsync(string id)
    {
        var data = new ModelDataListOutput();
        var info = await GetAsync(id);
        var input = new ModelDataQueryInput();
        input.LinkId = info.DataModelId.ParentId;
        input.SchemaName = info.DataModelId.Id;
        input.ColumnField = info.Column;
        var orderBy = info.Column.Adapt<OrderByFieldOutput>();
        orderBy.SortBy = DBSortByType.ASC;
        input.OrderByField = orderBy;
        data = await _dbService.GetFilterModelDataAsync(input);
        return data;
    }
}