using Poxiao.EventBus;

namespace Poxiao.Lab.EventBus;

/// <summary>
/// 产品规格变更事件源
/// </summary>
public sealed class SpecChangedEventSource : IEventSource
{
    public SpecChangedEventSource(string specId, SpecChangeKind kind, object? spec)
    {
        EventId = "Spec:Changed";
        SpecId = specId;
        Kind = kind;
        Spec = spec;
    }

    public string SpecId { get; }

    public SpecChangeKind Kind { get; }

    public object? Spec { get; }

    public string EventId { get; }

    public object Payload => Spec;

    public CancellationToken CancellationToken { get; }

    public DateTime CreatedTime { get; } = DateTime.UtcNow;
}

/// <summary>
/// 规格变更类型
/// </summary>
public enum SpecChangeKind
{
    Created,
    Updated,
    Deleted,
}
