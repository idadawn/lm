using Microsoft.Extensions.DependencyInjection;
using Poxiao.DependencyInjection;
using Poxiao.EventBus;
using Poxiao.Logging;
using Poxiao.Lab.EventBus;
using Poxiao.Lab.Service;

namespace Poxiao.EventHandler;

/// <summary>
/// 中间数据公式计算事件订阅（保留 CalcByBatch 作为旧路径兼容）。
/// 判定逻辑已迁移到 Worker（通过 RabbitMQ），不再使用 EventBus。
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
}
