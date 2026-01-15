namespace Poxiao.Kpi.Core.Entitys;

/// <summary>
/// 指标仪表板.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-18.
/// </summary>
[SugarTable("metric_dash", TableDescription = "指标仪表板")]
public class MetricDashEntity : CUEntityBase
{
    /// <summary>
    /// 思维图id.
    /// </summary>
    [SugarColumn(ColumnName = "got_id", ColumnDescription = "思维图id")]
    public string GotId { get; set; }

    /// <summary>
    /// 思维图类别.
    /// </summary>
    [SugarColumn(ColumnName = "got_type", ColumnDescription = "思维图类别", SqlParameterDbType = typeof(EnumToStringConvert))]
    public GotType GotType { get; set; }

    /// <summary>
    /// 表单数据.
    /// </summary>
    [SugarColumn(ColumnName = "form_json", ColumnDescription = "表单数据")]
    public string? FormJson { get; set; }
}