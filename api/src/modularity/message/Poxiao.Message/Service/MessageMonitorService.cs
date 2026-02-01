using Microsoft.AspNetCore.Mvc;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Filter;
using Poxiao.Infrastructure.Security;
using Poxiao.LinqBuilder;
using Poxiao.Message.Entitys.Dto.MessageMonitor;
using Poxiao.Message.Entitys.Entity;
using Poxiao.Systems.Entitys.Permission;
using SqlSugar;

namespace Poxiao.Message.Service;

/// <summary>
/// 消息监控
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "Message", Name = "MessageMonitor", Order = 240)]
[Route("api/message/[controller]")]
public class MessageMonitorService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<MessageMonitorEntity> _repository;

    public MessageMonitorService(ISqlSugarRepository<MessageMonitorEntity> repository)
    {
        _repository = repository;
    }

    #region Get

    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] MessageMonitorQuery input)
    {
        var whereLambda = LinqExpression.And<MessageMonitorEntity>();
        whereLambda = whereLambda.And(a => a.DeleteMark == null);
        var start = new DateTime();
        var end = new DateTime();
        if (input.endTime != null && input.startTime != null)
        {
            start = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 00:00:00}", input.startTime?.TimeStampToDateTime()));
            end = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 23:59:59}", input.endTime?.TimeStampToDateTime()));
            whereLambda = whereLambda.And(a => SqlFunc.Between(a.SendTime, start, end));
        }
        // 关键字（用户、IP地址、功能名称）
        if (!string.IsNullOrEmpty(input.Keyword))
            whereLambda = whereLambda.And(a => a.Title.Contains(input.Keyword));
        if (input.messageSource.IsNotEmptyOrNull())
            whereLambda = whereLambda.And(a => a.MessageSource.Contains(input.messageSource));
        if (input.messageType.IsNotEmptyOrNull())
            whereLambda = whereLambda.And(m => m.MessageType.Contains(input.messageType));
        var list = await _repository.AsSugarClient().Queryable<MessageMonitorEntity>()
            .Where(whereLambda)
            .OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .OrderBy(a => a.LastModifyTime, OrderByType.Desc)
            .Select(a => new MessageMonitorListOutput
            {
                id = a.Id,
                messageType = SqlFunc.Subqueryable<MessageDataTypeEntity>().Where(u => u.Type == "1" && u.EnCode == a.MessageType).Select(u => u.FullName),
                messageSource = SqlFunc.Subqueryable<MessageDataTypeEntity>().Where(u => u.Type == "4" && u.EnCode == a.MessageSource).Select(u => u.FullName),
                title = a.Title,
                sendTime = a.SendTime,
            }).ToPagedListAsync(input.CurrentPage, input.PageSize);
        return PageResult<MessageMonitorListOutput>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 详情.
    /// </summary>
    /// <returns></returns>
    [HttpGet("detail/{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = await _repository.AsSugarClient().Queryable<MessageMonitorEntity>().Where(a => a.Id == id && a.DeleteMark == null).Select(a => new MessageMonitorListOutput
        {
            id = a.Id,
            messageType = SqlFunc.Subqueryable<MessageDataTypeEntity>().Where(u => u.Type == "1" && u.EnCode == a.MessageType).Select(u => u.FullName),
            messageSource = SqlFunc.Subqueryable<MessageDataTypeEntity>().Where(u => u.Type == "4" && u.EnCode == a.MessageSource).Select(u => u.FullName),
            title = a.Title,
            sendTime = a.SendTime,
            receiveUser = a.ReceiveUser,
            content = a.Content
        }).FirstAsync();
        var userIds = output.receiveUser.ToList<string>();
        var userList = await _repository.AsSugarClient().Queryable<UserEntity>().Where(x => userIds.Contains(x.Id)).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)).ToListAsync();
        output.receiveUser = string.Join(",", userList);
        return output;
    }
    #endregion

    #region POST

    /// <summary>
    /// 批量删除.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpDelete("batchRemove")]
    public async Task Delete([FromBody] MessageMonitorDelInput input)
    {
        await _repository.AsDeleteable().In(it => it.Id, input.ids).ExecuteCommandAsync();
    }

    /// <summary>
    /// 一键删除.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("empty")]
    public async Task Delete()
    {
        await _repository.DeleteAsync(x => x.DeleteMark == null);
    }
    #endregion
}
