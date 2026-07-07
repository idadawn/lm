namespace Poxiao.Lab.CollectorAgent.Sources;

/// <summary>
/// 设备数据源统一接口：按增量位点拉取新记录。
/// </summary>
public interface IDeviceDataSource
{
    /// <summary>
    /// 拉取位点 <paramref name="lastPosition"/> 之后的新记录，最多 <paramref name="batchSize"/> 条。
    /// </summary>
    /// <param name="lastPosition">上次采集到的位点（自增主键或时间戳的字符串表示），首次采集为空字符串。</param>
    /// <param name="batchSize">单次最大采集条数。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    Task<List<CollectedRecord>> FetchNewAsync(string lastPosition, int batchSize, CancellationToken cancellationToken);
}

/// <summary>
/// 一条采集到的设备数据记录。
/// </summary>
public class CollectedRecord
{
    /// <summary>
    /// 数据源标识（对应 SourceOptions.Name）。
    /// </summary>
    public string SourceKey { get; set; } = string.Empty;

    /// <summary>
    /// 原始字段负载（列名 -> 值）。
    /// </summary>
    public Dictionary<string, object?> Payload { get; set; } = new();

    /// <summary>
    /// 采集到该记录的本机时间。
    /// </summary>
    public DateTimeOffset CollectedAt { get; set; }

    /// <summary>
    /// 该记录对应的位点（用于推进 lastPosition）。
    /// </summary>
    public string Position { get; set; } = string.Empty;
}
