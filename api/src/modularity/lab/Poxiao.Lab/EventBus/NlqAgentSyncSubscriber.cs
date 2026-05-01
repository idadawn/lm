using System.Net.Http.Json;
using Poxiao.DependencyInjection;
using Poxiao.EventBus;
using Poxiao.Logging;

namespace Poxiao.Lab.EventBus;

/// <summary>
/// NLQ Agent 规则/规格同步订阅者。
/// 在判定规则或产品规格发生变更时，通过 HTTP 通知 nlq-agent 更新向量索引。
/// 异常被吞掉不抛出，避免影响原始业务事务；丢失的同步由 nightly cron 兜底。
/// </summary>
public class NlqAgentSyncSubscriber : IEventSubscriber, ISingleton
{
    private readonly IHttpClientFactory _httpClientFactory;

    public NlqAgentSyncSubscriber(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [EventSubscribe("Rule:Changed")]
    public async Task HandleRuleChanged(EventHandlerExecutingContext context)
    {
        if (context?.Source is not RuleChangedEventSource src)
            return;

        var payload = new
        {
            rule_id = src.RuleId,
            change_kind = src.Kind.ToString().ToLowerInvariant(),
            rule = src.Rule,
        };

        await PostAsync("/api/v1/sync/rules", payload, $"rule={src.RuleId}");
    }

    [EventSubscribe("Spec:Changed")]
    public async Task HandleSpecChanged(EventHandlerExecutingContext context)
    {
        if (context?.Source is not SpecChangedEventSource src)
            return;

        var payload = new
        {
            spec_id = src.SpecId,
            change_kind = src.Kind.ToString().ToLowerInvariant(),
            spec = src.Spec,
        };

        await PostAsync("/api/v1/sync/specs", payload, $"spec={src.SpecId}");
    }

    private async Task PostAsync(string path, object payload, string logContext)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("nlq-agent");
            var resp = await client.PostAsJsonAsync(path, payload);
            resp.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Log.Error<NlqAgentSyncSubscriber>(
                $"nlq-agent sync 失败 ({logContext}): {ex.Message}",
                ex);
            // 不抛出 —— best-effort，nightly cron 兜底
        }
    }
}
