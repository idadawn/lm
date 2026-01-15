namespace Poxiao.Infrastructure.Models;

/// <summary>
/// 下拉框实体.
/// </summary>
[SuppressSniffer]
public class SelectorModel
{
    /// <summary>
    /// id.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }
}
