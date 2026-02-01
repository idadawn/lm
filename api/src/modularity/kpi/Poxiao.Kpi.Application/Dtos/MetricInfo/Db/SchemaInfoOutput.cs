namespace Poxiao.Kpi.Application;

/// <summary>
/// 表/视图信息
/// </summary>
[SuppressSniffer]
public class SchemaInfoOutput
{
    /// <summary>
    /// 存储类别
    /// </summary>
    [JsonProperty("schemaStorageType")]
    public SchemaStorageType SchemaStorageType { get; set; } = SchemaStorageType.Table;

    /// <summary>
    /// Id.
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
    [JsonProperty("schemaName")]
    public string SchemaName { get; set; }

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
