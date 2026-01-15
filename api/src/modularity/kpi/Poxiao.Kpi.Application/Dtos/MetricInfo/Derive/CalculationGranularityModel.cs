namespace Poxiao.Kpi.Application;

/// <summary>
/// 计算区间类型.
/// </summary>
public class CalculationGranularityModel
{
    /// <summary>
    /// 第一行.
    /// </summary>
    [JsonProperty("firstLine")]
    public string FirstLine { get; set; }

    /// <summary>
    /// 最后一行.
    /// </summary>
    [JsonProperty("lastLine")]
    public string LastLine { get; set; }

    /// <summary>
    /// 是否区间.
    /// </summary>
    [JsonProperty("isRange")]
    public string IsRange { get; set; }

    /// <summary>
    /// 相对于开始值.
    /// </summary>
    [JsonProperty("startLine")]
    public int? StartLine { get; set; }

    /// <summary>
    /// 相对于结束值.
    /// </summary>
    [JsonProperty("endLine")]
    public int? EndLine { get; set; }

    /// <summary>
    /// 区间值.
    /// </summary>
    [JsonProperty("rangeLine")]
    public int? RangeLine { get; set; }

}

/// <summary>
/// 计算区间类型常量.
/// </summary>
public class CalGranularityConst
{
    /// <summary>
    /// 第一行.
    /// </summary>
    public const string FIRSTLINE = "first";

    /// <summary>
    /// 最后一行.
    /// </summary>
    public const string LASTLINE = "last";

    /// <summary>
    /// 是否区间
    /// </summary>
    public const string ISRANGE = "range";
}