namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标数据导入导出，添加指标数据模板.
/// </summary>
[SuppressSniffer]
public class MetricDataIEInsertTemplateOutput
{

    /// <summary>
    /// 表名.
    /// </summary>
    [JsonProperty("tableName")]
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// 字段信息列表.
    /// </summary>
    [JsonProperty("fields")]
    public List<FieldInfo> Fields { get; set; }
}