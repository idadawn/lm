namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标分析任务服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2024-1-9.
/// </summary>
public class MetricAnalysisTaskService : IMetricAnalysisTaskService, ITransient
{
    /// <summary>
    /// 标签仓库.
    /// </summary>
    private readonly ISqlSugarRepository<MetricAnalysisTaskEntity> _repository;

    private readonly IMetricDataService _metricDataService;

    /// <summary>
    /// 初始化一个<see cref="MetricAnalysisTaskService"/>类型的新实例.
    /// </summary>
    public MetricAnalysisTaskService(ISqlSugarRepository<MetricAnalysisTaskEntity> repository, IMetricDataService metricDataService)
    {
        _repository = repository;
        _metricDataService = metricDataService;
    }

    /// <inheritdoc />
    public async Task<MetricAnalysisTaskOutput> CreateAsync(MetricAnalysisTaskCrInput input)
    {
        if (input.TimeDimensions == null)
            throw Oops.Oh(ErrorCode.K10021);

        var info = new MetricAnalysisTaskOutput()
        {
            Value = "2.00",
            Percentage = "-22%",
            EndData = input.EndData,
            StartData = input.StartData,
            End = "7",
            Start = "9",
            TaskId = SnowflakeIdHelper.NextId().ToString(),
            TaskStatus = AnalysisStatus.NotStarted,
            Trend = TrendType.Down
        };

        var analysisDataList = new List<MetricAnalysisDataInput>();
        foreach (var dim in input.Dimensions)
        {
            var qry = new MetricDataQryInput()
            {
                Dimensions = dim,
                MetricId = input.MetricId,
                Filters = new List<MetricFilterDto>()
                {
                    new MetricFilterDto()
                    {
                        WhereType = MetricWhereType.And,
                        DataType = input.TimeDimensions.DataType,
                        Field = input.TimeDimensions.Field,
                        FieldName = input.TimeDimensions.FieldName,
                        Type = MetricFilterModel.ByDateRang,
                        MinValue = input.StartData,
                        MinValueChecked = true,
                        MaxValue = input.EndData,
                        MaxValueChecked = true
                    }
                },
                SortBy = DBSortByType.ASC,

            };
            var output = await _metricDataService.GetChartDataAsync(qry);

            analysisDataList.Add(new MetricAnalysisDataInput()
            {
                Dimension = dim,
                DimensionName = dim.fieldName,
                Data = output.Data.List,
                EndData = input.EndData,
                StartData = input.StartData
            });
        }

        var tmp = analysisDataList.ToJsonString();
        //var entity = input.Adapt<MetricAnalysisTaskEntity>();
        //entity.Id = SnowflakeIdHelper.NextId();
        //var count = await _repository.AsInsertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(x => x.Create()).ExecuteCommandAsync();

        return info;
    }

    /// <inheritdoc />
    public async Task<int> UpdateAsync(MetricAnalysisTaskUpInput input)
    {
        var entity = input.Adapt<MetricAnalysisTaskEntity>();
        var count = await _repository.AsUpdateable(entity)
            .IgnoreColumns(ignoreAllNullColumns: true)
            .CallEntityMethod(x => x.Update())
            .ExecuteCommandAsync();
        return count;
    }

    /// <inheritdoc />
    public async Task<MetricAnalysisTaskStatusOutput> GetStatusAsync(string taskId)
    {
        var info = new MetricAnalysisTaskStatusOutput()
        {
            TaskId = taskId,
            TaskStatus = AnalysisStatus.Completed
        };
        return await Task.FromResult(info);
    }

    /// <inheritdoc />
    public async Task<MetricAnalysisSummaryOutput> GetSummaryAsync(string taskId)
    {
        var json = @"
{
            ""task_id"": ""anakqbikunti"",
            ""task_status"": ""FINISHED"",
            ""summary_content"": ""根据归因分析结果，总交易量的数据变化趋势从4下降到了0，说明该指标在过去的时间内出现了下降趋势。通过归因分析，我们发现订单月份是与总交易量相关性最高的指标维度。从潜在引起指标下降的维度值排名列表中，我们可以看到维度值为10的订单月份对应的指标数据变化趋势为4~0，贡献度为100%。因此，可以推断出总交易量下降的原因是在订单月份为10的时候，总交易量出现了明显的下降。\n\n至于潜在引起指标上升的维度值排名列表，由于在问题描述中没有给出，因此无法得出结论。"",
            ""msg"": null
        }
";
        var info = json.ToObject<MetricAnalysisSummaryOutput>();
        return await Task.FromResult(info);
    }

    /// <inheritdoc />
    public async Task<MetricAnalysisResultOutput> GetResultAsync(string taskId)
    {
        var json = "{\r\n  \"task_id\": \"anakqbikunti\",\r\n  \"start_time\": 1704762425803,\r\n  \"status\": \"FINISHED\",\r\n  \"msg\": \"Task finished.\",\r\n  \"base_period\": \"2015-10-18\",\r\n  \"base_period_value\": 4,\r\n  \"compared_period\": \"2018-12-30\",\r\n  \"compared_period_value\": 0,\r\n  \"analysis_result\": [\r\n    {\r\n      \"dimension\": \"订单月份\",\r\n      \"coefficient\": 1,\r\n      \"attribution_list\": [\r\n        {\r\n          \"dimension_value\": \"10\",\r\n          \"attribution_value\": 1,\r\n          \"base_period_value\": 4,\r\n          \"compared_period_value\": 0\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"dimension\": \"订单年份\",\r\n      \"coefficient\": 1,\r\n      \"attribution_list\": [\r\n        {\r\n          \"dimension_value\": \"2015\",\r\n          \"attribution_value\": 1,\r\n          \"base_period_value\": 4,\r\n          \"compared_period_value\": 0\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"dimension\": \"门店城市\",\r\n      \"coefficient\": 1,\r\n      \"attribution_list\": [\r\n        {\r\n          \"dimension_value\": \"奥古斯塔\",\r\n          \"attribution_value\": 0.25,\r\n          \"base_period_value\": 1,\r\n          \"compared_period_value\": 0\r\n        },\r\n        {\r\n          \"dimension_value\": \"俾斯麦\",\r\n          \"attribution_value\": 0.25,\r\n          \"base_period_value\": 1,\r\n          \"compared_period_value\": 0\r\n        },\r\n        {\r\n          \"dimension_value\": \"杰克逊\",\r\n          \"attribution_value\": 0.25,\r\n          \"base_period_value\": 1,\r\n          \"compared_period_value\": 0\r\n        },\r\n        {\r\n          \"dimension_value\": \"兰辛\",\r\n          \"attribution_value\": 0.25,\r\n          \"base_period_value\": 1,\r\n          \"compared_period_value\": 0\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"dimension\": \"商品分类\",\r\n      \"coefficient\": 1,\r\n      \"attribution_list\": [\r\n        {\r\n          \"dimension_value\": \"粮食\",\r\n          \"attribution_value\": 0.25,\r\n          \"base_period_value\": 1,\r\n          \"compared_period_value\": 0\r\n        },\r\n        {\r\n          \"dimension_value\": \"水果 & 蔬菜\",\r\n          \"attribution_value\": 0.25,\r\n          \"base_period_value\": 1,\r\n          \"compared_period_value\": 0\r\n        },\r\n        {\r\n          \"dimension_value\": \"点心\",\r\n          \"attribution_value\": 0.25,\r\n          \"base_period_value\": 1,\r\n          \"compared_period_value\": 0\r\n        },\r\n        {\r\n          \"dimension_value\": \"面包\",\r\n          \"attribution_value\": 0.25,\r\n          \"base_period_value\": 1,\r\n          \"compared_period_value\": 0\r\n        }\r\n      ]\r\n    }\r\n  ]\r\n}";

        var info = json.ToObject<MetricAnalysisResultOutput>();
        return await Task.FromResult(info);
    }
}