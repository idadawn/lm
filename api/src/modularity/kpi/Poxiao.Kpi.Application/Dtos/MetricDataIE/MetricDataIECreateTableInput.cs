namespace Poxiao.Kpi.Application;

/// <summary>
/// 创建指标表.
/// </summary>
[SuppressSniffer]
public class MetricDataIECreateTableInput
{
    /// <summary>
    /// 表名.
    /// </summary>
    [JsonProperty("tableName")]
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// 字段信息列表.
    /// </summary>
    [JsonProperty("fieldInfos")]
    public List<FieldInfo> FieldInfos { get; set; }

    /// <summary>
    /// 要插入的数据,
    /// Dictionary 一行数据,
    /// string 字段名，object 字段数据.
    /// </summary>
    [JsonProperty("data")]
    public List<Dictionary<string, object>>? Data { get; set; }
}

/// <summary>
/// 要创建表的字段信息.
/// </summary>
public class FieldInfo : FieldInfoBase
{
    /// <summary>
    /// 字段名字.
    /// </summary>
    [JsonProperty("fieldName")]
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// 是否为空.
    /// </summary>
    [JsonProperty("isNull")]
    public bool IsNull { get; set; }

    /// <summary>
    /// 是否是主键.
    /// </summary>
    [JsonProperty("isPrimaryKey")]
    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// 是否自增.
    /// </summary>
    [JsonProperty("isIncrement")]
    public bool IsIncrement { get; set; }
}