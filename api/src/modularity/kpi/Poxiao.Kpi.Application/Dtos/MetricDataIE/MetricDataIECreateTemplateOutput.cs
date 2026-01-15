namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标数据导入导出，创建指标表模板.
/// </summary>
[SuppressSniffer]
public class MetricDataIECreateTemplateOutput
{
    /// <summary>
    /// 字段信息列表.
    /// </summary>
    [JsonProperty("fields")]
    public List<FieldInfoBase> Fields { get; set; } = new List<FieldInfoBase>();
}

/// <summary>
/// 字段信息基类.
/// </summary>
public class FieldInfoBase
{
    /// <summary>
    /// 字段类型.
    /// </summary>
    [JsonProperty("type")]
    public string? Type { get; set; }

    /// <summary>
    /// 字段长度.
    /// </summary>
    [JsonProperty("len")]
    public int? Len { get; set; }
}