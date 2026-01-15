using Poxiao.Infrastructure.Configuration;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Manager;
using Poxiao.DependencyInjection;
using Poxiao.EventBus;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace Poxiao.EventHandler;

/// <summary>
/// 日记事件订阅.
/// </summary>
public class LogEventSubscriber : IEventSubscriber, ISingleton
{
    /// <summary>
    /// 初始化客户端.
    /// </summary>
    private static SqlSugarScope? _sqlSugarClient;

    /// <summary>
    /// 服务提供器.
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 构造函数.
    /// </summary>
    public LogEventSubscriber(IServiceProvider serviceProvider, ISqlSugarClient context)
    {
        _serviceProvider = serviceProvider;
        _sqlSugarClient = (SqlSugarScope)context;
    }

    /// <summary>
    /// 创建日记.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe("Log:CreateReLog")]
    [EventSubscribe("Log:CreateExLog")]
    [EventSubscribe("Log:CreateVisLog")]
    [EventSubscribe("Log:CreateOpLog")]
    public async Task CreateLog(EventHandlerExecutingContext context)
    {
        var log = (LogEventSource)context.Source;
        if (KeyVariable.MultiTenancy)
        {
            using var serviceScope = _serviceProvider.CreateScope();
            var _cacheManager = serviceScope.ServiceProvider.GetService<ICacheManager>();

            string cacheKey = string.Format("{0}", CommonConst.GLOBALTENANT);
            var tenant = (await _cacheManager.GetAsync<List<GlobalTenantCacheModel>>(cacheKey)).Find(it => it.TenantId.Equals(log.TenantId));

            if (KeyVariable.MultiTenancyType.Equals("COLUMN"))
            {
                string ServiceName = tenant.connectionConfig.IsolationField;
                _sqlSugarClient.Aop.DataExecuting = (oldValue, entityInfo) =>
                {
                    if (entityInfo.PropertyName == "TenantId" && entityInfo.OperationType == DataFilterType.InsertByObject)
                    {
                        entityInfo.SetValue(ServiceName);
                    }
                };
            }
            else
            {
                _sqlSugarClient.AddConnection(PoxiaoTenantExtensions.GetConfig(tenant.connectionConfig));
                _sqlSugarClient.ChangeDatabase(log.TenantId);
            }
        }

        await _sqlSugarClient.Insertable(log.Entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
    }
}