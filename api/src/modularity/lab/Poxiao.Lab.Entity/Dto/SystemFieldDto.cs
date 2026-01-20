using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Poxiao.Lab.Entity.Dto;

/// <summary>
/// 系统字段定义 DTO.
/// </summary>
public class SystemFieldDto
{
    /// <summary>
    /// 字段名 (实体属性名).
    /// </summary>
    /// </summary>
    [JsonPropertyName("field")]
    [JsonProperty("field")]
    public string Field { get; set; }

    /// <summary>
    /// 显示标签.
    /// </summary>
    /// </summary>
    [JsonPropertyName("label")]
    [JsonProperty("label")]
    public string Label { get; set; }

    /// <summary>
    /// 数据类型 (string, decimal, datetime, int).
    /// </summary>
    /// </summary>
    [JsonPropertyName("dataType")]
    [JsonProperty("dataType")]
    public string DataType { get; set; }

    /// <summary>
    /// 排序代码.
    /// </summary>
    /// </summary>
    [JsonPropertyName("sortCode")]
    [JsonProperty("sortCode")]
    public int SortCode { get; set; }

    /// <summary>
    /// 默认Excel列名.
    /// </summary>
    /// </summary>
    [JsonPropertyName("excelColumnNames")]
    [JsonProperty("excelColumnNames")]
    public List<string> ExcelColumnNames { get; set; }

    /// <summary>
    /// Excel列索引（A, B, C...）.
    /// </summary>
    /// </summary>
    [JsonPropertyName("excelColumnIndex")]
    [JsonProperty("excelColumnIndex")]
    public string ExcelColumnIndex { get; set; }

    /// <summary>
    /// 单位ID.
    /// </summary>
    /// </summary>
    [JsonPropertyName("unitId")]
    [JsonProperty("unitId")]
    public string UnitId { get; set; }

    /// <summary>
    /// 是否必填.
    /// </summary>
    /// </summary>
    [JsonPropertyName("required")]
    [JsonProperty("required")]
    public bool Required { get; set; }
}
