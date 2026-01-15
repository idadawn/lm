namespace Poxiao.Infrastructure.Models;

/// <summary>
/// 数据格式
/// </summary>
public class DataModelFormat
{
    /// <summary>
    /// 类别.
    /// </summary>
    [Description("类别")]
    public DataModelFormatType Type { get; set; } = DataModelFormatType.None;

    /// <summary>
    /// 小数位数.
    /// </summary>
    [Description("小数位数")]
    public int DecimalPlaces { get; set; } = 2;

    /// <summary>
    /// 单位.
    /// </summary>
    [Description("单位")]
    public DataModelUnit Unit { get; set; } = DataModelUnit.Default;

    /// <summary>
    /// 是否千分位分隔符.
    /// </summary>
    [Description("是否千分位分隔符")]
    public bool IsThousandsSeparator { get; set; } = false;

    /// <summary>
    /// 货币标志.
    /// </summary>
    [Description("货币标志")]
    public DataModelCurrencySymbol CurrencySymbol { get; set; } = DataModelCurrencySymbol.None;
}

/// <summary>
/// 数据格式类别.
/// </summary>
[JsonConverter(typeof(EnumUseNameConverter<DataModelFormatType>))]
public enum DataModelFormatType
{
    /// <summary>
    /// 无格式.
    /// </summary>
    [Description("无格式")]
    None,

    /// <summary>
    /// 数值.
    /// </summary>
    [Description("数值")]
    Number,

    /// <summary>
    /// 货币.
    /// </summary>
    [Description("货币")]
    Currency,

    /// <summary>
    /// 百分比.
    /// </summary>
    [Description("百分比")]
    Percentage
}

/// <summary>
/// 数据格式类别单位.
/// </summary>
[JsonConverter(typeof(EnumUseNameConverter<DataModelUnit>))]
public enum DataModelUnit
{
    /// <summary>
    /// 默认.
    /// 1234.00
    /// 1,234.00
    /// </summary>
    [Description("默认")]
    Default,

    /// <summary>
    /// 无.
    /// 1234.00
    /// 1,234.00
    /// </summary>
    [Description("无")]
    None,

    /// <summary>
    /// 万.
    /// 1000万
    /// 1,000万
    /// </summary>
    [Description("万")]
    Wan,

    /// <summary>
    /// 亿.
    /// 1000亿.
    /// 1,000亿.
    /// </summary>
    [Description("亿")]
    Yi,
}

/// <summary>
/// 数据格式货币.
/// </summary>
[JsonConverter(typeof(EnumUseNameConverter<DataModelCurrencySymbol>))]
public enum DataModelCurrencySymbol
{
    /// <summary>
    /// 无.
    /// 1234.10
    /// -1234.10
    /// </summary>
    [Description("无")]
    None,

    /// <summary>
    /// CNY 人民币.
    /// CNY1234.10
    /// CNY-1234.10
    /// </summary>
    [Description("CNY")]
    CNY,

    /// <summary>
    /// ¥ 人民币.
    /// ¥1234.10
    /// ¥-1234.10
    /// </summary>
    [Description("¥")]
    CNY1,

    /// <summary>
    /// USD 美元.
    /// USD1234.10
    /// USD-1234.10
    /// </summary>
    [Description("USD")]
    USD,

    /// <summary>
    /// $ 美元.
    /// $1234.10
    /// $-1234.10
    /// </summary>
    [Description("$")]
    USD1,
}