using Poxiao.EventBus;

namespace Poxiao.Lab.EventBus;

/// <summary>
/// 中间数据批次公式计算事件
/// </summary>
public class IntermediateDataCalcEventSource : IEventSource
{
    public IntermediateDataCalcEventSource(
        string eventId,
        string tenantId,
        string sessionId,
        string batchId,
        int dataCount
    )
    {
        EventId = eventId;
        TenantId = tenantId;
        SessionId = sessionId;
        BatchId = batchId;
        DataCount = dataCount;
    }

    public string TenantId { get; set; }

    public string SessionId { get; set; }

    public string BatchId { get; set; }

    public int DataCount { get; set; }

    public string EventId { get; }

    public object Payload => null;

    public CancellationToken CancellationToken { get; }

    public DateTime CreatedTime { get; } = DateTime.UtcNow;
}