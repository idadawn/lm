namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标分类服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-12.
/// </summary>
public class MetricCategoryService : IMetricCategoryService, ITransient
{
    /// <summary>
    /// 标签仓库.
    /// </summary>
    private readonly ISqlSugarRepository<MetricCategoryEntity> _repository;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 初始化一个<see cref="MetricCategoryService"/>类型的新实例.
    /// </summary>
    public MetricCategoryService(ISqlSugarRepository<MetricCategoryEntity> repository, ISqlSugarClient context)
    {
        _repository = repository;
        _db = context.AsTenant();
    }

    /// <inheritdoc />
    public async Task<MetricCategoryInfoOutput> GetAsync(string id)
    {
        var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
        var info = entity.Adapt<MetricCategoryInfoOutput>();
        if (info.CategoryIdTree.IsNotEmptyOrNull())
            info.CategoryIds = info.CategoryIdTree.Split(",").ToList();
        return info;
    }

    /// <inheritdoc />
    public async Task<List<MetricCategoryListOutput>> GetListAsync(MetricCategoryListQueryInput input)
    {
        var data = await _repository.AsQueryable()
            .WhereIF(input.IsShowDeleted != null, it => it.IsDeleted == input.IsShowDeleted)
            .WhereIF(!string.IsNullOrEmpty(input.Name), it => it.Name.Contains(input.Name))
            .Select(x => new MetricCategoryListOutput
            {
                Id = x.Id,
                Name = x.Name,
                Sort = x.Sort,
                OwnId = x.OwnId,
                ParentId = x.ParentId,
                CategoryIdTree = x.CategoryIdTree,
                CreatedTime = x.CreatedTime,
                CreatedUserId = x.CreatedUserId,
                LastModifiedTime = x.LastModifiedTime,
                LastModifiedUserId = x.LastModifiedUserId,
                DeleteTime = x.DeleteTime,
                DeleteUserId = x.DeleteUserId,
                Description = x.Description,
                IsDeleted = x.IsDeleted,
                TenantId = x.TenantId,
            })
            .ToPageListAsync(input.CurrentPage, input.PageSize);

        foreach (var x in data) x.CategoryIds = x.CategoryIdTree.Split(",").ToList();
        var treeList = data.Any(x => x.ParentId.Equals("-1"))
            ? data.OrderBy(x => x.Sort).ToList().ToTree("-1")
            : data.OrderBy(x => x.Sort).ToList().ToTree("0");

        return treeList;
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(MetricCategoryCrInput input)
    {
        // 查询相同级别指标目录是否存在.
        var isExist = await _repository.AsQueryable().AnyAsync(x => x.ParentId.Equals(input.ParentId) && x.Name.Equals(input.Name));
        if (isExist)
            throw Oops.Oh(ErrorCode.K10001);

        var entity = input.Adapt<MetricCategoryEntity>();
        entity.Id = SnowflakeIdHelper.NextId();


        var idList = new List<string> { entity.Id };

        if (entity.ParentId != "-1")
        {
            var ids = (await _repository.AsSugarClient().Queryable<MetricCategoryEntity>().ToParentListAsync(it => it.ParentId, entity.ParentId)).Select(x => x.Id).ToList();
            idList.AddRange(ids);
        }
        idList.Reverse();
        entity.CategoryIdTree = string.Join(",", idList);

        var count = await _repository.AsInsertable(entity).CallEntityMethod(x => x.Create()).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        return count < 0 ? throw Oops.Oh(ErrorCode.K10000) : count;
    }

    /// <inheritdoc />
    public async Task<int> UpdateAsync(MetricCategoryUpInput input)
    {
        // 判断当前指标是否存在.
        var oldEntity = await GetAsync(input.Id);
        if (oldEntity == null)
            throw Oops.Oh(ErrorCode.K10002);

        // 判断当前指标目录是否存在.
        var isExist = await _repository.AsQueryable().AnyAsync(x => x.ParentId.Equals(input.ParentId) && x.IsDeleted == 0 && x.Name.Equals(input.Name));
        if (isExist)
            throw Oops.Oh(ErrorCode.K10001);

        // 当前指标目录不能为自己的子指标目录.
        var childIdListById = await GetChildIdListWithSelfById(oldEntity.Id);
        if (childIdListById.Contains(input.ParentId))
            throw Oops.Oh(ErrorCode.K10003);

        var entity = input.Adapt<MetricCategoryEntity>();
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
                    var ids = (await _repository.AsSugarClient().Queryable<MetricCategoryEntity>().ToParentListAsync(it => it.ParentId, input.ParentId)).Select(x => x.Id).ToList();
                    idList.AddRange(ids);
                }
                idList.Reverse();
                entity.CategoryIdTree = string.Join(",", idList);

                // 修改包含当前id值的CategoryIdTree，不包括自身.
                var childList = await _repository.AsQueryable().Where(x => x.CategoryIdTree.Contains(oldEntity.Id) && x.Id != oldEntity.Id).ToListAsync();
                if (childList.Any())
                {
                    childList.ForEach(item =>
                    {
                        var childTreeList = item.CategoryIdTree.Split(oldEntity.Id).LastOrDefault();
                        item.CategoryIdTree = entity.CategoryIdTree + childTreeList;
                    });
                    await _repository.AsSugarClient().Updateable(childList).UpdateColumns(x => x.CategoryIdTree).ExecuteCommandAsync();
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
            throw Oops.Oh(ErrorCode.K10004);
        }
    }

    /// <inheritdoc />
    public async Task<int> DeleteAsync(string id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return 1;
        // 该指标目录下有子指标目录,不能删除.
        var childList = await _repository.AsQueryable().Where(x => x.ParentId.Equals(id) && x.IsDeleted == 0).ToListAsync();
        if (childList.Any())
            throw Oops.Oh(ErrorCode.K10005);

        var count = await _repository.AsUpdateable(entity)
        .CallEntityMethod(x => x.Delete())
        .UpdateColumns(it => new {
            it.DeleteTime,
            it.DeleteUserId,
            it.IsDeleted
        })
        .ExecuteCommandAsync();

        return count;
    }

    /// <inheritdoc />
    public async Task<List<MetricCategoryListOutput>> GetSelector()
    {
        var data = await _repository.AsQueryable()
            .Where(it => it.IsDeleted == 0)
            .Select(x => new MetricCategoryListOutput
            {
                Id = x.Id,
                Name = x.Name,
                Sort = x.Sort,
                OwnId = x.OwnId,
                ParentId = x.ParentId,
                CategoryIdTree = x.CategoryIdTree,
                CreatedTime = x.CreatedTime,
                CreatedUserId = x.CreatedUserId,
                LastModifiedTime = x.LastModifiedTime,
                LastModifiedUserId = x.LastModifiedUserId,
                DeleteTime = x.DeleteTime,
                DeleteUserId = x.DeleteUserId,
                Description = x.Description,
                IsDeleted = x.IsDeleted,
                TenantId = x.TenantId,
            })
            .ToListAsync();

        foreach (var x in data) x.CategoryIds = x.CategoryIdTree.Split(",").ToList();
        var treeList = data.Any(x => x.ParentId.Equals("-1"))
            ? data.OrderBy(x => x.Sort).ToList().ToTree("-1")
            : data.OrderBy(x => x.Sort).ToList().ToTree("0");

        return treeList;
    }

    /// <summary>
    /// 获取所有子节点Id集合，包含自己.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    private async Task<List<string>> GetChildIdListWithSelfById(string id)
    {
        var childIdList = await _repository.AsQueryable().Where(x => x.ParentId.Equals(id) && x.IsDeleted == 0).Select(u => u.Id).ToListAsync();
        childIdList.Add(id);
        return childIdList;
    }

    /// <summary>
    /// 构造 CategoryIdTree.
    /// </summary>
    private void StructureCategoryIdTree()
    {
        if (_repository.IsAny(x => SqlFunc.IsNullOrEmpty(SqlFunc.ToString(x.CategoryIdTree))))
        {
            var orgList = _repository.GetList(x => SqlFunc.IsNullOrEmpty(x.CategoryIdTree));

            orgList.ForEach(item =>
            {
                if (item.ParentId == "-1")
                {
                    item.CategoryIdTree = item.Id;
                }
                else
                {
                    var plist = _repository.AsQueryable().ToParentList(it => it.ParentId, item.Id).Select(x => x.Id);
                    plist = plist.Reverse();
                    item.CategoryIdTree = string.Join(",", plist);
                }
            });
            _repository.AsUpdateable(orgList).ExecuteCommand();
        }
    }

}