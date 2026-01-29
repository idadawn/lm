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
        int dataCount,
        Dictionary<string, UnitPrecisionInfo> unitPrecisions = null
    )
    {
        EventId = eventId;
        TenantId = tenantId;
        SessionId = sessionId;
        BatchId = batchId;
        DataCount = dataCount;
        UnitPrecisions = unitPrecisions ?? new Dictionary<string, UnitPrecisionInfo>();
    }

    public string TenantId { get; set; }

    public string SessionId { get; set; }

    public string BatchId { get; set; }

    public int DataCount { get; set; }

    public Dictionary<string, UnitPrecisionInfo> UnitPrecisions { get; set; }

    public string EventId { get; }

    public object Payload => null;

    public CancellationToken CancellationToken { get; }

    public DateTime CreatedTime { get; } = DateTime.UtcNow;
}

public class UnitPrecisionInfo
{
    public string UnitId { get; set; }

    public int? DecimalPlaces { get; set; }
}
