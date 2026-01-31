using System.Text.Json.Serialization;

namespace Poxiao.Lab.Entity.Dto.IntermediateDataFormula;

/// <summary>
/// 中间数据表公式维护DTO.
/// </summary>
public class IntermediateDataFormulaDto
{
    /// <summary>
    /// 主键ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// 来源类型：SYSTEM-系统默认，CUSTOM-自定义.
    /// </summary>
    [JsonPropertyName("sourceType")]
    public string SourceType { get; set; }

    /// <summary>
    /// 表名（枚举：INTERMEDIATE_DATA）.
    /// </summary>
    [JsonPropertyName("tableName")]
    public string TableName { get; set; }

    /// <summary>
    /// 中间数据表列名.
    /// </summary>
    [JsonPropertyName("columnName")]
    public string ColumnName { get; set; }

    /// <summary>
    /// 列显示名称（中文名称）.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }

    /// <summary>
    /// 公式名称.
    /// </summary>
    [JsonPropertyName("formulaName")]
    public string FormulaName { get; set; }

    /// <summary>
    /// 计算公式表达式.
    /// </summary>
    [JsonPropertyName("formula")]
    public string Formula { get; set; }

    /// <summary>
    /// 公式语言：EXCEL、MATH.
    /// </summary>
    [JsonPropertyName("formulaLanguage")]
    public string FormulaLanguage { get; set; } = "EXCEL";

    /// <summary>
    /// 公式类型：CALC-计算公式，JUDGE-判定公式.
    /// </summary>
    [JsonPropertyName("formulaType")]
    public string FormulaType { get; set; } = "CALC";

    /// <summary>
    /// 单位ID.
    /// </summary>
    [JsonPropertyName("unitId")]
    public string UnitId { get; set; }

    /// <summary>
    /// 单位名称.
    /// </summary>
    [JsonPropertyName("unitName")]
    public string UnitName { get; set; }

    /// <summary>
    /// 小数点保留位数.
    /// </summary>
    [JsonPropertyName("precision")]
    public int? Precision { get; set; }

    /// <summary>
    /// 是否启用.
    /// </summary>
    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 排序序号.
    /// </summary>
    [JsonPropertyName("sortOrder")]
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// 默认值（计算公式默认为0，判定公式默认为空）.
    /// </summary>
    [JsonPropertyName("defaultValue")]
    public string DefaultValue { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [JsonPropertyName("remark")]
    public string Remark { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    [JsonPropertyName("creatorTime")]
    public DateTime? CreatorTime { get; set; }

    /// <summary>
    /// 最后修改时间.
    /// </summary>
    [JsonPropertyName("lastModifyTime")]
    public DateTime? LastModifyTime { get; set; }
}

/// <summary>
/// 中间数据表可用列信息.
/// </summary>
public class IntermediateDataColumnInfo
{
    /// <summary>
    /// 列名（属性名）.
    /// </summary>
    [JsonPropertyName("columnName")]
    public string ColumnName { get; set; }

    /// <summary>
    /// 列显示名称.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }

    /// <summary>
    /// 数据类型.
    /// </summary>
    [JsonPropertyName("dataType")]
    public string DataType { get; set; }

    /// <summary>
    /// 是否可计算（数字类型）.
    /// </summary>
    [JsonPropertyName("isCalculable")]
    public bool IsCalculable { get; set; }

    /// <summary>
    /// 小数位数（精度，从 SugarColumn 的 DecimalDigits 获取）.
    /// </summary>
    [JsonPropertyName("decimalDigits")]
    public int? DecimalDigits { get; set; }

    /// <summary>
    /// 列描述/备注.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>
    /// 是否为范围列.
    /// </summary>
    [JsonPropertyName("isRange")]
    public bool IsRange { get; set; }

    /// <summary>
    /// 范围起始.
    /// </summary>
    [JsonPropertyName("rangeStart")]
    public int RangeStart { get; set; }

    /// <summary>
    /// 范围结束.
    /// </summary>
    [JsonPropertyName("rangeEnd")]
    public string RangeEnd { get; set; }

    /// <summary>
    /// 范围列前缀.
    /// </summary>
    [JsonPropertyName("rangePrefix")]
    public string RangePrefix { get; set; }

    /// <summary>
    /// 排序码（用于界面显示顺序）.
    /// </summary>
    [JsonPropertyName("sort")]
    public int Sort { get; set; }

    /// <summary>
    /// 外观特性大类列表（仅在获取可用列时返回）.
    /// </summary>
    [JsonPropertyName("featureCategories")]
    public List<AppearanceFeatureCategoryEntity> FeatureCategories { get; set; }

    /// <summary>
    /// 外观特性等级列表（仅在获取可用列时返回）.
    /// </summary>
    [JsonPropertyName("featureLevels")]
    public List<AppearanceFeatureLevelEntity> FeatureLevels { get; set; }
}

/// <summary>
/// 公式变量来源信息.
/// </summary>
public class FormulaVariableSource
{
    /// <summary>
    /// 来源类型：INTERMEDIATE_DATA、PUBLIC_DIMENSION、APPEARANCE_FEATURE、SEVERITY_LEVEL、PRODUCT_SPEC_ATTRIBUTE.
    /// </summary>
    [JsonPropertyName("sourceType")]
    public string SourceType { get; set; }

    /// <summary>
    /// 来源表名.
    /// </summary>
    [JsonPropertyName("tableName")]
    public string TableName { get; set; }

    /// <summary>
    /// 变量键名.
    /// </summary>
    [JsonPropertyName("variableKey")]
    public string VariableKey { get; set; }

    /// <summary>
    /// 变量显示名称.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }

    /// <summary>
    /// 数据类型.
    /// </summary>
    [JsonPropertyName("dataType")]
    public string DataType { get; set; }
}
