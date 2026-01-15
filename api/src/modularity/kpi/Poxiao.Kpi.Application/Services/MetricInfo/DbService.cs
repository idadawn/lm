using System.Formats.Asn1;
using Senparc.Weixin.Work.AdvancedAPIs.OaDataOpen;

namespace Poxiao.Kpi.Application;

/// <summary>
/// 数据源服务
/// </summary>
public class DbService : IDbService, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<DbLinkEntity> _repository;

    /// <summary>
    /// 数据库管理.
    /// </summary>
    private readonly IDataBaseManager _dataBaseManager;

    /// <summary>
    /// Db连接池.
    /// </summary>
    private readonly IDbLinkService _dbLinkService;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// jsonClient.
    /// </summary>
    private readonly IJsonClient _jsonClient = new JsonClient();

    /// <summary>
    ///
    /// </summary>
    private readonly IInfluxDBManager _influxDbManager;

    /// <summary>
    /// 初始化一个<see cref="DbService"/>类型的新实例.
    /// </summary>
    public DbService(
        ISqlSugarRepository<DbLinkEntity> repository,
        IDataBaseManager dataBaseManager,
        IUserManager userManager,
        IDbLinkService dbLinkService,
        IJsonClient jsonClient,
        IInfluxDBManager influxDbManager
    )
    {
        _repository = repository;
        _dataBaseManager = dataBaseManager;
        _userManager = userManager;
        _dbLinkService = dbLinkService;
        _influxDbManager = influxDbManager;
        _jsonClient.Context = _repository.AsSugarClient();
    }

    /// <inheritdoc />
    public async Task<List<DataModel4DbOutput>> GetDbListAsync()
    {
        // 获取默认数据库信息
        var dbOptions = App.GetOptions<ConnectionStringsOptions>();
        var defaultConnection = dbOptions.ConnectionConfigs.Find(it =>
            it.ConfigId?.ToString() == "default"
        );
        var defaultDBType = defaultConnection.DbType.ToString();
        if (defaultDBType.Equals("Kdbndp"))
        {
            defaultDBType = "KingbaseES";
        }

        if (defaultDBType.Equals("Dm"))
        {
            defaultDBType = "DM8";
        }

        // 创建默认数据库
        var list = new List<DataModel4DbOutput>
        {
            new DataModel4DbOutput()
            {
                Id = "0",
                Name = "默认数据库",
                DbType = defaultDBType,
                Host = defaultConnection.Host,
                SortCode = -1,
            },
            //new DataModel4DbOutput() {
            //    Id = "99",
            //    Name = "InfluxDB",
            //    DbType = "InfluxDB",
            //    Host = defaultConnection.Host,
            //    SortCode = -1
            //},
        };

        var data = (
            await _repository
                .AsSugarClient()
                .Queryable<DbLinkEntity, UserEntity, UserEntity>(
                    (a, b, c) =>
                        new JoinQueryInfos(
                            JoinType.Left,
                            a.CreatorUserId == b.Id,
                            JoinType.Left,
                            a.LastModifyUserId == c.Id
                        )
                )
                .Where((a, b, c) => a.DeleteMark == null)
                .Select(
                    (a, b, c) =>
                        new DbLinkListOutput()
                        {
                            Id = a.Id,
                            creatorTime = a.CreatorTime,
                            creatorUser = SqlFunc.MergeString(b.RealName, "/", b.Account),
                            dbType = a.DbType,
                            enabledMark = a.EnabledMark,
                            fullName = a.FullName,
                            host = a.Host,
                            lastModifyTime = a.LastModifyTime,
                            lastModifyUser = SqlFunc.MergeString(c.RealName, "/", c.Account),
                            port = a.Port.ToString(),
                            sortCode = a.SortCode,
                        }
                )
                .Distinct()
                .MergeTable()
                .OrderBy((a) => a.sortCode)
                .OrderBy((a) => a.creatorTime, OrderByType.Desc)
                .ToListAsync()
        ).Adapt<List<DataModel4DbOutput>>();

        list.AddRange(data);
        return list;
    }

    /// <inheritdoc />
    public async Task<List<DbSchemaOutput>> GetDbSchemaListAsync()
    {
        var list = new List<DbSchemaOutput>();
        var dbSources = await GetDbListAsync();
        var tmp = dbSources.Adapt<List<DbSchemaOutput>>();
        list.AddRange(tmp);
        var ids = dbSources.Select(x => x.Id).ToList();
        var dbLinks = await _repository.AsQueryable().Where(x => ids.Contains(x.Id)).ToListAsync();

        #region 添加默认数据库

        var defaultLink = _dataBaseManager.GetTenantDbLink(
            _userManager.TenantId,
            _userManager.TenantDbName
        );
        var defaultTables = _dataBaseManager.GetTableInfos(defaultLink);
        var defaultViews = _dataBaseManager.GetTableInfos(defaultLink, true);
        var defaultlink = dbSources.Find(x => x.Id.Equals("0"));

        if (defaultlink != null)
        {
            foreach (var table in defaultTables)
            {
                var defaultSchema = new DbSchemaOutput();
                defaultSchema.Name = table.Description.IsNullOrEmpty()
                    ? table.Name
                    : table.Description;
                defaultSchema.Id = table.Name;
                defaultSchema.SchemaStorageType = SchemaStorageType.Table;
                defaultSchema.DbType = defaultlink.DbType;
                defaultSchema.Host = defaultlink.Host;
                defaultSchema.ParentId = defaultlink.Id;
                defaultSchema.SortCode = 1;
                list.Add(defaultSchema);
            }

            foreach (var view in defaultViews)
            {
                var defaultSchema = new DbSchemaOutput();
                defaultSchema.Name = view.Description.IsNullOrEmpty()
                    ? view.Name
                    : view.Description;
                defaultSchema.Id = view.Name;
                defaultSchema.SchemaStorageType = SchemaStorageType.View;
                defaultSchema.DbType = defaultlink.DbType;
                defaultSchema.Host = defaultlink.Host;
                defaultSchema.ParentId = defaultlink.Id;
                defaultSchema.SortCode = 1;
                list.Add(defaultSchema);
            }
        }

        #endregion

        //#region 添加InfluxDB
        //_influxDbManager.Connect();
        //var measurements = await _influxDbManager.GetAllMeasurementsAsync();
        //var influxDbSchema = new DbSchemaOutput();
        //influxDbSchema.Name = "石钢数据采集";
        //influxDbSchema.Id = measurements[0];
        //influxDbSchema.SchemaStorageType = SchemaStorageType.RealTime;
        //influxDbSchema.DbType = "InfluxDB";
        //influxDbSchema.Host = "39.106.150.90";
        //influxDbSchema.ParentId = "99";
        //influxDbSchema.SortCode = 1;
        //list.Add(influxDbSchema);
        //#endregion

        foreach (var link in dbLinks)
        {
            if (link == null)
                continue;
            var tables = _dataBaseManager.GetTableInfos(link);
            var views = _dataBaseManager.GetTableInfos(link, true);
            foreach (var table in tables)
            {
                var schema = new DbSchemaOutput();
                schema.Name = table.Description.IsNullOrEmpty() ? table.Name : table.Description;
                schema.Id = table.Name;
                schema.SchemaStorageType = SchemaStorageType.Table;
                schema.DbType = link?.DbType;
                schema.Host = link?.Host;
                schema.ParentId = link?.Id;
                schema.SortCode = 1;
                list.Add(schema);
            }

            foreach (var view in views)
            {
                var schema = new DbSchemaOutput();
                schema.Name = view.Description.IsNullOrEmpty() ? view.Name : view.Description;
                schema.Id = view.Name;
                schema.SchemaStorageType = SchemaStorageType.View;
                schema.DbType = link?.DbType;
                schema.Host = link?.Host;
                schema.ParentId = link?.Id;
                schema.SortCode = 2;
                list.Add(schema);
            }
        }
        var treeList = list.ToTree("-1");
        return treeList;
    }

    /// <inheritdoc />
    public async Task<DatabaseTableInfoOutput> GetSchemaInfoAsync(string linkId, string schemaName)
    {
        var info = new DatabaseTableInfoOutput();
        if (linkId.Equals("99"))
        {
            info.hasTableData = true;
            info.tableFieldList = new List<TableFieldOutput>()
            {
                new TableFieldOutput()
                {
                    primaryKey = 1,
                    fieldName = "时间",
                    field = "time",
                    dataType = "datetime",
                    allowNull = 0,
                    dataLength = "50",
                },
                new TableFieldOutput()
                {
                    primaryKey = 1,
                    fieldName = "点位描述",
                    field = "attrDescription",
                    dataType = "string",
                    allowNull = 0,
                    dataLength = "50",
                },
                new TableFieldOutput()
                {
                    primaryKey = 1,
                    fieldName = "点位id",
                    field = "attrId",
                    dataType = "string",
                    allowNull = 0,
                    dataLength = "50",
                },
                new TableFieldOutput()
                {
                    primaryKey = 1,
                    fieldName = "点位名称",
                    field = "attrName",
                    dataType = "string",
                    allowNull = 0,
                    dataLength = "50",
                },
                new TableFieldOutput()
                {
                    primaryKey = 1,
                    fieldName = "linkId",
                    field = "linkId",
                    dataType = "string",
                    allowNull = 0,
                    dataLength = "50",
                },
                new TableFieldOutput()
                {
                    primaryKey = 1,
                    fieldName = "linkName",
                    field = "linkName",
                    dataType = "string",
                    allowNull = 0,
                    dataLength = "50",
                },
                new TableFieldOutput()
                {
                    primaryKey = 1,
                    fieldName = "protocolType",
                    field = "protocolType",
                    dataType = "string",
                    allowNull = 0,
                    dataLength = "50",
                },
                new TableFieldOutput()
                {
                    primaryKey = 1,
                    fieldName = "值",
                    field = "value",
                    dataType = "string",
                    allowNull = 0,
                    dataLength = "50",
                },
                new TableFieldOutput()
                {
                    primaryKey = 1,
                    fieldName = "valueType",
                    field = "valueType",
                    dataType = "string",
                    allowNull = 0,
                    dataLength = "50",
                },
            };
            info.tableInfo = new TableInfoOutput()
            {
                newTable = "",
                table = "linkAttrValues",
                tableName = "唐钢数据采集表",
            };
            return info;
        }

        var link = await _dbLinkService.GetInfo(linkId);
        if (string.IsNullOrEmpty(schemaName))
            return info;
        var tenantLink =
            link
            ?? _dataBaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
        info = _dataBaseManager.GetDataBaseTableInfo(tenantLink, schemaName);
        return info;
    }

    /// <summary>
    /// 预览数据.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    public async Task<ModelDataListOutput> GetFilterModelDataAsync(ModelDataQueryInput input)
    {
        // 计时开始,计算函数执行所需的时间，单位毫秒
        var watch = new Stopwatch();
        watch.Start();
        if (string.IsNullOrEmpty(input.LinkId))
            throw Oops.Oh(ErrorCode.K10009);
        if (string.IsNullOrEmpty(input.SchemaName))
            throw Oops.Oh(ErrorCode.K10008);
        var info = new ModelDataListOutput();
        var link = await _dbLinkService.GetInfo(input.LinkId);
        var tenantLink =
            link
            ?? _dataBaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);

        if (input.ColumnField.fieldName.IsNullOrEmpty())
            input.ColumnField.fieldName = input.ColumnField.field;

        // 默认只有值
        info.FilterModel.Add(MetricFilterModel.ByValue.ToString());

        // 判断字段是否是值类型
        if (input.ColumnField.dataType.IsByValueFilterModel())
            info.FilterModel.Add(MetricFilterModel.ByRange.ToString());
        if (input.ColumnField.dataType.IsByDateRangFilterModel())
            info.FilterModel.Add(MetricFilterModel.ByDateRang.ToString());

        var columnsStr =
            $"{SanitizeIdentifier(input.ColumnField.field)} '{SanitizeIdentifier(input.ColumnField.fieldName)}'";
        var groupStr = $"{SanitizeIdentifier(input.ColumnField.field)}";
        var orderStr = "";
        if (input.OrderByField.field.IsNotEmptyOrNull())
            orderStr =
                $"{SanitizeIdentifier(input.OrderByField.field)} {input.OrderByField.SortBy}";

        var executedSql = new StringBuilder();
        executedSql.AppendFormatLine(
            "SELECT {0} FROM {1} ",
            columnsStr,
            SanitizeIdentifier(input.SchemaName)
        );
        executedSql.AppendFormatLine("GROUP BY {0} ", groupStr);
        if (orderStr.IsNotEmptyOrNull())
            executedSql.AppendFormatLine("ORDER BY {0} ", orderStr);
        var sql = executedSql.ToString();

        var data = await _dataBaseManager.GetListStringAsync(tenantLink, sql);

        info.ExecutedSql = sql;
        info.Data = data;
        info.Metas = new List<TableFieldOutput>() { input.ColumnField };
        info.TotalTime = watch.ElapsedMilliseconds;
        watch.Stop();
        return info;
    }

    /// <inheritdoc />
    public async Task<ModelDataOutput> GetMetricDataAsync(ModelDataAggQueryInput input)
    {
        // 计时开始,计算函数执行所需的时间，单位毫秒
        var watch = new Stopwatch();
        watch.Start();
        if (string.IsNullOrEmpty(input.LinkId))
            throw Oops.Oh(ErrorCode.K10009);
        if (string.IsNullOrEmpty(input.SchemaName))
            throw Oops.Oh(ErrorCode.K10008);
        var info = new ModelDataOutput();
        var link = await _dbLinkService.GetInfo(input.LinkId);
        var tenantLink =
            link
            ?? _dataBaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
        var fileId = input.ColumnField.field;
        var fieldName = input.ColumnField.fieldName;

        // 处理聚合方式.
        var sqlAggType = "";
        switch (input.AggType)
        {
            case DBAggType.SUM:
                sqlAggType = $"SUM({fileId}) {fileId}";
                break;
            case DBAggType.MAX:
                sqlAggType = $"MAX({fileId}) {fileId}";
                break;
            case DBAggType.MIN:
                sqlAggType = $"MIN({fileId}) {fileId}";
                break;
            case DBAggType.AVG:
                sqlAggType = $"AVG({fileId}) {fileId}";
                break;
            case DBAggType.COUNT:
                sqlAggType = $"COUNT({fileId}) {fileId}";
                break;
            case DBAggType.COUNT_DISTINCT:
                sqlAggType = $"COUNT_DISTINCT({fileId}) {fileId}";
                break;
        }

        var sqlDimensions = "";
        var groupStr = "";
        var orderStr = "";

        if (input.Dimensions != null)
        {
            switch (input.Granularity)
            {
                case GranularityType.Day:
                    sqlDimensions = $" DATE({input.Dimensions.field}) AS day, ";
                    groupStr = $" day ";
                    orderStr = $" day DESC ";
                    break;
                case GranularityType.Week:
                    sqlDimensions =
                        $" YEAR({input.Dimensions.field}) AS year, WEEK({input.Dimensions.field}) AS week, ";
                    groupStr = $" year, week ";
                    orderStr = $" year DESC, week DESC ";
                    break;
                case GranularityType.Month:
                    sqlDimensions =
                        $" YEAR({input.Dimensions.field}) AS year, MONTH({input.Dimensions.field}) AS month, ";
                    groupStr = $" year, month ";
                    orderStr = $" year DESC, month DESC ";
                    break;
                case GranularityType.Quarter:
                    sqlDimensions =
                        $" YEAR({input.Dimensions.field}) AS year, QUARTER({input.Dimensions.field}) AS quarter, ";
                    groupStr = $" year, quarter ";
                    orderStr = $" year DESC, quarter DESC ";
                    break;
                case GranularityType.Year:
                    sqlDimensions = $" YEAR({input.Dimensions.field}) AS year, ";
                    groupStr = $" year ";
                    orderStr = $" year DESC ";
                    break;
                case null:
                    sqlDimensions = $"{input.Dimensions.field} {input.Dimensions.fieldName}, ";
                    groupStr = $" {input.Dimensions.field} ";
                    orderStr = $" {input.Dimensions.field} DESC ";
                    break;
            }
        }

        // 如果维度信息展示所有数据，去除维度信息.
        if (input.DisplayOption is DisplayOption.All)
        {
            sqlDimensions = "";
            groupStr = "";
            orderStr = "";
        }

        var columnsStr = "";
        if (sqlDimensions.IsNotEmptyOrNull())
            columnsStr = sqlDimensions + sqlAggType;
        else
            columnsStr = sqlAggType;

        var executedSql = new StringBuilder();
        executedSql.AppendFormatLine(
            "SELECT {0} {1} FROM ( ",
            SanitizeIdentifier(fileId),
            SanitizeIdentifier(fieldName)
        );
        executedSql.AppendFormatLine(
            "SELECT {0} FROM {1} ",
            columnsStr,
            SanitizeIdentifier(input.SchemaName)
        );
        var sqlWhere = DealFilters(input.Filters);
        executedSql.AppendLine(sqlWhere);
        if (groupStr.IsNotEmptyOrNull())
            executedSql.AppendFormatLine("GROUP BY {0} ", groupStr);
        if (orderStr.IsNotEmptyOrNull())
            executedSql.AppendFormatLine("ORDER BY {0} ", orderStr);
        executedSql.AppendFormatLine("limit 1 ");
        executedSql.AppendFormatLine(") t_{0} ", SanitizeIdentifier(input.TaskId));
        var sql = executedSql.ToString();
        var data = await _dataBaseManager.GetStringAsync(tenantLink, sql);

        info.ExecutedSql = sql;
        info.Data = data;
        info.Metas = new List<TableFieldOutput>() { input.ColumnField };
        info.TotalTime = watch.ElapsedMilliseconds;
        watch.Stop();
        return info;
    }

    /// <inheritdoc />
    public async Task<ModelChartDataOutput> GetMetricChartDataAsync(ModelDataAggQueryInput input)
    {
        // 计时开始,计算函数执行所需的时间，单位毫秒
        var watch = new Stopwatch();
        watch.Start();
        if (string.IsNullOrEmpty(input.LinkId))
            throw Oops.Oh(ErrorCode.K10009);
        if (string.IsNullOrEmpty(input.SchemaName))
            throw Oops.Oh(ErrorCode.K10008);
        var info = new ModelChartDataOutput();
        var link = await _dbLinkService.GetInfo(input.LinkId);
        var tenantLink =
            link
            ?? _dataBaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
        var fileId = input.ColumnField.field;
        var fieldName = input.ColumnField.fieldName;

        // 处理聚合方式.
        var sqlAggType = "";
        switch (input.AggType)
        {
            case DBAggType.SUM:
                sqlAggType = $"SUM({fileId}) AS value";
                break;
            case DBAggType.MAX:
                sqlAggType = $"MAX({fileId}) AS value";
                break;
            case DBAggType.MIN:
                sqlAggType = $"MIN({fileId}) AS value";
                break;
            case DBAggType.AVG:
                sqlAggType = $"AVG({fileId}) AS value";
                break;
            case DBAggType.COUNT:
                sqlAggType = $"COUNT({fileId}) AS value";
                break;
            case DBAggType.COUNT_DISTINCT:
                sqlAggType = $"COUNT(DISTINCT {fileId}) AS value";
                break;
        }

        var sqlDimensions = "";
        var groupStr = $" dimension ";
        var orderStr = $" dimension {input.SortBy.ToString().ToUpper()} ";

        // 处理维度信息.
        // 如果维度信息是时间类型，根据时间粒度处理.
        // 如果维度信息是值类型，直接展示.

        if (input.Dimensions != null)
        {
            // 处理sql_mode=only_full_group_by
            sqlDimensions = input.Dimensions.field.Equals(fileId)
                ? $"ANY_VALUE({input.Dimensions.field}) AS dimension, "
                : $"{input.Dimensions.field} AS dimension, ";
            if (input.Dimensions.dataType.IsByDateRangFilterModel())
            {
                switch (input.Granularity)
                {
                    case GranularityType.Day:
                        sqlDimensions = $" DATE({input.Dimensions.field}) AS dimension, ";
                        break;
                    case GranularityType.Week:
                        sqlDimensions =
                            $" CONCAT(YEAR({input.Dimensions.field}),' 第',WEEK({input.Dimensions.field}),'周') AS dimension, ";
                        break;
                    case GranularityType.Month:
                        sqlDimensions =
                            $" CONCAT(YEAR({input.Dimensions.field}),'-', LPAD(MONTH({input.Dimensions.field}),2,'0')) AS dimension, ";
                        break;
                    case GranularityType.Quarter:
                        sqlDimensions =
                            $" CONCAT(YEAR({input.Dimensions.field}),' 第',QUARTER({input.Dimensions.field}),'季度') AS dimension, ";
                        break;
                    case GranularityType.Year:
                        sqlDimensions = $" YEAR({input.Dimensions.field}) AS dimension, ";
                        break;
                }
            }
        }

        var columnsStr = $"{sqlDimensions}{sqlAggType}";

        var executedSql = new StringBuilder();
        executedSql.AppendLine("SELECT * FROM ( ");
        executedSql.AppendFormatLine(
            "SELECT {0} FROM {1} ",
            columnsStr,
            SanitizeIdentifier(input.SchemaName)
        );
        var sqlWhere = DealFilters(input.Filters);
        executedSql.AppendLine(sqlWhere);
        executedSql.AppendFormatLine("GROUP BY {0} ", groupStr);
        executedSql.AppendFormatLine("ORDER BY {0} ", orderStr);
        if (input.Limit > 0)
            executedSql.AppendFormatLine("limit {0} ", input.Limit);
        executedSql.AppendFormatLine(") t_{0} ", SanitizeIdentifier(input.TaskId));
        var sql = executedSql.ToString();
        var list = await _dataBaseManager.GetListAsync<ChartData>(tenantLink, sql);
        var rlt = list.Select(data => new List<object?>() { data.Dimension, data.Value }).ToList();
        info.ExecutedSql = sql;
        info.List = list;
        info.Data = rlt;
        info.Metas = new List<TableFieldOutput>() { input.ColumnField };
        info.TotalTime = watch.ElapsedMilliseconds;
        watch.Stop();
        return info;
    }

    /// <inheritdoc />
    public async Task<ModelDataOutput> GetRealDataAsync(RealDataQryInput input)
    {
        // 计时开始,计算函数执行所需的时间，单位毫秒
        var watch = new Stopwatch();
        watch.Start();
        if (string.IsNullOrEmpty(input.Name))
            throw Oops.Oh(ErrorCode.K10009);
        if (string.IsNullOrEmpty(input.key))
            throw Oops.Oh(ErrorCode.K10008);
        var info = new ModelDataOutput();
        try
        {
            _influxDbManager.Connect();
            var value = await _influxDbManager.QueryLastAsync(input.Name, input.key);
            info.Data = value;
        }
        catch (Exception e)
        {
            // ignore.
        }
        info.ExecutedSql = "";
        info.Metas = new List<TableFieldOutput>() { };
        info.TotalTime = watch.ElapsedMilliseconds;
        watch.Stop();
        return info;
    }

    /// <inheritdoc />
    public async Task<ModelChartDataOutput> GetRealDataChartDataAsync(RealDataAggQueryInput input)
    {
        // 计时开始,计算函数执行所需的时间，单位毫秒
        var watch = new Stopwatch();
        watch.Start();
        if (string.IsNullOrEmpty(input.Name))
            throw Oops.Oh(ErrorCode.K10009);
        if (string.IsNullOrEmpty(input.key))
            throw Oops.Oh(ErrorCode.K10008);
        var info = new ModelChartDataOutput();
        try
        {
            _influxDbManager.Connect();
            var rlt = await _influxDbManager.QueryByKeyAndTimeRangeAsync(
                input.Name,
                input.key,
                input.Min
            );
            info.Data = rlt;
        }
        catch (Exception e)
        {
            // ignore.
        }
        info.ExecutedSql = "";
        info.Metas = new List<TableFieldOutput>() { };
        info.TotalTime = watch.ElapsedMilliseconds;
        watch.Stop();
        return info;
    }

    /// <summary>
    /// 处理Filter过滤条件.
    /// </summary>
    /// <param name="Filters">过滤条件.</param>
    /// <returns></returns>
    private string DealFilters(List<MetricFilterDto>? Filters)
    {
        var strSqlBuilder = new StringBuilder();
        strSqlBuilder.AppendLine("WHERE 1=1 ");
        if (Filters == null)
            return strSqlBuilder.ToString();
        foreach (var filter in Filters)
        {
            var safeField = SanitizeIdentifier(filter.Field);
            var safeMin = SanitizeValue(filter.MinValue);
            var safeMax = SanitizeValue(filter.MaxValue);

            switch (filter.WhereType)
            {
                case MetricWhereType.And:
                    strSqlBuilder.AppendLine(" AND ");
                    break;
                case MetricWhereType.Or:
                    strSqlBuilder.AppendLine(" OR ");
                    break;
            }

            var op1 = filter.MinValueChecked ? ">=" : ">";
            var op2 = filter.MaxValueChecked ? "<=" : "<";

            switch (filter.Type)
            {
                case MetricFilterModel.ByRange:
                    strSqlBuilder.Append(
                        $"({safeField} {op1} '{safeMin}' AND {safeField} {op2} '{safeMax}')"
                    );
                    break;
                case MetricFilterModel.ByValue:
                    strSqlBuilder.Append("(");
                    if (filter.FieldValue.Count <= 5)
                    {
                        var safeValues = filter.FieldValue.Select(v => SanitizeValue(v)).ToList();
                        var parameterOr = string.Join(
                            " OR ",
                            safeValues.Select(value => $"{safeField} = '{value}'")
                        );
                        strSqlBuilder = strSqlBuilder.Append($"{parameterOr}");
                    }
                    else
                    {
                        var safeValues = filter
                            .FieldValue.Select(v => $"'{SanitizeValue(v)}'")
                            .ToList();
                        var parameterIn = string.Join(",", safeValues);
                        strSqlBuilder.Append($"{safeField} IN ({parameterIn})");
                    }
                    strSqlBuilder.Append(")");
                    break;
                case MetricFilterModel.ByDateRang:
                    // DateTime parsing ensures safety, but good to be explicit
                    if (
                        DateTime.TryParse(filter.MinValue, out var minTime)
                        && DateTime.TryParse(filter.MaxValue, out var maxTime)
                    )
                    {
                        var min = minTime.ToString("yyyy-MM-dd 00:00:00");
                        var max = maxTime.AddDays(1).ToString("yyyy-MM-dd 00:00:00");
                        strSqlBuilder.Append(
                            $"({safeField} {op1} CAST('{min}' AS DATETIME) AND {safeField} {op2} CAST('{max}' AS DATETIME))"
                        );
                    }
                    break;
            }
        }

        return strSqlBuilder.ToString();
    }

    /// <summary>
    /// Sanitize SQL identifiers (table names, column names) to prevent injection.
    /// Allows alphanumeric characters and underscores.
    /// </summary>
    private string SanitizeIdentifier(string? identifier)
    {
        if (string.IsNullOrEmpty(identifier))
            return string.Empty;
        // Basic check for dangerous characters.
        // For strictly safe identifiers, regex `^[a-zA-Z0-9_]+$` is best.
        // Assuming identifiers might have dots for schema.table (though unsafe if schema is user input).
        return identifier
            .Replace("'", "")
            .Replace(";", "")
            .Replace("--", "")
            .Replace("/*", "")
            .Replace("*/", "");
    }

    /// <summary>
    /// Sanitize SQL string literal values by escaping single quotes.
    /// </summary>
    private string SanitizeValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
        return value.Replace("'", "''");
    }
}
