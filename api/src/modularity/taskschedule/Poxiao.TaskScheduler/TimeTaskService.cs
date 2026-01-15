using Aop.Api.Domain;
using Poxiao.Infrastructure.Configuration;
using Poxiao.Infrastructure.Core.Job;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Filter;
using Poxiao.Infrastructure.Manager;
using Poxiao.Infrastructure.Models;
using Poxiao.Infrastructure.Security;
using Poxiao.DatabaseAccessor;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.LinqBuilder;
using Poxiao.Schedule;
using Poxiao.Systems.Interfaces.System;
using Poxiao.TaskScheduler.Entitys;
using Poxiao.TaskScheduler.Entitys.Dto.TaskScheduler;
using Poxiao.TaskScheduler.Entitys.Enum;
using Poxiao.TaskScheduler.Entitys.Model;
using Poxiao.TaskScheduler.Interfaces.TaskScheduler;
using Poxiao.TaskScheduler.Listener;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using SqlSugar;

namespace Poxiao.TaskScheduler;

/// <summary>
/// 定时任务
/// 版 本：V3.4.7
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2023-07-14
/// 数据接口 会手动创建一条持久化任务JobDetails数据
/// 本地任务 程序加载时会自动创建一条持久化任务JobDetails数据
/// 两者均会根据系统调度设计创建持久化触发器Trigger 数据.
/// </summary>
[ApiDescriptionSettings(Tag = "TaskScheduler", Name = "scheduletask", Order = 220)]
[Route("api/[controller]")]
public class TimeTaskService : ITimeTaskService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<TimeTaskEntity> _repository;
    private readonly IDataInterfaceService _dataInterfaceService;
    private readonly IUserManager _userManager;
    private readonly ICacheManager _cacheManager;
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IDataBaseManager _dataBaseManager;
    private readonly ILogger<TimeTaskService> _logger;

    /// <summary>
    /// 服务提供器.
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 初始化一个<see cref="TimeTaskService"/>类型的新实例.
    /// </summary>
    public TimeTaskService(
        IServiceProvider serviceProvider,
        ISqlSugarRepository<TimeTaskEntity> repository,
        IUserManager userManager,
        IDataInterfaceService dataInterfaceService,
        ICacheManager cacheManager,
        ISchedulerFactory schedulerFactory,
        IDataBaseManager dataBaseManager,
        ILogger<TimeTaskService> logger)
    {
        _serviceProvider = serviceProvider;
        _repository = repository;
        _userManager = userManager;
        _dataInterfaceService = dataInterfaceService;
        _cacheManager = cacheManager;
        _dataBaseManager = dataBaseManager;
        _schedulerFactory = schedulerFactory;
        _logger = logger;
    }

    #region Get

    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="input">请求参数</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] PageInputBase input)
    {
        var queryWhere = LinqExpression.And<TimeTaskEntity>().And(x => x.DeleteMark == null);
        if (!string.IsNullOrEmpty(input.Keyword))
            queryWhere = queryWhere.And(m => m.FullName.Contains(input.Keyword) || m.EnCode.Contains(input.Keyword));
        var list = await _repository.AsQueryable().Where(queryWhere).OrderBy(x => x.CreatorTime, OrderByType.Desc)
            .OrderByIF(!string.IsNullOrEmpty(input.Keyword), t => t.LastModifyTime, OrderByType.Desc).ToPagedListAsync(input.CurrentPage, input.PageSize);
        var pageList = new SqlSugarPagedList<TimeTaskListOutput>()
        {
            list = list.list.Adapt<List<TimeTaskListOutput>>(),
            pagination = list.pagination
        };
        return PageResult<TimeTaskListOutput>.SqlSugarPageResult(pageList);
    }

    /// <summary>
    /// 列表（执行记录）.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <param name="id">任务Id.</param>
    /// <returns></returns>
    [HttpGet("{id}/TaskLog")]
    public async Task<dynamic> GetTaskLogList([FromQuery] TaskLogInput input, string id)
    {
        var whereLambda = LinqExpression.And<TimeTaskLogEntity>().And(x => x.TaskId == id);
        if (input.runResult.IsNotEmptyOrNull())
            whereLambda = whereLambda.And(x => x.RunResult == input.runResult);
        if (input.endTime != null && input.startTime != null)
        {
            var start = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 00:00:00}", input.startTime?.TimeStampToDateTime()));
            var end = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 23:59:59}", input.endTime?.TimeStampToDateTime()));
            whereLambda = whereLambda.And(x => SqlFunc.Between(x.RunTime, start, end));
        }
        var list = await _repository.AsSugarClient().Queryable<TimeTaskLogEntity>().Where(whereLambda).OrderBy(x => x.RunTime, OrderByType.Desc).ToPagedListAsync(input.CurrentPage, input.PageSize);
        var pageList = new SqlSugarPagedList<TimeTaskTaskLogListOutput>()
        {
            list = list.list.Adapt<List<TimeTaskTaskLogListOutput>>(),
            pagination = list.pagination
        };
        return PageResult<TimeTaskTaskLogListOutput>.SqlSugarPageResult(pageList);
    }

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("Info/{id}")]
    public async Task<dynamic> GetInfo_Api(string id)
    {
        return (await GetInfo(id)).Adapt<TimeTaskInfoOutput>();
    }

    /// <summary>
    /// 本地方法.
    /// </summary>
    /// <returns></returns>
    [HttpGet("TaskMethods")]
    public async Task<dynamic> GetTaskMethodSelector()
    {
        return await GetTaskMethods();
    }

    #endregion

    #region Post

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">实体对象.</param>
    /// <returns></returns>
    [HttpPost("")]
    [UnitOfWork]
    public async Task Create([FromBody] TimeTaskCrInput input)
    {
        if (await _repository.IsAnyAsync(x => (x.EnCode == input.enCode || x.FullName == input.fullName) && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1004);
        var contentModel = input.executeContent.ToObject<ContentModel>();
        contentModel.TenantId = _userManager.TenantId;
        TimeTaskEntity? entity = input.Adapt<TimeTaskEntity>();
        entity.ExecuteContent = contentModel.ToJsonString();
        entity.ExecuteCycleJson = contentModel.cron;
        var result = await _repository.AsInsertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Create()).ExecuteReturnEntityAsync();
        var job = _repository.AsTenant().GetConnection("Poxiao-Job");

        // 动态创建作业
        Type jobType = null;

        switch (entity.EnabledMark)
        {
            case 1:
                var createType = RequestTypeEnum.Http;

                TriggerBuilder? triggerBuilder = ObtainTriggerBuilder(contentModel, result.Id, entity.FullName);
                triggerBuilder.SetResult("任务创建");

                var jobDetail = new JobDetails();
                switch (entity.ExecuteType)
                {
                    // 数据接口
                    case "1":
                        jobDetail = ObtainJobDetails(entity, result.Id);
                        jobType = typeof(PoxiaoHttpJob);

                        createType = RequestTypeEnum.Http;

                        _schedulerFactory.AddJob(
                            JobBuilder.Create(jobType).LoadFrom(jobDetail)
                            .SetJobType(jobType),
                            triggerBuilder);
                        break;

                    // 本地方法
                    case "3":
                        ScheduleResult scheduleResult = _schedulerFactory.TryGetJob(contentModel.localHostTaskId, out var scheduler);
                        createType = RequestTypeEnum.BuiltIn;
                        jobDetail = scheduler.GetJobDetail().Adapt<JobDetails>();
                        scheduler.AddTrigger(triggerBuilder);
                        break;
                }

                // 延迟一下等待持久化写入，再执行其他字段的更新
                await Task.Delay(500);

                await job.Updateable<JobDetails>().SetColumns(u => new JobDetails { CreateType = createType })
                      .Where(u => u.JobId.Equals(jobDetail.JobId)).ExecuteCommandAsync();
                break;
            case 0:
                var timeTaskLog = new TimeTaskLogEntity
                {
                    Id = SnowflakeIdHelper.NextId(),
                    TaskId = result.Id,
                    RunTime = DateTime.Now,
                    RunResult = 0,
                    Description = "任务创建",
                };
                await _repository.AsSugarClient().Insertable(timeTaskLog).ExecuteCommandAsync();
                break;
        }
    }

    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="id">主键值</param>
    /// <param name="input">实体对象</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] TimeTaskUpInput input)
    {
        if (await _repository.IsAnyAsync(x => x.Id != id && (x.EnCode == input.enCode || x.FullName == input.fullName) && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1004);

        var job = _repository.AsTenant().GetConnection("Poxiao-Job");

        var taskAdjustment = TaskAdjustmentEnum.Update;

        // 获取旧数据
        var entity = await _repository.GetFirstAsync(it => it.Id.Equals(id));
        var numberOfRuns = entity.RunCount;
        if (entity.EnCode != input.enCode)
            throw Oops.Oh(ErrorCode.D1102);
        var jobId = string.Empty;
        var contentModel = entity.ExecuteContent.ToObject<ContentModel>();
        switch (entity.ExecuteType)
        {
            case "1":
                jobId = string.Format("Job_Http_{0}_{1}", _userManager.TenantId, entity.EnCode);
                break;
            case "3":
                jobId = contentModel.localHostTaskId;
                break;
        }

        // 是否启用改禁用或者 禁用改启用
        switch (entity.EnabledMark != input.enabledMark)
        {
            /*
             * 开启改禁用
             * 无改动 纯粹暂停任务
             * 有改动 且暂停任务
             * -----------------------
             * 禁用改开启
             * 新增 持久化任务
             * 无改动 纯粹开启任务
             * 有改动 且开启任务
             */
            case true:
                {
                    switch (input.enabledMark)
                    {
                        // `禁用` 改 `启用`
                        case 1:

                            // 改启用时需要判断调度持久化是否已存在 需要注意 这里需要判断原数据任务类型是本地任务还是数据接口 因为本地任务JobDetails数据本身就存在因此是判断Trigger是否存在
                            var isExist = false;
                            switch (entity.ExecuteType)
                            {
                                case "1":
                                    isExist = _schedulerFactory.ContainsJob(jobId);
                                    break;
                                case "3":
                                    var scheduler = _schedulerFactory.GetJob(jobId);
                                    isExist = scheduler.ContainsTrigger(string.Format("{0}_trigger_schedule_{1}", _userManager.TenantId, id));
                                    break;
                            }

                            // 任务是否存在
                            switch (isExist)
                            {
                                // 不存在
                                case false:
                                    taskAdjustment = TaskAdjustmentEnum.OpenAndAdd;
                                    break;

                                // 存在
                                default:
                                    // 任务类型是否被更改
                                    switch (entity.ExecuteType != input.executeType)
                                    {
                                        case true:
                                            switch (input.executeType)
                                            {
                                                case "1":
                                                    taskAdjustment = TaskAdjustmentEnum.ChangeHttpAndOpen;
                                                    break;
                                                case "3":
                                                    taskAdjustment = TaskAdjustmentEnum.ChangeBuiltInAndOpen;
                                                    break;
                                            }
                                            break;
                                        case false:
                                            taskAdjustment = TaskAdjustmentEnum.Open;
                                            break;
                                    }
                                    break;
                            }
                            break;

                        // 启用改禁用
                        default:
                            // 任务类型是否被更改
                            switch (entity.ExecuteType != input.executeType)
                            {
                                case true:
                                    switch (input.executeType)
                                    {
                                        case "1":
                                            taskAdjustment = TaskAdjustmentEnum.ChangeHttpAndPause;
                                            break;
                                        case "3":
                                            taskAdjustment = TaskAdjustmentEnum.ChangeBuiltInAndPause;
                                            break;
                                    }
                                    break;
                                case false:
                                    taskAdjustment = TaskAdjustmentEnum.Suspend;
                                    break;
                            }
                            break;
                    }
                }

                break;

            // 本身就是 `启用` 或者 `禁用`
            case false:
                switch (input.enabledMark)
                {
                    // 本身就是 `启用`
                    case 1:
                        // 任务类型是否被更改
                        switch (entity.ExecuteType != input.executeType)
                        {
                            case true:
                                switch (input.executeType)
                                {
                                    case "1":
                                        taskAdjustment = TaskAdjustmentEnum.ChangeHttp;
                                        break;
                                    case "3":
                                        taskAdjustment = TaskAdjustmentEnum.ChangeBuiltIn;
                                        break;
                                }
                                break;
                        }
                        break;
                    case 0:
                        // 任务类型是否被更改
                        switch (entity.ExecuteType != input.executeType)
                        {
                            case true:
                                switch (input.executeType)
                                {
                                    case "1":
                                        taskAdjustment = TaskAdjustmentEnum.ChangeHttpAndPause;
                                        break;
                                    case "3":
                                        taskAdjustment = TaskAdjustmentEnum.ChangeBuiltInAndPause;
                                        break;
                                }
                                break;
                        }
                        break;

                }
                break;
        }

        entity = input.Adapt<TimeTaskEntity>();
        contentModel = input.executeContent.ToObject<ContentModel>();
        contentModel.TenantId = _userManager.TenantId;
        entity.ExecuteContent = contentModel.ToJsonString();
        entity.ExecuteCycleJson = contentModel.cron;
        entity.LastModifyTime = DateTime.Now;
        entity.LastModifyUserId = _userManager.UserId;
        var isOk = await _repository.AsUpdateable(entity).UpdateColumns(it => new {
            it.EnCode,
            it.FullName,
            it.ExecuteType,
            it.ExecuteContent,
            it.ExecuteCycleJson,
            it.Description,
            it.EnabledMark,
            it.LastModifyTime,
            it.LastModifyUserId,
        }).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);

        {
            var scheduler = _schedulerFactory.GetJob(jobId);

            TriggerBuilder? triggerBuilder = ObtainTriggerBuilder(contentModel, id, entity.FullName, numberOfRuns);
            ScheduleResult scheduleResult = new ScheduleResult();
            var trigger = new Trigger();
            var jobDetail = new JobDetails();

            // 动态创建作业
            Type jobType = null;

            var createType = RequestTypeEnum.Http;

            switch (taskAdjustment)
            {
                // 纯禁用 根据任务类型不同 暂停逻辑不同
                case TaskAdjustmentEnum.Suspend:

                    // 数据接口是一对一 本地任务是一对N
                    switch (input.executeType)
                    {
                        case "1":
                            triggerBuilder.SetResult("禁用任务");
                            scheduleResult = scheduler.TryUpdateTrigger(triggerBuilder, out trigger);
                            scheduler.PauseTrigger(trigger.TriggerId);
                            break;
                        case "3":
                            createType = RequestTypeEnum.BuiltIn;
                            triggerBuilder.SetResult("禁用任务");
                            scheduleResult = scheduler.TryUpdateTrigger(triggerBuilder, out trigger);
                            scheduler.PauseTrigger(trigger.TriggerId);
                            break;
                    }

                    break;
                case TaskAdjustmentEnum.ChangeBuiltInAndPause:
                    // http 转 内置 先删除 触发器 在删除任务
                    scheduleResult = scheduler.TryRemoveTrigger(triggerBuilder.TriggerId, out trigger);
                    scheduleResult = _schedulerFactory.TryRemoveJob(scheduler);

                    scheduleResult = _schedulerFactory.TryGetJob(contentModel.localHostTaskId, out scheduler);
                    createType = RequestTypeEnum.BuiltIn;
                    jobDetail = scheduler.GetJobDetail().Adapt<JobDetails>();
                    triggerBuilder.SetStatus(TriggerStatus.Pause);
                    triggerBuilder.SetResult("修改任务类型且禁用任务");
                    scheduler.AddTrigger(triggerBuilder);
                    break;
                case TaskAdjustmentEnum.ChangeHttpAndPause:
                    scheduleResult = scheduler.TryRemoveTrigger(triggerBuilder.TriggerId, out trigger);
                    jobType = typeof(PoxiaoHttpJob);
                    jobDetail = ObtainJobDetails(entity, id);
                    triggerBuilder.SetStatus(TriggerStatus.Pause);
                    triggerBuilder.SetResult("修改任务类型且禁用任务");

                    _schedulerFactory.AddJob(
                        JobBuilder.Create(jobType).LoadFrom(jobDetail)
                        .SetJobType(jobType),
                        triggerBuilder);
                    break;
                case TaskAdjustmentEnum.Open:
                    // 数据接口是一对一 本地任务是一对N
                    triggerBuilder.SetStatus(TriggerStatus.Ready);
                    triggerBuilder.SetResult("启用任务");
                    scheduler.StartTrigger(triggerBuilder.TriggerId);
                    break;
                case TaskAdjustmentEnum.OpenAndAdd:
                    switch (entity.ExecuteType)
                    {
                        // 数据接口
                        case "1":
                            jobDetail = ObtainJobDetails(entity, id);
                            jobType = typeof(PoxiaoHttpJob);

                            _schedulerFactory.AddJob(
                                JobBuilder.Create(jobType).LoadFrom(jobDetail)
                                .SetJobType(jobType),
                                triggerBuilder);
                            break;

                        // 本地方法
                        case "3":
                            scheduleResult = _schedulerFactory.TryGetJob(contentModel.localHostTaskId, out scheduler);
                            createType = RequestTypeEnum.BuiltIn;
                            jobDetail = scheduler.GetJobDetail().Adapt<JobDetails>();
                            scheduler.AddTrigger(triggerBuilder);
                            break;
                    }
                    break;
                case TaskAdjustmentEnum.ChangeBuiltIn:
                case TaskAdjustmentEnum.ChangeBuiltInAndOpen:
                    // http 转 内置 先删除 触发器 在删除任务
                    scheduleResult = scheduler.TryRemoveTrigger(triggerBuilder.TriggerId, out trigger);
                    scheduleResult = _schedulerFactory.TryRemoveJob(scheduler);

                    scheduleResult = _schedulerFactory.TryGetJob(contentModel.localHostTaskId, out scheduler);
                    createType = RequestTypeEnum.BuiltIn;
                    jobDetail = scheduler.GetJobDetail().Adapt<JobDetails>();
                    scheduler.AddTrigger(triggerBuilder);
                    break;
                case TaskAdjustmentEnum.ChangeHttp:
                case TaskAdjustmentEnum.ChangeHttpAndOpen:
                    scheduleResult = scheduler.TryRemoveTrigger(triggerBuilder.TriggerId, out trigger);
                    jobType = typeof(PoxiaoHttpJob);
                    jobDetail = ObtainJobDetails(entity, id);

                    _schedulerFactory.AddJob(
                        JobBuilder.Create(jobType).LoadFrom(jobDetail)
                        .SetJobType(jobType),
                        triggerBuilder);
                    break;

                // 纯更新
                default:
                    switch (input.enabledMark)
                    {
                        case 1:
                            scheduler.UpdateTrigger(triggerBuilder);
                            break;
                    }
                    break;
            }

            // 延迟一下等待持久化写入，再执行其他字段的更新
            await Task.Delay(500);

            await job.Updateable<JobDetails>().SetColumns(u => new JobDetails { CreateType = createType })
                .Where(u => u.JobId.Equals(jobDetail.JobId)).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await GetInfo(id);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);
        var isOk = await _repository.AsUpdateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);

        var jobId = string.Empty;
        var contentModel = entity.ExecuteContent.ToObject<ContentModel>();
        switch (entity.ExecuteType)
        {
            case "1":
                jobId = string.Format("Job_Http_{0}_{1}", _userManager.TenantId, entity.EnCode);
                break;
            case "3":
                jobId = contentModel.localHostTaskId;
                break;
        }

        var scheduler = _schedulerFactory.GetJob(jobId);

        switch (entity.ExecuteType)
        {
            case "1":
                var scheduleResult = _schedulerFactory.TryRemoveJob(scheduler);
                break;
            case "3":
                scheduler.RemoveTrigger(string.Format("{0}_trigger_schedule_{1}", _userManager.TenantId, id));
                break;
        }

    }

    #endregion

    #region PublicMethod

    /// <summary>
    /// 根据类型执行任务.
    /// </summary>
    /// <param name="entity">任务实体.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<string> PerformJob(TimeTaskEntity entity)
    {
        var model = entity.ExecuteContent.ToObject<ContentModel>();
        _logger.LogInformation($"任务调度触发：【任务名称：{entity.FullName},任务类型：数据接口,请求接口：{model.interfaceName}】");
        try
        {
            var parameters = model.parameter.ToDictionary(key => key.field, value => value.value.IsNotEmptyOrNull() ? value.value : value.defaultValue);
            await _dataInterfaceService.GetResponseByType(model.interfaceId, 3, model.TenantId, null, parameters);
            return string.Empty;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    #endregion

    #region PrivateMethod

    /// <summary>
    /// 详情.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    private async Task<TimeTaskEntity> GetInfo(string id)
    {
        return await _repository.GetFirstAsync(x => x.Id == id && x.DeleteMark == null);
    }

    /// <summary>
    /// 获取所有本地任务.
    /// </summary>
    /// <returns></returns>
    private async Task<List<TaskMethodInfo>> GetTaskMethods()
    {
        var list = new List<TaskMethodInfo>();

        // 获取所有定义的作业
        List<SchedulerBuilder>? allJobs = App.EffectiveTypes.ScanToBuilders().ToList();

        // 遍历所有定义的作业
        foreach (var schedulerBuilder in allJobs)
        {
            // 获取作业信息构建器
            var jobBuilder = schedulerBuilder.GetJobBuilder();

            // 忽略不视为本地任务的任务组
            var ignoreRecordingTasks = new List<string>() { "schedule", "default" };

            if (!ignoreRecordingTasks.Contains(jobBuilder.GroupName))
            {
                list.Add(new TaskMethodInfo
                {
                    id = jobBuilder.JobId,
                    fullName = jobBuilder.Description
                });
            }
        }

        return list;
    }

    /// <summary>
    /// 获取作业触发器构建器.
    /// </summary>
    /// <returns></returns>
    private TriggerBuilder ObtainTriggerBuilder(ContentModel contentModel, string id, string fullName, int? numberOfRuns = null)
    {
        var cornType = contentModel.cron.Split(" ").Length == 7 ? 3 : 2;
        var args = new List<object>
        {
            contentModel.cron,
            cornType
        };

        TriggerBuilder? triggerBuilder = TriggerBuilder.Create(string.Format("{0}_trigger_schedule_{1}", _userManager.TenantId, id));
        if (numberOfRuns != null)
            triggerBuilder.SetNumberOfRuns(numberOfRuns.ParseToLong());
        triggerBuilder.SetTriggerType("Poxiao", "Poxiao.Schedule.CronTrigger");
        triggerBuilder.SetArgs(args.ToJsonString());
        triggerBuilder.SetDescription(string.Format("{0}调度触发器", fullName));
        triggerBuilder.SetStartTime(contentModel.startTime);
        if (contentModel.endTime != null)
        {
            triggerBuilder.SetEndTime(contentModel.endTime);
        }

        return triggerBuilder;
    }

    /// <summary>
    /// 获取作业信息.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public JobDetails ObtainJobDetails(TimeTaskEntity entity, string id)
    {
        var server = _serviceProvider.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>();
        var addressesFeature = server.Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>();
        var addresses = addressesFeature?.Addresses;
        var localAddress = addresses.FirstOrDefault().Replace("[::]", "localhost");

        var properties = new Dictionary<string, object>();

        var scheduleTaskModel = new ScheduleTaskModel();
        scheduleTaskModel.taskParams.Add("entity", entity);

        var httpJob = new PoxiaoHttpJobMessage
        {
            RequestUri = string.Format("{0}/ScheduleTask/timetask", localAddress),
            HttpMethod = HttpMethod.Post,
            Body = scheduleTaskModel.ToJsonString(),
            TaskId = id,
            TenantId = _userManager.TenantId,
            UserId = _userManager.UserId,
        };

        properties.Add("PoxiaoHttpJob", httpJob.ToJsonString());

        return new JobDetails
        {
            JobId = string.Format("Job_Http_{0}_{1}", _userManager.TenantId, entity.EnCode), // 作业 Id
            Description = string.Format("租户`{0}`下名称为`{1}`的HTTP系统调度", _userManager.TenantId, entity.FullName),
            GroupName = "TimeTask", // 作业组名称
            Concurrent = true, // 并行还是串行方式，false 为 串行
            IncludeAnnotations = true, // 是否扫描 IJob 类型的触发器特性，true 为 扫描
            CreateType = RequestTypeEnum.Http,
            TenantId = _userManager.TenantId,
            Properties = properties.ToJsonString()
        };
    }

    #endregion
}