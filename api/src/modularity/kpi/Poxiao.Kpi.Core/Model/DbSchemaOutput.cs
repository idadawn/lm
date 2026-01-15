namespace Poxiao.Kpi.Core;

/// <summary>
/// 数据源信息
/// </summary>
[SuppressSniffer]
public class DbSchemaOutput : TreeModel
{
    /// <summary>
    /// 模型类别.
    /// </summary>
    [JsonProperty("type")]
    private DataModelType Type { get; set; } = DataModelType.Db;

    /// <summary>
    /// 默认是数据源信息 表/视图
    /// </summary>
    [JsonProperty("schemaStorageType")]
    public SchemaStorageType? SchemaStorageType { get; set; } = null;

    /// <summary>
    /// 数据库类型.
    /// </summary>
    [JsonProperty("dbType")]
    public string DbType { get; set; }

    /// <summary>
    /// 表/视图 名称.
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
