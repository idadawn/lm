using Microsoft.Extensions.DependencyInjection;
using Poxiao.DependencyInjection;
using Poxiao.EventBus;
using Poxiao.Logging;
using Poxiao.Lab.EventBus;
using Poxiao.Lab.Service;
using System.Text.Json;

namespace Poxiao.EventHandler;

/// <summary>
/// 中间数据公式计算事件订阅
/// </summary>
public class IntermediateDataCalcEventSubscriber : IEventSubscriber, ISingleton
{
    private readonly IServiceProvider _serviceProvider;

    public IntermediateDataCalcEventSubscriber(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [EventSubscribe("IntermediateData:CalcByBatch")]
    public async Task HandleCalcByBatch(EventHandlerExecutingContext context)
    {
        if (context?.Source is not IntermediateDataCalcEventSource source)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(source.BatchId))
        {
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var calculator = scope.ServiceProvider.GetService<IntermediateDataFormulaBatchCalculator>();
        if (calculator == null)
        {
            return;
        }

        try
        {
            await calculator.CalculateByBatchAsync(source.BatchId, source.UnitPrecisions);
        }
        catch (Exception ex)
        {
            Log.Error($"中间数据公式计算事件处理失败: {ex.Message}");
        }
    }

    [EventSubscribe("IntermediateData:JudgeByIds")]
    public async Task HandleJudgeByIds(EventHandlerExecutingContext context)
    {
        var ids = ResolveJudgeIds(context);

        if (ids == null || ids.Count == 0)
        {
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var calculator = scope.ServiceProvider.GetService<IntermediateDataFormulaBatchCalculator>();
        if (calculator == null)
        {
            return;
        }

        try
        {
            await calculator.JudgeByIdsAsync(ids);
        }
        catch (Exception ex)
        {
            Log.Error($"中间数据判定事件处理失败: {ex.Message}");
        }
    }

    private static List<string> ResolveJudgeIds(EventHandlerExecutingContext context)
    {
        if (context?.Source is IntermediateDataJudgeEventSource directSource)
        {
            return NormalizeIds(directSource.IntermediateDataIds);
        }

        if (context?.Source is not ChannelEventSource channelSource)
        {
            return new List<string>();
        }

        if (channelSource.Payload is JsonElement payloadElement)
        {
            // 兼容 payload = ["id1","id2"] 与 payload = { intermediateDataIds: [...] } 两种格式。
            if (payloadElement.ValueKind == JsonValueKind.Array)
            {
                var ids = payloadElement.Deserialize<List<string>>();
                return NormalizeIds(ids);
            }

            if (payloadElement.ValueKind == JsonValueKind.Object)
            {
                var payload = payloadElement.Deserialize<IntermediateDataJudgeEventPayload>();
                return NormalizeIds(payload?.IntermediateDataIds);
            }
        }

        if (channelSource.Payload is IntermediateDataJudgeEventPayload payloadObj)
        {
            return NormalizeIds(payloadObj.IntermediateDataIds);
        }

        return new List<string>();
    }

    private static List<string> NormalizeIds(IEnumerable<string> ids)
    {
        return ids?
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();
    }
}
