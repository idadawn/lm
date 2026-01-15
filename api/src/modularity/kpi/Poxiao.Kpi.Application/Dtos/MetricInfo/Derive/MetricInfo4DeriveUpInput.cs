namespace Poxiao.Kpi.Application;

/// <summary>
/// 新增派生指标定义信息.
/// </summary>
[SuppressSniffer]
public class MetricInfo4DeriveUpInput : MetricInfo4DeriveCrInput
{
    /// <summary>
    /// 主键
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }
}
