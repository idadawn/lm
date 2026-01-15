using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Poxiao.Lab.Entity.Config;

/// <summary>
/// Excel导入模板配置.
/// </summary>
public class ExcelTemplateConfig
{
    /// <summary>
    /// 配置版本.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// 配置描述.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>
    /// 字段映射配置.
    /// </summary>
    [JsonPropertyName("fieldMappings")]
    public List<TemplateColumnMapping> FieldMappings { get; set; } = new List<TemplateColumnMapping>();

    /// <summary>
    /// 检测列配置.
    /// </summary>
    [JsonPropertyName("detectionColumns")]
    public DetectionColumnConfig DetectionColumns { get; set; } = new DetectionColumnConfig();

    /// <summary>
    /// 验证规则配置.
    /// </summary>
    [JsonPropertyName("validation")]
    public TemplateValidationConfig Validation { get; set; } = new TemplateValidationConfig();
}

/// <summary>
/// 模板列映射配置.
/// </summary>
public class TemplateColumnMapping
{
    /// <summary>
    /// 数据字段名（对应RawDataEntity字段）.
    /// </summary>
    [JsonPropertyName("field")]
    public string Field { get; set; }

    /// <summary>
    /// Excel列名（支持多个备选名称）.
    /// </summary>
    [JsonPropertyName("excelColumnNames")]
    public List<string> ExcelColumnNames { get; set; } = new List<string>();

    /// <summary>
    /// 是否必填.
    /// </summary>
    [JsonPropertyName("required")]
    public bool Required { get; set; } = true;

    /// <summary>
    /// 数据类型：datetime/string/decimal/int.
    /// </summary>
    [JsonPropertyName("dataType")]
    public string DataType { get; set; } = "string";

    /// <summary>
    /// 默认值（字符串形式，使用时根据DataType转换）.
    /// </summary>
    [JsonPropertyName("defaultValue")]
    public string DefaultValue { get; set; }
}

/// <summary>
/// 检测列配置.
/// </summary>
public class DetectionColumnConfig
{
    /// <summary>
    /// 检测列表头模式（{col}会被替换为列号）.
    /// </summary>
    [JsonPropertyName("patterns")]
    public List<string> Patterns { get; set; } = new List<string>
    {
        "{col}",
        "检测{col}",
        "列{col}",
        "第{col}列",
        "检测列{col}"
    };

    /// <summary>
    /// 最小列号.
    /// </summary>
    [JsonPropertyName("minColumn")]
    public int MinColumn { get; set; } = 1;

    /// <summary>
    /// 最大列号.
    /// </summary>
    [JsonPropertyName("maxColumn")]
    public int MaxColumn { get; set; } = 100;
}

/// <summary>
/// 模板验证配置.
/// </summary>
public class TemplateValidationConfig
{
    /// <summary>
    /// 必填字段列表.
    /// </summary>
    [JsonPropertyName("requiredFields")]
    public List<string> RequiredFields { get; set; } = new List<string>();

    /// <summary>
    /// 字段级别验证规则.
    /// </summary>
    [JsonPropertyName("fieldRules")]
    public Dictionary<string, FieldValidationRule> FieldRules { get; set; } = new Dictionary<string, FieldValidationRule>();
}

/// <summary>
    /// 字段验证规则.
    /// </summary>
    public class FieldValidationRule
    {
        /// <summary>
        /// 正则表达式模式.
        /// </summary>
        [JsonPropertyName("pattern")]
        public string Pattern { get; set; }

        /// <summary>
        /// 错误提示信息.
        /// </summary>
        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 最小值（数值类型）.
        /// </summary>
        [JsonPropertyName("min")]
        public decimal? Min { get; set; }

        /// <summary>
        /// 最大值（数值类型）.
        /// </summary>
        [JsonPropertyName("max")]
        public decimal? Max { get; set; }
    }