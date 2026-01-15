using Poxiao.Infrastructure.Filter;
#pragma warning disable SA1519

namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标思维图服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-5.
/// </summary>
public class MetricGotService : IMetricGotService, ITransient
{
    /// <summary>
    /// 标签仓库.
    /// </summary>
    private readonly ISqlSugarRepository<MetricGotEntity> _repository;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 初始化一个<see cref="MetricGotService"/>类型的新实例.
    /// </summary>
    public MetricGotService(ISqlSugarRepository<MetricGotEntity> repository, ISqlSugarClient context)
    {
        _repository = repository;
        _db = context.AsTenant();
    }

    /// <inheritdoc />
    public async Task<MetricGotInfoOutput> GetAsync(string id)
    {
        var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
        var info = entity.Adapt<MetricGotInfoOutput>();
        return info;
    }

    /// <inheritdoc />
    public async Task<PagedResultDto<MetricGotListOutput>> GetListAsync(GotType type, MetricGotListQueryInput input)
    {
        var data = await _repository.AsQueryable()
            .Where(x => type.ToString().Equals(x.Type))
            .WhereIF(input.Keyword != null && !input.Keyword.IsNullOrEmpty(), x => x.Name.Contains(input.Keyword) || x.Id.Contains(input.Keyword))
            .WhereIF(input.MetricTags is { Count: > 0 }, x => x.MetricTag != null && input.MetricTags.Contains(x.MetricTag))
            .Select(x => new MetricGotListOutput
            {
                Id = x.Id,
                Type = x.Type,
                Sort = x.Sort,
                Name = x.Name,
                Description = x.Description,
                ImgName = x.ImgName,
                MetricTag = x.MetricTag,
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

        var pageList = PagedResultDto<MetricGotListOutput>.SqlSugarPageResult(data);
        if (pageList.List.Count < 1) return pageList;
        pageList.List.ForEach(x => x.TypeStr = x.Type?.GetDescription());
        // 获取所有 MetricTag
        var tags = pageList.List.Where(x => x.MetricTag.IsNotEmptyOrNull()).Select(x => x.MetricTag).ToList();
        if (tags.Count <= 0) return pageList;

        // 获取所有 TagIds
        var tagIds = tags.SelectMany(tag => tag.Split(",", StringSplitOptions.RemoveEmptyEntries)).ToList();
        if (tagIds.Count <= 0) return pageList;

        // 获取 MetricTags 的字典
        var dicTags = await _repository.AsSugarClient().Queryable<MetricTagsEntity>().Where(x => tagIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name);
        if (dicTags.Count <= 0) return pageList;

        // 设置 MetricTagNames
        pageList.List.ForEach(item =>
        {
            var tagIdList = item.MetricTag.Split(",", StringSplitOptions.RemoveEmptyEntries);
            var tagNames = tagIdList
                .Where(tagId => dicTags.ContainsKey(tagId))
                .Select(tagId => dicTags[tagId].ToString())
                .ToList();
            if (tagNames.Count <= 0) return;
            item.MetricTagNames.AddRange(tagNames);
        });
        return pageList;
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(MetricGotCrInput input)
    {
        var entity = input.Adapt<MetricGotEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        var count = await _repository.AsInsertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(x => x.Create()).ExecuteCommandAsync();
        return count;
    }

    /// <inheritdoc />
    public async Task<int> UpdateAsync(MetricGotUpInput input)
    {
        var entity = input.Adapt<MetricGotEntity>();
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
        var isHasChild = await _repository.AsSugarClient().Queryable<MetricCovEntity>()
            .AnyAsync(x => x.GotParentId != null && x.GotParentId.Equals(entity.Id));
        if (isHasChild) throw Oops.Oh(ErrorCode.K10022);
        try
        {
            // 开启事务
            await _db.BeginTranAsync();

            var count = await _repository.AsDeleteable().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
            if (entity.Type == GotType.Cov)
                count += await _repository.AsSugarClient().Deleteable<MetricCovEntity>().Where(x => x.GotId.Equals(id)).ExecuteCommandAsync();
            else
                count += await _repository.AsSugarClient().Deleteable<MetricDashEntity>().Where(x => x.GotId.Equals(id)).ExecuteCommandAsync();
            await _db.CommitTranAsync();
            return count;
        }
        catch (Exception)
        {
            await _db.RollbackTranAsync();
            throw Oops.Oh(ErrorCode.K10015);
        }
    }

    /// <inheritdoc />
    public async Task<List<string>> GetGotIdByTag(string tag)
    {
        var list = new List<string>();

        if (tag.Equals(AggTypeConst.Other))
            list = await _repository.AsQueryable()
                .Where(x => x.MetricTag == null || x.MetricTag == "")
                .Select(x => x.Id).ToListAsync();
        else
            list = await _repository.AsQueryable()
               .Where(x => x.MetricTag != null && x.MetricTag.Equals(tag))
               .Select(x => x.Id).ToListAsync();

        return list;
    }
}