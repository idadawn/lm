using Microsoft.Extensions.DependencyInjection;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Manager;
using Poxiao.Logging;
using Poxiao.Schedule;

namespace Poxiao.TaskScheduler.Listener;

[JobDetail("job_onlineUser", Description = "清理在线用户", GroupName = "default", Concurrent = false)]
[PeriodSeconds(1, TriggerId = "trigger_onlineUser", Description = "清理在线用户", MaxNumberOfRuns = 1, RunOnStart = true)]
public class OnlineUserJob : IJob
{
    private readonly IServiceProvider _serviceProvider;

    public OnlineUserJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        using var serviceScope = _serviceProvider.CreateScope();

        var _cacheManager = serviceScope.ServiceProvider.GetService<ICacheManager>();

        var keys = _cacheManager.GetAllCacheKeys().FindAll(q => q.Contains(CommonConst.CACHEKEYONLINEUSER));

        if (keys.Any())
        {
            foreach (var key in keys)
            {
                await _cacheManager.DelAsync(key);
            }
        }

        Log.Information("【{Now}】服务重启清空在线用户", DateTime.Now);
    }
}