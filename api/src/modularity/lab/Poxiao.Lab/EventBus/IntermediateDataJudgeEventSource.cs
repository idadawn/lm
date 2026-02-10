using Poxiao.EventBus;

namespace Poxiao.Lab.EventBus;

/// <summary>
/// 中间数据批量判定事件.
/// </summary>
public class IntermediateDataJudgeEventSource : IEventSource
{
    public IntermediateDataJudgeEventSource(
        string eventId,
        string tenantId,
        string sessionId,
        List<string> intermediateDataIds,
        int chunkSize = 10
    )
    {
        EventId = eventId;
        TenantId = tenantId;
        SessionId = sessionId;
        IntermediateDataIds = intermediateDataIds ?? new List<string>();
        ChunkSize = chunkSize <= 0 ? 10 : chunkSize;
    }

    public string TenantId { get; set; }

    public string SessionId { get; set; }

    public List<string> IntermediateDataIds { get; set; }

    public int ChunkSize { get; set; }

    public string EventId { get; }

    public object Payload => null;

    public CancellationToken CancellationToken { get; }

    public DateTime CreatedTime { get; } = DateTime.UtcNow;
}
