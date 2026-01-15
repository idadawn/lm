namespace Poxiao.Infrastructure.Filter;

/// <summary>
/// 关键字输入.
/// </summary>
[SuppressSniffer]
public class KeywordInput
{
    /// <summary>
    /// 查询关键字.
    /// </summary>
    [JsonProperty("keyword")]
    public string? Keyword { get; set; }
}