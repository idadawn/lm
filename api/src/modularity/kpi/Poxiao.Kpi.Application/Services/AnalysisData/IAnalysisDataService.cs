namespace Poxiao.Kpi.Application;

/// <summary>
/// 数据分析接口服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-12-21.
/// </summary>
public interface IAnalysisDataService
{
    /// <summary>
    /// 获取正态分布及直方图数据.
    /// </summary>
    AnalysisDataNormalListOutput GetNHChart();

    /// <summary>
    /// 获取均值-极差控制图数据.
    /// </summary>
    AnalysisXbarRbarOutput GetXRChart();
}