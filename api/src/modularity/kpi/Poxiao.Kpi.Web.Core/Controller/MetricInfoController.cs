namespace Poxiao.Kpi.Web.Core;

/// <summary>
/// 指标定义接口.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-12.
/// </summary>
[ApiDescriptionSettings(Groups = new[] { "KPI" }, Tag = "MetricInfo", Name = "Metric", Order = 200)]
[Route("api/kpi/v1/[controller]")]
public class MetricInfoController : IDynamicApiController
{
    private readonly IMetricInfoService _metricInfoService;
    private readonly IDbService _dbService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="MetricInfoService"></param>
    /// <param name="dbService"></param>
    public MetricInfoController(IMetricInfoService MetricInfoService, IDbService dbService)
    {
        _metricInfoService = MetricInfoService;
        _dbService = dbService;
    }

    /// <summary>
    /// 获取数据源信息.
    /// </summary>
    /// <returns></returns>
    [HttpGet("db")]
    public async Task<dynamic> GetDbListAsync()
    {
        var list = await _dbService.GetDbListAsync();
        return list;
    }

    /// <summary>
    /// 获取数据源信息.
    /// </summary>
    /// <returns></returns>
    [HttpGet("schema")]
    public async Task<dynamic> GetDbSchemaListAsync()
    {
        var list = await _dbService.GetDbSchemaListAsync();
        return list;
    }

    /// <summary>
    /// 获取schema信息.
    /// </summary>
    /// <returns></returns>
    [HttpGet("{linkId}/schema/{schemaName}")]
    public async Task<dynamic> GetSchemaInfoAsync(string linkId, string schemaName)
    {
        var info = await _dbService.GetSchemaInfoAsync(linkId, schemaName);
        return info;
    }

    /// <summary>
    /// 获取筛选数据.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPost("filter_model_data")]
    public async Task<dynamic> GetFilterModelDataAsync(ModelDataQueryInput input)
    {
        var info = await _dbService.GetFilterModelDataAsync(input);
        return info;
    }

    /// <summary>
    /// 获取聚合方式列表.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("agg_type")]
    public async Task<dynamic> GetAggListAsync([FromBody] TableFieldOutput input)
    {
        var list = await _metricInfoService.GetAggListAsync(input);
        return list;
    }

    /// <summary>
    /// 获取指标定义信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var info = await _metricInfoService.GetAsync(id);
        return info;
    }

    /// <summary>
    /// 获取指标定义列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPost("list")]
    public async Task<dynamic> GetList([FromBody] MetricInfoListQueryInput input)
    {
        var list = await _metricInfoService.GetListAsync(input);
        return list;
    }

    /// <summary>
    /// 新建指标定义.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task CreateAsync([FromBody] MetricInfoCrInput input)
    {
        var isOk = await _metricInfoService.CreateAsync(input);
        if (!(isOk > 0))
            throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新指标定义.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] MetricInfoUpInput input)
    {
        input.Id = id;
        var isOk = await _metricInfoService.UpdateAsync(input);
        if (!(isOk > 0))
            throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除指标定义.
    /// </summary>
    /// <param name="ids">主键值.</param>
    /// <returns></returns>
    [HttpDelete("")]
    public async Task Delete(List<string> ids)
    {
        var isOk = await _metricInfoService.DeleteAsync(ids);
        if (!(isOk > 0))
            throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 指标在线.
    /// </summary>
    /// <param name="ids">主键值.</param>
    /// <returns></returns>
    [HttpPut("online")]
    public async Task SetOnlineAsync(List<string> ids)
    {
        var isOk = await _metricInfoService.SetEnableAsync(ids, true);
        if (!(isOk > 0))
            throw Oops.Oh(ErrorCode.K10016);
    }

    /// <summary>
    /// 指标下线.
    /// </summary>
    /// <param name="ids">主键值.</param>
    /// <returns></returns>
    [HttpPut("offline")]
    public async Task SetOfflineAsync(List<string> ids)
    {
        var isOk = await _metricInfoService.SetEnableAsync(ids, false);
        if (!(isOk > 0))
            throw Oops.Oh(ErrorCode.K10017);
    }

    /// <summary>
    /// 获取所有指标信息.
    /// </summary>
    /// <returns></returns>
    [HttpGet("all")]
    public async Task<List<MetricInfoAllOutput>> GetAllAsync()
    {
        var list = await _metricInfoService.GetAllAsync(false);
        return list;
    }

    /// <summary>
    /// 获取所有可派生的指标信息.
    /// </summary>
    /// <returns></returns>
    [HttpGet("all-derive")]
    public async Task<List<MetricInfoAllOutput>> GetAll4DeriveAsync()
    {
        var list = await _metricInfoService.GetAllAsync(true);
        return list;
    }

    /// <summary>
    /// 获取所有维度信息.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("dims")]
    public async Task<MetricInfoDimensionsOutput> GetDimensionsAsync(MetricInfoDimQryCrInput input)
    {
        var info = await _metricInfoService.GetDimensionsAsync(input);
        return info;
    }

    /// <summary>
    /// 获取筛选数据.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPost("filter_metric_data")]
    public async Task<dynamic> GetFilterMetricDataAsync(ModelDataListQueryInput input)
    {
        var info = await _metricInfoService.GetFilterMetricDataAsync(input);
        return info;
    }

    /// <summary>
    /// 拷贝指标.
    /// </summary>
    /// <param name="id">参数.</param>
    /// <returns></returns>
    [HttpPost("copy/{id}")]
    public async Task CopyAsync(string id)
    {
        var isOk = await _metricInfoService.CopyAsync(id);
        if (!(isOk > 0))
            throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 获取实时数据列.
    /// </summary>
    /// <param name="name">请求参数.</param>
    /// <returns></returns>
    [HttpGet("real-time/{name}")]
    public async Task<dynamic> GetRtSeriesList(string name)
    {
        var list = await _metricInfoService.GetRtSeriesListAsync(name);
        return list;
    }

    /// <summary>
    /// 获取指标图表数据.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPost("chart_data")]
    public async Task<ModelChartDataOutput> GetMetricChartDataAsync(
        [FromBody] ModelDataAggQueryInput input
    )
    {
        var info = await _metricInfoService.GetMetricChartDataAsync(input);
        return info;
    }
}
