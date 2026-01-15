namespace Poxiao.Kpi.Core.Entitys;

/// <summary>
/// 指标价值链规则.
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2023-11-18.
/// </summary>
[SugarTable("metric_cov_rule", TableDescription = "指标价值链规则")]
public class MetricCovRuleEntity : CUEntityBase
{
    /// <summary>
    /// 价值链id.
    /// </summary>
    [SugarColumn(ColumnName = "cov_id", ColumnDescription = "价值链id")]
    public string CovId { get; set; }

    /// <summary>
    /// 级别.
    /// </summary>
    [SugarColumn(ColumnName = "level", ColumnDescription = "级别")]
    public string Level { get; set; }

    /// <summary>
    /// 类型.
    /// </summary>
    [SugarColumn(ColumnName = "type", ColumnDescription = "类型")]
    public string Type { get; set; }

    /// <summary>
    /// 操作符.
    /// </summary>
    [SugarColumn(ColumnName = "operators", ColumnDescription = "操作符")]
    public string Operators { get; set; }

    /// <summary>
    /// 值.
    /// </summary>
    [SugarColumn(ColumnName = "value", ColumnDescription = "值")]
    public decimal? Value { get; set; }

    /// <summary>
    /// 最小值.
    /// </summary>
    [SugarColumn(ColumnName = "min_value", ColumnDescription = "最小值")]
    public decimal? MinValue { get; set; }

    /// <summary>
    /// 最大值.
    /// </summary>
    [SugarColumn(ColumnName = "max_value", ColumnDescription = "最大值")]
    public decimal? MaxValue { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    [SugarColumn(ColumnName = "status", ColumnDescription = "状态")]
    public string? Status { get; set; }

}