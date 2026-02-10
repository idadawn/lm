namespace Poxiao.Lab.EventBus;

/// <summary>
/// 中间数据判定事件载荷.
/// </summary>
public class IntermediateDataJudgeEventPayload
{
    public string TenantId { get; set; }

    public string SessionId { get; set; }

    public List<string> IntermediateDataIds { get; set; } = new();

    public int ChunkSize { get; set; } = 10;
}
