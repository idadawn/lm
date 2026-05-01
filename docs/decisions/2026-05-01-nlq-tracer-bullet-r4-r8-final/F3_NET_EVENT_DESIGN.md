# F3 — .NET Event-Bus → nlq-agent Rule Sync (Design)

**Status:** Design proposal (read-only research). Implementation deferred to a future round once API contract is approved.
**Date:** 2026-05-01
**Author:** lead (autonomous research)

## Why

`nlq-agent` exposes `POST /api/v1/sync/rules` and `POST /api/v1/sync/specs` so the .NET monolith can push business-rule changes into Qdrant when judgment rules / product specs are saved or deleted. Today nothing calls those endpoints — the agent's vector index drifts from MySQL state until manual `init_semantic_layer.py` re-runs.

F3 closes that loop with the existing in-process event-bus.

## Existing pattern (already in repo)

`api/framework/Poxiao/EventBus/` is a self-rolled event bus. Established usage in `lab` module:

```csharp
// publish — RawDataImportSessionService.cs
private readonly IEventPublisher _eventPublisher;
await _eventPublisher.PublishAsync(
    new IntermediateDataCalcEventSource(batchId, unitPrecisions));

// subscribe — Poxiao.Lab/EventBus/IntermediateDataCalcEventSubscriber.cs
public class IntermediateDataCalcEventSubscriber : IEventSubscriber, ISingleton
{
    [EventSubscribe("IntermediateData:CalcByBatch")]
    public async Task HandleCalcByBatch(EventHandlerExecutingContext context) { ... }
}
```

**Topic naming:** `Domain:Action` (e.g. `IntermediateData:CalcByBatch`).
**Source object:** strongly-typed POCO under `Poxiao.Lab/EventBus/` implementing `IEventSource`.
**Subscriber registration:** auto-DI via `IEventSubscriber + ISingleton` interfaces — no manual wire-up.

## Proposed events

| Topic | Source POCO | Published from | Maps to nlq-agent endpoint |
|---|---|---|---|
| `Rule:Changed` | `RuleChangedEventSource(string ruleId, RuleChangeKind kind)` | `IntermediateDataFormulaService.SaveAsync` / `DeleteAsync` | `POST /api/v1/sync/rules` |
| `Spec:Changed` | `SpecChangedEventSource(string specId, SpecChangeKind kind)` | `ProductSpecService.SaveAsync` / `DeleteAsync` | `POST /api/v1/sync/specs` |

`*ChangeKind` enum: `Created` / `Updated` / `Deleted`.

## Subscriber sketch

```csharp
// Poxiao.Lab/EventBus/NlqAgentSyncSubscriber.cs (new file, ~60 lines)
namespace Poxiao.EventHandler;

public class NlqAgentSyncSubscriber : IEventSubscriber, ISingleton
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceProvider _serviceProvider;

    public NlqAgentSyncSubscriber(IHttpClientFactory hcf, IServiceProvider sp)
    {
        _httpClientFactory = hcf;
        _serviceProvider = sp;
    }

    [EventSubscribe("Rule:Changed")]
    public async Task HandleRuleChanged(EventHandlerExecutingContext context)
    {
        if (context?.Source is not RuleChangedEventSource src) return;

        using var scope = _serviceProvider.CreateScope();
        var ruleService = scope.ServiceProvider.GetRequiredService<IIntermediateDataFormulaService>();
        var rule = src.Kind == RuleChangeKind.Deleted
            ? null
            : await ruleService.GetByIdAsync(src.RuleId);

        var client = _httpClientFactory.CreateClient("nlq-agent");
        var payload = new {
            rule_id = src.RuleId,
            change_kind = src.Kind.ToString().ToLowerInvariant(),
            rule = rule  // null for delete
        };

        try
        {
            var resp = await client.PostAsJsonAsync("/api/v1/sync/rules", payload);
            resp.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            // Don't propagate — sync failure must not break the original save.
            Log.Error($"nlq-agent sync 失败 (rule={src.RuleId}): {ex.Message}");
            // TODO: enqueue to outbox table for retry — see Reliability section.
        }
    }

    // ... mirror for HandleSpecChanged
}
```

`HttpClient` named `nlq-agent` registered in `Program.cs` with `BaseAddress` from `appsettings.json`:

```json
"NlqAgent": { "BaseUrl": "http://nlq-agent:18100", "TimeoutSeconds": 5 }
```

## Reliability (the open question)

The naive sketch above is **best-effort**: if nlq-agent is down or returns 5xx, the rule update still commits to MySQL but the Qdrant index drifts. Two options for production:

1. **Outbox table** (recommended) — write `Lab_NlqSyncOutbox` row in the same DB transaction as the rule save, then an idle background worker drains and POSTs. Survives nlq-agent downtime.
2. **At-startup full sync** — accept temporary drift; cron job runs `init_semantic_layer.py` nightly to bulk-resync. Simpler, but lossy during the day.

**Decision needed from PM/architect** before implementation:
- Is real-time consistency required, or is nightly resync acceptable?
- If real-time: outbox table + background worker (~100 LOC) and migration for the new table.
- If eventual: nightly cron is one shell-script PR.

## What this design intentionally does NOT cover

- **Cross-service auth** — nlq-agent's sync endpoints currently have no auth. Either add an internal token check (simple) or run nlq-agent in a private network only (already the docker-compose default). Not a blocker for design.
- **Bulk re-sync UI** — admin-triggered manual full-resync button. Out of scope for F3; could be a follow-up PR adding an admin endpoint that fires `Rule:BulkResync`.
- **Backwards compatibility for the .NET `lab` module's existing event handlers** — none. We're adding a new subscriber, not modifying existing topics.

## Estimated effort

- Events + sources + subscriber: ~150 LOC (1 worker round)
- Outbox table option: +120 LOC (migration + draining worker, second round)
- Tests: ~80 LOC (mock HttpClient + verify POST body shape)
- Total: 1–2 worker rounds + 1 PM/architect decision call

## Next step (when ready to implement)

1. PM/architect approves "outbox vs nightly".
2. Dispatch a single .NET worker (KIMI or GLM in `c#` mode) with this design doc as the prompt's reference.
3. Add `tests/integration/test_nlq_sync_publishes.cs` using xUnit + a stubbed HTTP server.
4. Verify against a running `nlq-agent` in docker-compose (manual smoke test).

This file lives in the R4–R8 ADR archive so future contributors can find the design before re-litigating it.
