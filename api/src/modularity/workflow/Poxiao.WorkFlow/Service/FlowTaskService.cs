using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Models.WorkFlow;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.WorkFlow.Entitys.Model;
using Poxiao.WorkFlow.Interfaces.Manager;
using Poxiao.WorkFlow.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;

namespace Poxiao.WorkFlow.Service;

/// <summary>
/// 流程任务.
/// </summary>
[ApiDescriptionSettings(Tag = "WorkflowEngine", Name = "FlowTask", Order = 306)]
[Route("api/workflow/Engine/[controller]")]
public class FlowTaskService : IFlowTaskService, IDynamicApiController, ITransient
{
    private readonly IFlowTaskManager _flowTaskManager;
    private readonly IUserManager _userManager;

    public FlowTaskService(IFlowTaskManager flowTaskManager, IUserManager userManager)
    {
        _flowTaskManager = flowTaskManager;
        _userManager = userManager;
    }

    #region Post

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="flowTaskSubmit">请求参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task<dynamic> Create([FromBody] FlowTaskSubmitModel flowTaskSubmit)
    {
        try
        {
            var flowTaskCandidateModel = new List<FlowTaskCandidateModel>();
            flowTaskSubmit.isDelegate = flowTaskSubmit.delegateUserList.Any();//是否委托发起.
            if (!flowTaskSubmit.isDelegate) flowTaskSubmit.delegateUserList.Add(_userManager.UserId);
            foreach (var item in flowTaskSubmit.delegateUserList)
            {
                flowTaskSubmit.crUser = item;
                if (flowTaskSubmit.status == 1)
                {
                    await _flowTaskManager.Save(flowTaskSubmit);
                }
                else
                {
                    flowTaskCandidateModel = await _flowTaskManager.Submit(flowTaskSubmit);
                    if (flowTaskCandidateModel.Any())
                    {
                        return flowTaskCandidateModel;
                    }
                }
            }
            return flowTaskCandidateModel;
        }
        catch (AppFriendlyException ex)
        {
            throw Oops.Oh(ex.ErrorCode, ex.Args);
        }
    }

    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="flowTaskSubmit">请求参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<dynamic> Update(string id, [FromBody] FlowTaskSubmitModel flowTaskSubmit)
    {
        try
        {
            //if (_userManager.UserId.Equals("admin"))
            //    throw PoxiaoException.Oh(ErrorCode.WF0004);
            if (flowTaskSubmit.status == 1)
            {
                await _flowTaskManager.Save(flowTaskSubmit);
                return new List<FlowTaskCandidateModel>();
            }
            else
            {
                return await _flowTaskManager.Submit(flowTaskSubmit);
            }
        }
        catch (AppFriendlyException ex)
        {
            throw Oops.Oh(ex.ErrorCode, ex.Args);
        }
    }
    #endregion
}
