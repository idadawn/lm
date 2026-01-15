using Poxiao.Infrastructure.Core.Manager;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.Extend.Entitys;
using Poxiao.Extend.Entitys.Dto.Email;
using Poxiao.Message.Entitys;
using Poxiao.Message.Interfaces.Message;
using Poxiao.Systems.Entitys.Permission;
using Poxiao.VisualDev.Entitys.Dto.Dashboard;
using Poxiao.WorkFlow.Entitys.Entity;
using Poxiao.WorkFlow.Interfaces.Repository;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace Poxiao.VisualDev;

/// <summary>
///  业务实现：主页显示.
/// </summary>
[ApiDescriptionSettings(Tag = "VisualDev", Name = "Dashboard", Order = 174)]
[Route("api/visualdev/[controller]")]
public class DashboardService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<EmailReceiveEntity> _emailReceiveRepository;

    /// <summary>
    /// 流程任务.
    /// </summary>
    private readonly IFlowTaskRepository _flowTaskRepository;

    /// <summary>
    /// 系统消息服务.
    /// </summary>
    private readonly IMessageService _messageService;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="DashboardService"/>类型的新实例.
    /// </summary>
    public DashboardService(
        ISqlSugarRepository<EmailReceiveEntity> emailReceiveRepository,
        IFlowTaskRepository flowTaskRepository,
        IMessageService messageService,
        IUserManager userManager)
    {
        _emailReceiveRepository = emailReceiveRepository;
        _flowTaskRepository = flowTaskRepository;
        _messageService = messageService;
        _userManager = userManager;
    }

    #region Get

    /// <summary>
    /// 获取待办事项.
    /// </summary>
    [HttpGet("FlowTodo")]
    public async Task<dynamic> GetFlowTodo()
    {
        dynamic list = await _flowTaskRepository.GetPortalWaitList();
        return new { list = list };
    }

    /// <summary>
    /// 获取我的待办事项.
    /// </summary>
    [HttpGet("MyFlowTodo")]
    public async Task<dynamic> GetMyFlowTodo()
    {
        List<FlowTodoOutput> list = new List<FlowTodoOutput>();
        (await _flowTaskRepository.GetWaitList()).ForEach(l =>
        {
            list.Add(new FlowTodoOutput()
            {
                id = l.id,
                fullName = l.flowName,
                creatorTime = l.creatorTime
            });
        });
        return new { list = list };
    }

    /// <summary>
    /// 获取未读邮件.
    /// </summary>
    [HttpGet("Email")]
    public async Task<dynamic> GetEmail()
    {
        List<EmailHomeOutput>? res = (await _emailReceiveRepository.AsQueryable().Where(x => x.Read == 0 && x.CreatorUserId == _userManager.UserId && x.DeleteMark == null)
            .OrderBy(x => x.CreatorTime, OrderByType.Desc).ToListAsync()).Adapt<List<EmailHomeOutput>>();
        return new { list = res };
    }

    #endregion

    #region Post

    /// <summary>
    /// 获取我的待办.
    /// </summary>
    [HttpPost("FlowTodoCount")]
    public async Task<dynamic> GetFlowTodoCount([FromBody] FlowTodoCountInput input)
    {
        int flowCount = await _emailReceiveRepository.AsSugarClient().Queryable<FlowDelegateEntity>().Where(x => x.CreatorUserId == _userManager.UserId && x.DeleteMark == null).CountAsync();
        var waitList = await _flowTaskRepository.GetWaitList();
        if (input.toBeReviewedType.Any())
        {
            waitList = waitList.FindAll(x => input.toBeReviewedType.Contains(x.flowCategory));
        }
        List<FlowTaskEntity>? trialList = await _flowTaskRepository.GetTrialList();
        if (input.flowDoneType.Any())
        {
            trialList = trialList.FindAll(x => input.flowDoneType.Contains(x.FlowCategory));
        }
        int flowCirculateCount = await _emailReceiveRepository.AsSugarClient().Queryable<FlowTaskEntity, FlowTaskCirculateEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.Id == b.TaskId)).Where((a, b) => b.ObjectId == _userManager.UserId).WhereIF(input.flowCirculateType.Any(), a => input.flowCirculateType.Contains(a.FlowCategory)).CountAsync();
        return new FlowTodoCountOutput()
        {
            toBeReviewed = waitList.Count(),
            entrust = flowCount,
            flowDone = trialList.Count(),
            flowCirculate = flowCirculateCount
        };
    }

    /// <summary>
    /// 获取通知公告.
    /// </summary>
    [HttpPost("Notice")]
    public async Task<dynamic> GetNotice([FromBody] FlowTodoCountInput input)
    {
        List<NoticeOutput> list = await _emailReceiveRepository.AsSugarClient().Queryable<MessageEntity, MessageReceiveEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.Id == b.MessageId))
            .Where((a, b) => a.Type == 1 && a.DeleteMark == null && b.UserId == _userManager.UserId && a.EnabledMark != 2)
            .WhereIF(input.typeList.Any(), a => input.typeList.Contains(a.Category))
            .OrderBy(a => a.LastModifyTime, OrderByType.Desc)
            .Select((a) => new NoticeOutput()
            {
                id = a.Id,
                category = SqlFunc.IF(a.Category.Equals("1")).Return("公告").End("通知"),
                fullName = a.Title,
                creatorTime = a.CreatorTime,
                coverImage = a.CoverImage,
                creatorUser = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == a.CreatorUserId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
                releaseTime = a.LastModifyTime,
                releaseUser = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == a.LastModifyUserId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
                excerpt = a.Excerpt
            }).ToListAsync();

        return new { list = list };
    }
    #endregion
}