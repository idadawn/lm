namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标定义服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-12.
/// </summary>
public class MetricInfoService : IMetricInfoService, ITransient
{
    /// <summary>
    /// 标签仓库.
    /// </summary>
    private readonly ISqlSugarRepository<MetricInfoEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 数据服务
    /// </summary>
    private readonly IDbService _dbService;

    /// <summary>
    /// IInfluxDBManager
    /// </summary>
    private readonly IInfluxDBManager _influxDbManager;

    /// <summary>
    /// 初始化一个<see cref="MetricInfoService"/>类型的新实例.
    /// </summary>
    public MetricInfoService(
        ISqlSugarRepository<MetricInfoEntity> repository,
        IUserManager userManager,
        IDbService dbService,
        IInfluxDBManager influxDbManager
    )
    {
        _repository = repository;
        _userManager = userManager;
        _dbService = dbService;
        _influxDbManager = influxDbManager;
    }

    /// <inheritdoc />
    public async Task<MetricInfoInfoOutput> GetAsync(string id)
    {
        var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
        var info = entity.Adapt<MetricInfoInfoOutput>();
        await DealDataAsync(info);
        return info;
    }

    /// <inheritdoc />
    public async Task<PagedResultDto<MetricInfoListOutput>> GetListAsync(
        MetricInfoListQueryInput input
    )
    {
        var data = await _repository
            .AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(input.MenuId), it => it.MetricCategory == input.MenuId)
            .WhereIF(input.IsEnabled != null, it => it.IsEnabled == input.IsEnabled)
            .WhereIF(input.Type != null, it => it.Type == input.Type)
            .WhereIF(input.IsShowDeleted != null, it => it.IsDeleted == input.IsShowDeleted)
            .WhereIF(
                input.Tags != null && input.Tags.Count > 0,
                it => input.Tags!.Contains(it.MetricTag)
            )
            .WhereIF(
                !string.IsNullOrEmpty(input.Keyword),
                it => (it.Name.Contains(input.Keyword) || it.Code.Contains(input.Keyword))
            )
            .Select(x => new MetricInfoListOutput
            {
                Id = x.Id,
                Sort = x.Sort,
                Type = x.Type.ToString(),
                Name = x.Name,
                Code = x.Code,
                DateModelType = x.DateModelType.ToString(),
                DataModelId = x.DataModelId,
                Format = x.Format,
                Expression = x.Expression,
                Dimensions = x.Dimensions,
                TimeDimensions = x.TimeDimensions,
                DisplayMode = x.DisplayMode.ToString(),
                CreatedTime = x.CreatedTime,
                CreatedUserid = x.CreatedUserId,
                LastModifiedTime = x.LastModifiedTime,
                LastModifiedUserid = x.LastModifiedUserId,
                DeleteTime = x.DeleteTime,
                DeleteUserid = x.DeleteUserId,
                IsDeleted = x.IsDeleted,
                IsEnabled = x.IsEnabled,
                Description = x.Description,
                TenantId = x.TenantId,
                MetricTag = x.MetricTag,
                MetricCategory = x.MetricCategory,
            })
            .MergeTable()
            .OrderBy(x => x.CreatedTime)
            .OrderBy(x => x.LastModifiedTime)
            .OrderBy(x => x.Name)
            .ToPagedListAsync(input.CurrentPage, input.PageSize);

        var pageList = PagedResultDto<MetricInfoListOutput>.SqlSugarPageResult(data);
        if (pageList.List.Count <= 0)
            return pageList;

        await DealDataAsync(pageList.List);
        return pageList;
    }

    /// <summary>
    /// 处理数据.
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    private async Task DealDataAsync(List<MetricInfoListOutput> list)
    {
        // 设置 TypeName
        list.ForEach(x => x.TypeName = x.Type.ToEnum<MetricType>().GetDescription());

        var categorizes = list.Where(x => x.MetricCategory.IsNotEmptyOrNull())
            .Select(x => x.MetricCategory)
            .Distinct()
            .ToList();
        if (categorizes.Count > 0)
        {
            // 获取 MetricCategory 的字典
            var dicCategory = await _repository
                .AsSugarClient()
                .Queryable<MetricCategoryEntity>()
                .Where(x => categorizes.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            // 设置 MetricCategoryName
            list.ForEach(item =>
            {
                if (dicCategory.TryGetValue(item.MetricCategory, out var categoryName))
                {
                    item.MetricCategoryName = categoryName.ToString();
                }
            });
        }

        // 获取所有 MetricTag
        var tags = list.Where(x => x.MetricTag.IsNotEmptyOrNull())
            .Select(x => x.MetricTag)
            .ToList();
        if (tags.Count <= 0)
            return;

        // 获取所有 TagIds
        var tagIds = tags.SelectMany(tag => tag.Split(",", StringSplitOptions.RemoveEmptyEntries))
            .ToList();
        if (tagIds.Count <= 0)
            return;

        // 获取 MetricTags 的字典
        var dicTags = await _repository
            .AsSugarClient()
            .Queryable<MetricTagsEntity>()
            .Where(x => tagIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name);
        if (dicTags.Count <= 0)
            return;

        // 设置 MetricTagNames
        list.ForEach(item =>
        {
            var tagIdList = item.MetricTag.Split(",", StringSplitOptions.RemoveEmptyEntries);
            var tagNames = tagIdList
                .Where(tagId => dicTags.ContainsKey(tagId))
                .Select(tagId => dicTags[tagId].ToString())
                .ToList();
            if (tagNames.Count <= 0)
                return;
            item.MetricTagNames.AddRange(tagNames);
        });
    }

    /// <summary>
    /// 处理数据
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    private async Task DealDataAsync(MetricInfoInfoOutput entity)
    {
        entity.TypeName = entity.Type.GetDescription();
        entity.ParentIds = entity.ParentId.Split2List(",", true);
        if (entity.Dimensions != null)
        {
            entity.Dimensions = entity
                .Dimensions.OrderByDescending(x =>
                    AggTypeConst.DbByDateRangeFilterModel.Contains(x.dataType)
                )
                .ToList();
            entity.DimensionItems = entity.Dimensions?.Select(x => x.field).ToList();
        }

        if (entity.MetricCategory.IsNotEmptyOrNull())
        {
            // 获取 MetricCategory 的字典
            var dicCategory = await _repository
                .AsSugarClient()
                .Queryable<MetricCategoryEntity>()
                .Where(x => entity.MetricCategory.Equals(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name);
            if (dicCategory.TryGetValue(entity.MetricCategory, out var categoryName))
            {
                entity.MetricCategoryName = categoryName.ToString();
            }
        }

        if (entity.MetricTag.IsNotEmptyOrNull())
        {
            var tagIdList = entity.MetricTag.Split(",", StringSplitOptions.RemoveEmptyEntries);
            var dicTags = await _repository
                .AsSugarClient()
                .Queryable<MetricTagsEntity>()
                .Where(x => tagIdList.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name);
            if (dicTags.Count <= 0)
                return;

            var tagNames = tagIdList
                .Where(tagId => dicTags.ContainsKey(tagId))
                .Select(tagId => dicTags[tagId].ToString())
                .ToList();
            if (tagNames.Count <= 0)
                return;

            entity.MetricTagNames.AddRange(tagNames);
        }
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(MetricInfoCrInput input)
    {
        if (await CheckNameAsync(input.Name))
            throw Oops.Oh(ErrorCode.K10010);
        if (await CheckCodeAsync(input.Code))
            throw Oops.Oh(ErrorCode.K10011);
        var entity = input.Adapt<MetricInfoEntity>();
        DealBasicMetricAsync(input, entity);

        entity.Id = SnowflakeIdHelper.NextId();

        var count = await _repository
            .AsInsertable(entity)
            .CallEntityMethod(x => x.Create())
            .IgnoreColumns(ignoreNullColumn: true)
            .ExecuteCommandAsync();
        return count;
    }

    /// <summary>
    /// 处理基础指标数据
    /// </summary>
    /// <param name="input"></param>
    /// <param name="entity"></param>
    private void DealBasicMetricAsync(MetricInfoCrInput input, MetricInfoEntity entity)
    {
        entity.ParentId = "-1";
        switch (input.AggType)
        {
            case DBAggType.SUM:
                entity.Expression = $"SUM('{input.Column.field}')";
                break;
            case DBAggType.MAX:
                entity.Expression = $"MAX('{input.Column.field}')";
                break;
            case DBAggType.MIN:
                entity.Expression = $"MIN('{input.Column.field}')";
                break;
            case DBAggType.AVG:
                entity.Expression = $"AVG('{input.Column.field}')";
                break;
            case DBAggType.COUNT:
                entity.Expression = $"COUNT('{input.Column.field}')";
                break;
            case DBAggType.COUNTDISTINCT:
                entity.Expression = $"COUNT_DISTINCT('{input.Column.field}')";
                break;
        }
    }

    /// <inheritdoc />
    public async Task<int> UpdateAsync(MetricInfoUpInput input)
    {
        // 判读是否存在
        var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(input.Id));
        if (entity == null)
            throw Oops.Oh(ErrorCode.K10019);
        entity = input.Adapt<MetricInfoEntity>();
        var count = await _repository
            .AsUpdateable(entity)
            .IgnoreColumns(ignoreAllNullColumns: true)
            .CallEntityMethod(x => x.Update())
            .ExecuteCommandAsync();
        return count;
    }

    /// <inheritdoc />
    public async Task<int> DeleteAsync(List<string> ids)
    {
        var count = await _repository
            .AsSugarClient()
            .Updateable<MetricInfoEntity>()
            .SetColumns(x => new MetricInfoEntity()
            {
                IsDeleted = 1,
                DeleteTime = SqlFunc.GetDate(),
                DeleteUserId = _userManager.UserId,
                LastModifiedUserId = _userManager.UserId,
                LastModifiedTime = SqlFunc.GetDate(),
            })
            .Where(x => ids.Contains(x.Id))
            .ExecuteCommandAsync();
        return count;
    }

    /// <inheritdoc />
    public async Task<int> SetEnableAsync(List<string> ids, bool isEnabled)
    {
        var count = await _repository
            .AsSugarClient()
            .Updateable<MetricInfoEntity>()
            .SetColumns(x => new MetricInfoEntity()
            {
                LastModifiedUserId = _userManager.UserId,
                LastModifiedTime = SqlFunc.GetDate(),
                IsEnabled = isEnabled,
            })
            .Where(it => ids.Contains(it.Id))
            .ExecuteCommandAsync();
        return count;
    }

    /// <inheritdoc />
    public Task<List<MetricAggInfoListOutput>> GetAggListAsync(TableFieldOutput input)
    {
        // 枚举DBAggType 生成List
        var aggTypes = Enum.GetValues(typeof(DBAggType));
        var list = (
            from DBAggType agg in aggTypes
            select new MetricAggInfoListOutput
            {
                AggType = agg.ToString(),
                DisplayName = agg.GetDescription(),
                IsDisable = true,
            }
        ).ToList();
        var isDisable = input.dataType.IsCanAgg();
        foreach (
            var agg in list.Where(agg =>
                agg.AggType == DBAggType.SUM.ToString() || agg.AggType == DBAggType.AVG.ToString()
            )
        )
        {
            agg.IsDisable = isDisable;
        }
        return Task.FromResult(list);
    }

    /// <summary>
    /// 检查当前指标名称.
    /// </summary>
    /// <param name="name">指标名称.</param>
    /// <returns></returns>
    public async Task<bool> CheckNameAsync(string name)
    {
        var isExist = await _repository
            .AsQueryable()
            .AnyAsync(x => x.Name.Equals(name) && x.IsDeleted == 0);
        return isExist;
    }

    /// <summary>
    /// 检查当前指标名称.
    /// </summary>
    /// <param name="code">指标名称.</param>
    /// <returns></returns>
    public async Task<bool> CheckCodeAsync(string code)
    {
        var isExist = await _repository
            .AsQueryable()
            .AnyAsync(x => x.Code.Equals(code) && x.IsDeleted == 0);
        return isExist;
    }

    /// <inheritdoc />
    public async Task<List<MetricInfoAllOutput>> GetAllAsync(bool isDerive)
    {
        var list = await _repository
            .AsQueryable()
            .Where(x => x.IsDeleted == 0 && x.IsEnabled == true)
            .WhereIF(isDerive, x => !x.Type.Equals(MetricType.Derive.ToString()))
            .Select(x => new MetricInfoAllOutput { Id = x.Id, Name = x.Name })
            .ToListAsync();
        return list;
    }

    /// <inheritdoc />
    public async Task<MetricInfoDimensionsOutput> GetDimsUnionAsync(MetricInfoDimQryCrInput input)
    {
        var list = await _repository
            .AsQueryable()
            .Where(x => input.MetricIds.Contains(x.Id))
            .Select(x => x.Dimensions)
            .ToListAsync();

        var dimensions = list.Where(dim => dim != null)
            .SelectMany(dim =>
                dim.ToObject<List<TableFieldOutput>>() ?? Enumerable.Empty<TableFieldOutput>()
            )
            .Where(tmp => !tmp.field.IsNullOrEmpty())
            .GroupBy(tmp => tmp.field)
            .Select(group => group.First())
            .ToList();

        var rlt = new MetricInfoDimensionsOutput { Dimensions = dimensions };
        return rlt;
    }

    /// <inheritdoc />
    public async Task<MetricInfoDimensionsOutput> GetDimensionsAsync(MetricInfoDimQryCrInput input)
    {
        // 获取每个指标的维度列表
        var dimensionsLists = await _repository
            .AsQueryable()
            .Where(x => input.MetricIds.Contains(x.Id))
            .Select(x => x.Dimensions)
            .ToListAsync();

        // 确保没有空的维度列表
        var filteredDimensionsLists = dimensionsLists.Where(dimList => dimList != null).ToList();

        // 将每个维度列表转换为TableFieldOutput对象的集合
        var convertedDimensionsLists = filteredDimensionsLists
            .Select(dimList =>
                dimList.ToObject<List<TableFieldOutput>>() ?? new List<TableFieldOutput>()
            )
            .ToList();

        // 确保所有列表都有维度，否则返回空的结果
        if (convertedDimensionsLists.Any(list => !list.Any()))
        {
            return new MetricInfoDimensionsOutput { Dimensions = new List<TableFieldOutput>() };
        }

        // 找出所有列表的交集
        var intersection = convertedDimensionsLists
            .Skip(1)
            .Aggregate(
                new HashSet<TableFieldOutput>(
                    convertedDimensionsLists.First(),
                    new TableFieldOutputComparer()
                ),
                (h, e) =>
                {
                    h.IntersectWith(e);
                    return h;
                }
            )
            .ToList();

        // dataType=date或者dataType=datetime类型的维度排在前面
        intersection = intersection
            .OrderBy(x => AggTypeConst.DbByDateRangeFilterModel.Contains(x.dataType))
            .ToList();

        var rlt = new MetricInfoDimensionsOutput { Dimensions = intersection };
        return rlt;
    }

    // 用于比较TableFieldOutput对象的比较器
    private class TableFieldOutputComparer : IEqualityComparer<TableFieldOutput>
    {
        public bool Equals(TableFieldOutput? x, TableFieldOutput? y)
        {
            // 根据需要定义如何确定两个TableFieldOutput对象是否相等
            return x.field == y.field;
        }

        public int GetHashCode(TableFieldOutput obj)
        {
            return obj.field.GetHashCode();
        }
    }

    /// <inheritdoc />
    public async Task<ModelDataListOutput> GetFilterMetricDataAsync(ModelDataListQueryInput input)
    {
        var result = new ModelDataListOutput();
        // 获取每个指标的维度列表.
        var metrics = await _repository
            .AsQueryable()
            .Where(x => input.Metrics.Contains(x.Id))
            .ToListAsync();

        var tasks = metrics.Select(async metric =>
        {
            var dataModel = metric.DataModelId.ToObject<DbSchemaOutput>();
            var dim = input.ColumnField;
            var sortBy = input.OrderByField;

            var filterInput = new ModelDataQueryInput()
            {
                ColumnField = dim,
                SchemaName = dataModel.Id,
                LinkId = dataModel.ParentId,
                OrderByField = sortBy,
            };
            return await _dbService.GetFilterModelDataAsync(filterInput);
        });

        var dataResults = await Task.WhenAll(tasks);

        foreach (var data in dataResults)
        {
            result.Data.AddRange(data.Data);
            result.FilterModel = data.FilterModel; // Note: This might overwrite if models differ, but assuming consistent model for now or union behavior
            result.ExecutedSql += data.ExecutedSql + Environment.NewLine;
            result.Metas.AddRange(data.Metas);
            result.TotalTime += data.TotalTime;
        }

        result.Data = result.Data.Distinct().ToList();
        return result;
    }

    /// <inheritdoc />
    public async Task<int> CopyAsync(string id)
    {
        // 根据Id拷贝一个指标，从命名指标和指标编码.
        var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
        if (entity == null)
            throw Oops.Oh(ErrorCode.K10019);
        var newEntity = entity.Adapt<MetricInfoEntity>();
        // 随机生成6位不重复的字符.
        var random = new Random();
        var randomStr = random.NextLetterAndNumberString(6);
        newEntity.Id = SnowflakeIdHelper.NextId();
        newEntity.Name = $"{entity.Name}_copy_{randomStr}";
        newEntity.Code = $"{entity.Code}_copy_{randomStr}";

        if (await CheckNameAsync(newEntity.Name))
            throw Oops.Oh(ErrorCode.K10010);
        if (await CheckCodeAsync(newEntity.Code))
            throw Oops.Oh(ErrorCode.K10011);

        var count = await _repository
            .AsInsertable(newEntity)
            .CallEntityMethod(x => x.Create())
            .IgnoreColumns(ignoreNullColumn: true)
            .ExecuteCommandAsync();
        return count;
    }

    /// <inheritdoc />
    public async Task<List<MetricInfoListOutput>> GetListByIdAsync(List<string> metricId)
    {
        var data = await _repository
            .AsQueryable()
            .Where(it => metricId.Contains(it.Id))
            .Select(x => new MetricInfoListOutput
            {
                Id = x.Id,
                Sort = x.Sort,
                Type = x.Type.ToString(),
                Name = x.Name,
                Code = x.Code,
                DateModelType = x.DateModelType.ToString(),
                DataModelId = x.DataModelId,
                Format = x.Format,
                Expression = x.Expression,
                Dimensions = x.Dimensions,
                TimeDimensions = x.TimeDimensions,
                DisplayMode = x.DisplayMode.ToString(),
                CreatedTime = x.CreatedTime,
                CreatedUserid = x.CreatedUserId,
                LastModifiedTime = x.LastModifiedTime,
                LastModifiedUserid = x.LastModifiedUserId,
                DeleteTime = x.DeleteTime,
                DeleteUserid = x.DeleteUserId,
                IsDeleted = x.IsDeleted,
                IsEnabled = x.IsEnabled,
                Description = x.Description,
                TenantId = x.TenantId,
                MetricTag = x.MetricTag,
                MetricCategory = x.MetricCategory,
            })
            .MergeTable()
            .ToListAsync();

        return data;
    }

    /// <inheritdoc />
    public async Task<List<TableFieldOutput>> GetRtSeriesListAsync(string name)
    {
        var rlt = new List<TableFieldOutput>();
        try
        {
            _influxDbManager.Connect();
            var list = await _influxDbManager.GetSeriesByMeasurementAsync(name);
            if (list.Count > 0)
            {
                rlt = list.Select(x => new TableFieldOutput()
                {
                    field = x.AttrName,
                    fieldName = x.AttrDescription,
                    dataType = "string",
                    primaryKey = 0,
                    allowNull = 0,
                    dataLength = "50",
                })
                    .ToList();
            }
        }
        catch
        {
            // ignore.
        }
        return rlt;
    }

    /// <inheritdoc />
    public async Task<List<MetricInfoListForChatDto>> GetAll4ChatAsync()
    {
        var list = await _repository
            .AsQueryable()
            .Where(x => x.IsDeleted == 0 && x.IsEnabled == true)
            .Take(100)
            .Select(x => new MetricInfoListForChatDto { Id = x.Id, Name = x.Name })
            .ToListAsync();
        return list;
    }

    /// <inheritdoc />
    public async Task<MetricInfoInfoOutput> GetByNameAsync(string name)
    {
        var entity = await _repository.AsQueryable().FirstAsync(it => it.Name.Contains(name));
        var info = entity.Adapt<MetricInfoInfoOutput>();
        await DealDataAsync(info);
        return info;
    }

    /// <inheritdoc />
    public async Task<ModelChartDataOutput> GetMetricChartDataAsync(ModelDataAggQueryInput input)
    {
        // 1. Get Metric Info to find schema/db connection
        if (string.IsNullOrEmpty(input.MetricId))
            throw Oops.Oh("MetricId is required");

        var metric = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(input.MetricId));
        if (metric == null)
            throw Oops.Oh(ErrorCode.K10019);

        var dataModel = metric.DataModelId.ToObject<DbSchemaOutput>();

        // 2. Populate input with schema info if missing (or trust input if provided by caller?)
        // Usually safety dictates we use the trusted config from DB.
        input.SchemaName = dataModel.Id;
        input.LinkId = dataModel.ParentId;

        // 3. Set Aggregation Type from Metric Definition if not overridden
        if (input.AggType == DBAggType.None && metric.AggType.HasValue)
        {
            input.AggType = metric.AggType.Value;
        }
        else if (input.AggType == DBAggType.None)
        {
            input.AggType = DBAggType.SUM; // Default to SUM if not specified
        }

        // 4. Execute
        return await _dbService.GetMetricChartDataAsync(input);
    }
}
