namespace Poxiao.Kpi.Application;

/// <summary>
/// 标签服务.
/// </summary>
public class MetricTagsService : IMetricTagsService, ITransient
{
    /// <summary>
    /// 标签仓库.
    /// </summary>
    private readonly ISqlSugarRepository<MetricTagsEntity> _repository;

    /// <summary>
    /// 构造函数.
    /// </summary>
    /// <param name="repository"></param>
    public MetricTagsService(ISqlSugarRepository<MetricTagsEntity> repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<MetricTagsInfoOutput> GetAsync(string id)
    {
        var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
        var info = entity.Adapt<MetricTagsInfoOutput>();
        return info;
    }

    /// <inheritdoc />
    public async Task<dynamic> GetListAsync(MetricTagsListQueryInput input)
    {
        var data = await _repository.AsQueryable()
            .WhereIF(input.IsShowDeleted != null, it => it.IsDeleted == input.IsShowDeleted)
            .WhereIF(!string.IsNullOrEmpty(input.Name), it => it.Name.Contains(input.Name))
            .Select(x => new MetricTagsListOutput
            {
                Id = x.Id,
                Name = x.Name,
                Sort = x.Sort,
                CreatedTime = x.CreatedTime,
                CreatedUserId = x.CreatedUserId,
                LastModifiedTime = x.LastModifiedTime,
                LastModifiedUserId = x.LastModifiedUserId,
                DeleteTime = x.DeleteTime,
                DeleteUserId = x.DeleteUserId,
                IsDeleted = x.IsDeleted,
                Description = x.Description,
                TenantId = x.TenantId
            })
            .MergeTable()
            .OrderBy(x => x.LastModifiedTime)
            .OrderBy(x => x.CreatedTime)
            .ToPagedListAsync(input.CurrentPage, input.PageSize);

        return PageResult<MetricTagsListOutput>.SqlSugarPageResult(data);
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(MetricTagCrInput input)
    {
        if (await CheckNameAsync(input.Name))
            throw Oops.Oh(ErrorCode.K10006);
        var entity = input.Adapt<MetricTagsEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        var count = await _repository.AsInsertable(entity).CallEntityMethod(x => x.Create()).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        return count;
    }

    /// <inheritdoc />
    public async Task<int> UpdateAsync(MetricTagUpInput input)
    {
        //判断当前标签是否存在
        var oldEntity = await _repository.GetByIdAsync(input.Id);
        if (oldEntity == null)
            throw Oops.Oh(ErrorCode.K10007);

        if (await CheckNameAsync(input.Name))
            throw Oops.Oh(ErrorCode.K10006);

        var entity = input.Adapt<MetricTagsEntity>();
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

    /// <inheritdoc />
    public async Task<List<MetricTagsListOutput>> GetSelector()
    {
        var data = await _repository.AsQueryable()
            .Where(it => it.IsDeleted == 0)
            .Select(x => new MetricTagsListOutput
            {
                Id = x.Id,
                Name = x.Name,
                Sort = x.Sort,
                CreatedTime = x.CreatedTime,
                CreatedUserId = x.CreatedUserId,
                LastModifiedTime = x.LastModifiedTime,
                LastModifiedUserId = x.LastModifiedUserId,
                DeleteTime = x.DeleteTime,
                DeleteUserId = x.DeleteUserId,
                IsDeleted = x.IsDeleted,
                Description = x.Description,
                TenantId = x.TenantId
            })
            .MergeTable()
            .OrderBy(x => x.LastModifiedTime)
            .OrderBy(x => x.CreatedTime)
            .ToListAsync();
        return data;
    }

    /// <summary>
    /// 检查当前标签名称是否存在
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<bool> CheckNameAsync(string name)
    {
        var isExist = await _repository.AsQueryable().AnyAsync(x => x.Name.Equals(name) && x.IsDeleted == 0);
        return isExist;
    }
}