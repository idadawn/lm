using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Models;
using Poxiao.Infrastructure.Security;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.Systems.Entitys.Entity.System;
using Poxiao.Systems.Interfaces.System;
using Poxiao.TaskScheduler.Entitys;
using Poxiao.TaskScheduler.Interfaces.TaskScheduler;
using Poxiao.WorkFlow.Entitys.Model;
using Poxiao.WorkFlow.Entitys.Model.Properties;
using Poxiao.WorkFlow.Interfaces.Manager;
using Microsoft.AspNetCore.Mvc;

namespace Poxiao.Systems.Common;

/// <summary>
/// 定时任务(内部调用).
/// </summary>
[ApiDescriptionSettings(Name = "ScheduleTask", Order = 306)]
[Route("[controller]")]
public class ScheduleTaskService : IDynamicApiController, ITransient
{
    private readonly IScheduleService _scheduleService;
    private readonly IFlowTaskManager _flowTaskManager;
    private readonly ITimeTaskService _timeTaskService;

    public ScheduleTaskService(
        IScheduleService scheduleService,
        IFlowTaskManager flowTaskManager,
        ITimeTaskService timeTaskService)
    {
        _scheduleService = scheduleService;
        _flowTaskManager = flowTaskManager;
        _timeTaskService = timeTaskService;
    }

    /// <summary>
    /// 定时任务.
    /// </summary>
    /// <param name="taskCode"></param>
    /// <param name="scheduleTaskModel"></param>
    /// <returns></returns>
    [HttpPost("{taskCode}")]
    public async Task<dynamic> ScheduleTask(string taskCode, [FromBody] ScheduleTaskModel scheduleTaskModel)
    {
        switch (taskCode)
        {
            case "schedule":
                var scheduleEntity = scheduleTaskModel.taskParams["entity"].ToObject<ScheduleEntity>();
                var userList = scheduleTaskModel.taskParams["userList"].ToObject<List<string>>();
                var type = scheduleTaskModel.taskParams["type"].ToString();
                var enCode = scheduleTaskModel.taskParams["enCode"].ToString();
                await _scheduleService.SendScheduleMsg(scheduleEntity, userList, type, enCode);
                break;
            case "flowtask":
                var approPro = scheduleTaskModel.taskParams["approPro"].ToObject<ApproversProperties>();
                var flowTaskParamter = scheduleTaskModel.taskParams["flowTaskParamter"].ToObject<FlowTaskParamter>();
                var nodeId = scheduleTaskModel.taskParams["nodeId"].ToString();
                var count = scheduleTaskModel.taskParams["count"].ParseToInt();
                var isTimeOut = scheduleTaskModel.taskParams["isTimeOut"].ParseToBool();
                await _flowTaskManager.NotifyEvent(approPro, flowTaskParamter, nodeId, count, isTimeOut);
                break;
            case "timetask":
                var entity = scheduleTaskModel.taskParams["entity"].ToObject<TimeTaskEntity>();
                await _timeTaskService.PerformJob(entity);
                break;
            default:
                break;
        }
        return string.Empty;
    }
}
