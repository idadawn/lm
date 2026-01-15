namespace Poxiao.Kpi.Application;

/// <summary>
/// 
/// </summary>
public interface IDbService
{
    /// <summary>
    /// 获取数据源信息.
    /// </summary>
    /// <returns></returns>
    Task<List<DataModel4DbOutput>> GetDbListAsync();


    /// <summary>
    /// 根据数据源获取表.
    /// </summary>
    /// <returns></returns>
    Task<List<DbSchemaOutput>> GetDbSchemaListAsync();

    /// <summary>
    /// 获取schema信息.
    /// </summary>
    /// <param name="linkId">连接Id.</param>
    /// <param name="schemaName">schema名称.</param>
    /// <returns></returns>
    Task<DatabaseTableInfoOutput> GetSchemaInfoAsync(string linkId, string schemaName);

    /// <summary>
    /// 获取筛选数据.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    Task<ModelDataListOutput> GetFilterModelDataAsync(ModelDataQueryInput input);

    /// <summary>
    /// 获取聚合查询数据.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    Task<ModelDataOutput> GetMetricDataAsync(ModelDataAggQueryInput input);

    /// <summary>
    /// 获取指标图标数据.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    Task<ModelChartDataOutput> GetMetricChartDataAsync(ModelDataAggQueryInput input);

    /// <summary>
    /// 获取实时数据.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    Task<ModelDataOutput> GetRealDataAsync(RealDataQryInput input);

    /// <summary>
    /// 获取实时数据.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<ModelChartDataOutput> GetRealDataChartDataAsync(RealDataAggQueryInput input);
}