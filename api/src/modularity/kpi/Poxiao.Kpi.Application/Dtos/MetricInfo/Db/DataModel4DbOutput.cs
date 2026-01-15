namespace Poxiao.Kpi.Application;

/// <summary>
/// 数据源信息
/// </summary>
[SuppressSniffer]
public class DataModel4DbOutput
{
    /// <summary>
    /// 模型类别.
    /// </summary>
    [JsonProperty("type")]
    private DataModelType Type { get; set; } = DataModelType.Db;

    /// <summary>
    /// id.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// 数据库类型.
    /// </summary>
    [JsonProperty("dbType")]
    public string DbType { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// 主机地址.
    /// </summary>
    [JsonProperty("host")]
    public string Host { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    [JsonProperty("sortCode")]
    public long? SortCode { get; set; }
}
