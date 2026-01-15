using Microsoft.AspNetCore.Authorization;

namespace Poxiao.Kpi.Web.Core;

/// <summary>
/// 数据分析接口.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-12-21.
/// </summary>
[ApiDescriptionSettings(Groups = new[] { "KPI" }, Tag = "AnalysisData", Name = "AnalysisData", Order = 200)]
[Route("api/kpi/v1/[controller]")]
public class AnalysisDataController : IDynamicApiController
{
    private readonly IAnalysisDataService _analysisDataService;

    /// <summary>
    /// 构造函数.
    /// </summary>
    /// <param name="AnalysisDataService"></param>
    public AnalysisDataController(IAnalysisDataService AnalysisDataService)
    {
        _analysisDataService = AnalysisDataService;
    }

    /// <summary>
    /// 获取正态分布数据.
    /// </summary>
    /// <returns></returns>
    [HttpGet("list")]
    public AnalysisDataNormalListOutput GetNormalList()
    {
        var res = _analysisDataService.GetNHChart();
        return res;
    }

    /// <summary>
    /// 获取均值-极差图数据.
    /// </summary>
    /// <returns></returns>
    [HttpGet("rb")]
    public AnalysisXbarRbarOutput GetRbarOutput()
    {
        return _analysisDataService.GetXRChart();
    }
}