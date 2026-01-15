namespace Poxiao.Kpi.Core.Entitys;

/// <summary>
/// 价值链状态.
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2023-12-6
/// </summary>
[SugarTable("metric_cov_status", TableDescription = "价值链状态")]
public class MetricCovStatusEntity : CUEntityBase
{
    /// <summary>
    /// 名称.
    /// </summary>
    [SugarColumn(ColumnName = "name", ColumnDescription = "名称")]
    public string Name { get; set; }

    /// <summary>
    /// 颜色.
    /// </summary>
    [SugarColumn(ColumnName = "color", ColumnDescription = "颜色")]
    public string Color { get; set; }

}