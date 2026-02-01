using Mapster;
using Microsoft.AspNetCore.Mvc;
using Poxiao.DatabaseAccessor;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Core.Handlers;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Dtos.Message;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Filter;
using Poxiao.Infrastructure.Options;
using Poxiao.Infrastructure.Security;
using Poxiao.LinqBuilder;
using Poxiao.Message.Entitys;
using Poxiao.Message.Entitys.Dto.Message;
using Poxiao.Message.Entitys.Entity;
using Poxiao.Message.Interfaces;
using Poxiao.Message.Interfaces.Message;
using Poxiao.RemoteRequest.Extensions;
using Poxiao.Systems.Entitys.Permission;
using Poxiao.Systems.Interfaces.Permission;
using SqlSugar;

namespace Poxiao.Message;

/// <summary>
/// 系统消息
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "Message", Name = "message", Order = 240)]
[Route("api/[controller]")]
public class MessageService : IMessageService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<MessageEntity> _repository;
    private readonly MessageOptions _messageOptions = App.GetConfig<MessageOptions>("Message", true);
    private readonly IMHandler _imHandler;
    private readonly IMessageManager _messageManager;

    /// <summary>
    /// 用户服务.
    /// </summary>
    private readonly IUsersService _usersService;

    /// <summary>
    /// 用户服务.
    /// </summary>
    private readonly IUserRelationService _userRelationService;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="MessageService"/>类型的新实例.
    /// </summary>
    public MessageService(
        ISqlSugarRepository<MessageEntity> repository,
        IUsersService usersService,
        IUserRelationService userRelationService,
        IMessageManager messageManager,
        IUserManager userManager,
        IMHandler imHandler)
    {
        _repository = repository;
        _usersService = usersService;
        _userRelationService = userRelationService;
        _messageManager = messageManager;
        _userManager = userManager;
        _imHandler = imHandler;
    }

    #region Get

    /// <summary>
    /// 列表（通知公告/系统消息/私信消息）.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetMessageList([FromQuery] MessageListQueryInput input)
    {
        var list = await _repository.AsSugarClient().Queryable<MessageEntity, MessageReceiveEntity, UserEntity>((a, b, c) => new JoinQueryInfos(JoinType.Left, a.Id == b.MessageId, JoinType.Left, a.LastModifyUserId == c.Id))
            .Where((a, b) => b.UserId == _userManager.UserId && a.DeleteMark == null)
            .WhereIF(input.type.IsNotEmptyOrNull(), a => a.Type == input.type)
            .WhereIF(input.isRead.IsNotEmptyOrNull(), (a, b) => b.IsRead == SqlFunc.ToInt32(input.isRead))
            .WhereIF(input.Keyword.IsNotEmptyOrNull(), a => a.DefaultTitle.Contains(input.Keyword) || a.Title.Contains(input.Keyword))
            .OrderBy(a => a.LastModifyTime, OrderByType.Desc)
            .Select((a, b, c) => new MessageListOutput
            {
                id = a.Id,
                releaseTime = a.LastModifyTime,
                releaseUser = SqlFunc.MergeString(c.RealName, "/", c.Account),
                title = SqlFunc.IsNullOrEmpty(a.DefaultTitle) ? a.Title : a.DefaultTitle,
                type = a.Type,
                isRead = b.IsRead,
                flowType = a.FlowType
            }).ToPagedListAsync(input.CurrentPage, input.PageSize);
        return PageResult<MessageListOutput>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfoApi(string id)
    {
        var data = await _repository.AsSugarClient().Queryable<MessageEntity>()
            .Where(a => a.Id == id && a.DeleteMark == null)
            .Select((a) => new MessageInfoOutput
            {
                id = a.Id,
                releaseTime = a.LastModifyTime,
                releaseUser = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == a.LastModifyUserId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
                creatorUser = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == a.CreatorUserId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
                title = a.Title,
                bodyText = a.BodyText,
                files = a.Files,
                toUserIds = a.ToUserIds,
                category = a.Category,
                coverImage = a.CoverImage,
                remindCategory = a.RemindCategory,
                sendConfigId = a.SendConfigId,
                excerpt = a.Excerpt,
                expirationTime = a.ExpirationTime,
                sendConfigName = SqlFunc.Subqueryable<MessageSendEntity>().Where(u => u.Id == a.SendConfigId).Select(u => u.FullName),
            }).FirstAsync();
        data.releaseUser = data.releaseUser.IsNotEmptyOrNull() ? data.releaseUser : data.creatorUser;
        return data;
    }

    /// <summary>
    /// 读取消息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("ReadInfo/{id}")]
    public async Task<dynamic> ReadInfo(string id)
    {
        var data = await _repository.AsSugarClient().Queryable<MessageEntity, UserEntity, MessageReceiveEntity>((a, b, c) => new JoinQueryInfos(JoinType.Left, a.LastModifyUserId == b.Id, JoinType.Left, a.Id == c.MessageId))
            .Where((a, b, c) => a.Id == id && a.DeleteMark == null && c.UserId == _userManager.UserId)
            .OrderBy(a => a.LastModifyTime)
            .Select((a, b, c) => new MessageReadInfoOutput
            {
                id = a.Id,
                releaseTime = a.LastModifyTime,
                releaseUser = SqlFunc.MergeString(b.RealName, "/", b.Account),
                title = a.Title,
                bodyText = c.BodyText,
                files = a.Files,
                excerpt = a.Excerpt,
            }).FirstAsync();
        if (data != null)
            await MessageRead(id, null, null, null);
        return data;
    }

    /// <summary>
    /// 读取消息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("getUnReadMsgNum")]
    public async Task<dynamic> GetUnReadMsgNum()
    {
        var unreadNoticeCount = await _repository.AsSugarClient().Queryable<MessageEntity, MessageReceiveEntity>((m, mr) => new JoinQueryInfos(JoinType.Left, m.Id == mr.MessageId)).Where((m, mr) => m.Type == 1 && m.DeleteMark == null && mr.UserId == _userManager.UserId && mr.IsRead == 0).Select((m, mr) => new { mr.Id, mr.UserId, mr.IsRead, m.Type, m.DeleteMark }).CountAsync();
        var unreadMessageCount = await _repository.AsSugarClient().Queryable<MessageEntity, MessageReceiveEntity>((m, mr) => new JoinQueryInfos(JoinType.Left, m.Id == mr.MessageId)).Where((m, mr) => m.Type == 2 && m.DeleteMark == null && mr.UserId == _userManager.UserId && mr.IsRead == 0).Select((m, mr) => new { mr.Id, mr.UserId, mr.IsRead, m.Type, m.DeleteMark }).CountAsync();
        var unreadSystemMessageCount = await _repository.AsSugarClient().Queryable<MessageEntity, MessageReceiveEntity>((m, mr) => new JoinQueryInfos(JoinType.Left, m.Id == mr.MessageId)).Where((m, mr) => m.Type == 3 && m.DeleteMark == null && mr.UserId == _userManager.UserId && mr.IsRead == 0).Select((m, mr) => new { mr.Id, mr.UserId, mr.IsRead, m.Type, m.DeleteMark }).CountAsync();
        var unreadScheduleCount = await _repository.AsSugarClient().Queryable<MessageEntity, MessageReceiveEntity>((m, mr) => new JoinQueryInfos(JoinType.Left, m.Id == mr.MessageId)).Where((m, mr) => m.Type == 4 && m.DeleteMark == null && mr.UserId == _userManager.UserId && mr.IsRead == 0).Select((m, mr) => new { mr.Id, mr.UserId, mr.IsRead, m.Type, m.DeleteMark }).CountAsync();
        var unreadTotalCount = await _repository.AsSugarClient().Queryable<MessageEntity, MessageReceiveEntity>((m, mr) => new JoinQueryInfos(JoinType.Left, m.Id == mr.MessageId)).Where((m, mr) => m.DeleteMark == null && mr.UserId == _userManager.UserId && mr.IsRead == 0).Select((m, mr) => new { mr.Id, mr.UserId, mr.IsRead, m.Type, m.DeleteMark }).CountAsync();
        return new { unReadMsg = unreadMessageCount, unReadNotice = unreadNoticeCount, unReadSystemMsg = unreadSystemMessageCount, unReadSchedule = unreadScheduleCount, unReadNum = unreadTotalCount };
    }

    #endregion

    #region Post

    /// <summary>
    /// 列表（通知公告）.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPost("Notice")]
    public async Task<dynamic> GetNoticeList([FromBody] MessageNoticeQueryInput input)
    {
        #region 修改过期状态
        if (await _repository.IsAnyAsync(a => a.ExpirationTime != null && SqlFunc.GetDate() > a.ExpirationTime && a.EnabledMark == 1))
        {
            await _repository.AsUpdateable().SetColumns(it => new MessageEntity()
            {
                EnabledMark = 2,
            }).Where(a => a.ExpirationTime != null && SqlFunc.GetDate() > a.ExpirationTime && a.EnabledMark == 1).ExecuteCommandAsync();
        }
        #endregion
        //var whereLambda = LinqExpression.And<MessageEntity>();
        //whereLambda = whereLambda.And(a => a.DeleteMark == null && a.Type == 1);
        //if (input.enabledMark.Any())
        //{
        //    if (input.enabledMark.Contains("2"))
        //    {
        //        whereLambda = whereLambda.And(a => input.enabledMark.Contains(SqlFunc.ToString(a.EnabledMark)) || (!SqlFunc.IsNullOrEmpty(a.ExpirationTime) && SqlFunc.GetDate() > a.ExpirationTime));
        //    }
        //    else
        //    {
        //        whereLambda = whereLambda.And(a => input.enabledMark.Contains(SqlFunc.ToString(a.EnabledMark)) && (SqlFunc.IsNullOrEmpty(a.ExpirationTime) || SqlFunc.GetDate() <= a.ExpirationTime));
        //    }
        //}
        var list = await _repository.AsSugarClient().Queryable<MessageEntity>()
            .Where(a => a.DeleteMark == null && a.Type == 1 && a.EnabledMark != 3 && !SqlFunc.IsNullOrEmpty(a.Category))
            .WhereIF(!string.IsNullOrEmpty(input.Keyword), a => a.Title.Contains(input.Keyword))
            .WhereIF(input.type.Any(), a => input.type.Contains(a.Category))
            .WhereIF(input.enabledMark.Any(), a => input.enabledMark.Contains(SqlFunc.ToString(a.EnabledMark)))
            .OrderBy(a => a.EnabledMark)
            .OrderBy(a => a.LastModifyTime, OrderByType.Desc)
            .OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .Select((a) => new MessageNoticeOutput
            {
                id = a.Id,
                releaseTime = a.LastModifyTime,
                enabledMark = a.EnabledMark,
                releaseUser = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == a.LastModifyUserId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
                title = a.Title,
                type = a.Type,
                creatorTime = a.CreatorTime,
                creatorUser = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == a.CreatorUserId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
                category = a.Category.Equals("1") ? "公告" : "通知",
                excerpt = a.Excerpt,
                expirationTime = a.ExpirationTime
            }).ToPagedListAsync(input.CurrentPage, input.PageSize);
        return PageResult<MessageNoticeOutput>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [UnitOfWork]
    public async Task Delete(string id)
    {
        try
        {
            await _repository.AsSugarClient().Deleteable<MessageReceiveEntity>().Where(x => x.MessageId == id).ExecuteCommandAsync();
            await _repository.AsUpdateable().SetColumns(it => new MessageEntity()
            {
                DeleteMark = 1,
                DeleteUserId = _userManager.UserId,
                DeleteTime = SqlFunc.GetDate()
            }).Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        }
        catch (Exception)
        {
            throw Oops.Oh(ErrorCode.COM1002);
        }
    }

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">实体对象.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] MessageCrInput input)
    {
        var entity = input.Adapt<MessageEntity>();
        entity.Type = 1;
        entity.EnabledMark = 0;
        entity.DefaultTitle = input.title;
        var isOk = await _repository.AsInsertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="id">主键值</param>
    /// <param name="input">实体对象</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] MessageUpInput input)
    {
        var entity = input.Adapt<MessageEntity>();
        entity.DefaultTitle = entity.Title;
        var isOk = await _repository.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 发布公告.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpPut("{id}/Actions/Release")]
    public async Task Release(string id)
    {
        var entity = await _repository.GetFirstAsync(x => x.Id == id && x.DeleteMark == null);
        if (entity != null)
        {
            var toUserIds = new List<string>();
            if (entity.ToUserIds.IsNullOrEmpty())
                toUserIds = (await _usersService.GetList()).Select(x => x.Id).ToList();
            else
                toUserIds = await _userRelationService.GetUserId(entity.ToUserIds.Split(",").ToList());
            // 发送
            if (entity.RemindCategory == 1)
            {
                var messageReceiveList = _messageManager.GetMessageReceiveList(toUserIds, entity, null);
                await _messageManager.SendDefaultMsg(toUserIds, entity, messageReceiveList);
            }
            if (entity.RemindCategory == 2)
            {
                var messageSendModelList = await _messageManager.GetMessageSendModels(entity.SendConfigId);
                foreach (var item in messageSendModelList)
                {
                    item.toUser = toUserIds;
                    item.paramJson.Clear();
                    item.paramJson.Add(new MessageSendParam
                    {
                        field = "@MsgId",
                        value = entity.Id
                    });
                    item.paramJson.Add(new MessageSendParam
                    {
                        field = "@Title",
                        value = entity.Title
                    });
                    item.paramJson.Add(new MessageSendParam
                    {
                        field = "@CreatorUserName",
                        value = _userManager.GetUserName(entity.CreatorUserId)
                    });
                    item.paramJson.Add(new MessageSendParam
                    {
                        field = "@Content",
                        value = entity.BodyText
                    });
                    item.paramJson.Add(new MessageSendParam
                    {
                        field = "@Remark",
                        value = entity.Excerpt
                    });
                    await _messageManager.SendDefinedMsg(item, null);
                }
            }

            entity.EnabledMark = 1;
            entity.LastModifyTime = DateTime.Now;
            entity.LastModifyUserId = _userManager.UserId;
            _repository.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommand();
        }
    }

    /// <summary>
    /// 全部已读.
    /// </summary>
    /// <returns></returns>
    [HttpPost("Actions/ReadAll")]
    public async Task AllRead([FromQuery] string isRead, [FromQuery] string keyword, [FromQuery] string type)
    {
        await MessageRead(string.Empty, isRead, keyword, type);
    }

    /// <summary>
    /// 删除记录.
    /// </summary>
    /// <param name="postParam">请求参数.</param>
    /// <returns></returns>
    [HttpDelete("Record")]
    public async Task DeleteRecordApi([FromBody] dynamic postParam)
    {
        string[] ids = postParam.ids.ToString().Split(',');
        var isOk = await _repository.AsSugarClient().Deleteable<MessageReceiveEntity>().Where(m => m.UserId == _userManager.UserId && ids.Contains(m.MessageId)).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);
    }
    #endregion

    #region PublicMethod

    /// <summary>
    /// 创建.
    /// </summary>
    /// <param name="entity">实体对象.</param>
    /// <param name="receiveEntityList">收件用户.</param>
    [NonAction]
    private int Create(MessageEntity entity, List<MessageReceiveEntity> receiveEntityList)
    {
        try
        {
            _repository.AsSugarClient().Insertable(receiveEntityList).ExecuteCommand();

            return _repository.AsInsertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Create()).ExecuteCommand();
        }
        catch (Exception)
        {
            return 0;
        }
    }

    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="entity">实体对象.</param>
    /// <param name="receiveEntityList">收件用户.</param>
    [NonAction]
    private int Update(MessageEntity entity, List<MessageReceiveEntity> receiveEntityList)
    {
        try
        {
            _repository.AsSugarClient().Insertable(receiveEntityList).ExecuteCommand();
            return _repository.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommand();
        }
        catch (Exception)
        {
            return 0;
        }
    }

    /// <summary>
    /// 消息已读（全部）.
    /// </summary>
    /// <param name="id">id.</param>
    [NonAction]
    private async Task MessageRead(string id, string isRead, string keyword, string type)
    {
        var ids = await _repository.AsSugarClient().Queryable<MessageEntity, MessageReceiveEntity, UserEntity>((a, b, c) => new JoinQueryInfos(JoinType.Left, a.Id == b.MessageId, JoinType.Left, a.CreatorUserId == c.Id))
            .Where((a, b) => b.UserId == _userManager.UserId && a.DeleteMark == null)
            .WhereIF(id.IsNotEmptyOrNull(), a => a.Id == id)
            .WhereIF(type.IsNotEmptyOrNull(), a => a.Type == SqlFunc.ToInt32(type))
            .WhereIF(!string.IsNullOrEmpty(isRead), a => a.IsRead == SqlFunc.ToInt32(isRead))
            .WhereIF(!string.IsNullOrEmpty(keyword), a => a.Title.Contains(keyword))
            .Select((a, b, c) => b.Id).ToListAsync();
        if (!_repository.AsSugarClient().Queryable<MessageReceiveEntity>().Any(x => x.IsRead == 0 && x.UserId == _userManager.UserId) && id.IsNullOrEmpty())
        {
            throw Oops.Oh(ErrorCode.D7017);
        }
        await _repository.AsSugarClient().Updateable<MessageReceiveEntity>().SetColumns(it => it.ReadCount == it.ReadCount + 1).SetColumns(x => new MessageReceiveEntity()
        {
            IsRead = 1,
            ReadTime = DateTime.Now
        }).Where(x => ids.Contains(x.Id)).ExecuteCommandAsync();
    }

    /// <summary>
    /// 发送公告.
    /// </summary>
    /// <param name="entity">消息信息.</param>
    [NonAction]
    private async Task SentNotice(MessageEntity entity)
    {
        try
        {
            var toUserIds = new List<string>();
            entity.EnabledMark = 1;
            if (entity.ToUserIds.IsNullOrEmpty())
                toUserIds = (await _usersService.GetList()).Select(x => x.Id).ToList();
            else
                toUserIds = await _userRelationService.GetUserId(entity.ToUserIds.Split(",").ToList());
            List<MessageReceiveEntity> receiveEntityList = toUserIds
                .Select(x => new MessageReceiveEntity()
                {
                    Id = SnowflakeIdHelper.NextId(),
                    MessageId = entity.Id,
                    UserId = x,
                    IsRead = 0,
                    BodyText = entity.BodyText,
                }).ToList();

            Update(entity, receiveEntityList);
            if (entity.ToUserIds.IsNullOrEmpty())
            {
                await _imHandler.SendMessageToTenantAsync(_userManager.TenantId, new { method = "messagePush", messageType = 1, userId = _userManager.UserId, toUserId = toUserIds, title = entity.Title, unreadNoticeCount = 1, id = entity.Id }.ToJsonString());
            }
            else
            {
                foreach (var item in toUserIds)
                {
                    var userId = item.Replace("-delegate", string.Empty);
                    // 消息推送 - 指定用户
                    await _imHandler.SendMessageToUserAsync(string.Format("{0}-{1}", _userManager.TenantId, userId), new { method = "messagePush", messageType = 2, userId = _userManager.UserId, toUserId = toUserIds, title = entity.Title, unreadNoticeCount = 1, id = entity.Id }.ToJsonString());

                }
            }
        }
        catch (Exception ex)
        {
            throw Oops.Oh(ErrorCode.D7003);
        }
    }

    /// <summary>
    /// 发送站内消息.
    /// </summary>
    /// <param name="toUserIds">发送用户.</param>
    /// <param name="title">标题.</param>
    /// <param name="bodyText">内容.</param>
    [NonAction]
    public async Task SentMessage(List<string> toUserIds, string title, string bodyText = null, Dictionary<string, object> bodyDic = null, int type = 2, string flowType = "1")
    {
        try
        {
            MessageEntity entity = new MessageEntity();
            entity.Id = SnowflakeIdHelper.NextId();
            entity.Title = title;
            entity.BodyText = bodyText;
            entity.Type = type;
            entity.LastModifyTime = DateTime.Now;
            entity.LastModifyUserId = _userManager.UserId;
            entity.FlowType = flowType.ParseToInt();
            List<MessageReceiveEntity> receiveEntityList = toUserIds
                .Select(x => new MessageReceiveEntity()
                {
                    Id = SnowflakeIdHelper.NextId(),
                    MessageId = entity.Id,
                    UserId = x.Replace("-delegate", string.Empty),
                    IsRead = 0,
                    BodyText = bodyDic.IsNotEmptyOrNull() && bodyDic.ContainsKey(x) ? bodyDic[x].ToJsonString() : null,
                }).ToList();

            if (Create(entity, receiveEntityList) >= 1)
            {
                foreach (var item in toUserIds)
                {
                    var userId = item.Replace("-delegate", string.Empty);
                    // 消息推送 - 指定用户
                    await _imHandler.SendMessageToUserAsync(string.Format("{0}-{1}", _userManager.TenantId, userId), new { method = "messagePush", messageType = 2, userId = _userManager.UserId, toUserId = toUserIds, title = entity.Title, unreadNoticeCount = 1, id = entity.Id }.ToJsonString());
                }
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    /// <summary>
    /// 发送个推.
    /// </summary>
    /// <param name="toUserIds">推送用户.</param>
    /// <param name="title">标题.</param>
    /// <param name="content">内容.</param>
    /// <param name="msgId">消息id.</param>
    /// <param name="type">1:公告消息、2:流程消息、3:聊天消息.</param>
    /// <returns></returns>
    private async Task GeTuiMessage(List<string> toUserIds, string title, string content, string msgId, string type)
    {
        var getuiUrl = "{0}?clientId={1}&title={2}&content={3}1&text={4}&create=true";
        if (toUserIds.Any())
        {
            var clientIdList = await _repository.AsSugarClient().Queryable<UserDeviceEntity>().Where(x => toUserIds.Contains(x.UserId) && x.DeleteMark == null).Select(x => x.ClientId).ToListAsync();
            if (clientIdList.Any())
            {
                var clientId = string.Join(",", clientIdList);
                var textDic = new Dictionary<string, string>();
                textDic.Add("type", type);
                if (type == "3")
                {
                    var userName = await _usersService.GetUserName(_userManager.UserId);
                    textDic.Add("name", userName);
                    textDic.Add("formUserId", _userManager.UserId);
                    textDic.Add("headIcon", "/api/File/Image/userAvatar/" + _userManager.User.HeadIcon);
                }
                else
                {
                    textDic.Add("id", msgId);
                    textDic.Add("title", title);
                }
                getuiUrl = string.Format(getuiUrl, _messageOptions.AppPushUrl, clientId, title, content, textDic.ToJsonString());
                await getuiUrl.GetAsStringAsync();
            }
        }
    }
    #endregion
}