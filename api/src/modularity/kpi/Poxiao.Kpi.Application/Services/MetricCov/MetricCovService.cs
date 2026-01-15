using Microsoft.CodeAnalysis.Operations;

namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标价值链服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-18.
/// </summary>
public class MetricCovService : IMetricCovService, ITransient
{
    /// <summary>
    /// 标签仓库.
    /// </summary>
    private readonly ISqlSugarRepository<MetricCovEntity> _repository;

    private readonly IMetricGotService _gotService;
    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;


    /// <summary>
    /// 初始化一个<see cref="MetricCovService"/>类型的新实例.
    /// </summary>
    public MetricCovService(ISqlSugarRepository<MetricCovEntity> repository, ISqlSugarClient context, IMetricGotService gotService)
    {
        _repository = repository;
        _gotService = gotService;
        _db = context.AsTenant();
    }

    /// <inheritdoc />
    public async Task<MetricCovInfoOutput> GetAsync(string id)
    {
        var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
        var info = entity.Adapt<MetricCovInfoOutput>();
        return info;
    }

    /// <inheritdoc />
    public async Task<List<MetricCovListOutput>> GetListAsync(string gotId)
    {
        var data = await _repository.AsQueryable()
            .Where(x => gotId.Equals(x.GotId))
            .Select(x => new MetricCovListOutput
            {
                Id = x.Id,
                Name = x.Name,
                GotType = x.GotType,
                GotId = x.GotId,
                MetricId = x.MetricId,
                ParentId = x.ParentId,
                CovTreeId = x.CovTreeId,
                GotParentId = x.GotParentId,
                GotTreeId = x.GotTreeId,
                IsRoot = x.IsRoot,
                CreatedTime = x.CreatedTime,
                CreatedUserid = x.CreatedUserId,
                LastModifiedTime = x.LastModifiedTime,
                LastModifiedUserid = x.LastModifiedUserId,
                TenantId = x.TenantId,
            })
            .ToListAsync();


        foreach (var x in data) x.CovTreeIds = x.CovTreeId.Split(",").ToList();
        var treeList = data.Any(x => x.ParentId.Equals("-1"))
            ? data.OrderBy(x => x.CreatedTime).ToList().ToTree("-1")
            : data.OrderBy(x => x.CreatedTime).ToList().ToTree("0");

        return treeList;
    }

    /// <inheritdoc />
    public async Task<MetricCovInfoOutput> CreateAsync(MetricCovCrInput input)
    {
        // 查询相同级别指标目录是否存在.
        var isExist = await _repository.AsQueryable().AnyAsync(x => x.ParentId.Equals(input.ParentId) && x.GotId.Equals(input.GotId) && x.Name.Equals(input.Name));
        if (isExist)
            throw Oops.Oh(ErrorCode.K10012);

        var entity = input.Adapt<MetricCovEntity>();
        entity.Id = SnowflakeIdHelper.NextId();


        var idList = new List<string> { entity.Id };
        if (entity.ParentId != "-1")
        {
            var ids = (await _repository.AsSugarClient().Queryable<MetricCovEntity>().ToParentListAsync(it => it.ParentId, entity.ParentId)).Select(x => x.Id).ToList();
            idList.AddRange(ids);
        }
        idList.Reverse();
        entity.CovTreeId = string.Join(",", idList);

        entity.GotParentId = entity.ParentId;

        // 默认创建根节点,无需处理.
        if (entity.IsRoot)
        {
            entity.GotTreeId = entity.CovTreeId;
        }
        else
        {
            var gotList = new List<string> { entity.Id };
            if (entity.GotParentId != "-1")
            {
                var ids = (await _repository.AsSugarClient().Queryable<MetricCovEntity>().ToParentListAsync(it => it.GotParentId, entity.GotParentId)).Select(x => x.Id).ToList();
                gotList.AddRange(ids);
            }
            gotList.Reverse();
            entity.GotTreeId = string.Join(",", gotList);
        }

        var result = await _repository.AsInsertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(x => x.Create()).ExecuteReturnEntityAsync();
        var info = result.Adapt<MetricCovInfoOutput>();
        return info;
    }

    /// <inheritdoc />
    public async Task<int> UpdateAsync(MetricCovUpInput input)
    {
        // 判断当前指标是否存在.
        var oldEntity = await GetAsync(input.Id);
        if (oldEntity == null)
            throw Oops.Oh(ErrorCode.K10013);

        // 判断当前指标目录是否存在,原名称相同不检测.
        if (!oldEntity.Name.Equals(input.Name))
        {
            var isExist = await _repository.AsQueryable().AnyAsync(x => x.ParentId.Equals(input.ParentId) && x.GotId.Equals(input.GotId) && x.Name.Equals(input.Name));
            if (isExist)
                throw Oops.Oh(ErrorCode.K10012);
        }

        // 当前指标目录不能为自己的子指标目录.
        var childIdListById = await GetChildIdListWithSelfById(oldEntity.Id);
        if (childIdListById.Contains(input.ParentId))
            throw Oops.Oh(ErrorCode.K10014);

        var entity = input.Adapt<MetricCovEntity>();
        // 如果修改了ParentId,则需要重新计算CategoryIdTree的值,并且迁移当前指标下所有子指标的CategoryIdTree值;
        // CategoryIdTree的逻辑如下;
        // 1.ParentId=="-1" CategoryIdTree值为：Id值;
        // 2.ParentId!="-1" CategoryIdTree值为：ParentId的CategoryIdTree,拼接当前Id值;
        try
        {
            // 开启事务
            await _db.BeginTranAsync();
            if (oldEntity.ParentId != entity.ParentId)
            {
                var idList = new List<string> { oldEntity.Id };

                if (input.ParentId != "-1")
                {
                    var ids = (await _repository.AsSugarClient().Queryable<MetricCovEntity>().ToParentListAsync(it => it.ParentId, input.ParentId)).Select(x => x.Id).ToList();
                    idList.AddRange(ids);
                }
                idList.Reverse();
                entity.CovTreeId = string.Join(",", idList);

                // 修改包含当前id值的CategoryIdTree，不包括自身.
                var childList = await _repository.AsQueryable().Where(x => x.CovTreeId.Contains(oldEntity.Id) && x.Id != oldEntity.Id).ToListAsync();
                if (childList.Any())
                {
                    childList.ForEach(item =>
                    {
                        var childTreeList = item.CovTreeId.Split(oldEntity.Id).LastOrDefault();
                        item.CovTreeId = entity.CovTreeId + childTreeList;
                    });
                    await _repository.AsSugarClient().Updateable(childList).UpdateColumns(x => x.CovTreeId).ExecuteCommandAsync();
                }
            }

            if (oldEntity.IsRoot && oldEntity.GotParentId != entity.GotParentId)
            {
                var godList = new List<string> { oldEntity.Id };

                if (input.GotParentId != "-1")
                {
                    var ids = (await _repository.AsSugarClient().Queryable<MetricCovEntity>().ToParentListAsync(it => it.GotParentId, input.GotParentId)).Select(x => x.Id).ToList();
                    godList.AddRange(ids);
                }
                godList.Reverse();
                entity.GotTreeId = string.Join(",", godList);

                // 修改包含当前id值的CategoryIdTree，不包括自身.
                var childList = await _repository.AsQueryable().Where(x => x.GotTreeId.Contains(oldEntity.Id) && x.Id != oldEntity.Id).ToListAsync();
                if (childList.Any())
                {
                    childList.ForEach(item =>
                    {
                        var childTreeList = item.GotTreeId.Split(oldEntity.Id).LastOrDefault();
                        item.GotTreeId = entity.GotTreeId + childTreeList;
                    });
                    await _repository.AsSugarClient().Updateable(childList).UpdateColumns(x => x.GotTreeId).ExecuteCommandAsync();
                }
            }
            var count = await _repository.AsSugarClient().Updateable(entity)
                .IgnoreColumns(ignoreAllNullColumns: true)
                .CallEntityMethod(x => x.Update())
                .ExecuteCommandAsync();

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
    public async Task<bool> DeleteAsync(string id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return true;
        // 判断是否存在子节点，如果存在不能删除.
        var isHasChild = await _repository.IsAnyAsync(x => x.ParentId.Equals(entity.Id));
        if (isHasChild) throw Oops.Oh(ErrorCode.K10022);
        var flag = await _repository.DeleteByIdAsync(id);
        return flag;
    }

    /// <inheritdoc />
    public async Task<List<MetricCovSelectorOutput>> GetSelector()
    {
        // 获取所有指标价值链树.
        var data = await _repository
                        .AsQueryable()
                        .Where(x => x.GotType.Equals(GotType.Cov))
                        .Select(x => new MetricCovSelectorOutput
                        {
                            Id = x.Id,
                            Name = x.Name,
                            GotId = x.GotId,
                            ParentId = x.ParentId
                        })
                        .ToListAsync();

        var tree = data.ToTree("-1");
        return tree;
    }

    /// <inheritdoc />
    public async Task<List<MetricCovListOutput>> GetKpiListAsync(string tag)
    {
        var gotIds = await _gotService.GetGotIdByTag(tag);
        var data = await _repository.AsQueryable()
            .Where(x => gotIds.Contains(x.GotId))
            .Select(x => new MetricCovListOutput
            {
                Id = x.Id,
                Name = x.Name,
                GotType = x.GotType,
                GotId = x.GotId,
                MetricId = x.MetricId,
                ParentId = x.GotParentId,
                CovTreeId = x.GotParentId,
                IsRoot = x.IsRoot,
                CreatedTime = x.CreatedTime,
                CreatedUserid = x.CreatedUserId,
                LastModifiedTime = x.LastModifiedTime,
                LastModifiedUserid = x.LastModifiedUserId,
                TenantId = x.TenantId,
            })
            .ToListAsync();

        foreach (var x in data) x.CovTreeIds = x.CovTreeId.Split(",").ToList();
        var treeList = data.Any(x => x.ParentId.Equals("-1"))
            ? data.OrderBy(x => x.CreatedTime).ToList().ToTree("-1")
            : data.OrderBy(x => x.CreatedTime).ToList().ToTree("0");

        return treeList;
    }

    /// <summary>
    /// 获取所有子节点Id集合，包含自己.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    private async Task<List<string>> GetChildIdListWithSelfById(string id)
    {
        var childIdList = await _repository.AsQueryable().Where(x => x.ParentId.Equals(id)).Select(u => u.Id).ToListAsync();
        childIdList.Add(id);
        return childIdList;
    }
}