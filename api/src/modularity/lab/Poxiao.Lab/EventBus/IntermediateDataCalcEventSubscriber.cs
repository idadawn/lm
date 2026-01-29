using Microsoft.Extensions.DependencyInjection;
using Poxiao.DependencyInjection;
using Poxiao.EventBus;
using Poxiao.Lab.EventBus;
using Poxiao.Lab.Interfaces;

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
        var service = scope.ServiceProvider.GetService<IIntermediateDataService>();
        if (service == null)
        {
            return;
        }

        try
        {
            await service.BatchCalculateFormulasByBatchIdAsync(source.BatchId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"中间数据公式计算事件处理失败: {ex.Message}");
        }
    }
}