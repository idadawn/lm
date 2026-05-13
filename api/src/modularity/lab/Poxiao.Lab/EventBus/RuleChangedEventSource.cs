using Poxiao.EventBus;

namespace Poxiao.Lab.EventBus;

/// <summary>
/// 判定规则变更事件源
/// </summary>
public sealed class RuleChangedEventSource : IEventSource
{
    public RuleChangedEventSource(string ruleId, RuleChangeKind kind, object? rule)
    {
        EventId = "Rule:Changed";
        RuleId = ruleId;
        Kind = kind;
        Rule = rule;
    }

    public string RuleId { get; }

    public RuleChangeKind Kind { get; }

    public object? Rule { get; }

    public string EventId { get; }

    public object Payload => Rule;

    public CancellationToken CancellationToken { get; }

    public DateTime CreatedTime { get; } = DateTime.UtcNow;
}

/// <summary>
/// 规则变更类型
/// </summary>
public enum RuleChangeKind
{
    Created,
    Updated,
    Deleted,
}
