using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Poxiao.Infrastructure.Configuration;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Manager;
using Poxiao.Infrastructure.Security;
using Poxiao.Schedule;
using Poxiao.TaskScheduler.Entitys;
using Poxiao.TaskScheduler.Entitys.Enum;
using SqlSugar;

namespace Poxiao.Infrastructure.Core;

/// <summary>
/// 作业持久化（数据库）.
/// </summary>
public class DbJobPersistence : IJobPersistence
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public DbJobPersistence(
        IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// 作业调度服务启动时.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<SchedulerBuilder> Preload()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var sqlSugarClient = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();

        // 获取到对应库连接
        var sqlSugarScope = sqlSugarClient.AsTenant().GetConnectionScopeWithAttr<JobDetails>();
        var dynamicJobCompiler = scope.ServiceProvider.GetRequiredService<DynamicJobCompiler>();

        // 获取所有定义的作业
        var allJobs = App.EffectiveTypes.ScanToBuilders().ToList();

        // 若数据库不存在任何作业，则直接返回
        if (!sqlSugarScope.Queryable<JobDetails>().Any(u => true)) return allJobs;

        // 遍历所有定义的作业
        foreach (var schedulerBuilder in allJobs)
        {
            // 获取作业信息构建器
            var jobBuilder = schedulerBuilder.GetJobBuilder();

            // 加载数据库数据
            var dbDetail = sqlSugarScope.Queryable<JobDetails>().First(u => u.JobId == jobBuilder.JobId);
            if (dbDetail == null) continue;

            // 同步数据库数据
            jobBuilder.LoadFrom(dbDetail);

            // 获取作业的所有数据库的触发器
            var dbTriggers = sqlSugarScope.Queryable<JobTriggers>().Where(u => u.JobId == jobBuilder.JobId).ToList();

            // 遍历所有作业触发器
            foreach (var (_, triggerBuilder) in schedulerBuilder.GetEnumerable())
            {
                // 加载数据库数据
                var dbTrigger = dbTriggers.FirstOrDefault(u => u.JobId == jobBuilder.JobId && u.TriggerId == triggerBuilder.TriggerId);
                if (dbTrigger == null) continue;

                triggerBuilder.LoadFrom(dbTrigger).Updated(); // 标记更新
            }

            // 遍历所有非编译时定义的触发器加入到作业中
            foreach (var dbTrigger in dbTriggers)
            {
                if (schedulerBuilder.GetTriggerBuilder(dbTrigger.TriggerId)?.JobId == jobBuilder.JobId) continue;
                var triggerBuilder = TriggerBuilder.Create(dbTrigger.TriggerId).LoadFrom(dbTrigger);
                schedulerBuilder.AddTriggerBuilder(triggerBuilder); // 先添加
                triggerBuilder.Updated(); // 再标记更新
            }

            // 标记更新
            schedulerBuilder.Updated();
        }

        // 获取数据库所有通过脚本创建的作业
        var allDbScriptJobs = sqlSugarScope.Queryable<JobDetails>().Where(u => u.CreateType != RequestTypeEnum.BuiltIn).ToList();
        foreach (var dbDetail in allDbScriptJobs)
        {
            // 动态创建作业
            Type jobType;
            switch (dbDetail.CreateType)
            {
                case RequestTypeEnum.Http:
                    jobType = typeof(PoxiaoHttpJob);
                    break;

                default:
                    throw new NotSupportedException();
            }

            // 动态构建的 jobType 的程序集名称为随机名称，需重新设置
            dbDetail.AssemblyName = jobType.Assembly.FullName!.Split(',')[0];
            var jobBuilder = JobBuilder.Create(jobType).LoadFrom(dbDetail);

            // 强行设置为不扫描 IJob 实现类 [Trigger] 特性触发器，否则 SchedulerBuilder.Create 会再次扫描，导致重复添加同名触发器
            jobBuilder.SetIncludeAnnotations(false);

            // 获取作业的所有数据库的触发器加入到作业中
            var dbTriggers = sqlSugarScope.Queryable<JobTriggers>().Where(u => u.JobId == jobBuilder.JobId).ToArray();
            var triggerBuilders = dbTriggers.Select(u => TriggerBuilder.Create(u.TriggerId).LoadFrom(u).Updated());
            var schedulerBuilder = SchedulerBuilder.Create(jobBuilder, triggerBuilders.ToArray());

            // 标记更新
            schedulerBuilder.Updated();

            allJobs.Add(schedulerBuilder);
        }

        return allJobs;
    }

    /// <summary>
    /// 作业计划初始化通知.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public SchedulerBuilder OnLoading(SchedulerBuilder builder)
    {
        return builder;
    }

    /// <summary>
    /// 作业计划Scheduler的JobDetail变化时.
    /// </summary>
    /// <param name="context"></param>
    public void OnChanged(PersistenceContext context)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var sqlSugarClient = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();

        // 获取到对应库连接
        var sqlSugarScope = sqlSugarClient.AsTenant().GetConnectionScopeWithAttr<JobDetails>();

        var jobDetail = context.JobDetail.Adapt<JobDetails>();
        switch (context.Behavior)
        {
            case PersistenceBehavior.Appended:
                sqlSugarScope.Insertable(jobDetail).ExecuteCommand();
                break;

            case PersistenceBehavior.Updated:
                sqlSugarScope.Updateable(jobDetail).WhereColumns(u => new { u.JobId }).IgnoreColumns(u => new { u.Id, u.CreateType, u.ScriptCode, u.TenantId }).ExecuteCommand();
                break;

            case PersistenceBehavior.Removed:
                sqlSugarScope.Deleteable<JobDetails>().Where(u => u.JobId == jobDetail.JobId).ExecuteCommand();
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// 作业计划Scheduler的触发器Trigger变化时.
    /// </summary>
    /// <param name="context"></param>
    public void OnTriggerChanged(PersistenceTriggerContext context)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var sqlSugarClient = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();

        // 获取到对应库连接
        var sqlSugarScope = sqlSugarClient.AsTenant().GetConnectionScopeWithAttr<JobDetails>();

        var jobTrigger = context.Trigger.Adapt<JobTriggers>();
        var timeTask = jobTrigger.Adapt<TimeTaskEntity>();

        // 根据 `作业Id` 获取到租户ID
        var tenantId = jobTrigger.TriggerId.Match("(.+(?=_trigger_schedule_))");
        timeTask.Id = jobTrigger.TriggerId.Match("((?<=_trigger_schedule_).+)");
        if (KeyVariable.MultiTenancy && !string.IsNullOrEmpty(tenantId))
        {
            var _cacheManager = scope.ServiceProvider.GetService<ICacheManager>();

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

        switch (context.Behavior)
        {
            case PersistenceBehavior.Appended:
                sqlSugarScope.Insertable(jobTrigger).ExecuteCommand();
                break;

            case PersistenceBehavior.Updated:
                sqlSugarScope.Updateable(jobTrigger).WhereColumns(u => new { u.TriggerId, u.JobId }).IgnoreColumns(u => new { u.Id }).ExecuteCommand();
                if (!string.IsNullOrEmpty(tenantId) && !string.IsNullOrEmpty(timeTask.Id))
                {
                    timeTask.LastModifyTime = DateTime.Now;
                    sqlSugarClient.Updateable(timeTask).WhereColumns(u => new { u.Id }).UpdateColumns(u => new { u.LastRunTime, u.NextRunTime, u.RunCount, u.LastModifyTime }).ExecuteCommand();

                    // 执行结果不为空、状态 为就绪时记录日记
                    if (jobTrigger.Status.Equals(TriggerStatus.Ready) && !string.IsNullOrEmpty(context.Trigger.Result))
                    {
                        var timeTaskLog = new TimeTaskLogEntity
                        {
                            Id = SnowflakeIdHelper.NextId(),
                            TaskId = timeTask.Id,
                            RunTime = timeTask.LastRunTime,
                            RunResult = 0,
                            Description = context.Trigger.Result.ToJsonString(),
                        };
                        sqlSugarClient.Insertable(timeTaskLog).ExecuteCommand();
                    }
                }
                break;

            case PersistenceBehavior.Removed:
                sqlSugarScope.Deleteable<JobTriggers>().Where(u => u.TriggerId == jobTrigger.TriggerId && u.JobId == jobTrigger.JobId).ExecuteCommand();
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}