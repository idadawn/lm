using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Poxiao.Infrastructure.Configuration;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Manager;
using Poxiao.Infrastructure.Security;
using Poxiao.Schedule;
using Poxiao.TaskScheduler.Entitys;
using SqlSugar;

namespace Poxiao.TaskScheduler.Listener;

/// <summary>
/// 本地任务Demo.
/// </summary>
[JobDetail("job_builtIn_test", Description = "本地任务Demo", GroupName = "BuiltIn", Concurrent = true)]
public class SpareTimeDemo : IJob
{
    /// <summary>
    /// 服务提供器.
    /// </summary>
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SpareTimeDemo> _logger;

    /// <summary>
    /// 构造函数.
    /// </summary>
    /// <param name="serviceProvider">服务提供器.</param>
    /// <param name="logger">日记</param>
    public SpareTimeDemo(IServiceProvider serviceProvider, ILogger<SpareTimeDemo> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        using var serviceScope = _serviceProvider.CreateScope();
        var sqlSugarClient = _serviceProvider.GetRequiredService<ISqlSugarClient>();

        // 多租户场景 需要获取到 租户ID
        var tenantId = context.TriggerId.Match("(.+(?=_trigger_schedule_))");
        var taskId = context.TriggerId.Match("((?<=_trigger_schedule_).+)");

        _logger.LogInformation($"租户ID:{tenantId}");

        if (KeyVariable.MultiTenancy)
        {
            var _cacheManager = _serviceProvider.GetService<ICacheManager>();

            string cacheKey = string.Format("{0}", CommonConst.GLOBALTENANT);
            var tenant = _cacheManager.Get<List<GlobalTenantCacheModel>>(cacheKey).Find(it => it.TenantId.Equals(tenantId));

            if (KeyVariable.MultiTenancyType.Equals("COLUMN"))
            {
                string serviceName = tenant.connectionConfig.IsolationField;
                sqlSugarClient.Aop.DataExecuting = (oldValue, entityInfo) =>
                {
                    if (entityInfo.PropertyName == "TenantId" && entityInfo.OperationType == DataFilterType.InsertByObject)
                    {
                        entityInfo.SetValue(serviceName);
                    }
                };
            }
            else
            {
                sqlSugarClient.AsTenant().AddConnection(PoxiaoTenantExtensions.GetConfig(tenant.connectionConfig));
                sqlSugarClient.AsTenant().ChangeDatabase(tenantId);
            }
        }

        // 对应的数据库操作=========================

        await Task.Delay(2000, stoppingToken); // 这里模拟耗时操作，比如耗时2秒

        // 记录本地任务日记
        var entity = new TimeTaskLogEntity
        {
            Id = SnowflakeIdHelper.NextId(),
            TaskId = taskId,
            RunTime = DateTime.Now,
            RunResult = 0,
            Description = "执行成功",
        };
        await sqlSugarClient.Insertable(entity).ExecuteCommandAsync();
    }
}