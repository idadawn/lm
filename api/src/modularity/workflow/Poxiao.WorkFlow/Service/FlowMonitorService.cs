using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.WorkFlow.Entitys.Dto.FlowMonitor;
using Poxiao.WorkFlow.Interfaces.Repository;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace Poxiao.WorkFlow.Service;

/// <summary>
/// 流程监控.
/// </summary>
[ApiDescriptionSettings(Tag = "WorkflowEngine", Name = "FlowMonitor", Order = 304)]
[Route("api/workflow/Engine/[controller]")]
public class FlowMonitorService : IDynamicApiController, ITransient
{
    private readonly IFlowTaskRepository _flowTaskRepository;

    /// <param name="flowTaskRepository"></param>
    public FlowMonitorService(IFlowTaskRepository flowTaskRepository)
    {
        _flowTaskRepository = flowTaskRepository;
    }

    #region GET

    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] FlowMonitorListQuery input)
    {
        return await _flowTaskRepository.GetMonitorList(input);
    }

    /// <summary>
    /// 批量删除.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpDelete]
    public async Task Delete([FromBody] FlowMonitorDeleteInput input)
    {
        var ids = input.ids.Split(",").ToList();
        var tsakList = await _flowTaskRepository.GetTaskList(x => ids.Contains(x.Id) && x.FlowType == 1);
        if (tsakList.Any()) throw Oops.Oh(ErrorCode.WF0012, tsakList.FirstOrDefault().FullName);
        tsakList = await _flowTaskRepository.GetTaskList(x => ids.Contains(x.Id) && !x.ParentId.Equals("0") && !SqlFunc.IsNullOrEmpty(x.ParentId));
        if (tsakList.Any()) throw Oops.Oh(ErrorCode.WF0003, tsakList.FirstOrDefault().FullName);
        tsakList = await _flowTaskRepository.GetTaskList(x => ids.Contains(x.Id) && x.Suspend == 1);
        if (tsakList.Any()) throw Oops.Oh(ErrorCode.WF0047, tsakList.FirstOrDefault().FullName);
        tsakList = await _flowTaskRepository.GetTaskList(x => ids.Contains(x.ParentId) && x.DeleteMark == null && x.Suspend == 1);
        if (tsakList.Any()) throw Oops.Oh(ErrorCode.WF0047, tsakList.FirstOrDefault().FullName);
        foreach (var item in input.ids.Split(","))
        {
            var entity = _flowTaskRepository.GetTaskFirstOrDefault(item);
            if (entity.IsNotEmptyOrNull())
            {
                await _flowTaskRepository.DeleteSubTask(entity);
                await _flowTaskRepository.DeleteTask(entity);
            }
        }
    }
    #endregion
}
