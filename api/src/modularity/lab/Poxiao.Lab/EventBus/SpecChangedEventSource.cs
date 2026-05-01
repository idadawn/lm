using Poxiao.EventBus;

namespace Poxiao.Lab.EventBus;

/// <summary>
/// 产品规格变更事件源
/// </summary>
public sealed class SpecChangedEventSource : ChannelEventSource
{
    public SpecChangedEventSource(string specId, SpecChangeKind kind, object? spec)
        : base("Spec:Changed")
    {
        SpecId = specId;
        Kind = kind;
        Spec = spec;
    }

    public string SpecId { get; }

    public SpecChangeKind Kind { get; }

    public object? Spec { get; }
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
