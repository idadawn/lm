using Mapster;
using Poxiao.DataEncryption;
using Poxiao.Extras.WebSockets.Models;
using Poxiao.Infrastructure.Configuration;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Manager;
using Poxiao.Infrastructure.Models.User;
using Poxiao.Infrastructure.Options;
using Poxiao.Infrastructure.Security;
using Poxiao.Message.Entitys;
using Poxiao.Message.Entitys.Dto.IM;
using Poxiao.Message.Entitys.Entity;
using Poxiao.Message.Entitys.Enums;
using Poxiao.Message.Entitys.Model.IM;
using Poxiao.RemoteRequest.Extensions;
using Poxiao.Systems.Entitys.Permission;
using Poxiao.WebSockets;
using SqlSugar;
using System.Net.WebSockets;

namespace Poxiao.Infrastructure.Core.Handlers;

/// <summary>
/// IM 处理程序.
/// </summary>
public class IMHandler : WebSocketHandler
{
    /// <summary>
    /// SqlSugarClient客户端.
    /// </summary>
    private static SqlSugarScope? _sqlSugarClient;

    /// <summary>
    /// 缓存管理.
    /// </summary>
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 消息配置.
    /// </summary>
    private readonly MessageOptions _messageOptions = App.GetConfig<MessageOptions>("Message", true);

    /// <summary>
    /// 初始化一个<see cref="IMHandler"/>类型的新实例.
    /// </summary>
    public IMHandler(
        WebSocketConnectionManager webSocketConnectionManager,
        ISqlSugarClient sqlSugarClient,
        ICacheManager cacheManager)
        : base(webSocketConnectionManager)
    {
        _sqlSugarClient = (SqlSugarScope)sqlSugarClient;
        _cacheManager = cacheManager;
    }

    /// <summary>
    /// 消息接收.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="result"></param>
    /// <param name="receivedMessage"></param>
    /// <returns></returns>
    public override async Task ReceiveAsync(WebSocketClient client, WebSocketReceiveResult result, string receivedMessage)
    {
        try
        {
            MessageInput? message = receivedMessage.ToObject<MessageInput>();
            var claims = JWTEncryption.ReadJwtToken(message.token.Replace("Bearer ", string.Empty).Replace("bearer ", string.Empty))?.Claims;
            client.UserId = claims.FirstOrDefault(e => e.Type == ClaimConst.CLAINMUSERID)?.Value;
            client.Account = claims.FirstOrDefault(e => e.Type == ClaimConst.CLAINMACCOUNT)?.Value;
            client.UserName = claims.FirstOrDefault(e => e.Type == ClaimConst.CLAINMREALNAME)?.Value;
            client.TenantId = claims.FirstOrDefault(e => e.Type == ClaimConst.TENANTID)?.Value;
            client.LoginTime = string.Format("{0:yyyy-MM-dd HH:mm}", string.Format("{0}000", claims.FirstOrDefault(e => e.Type == "iat")?.Value).TimeStampToDateTime());
            client.LoginIpAddress = client.LoginIpAddress;
            client.IsMobileDevice = message.mobileDevice;
            client.onlineTicket = claims.FirstOrDefault(e => e.Type == ClaimConst.OnlineTicket)?.Value;
            if (client.WebSocket.State != WebSocketState.Open) return;
            await OnConnected(client.ConnectionId, client);
            WebSocketConnectionManager.AddToTenant(client.ConnectionId, client.TenantId);
            WebSocketConnectionManager.AddToUser(client.ConnectionId, string.Format("{0}-{1}", client.TenantId, client.UserId));
            message.sendClientId = client.ConnectionId;
            await MessageRoute(message);
        }
        catch (Exception e)
        {
        }
    }

    /// <summary>
    /// 消息通道.
    /// </summary>
    /// <param name="message"></param>
    private async Task MessageRoute(MessageInput message)
    {
        WebSocketClient client = WebSocketConnectionManager.GetSocketById(message.sendClientId);
        if (string.IsNullOrEmpty(client.UserId))
        {
            await SendMessageAsync(client.ConnectionId, new { method = "logout" }.ToJsonString());
            return;
        }

        var tenant = await GetGlobalTenantCache(client.TenantId);
        client.SingleLogin = (LoginMethod)tenant.SingleLogin;

        // 判断ORM内是否存在该连接
        if (!_sqlSugarClient.IsAnyConnection(client.TenantId))
        {
            _sqlSugarClient.AddConnection(PoxiaoTenantExtensions.GetConfig(tenant.connectionConfig));
            _sqlSugarClient.Ado.CommandTimeOut = 10;
        }

        // 当前数据连接ConfigId是否等于目标库ConfigId
        if (_sqlSugarClient.CurrentConnectionConfig.ConfigId?.ToString() != client.TenantId)
        {
            _sqlSugarClient.ChangeDatabase(client.TenantId);
        }

        // 验证连接是否成功
        if (!_sqlSugarClient.Ado.IsValidConnection())
        {
            await OnDisconnected(client.WebSocket);
            return;
        }

        if (string.IsNullOrEmpty(client.HeadIcon))
        {
            UserEntity userEntity = await _sqlSugarClient.Queryable<UserEntity>().SingleAsync(it => it.Id == client.UserId);
            if (userEntity != null)
            {
                client.HeadIcon = "/api/file/Image/userAvatar/" + userEntity.HeadIcon;
                await OnConnected(client.ConnectionId, client);
            }
        }

        switch (message.method)
        {
            // 建立连接
            case MothodType.OnConnection:
                {
                    List<UserOnlineModel> list = await GetOnlineUserList(client.TenantId);
                    if (list == null)
                    {
                        list = new List<UserOnlineModel>();
                    }

                    switch (client.SingleLogin)
                    {
                        case LoginMethod.Single:
                            {
                                UserOnlineModel? user = list.Find(it => it.userId.Equals(client.UserId) && it.isMobileDevice.Equals(client.IsMobileDevice));
                                if (user == null)
                                {
                                    list.Add(new UserOnlineModel()
                                    {
                                        connectionId = client.ConnectionId,
                                        userId = client.UserId,
                                        account = client.Account,
                                        userName = client.UserName,
                                        lastTime = DateTime.Now,
                                        lastLoginIp = client.LoginIpAddress,
                                        tenantId = client.TenantId,
                                        lastLoginPlatForm = client.LoginPlatForm,
                                        isMobileDevice = client.IsMobileDevice,
                                        token = message.token,
                                        onlineTicket = client.onlineTicket
                                    });
                                    await SetOnlineUserList(client.TenantId, list);
                                }

                                // 不同浏览器
                                else if (user != null && !user.token.Equals(message.token))
                                {
                                    var onlineUser = WebSocketConnectionManager.GetSocketById(user.connectionId);
                                    if (onlineUser != null)
                                        await SendMessageAsync(onlineUser.ConnectionId, new { method = MessageSendType.logout.ToString(), msg = "此账号已在其他地方登录" }.ToJsonString());
                                    list.RemoveAll((x) => x.connectionId == user.connectionId);

                                    list.Add(new UserOnlineModel()
                                    {
                                        connectionId = client.ConnectionId,
                                        userId = client.UserId,
                                        account = client.Account,
                                        userName = client.UserName,
                                        lastTime = DateTime.Now,
                                        lastLoginIp = client.LoginIpAddress,
                                        tenantId = client.TenantId,
                                        lastLoginPlatForm = client.LoginPlatForm,
                                        isMobileDevice = client.IsMobileDevice,
                                        token = message.token,
                                        onlineTicket = client.onlineTicket
                                    });
                                    await SetOnlineUserList(client.TenantId, list);
                                }
                            }

                            break;
                        case LoginMethod.SameTime:
                            {
                                UserOnlineModel? user = list.Find(it => it.token.Equals(message.token));
                                if (user != null)
                                {
                                    WebSocketClient? onlineUser = WebSocketConnectionManager.GetSocketById(user.connectionId);
                                    if (onlineUser != null)
                                        await SendMessageAsync(onlineUser.ConnectionId, new { method = MessageSendType.closeSocket.ToString() }.ToJsonString());
                                    list.RemoveAll((x) => x.connectionId == user.connectionId);
                                }

                                list.Add(new UserOnlineModel()
                                {
                                    connectionId = client.ConnectionId,
                                    userId = client.UserId,
                                    account = client.Account,
                                    userName = client.UserName,
                                    lastTime = DateTime.Now,
                                    lastLoginIp = client.LoginIpAddress,
                                    tenantId = client.TenantId,
                                    lastLoginPlatForm = client.LoginPlatForm,
                                    isMobileDevice = client.IsMobileDevice,
                                    token = message.token,
                                    onlineTicket = client.onlineTicket
                                });
                                await SetOnlineUserList(client.TenantId, list);
                            }

                            break;
                    }

                    var onlineUserList = GetAllUserIdFromTenant(client.TenantId);

                    // 获取接收者为当前用户的聊天且未读的信息
                    var imContentList = _sqlSugarClient.Queryable<IMContentEntity>().Where(x => x.ReceiveUserId.Equals(client.UserId) && x.State.Equals(0) && (SqlFunc.IsNullOrEmpty(x.SendDeleteMark) || x.SendDeleteMark != client.UserId)).GroupBy(x => new { x.SendUserId, x.ReceiveUserId }).Select(x => new IMContentEntity
                    {
                        State = SqlFunc.AggregateSum(SqlFunc.IIF(x.State == 0, 1, 0)),
                        SendUserId = x.SendUserId,
                        ReceiveUserId = x.ReceiveUserId
                    }).ToList();

                    var receiveList = _sqlSugarClient.Queryable<IMContentEntity>().Where(x => x.ReceiveUserId == client.UserId && (SqlFunc.IsNullOrEmpty(x.SendDeleteMark) || x.SendDeleteMark != client.UserId)).OrderBy(x => x.SendTime, OrderByType.Desc).ToList();
                    var unreadNums = imContentList.Adapt<List<IMUnreadNumModel>>();
                    foreach (var item in unreadNums)
                    {
                        var entity = receiveList.FirstOrDefault(x => x.SendUserId == item.sendUserId);
                        item.defaultMessage = entity?.Content;
                        item.defaultMessageType = entity?.ContentType;
                        item.defaultMessageTime = entity?.SendTime.ToString();
                    }
                    var unreadNoticeCount = await _sqlSugarClient.Queryable<MessageEntity, MessageReceiveEntity>((m, mr) => new JoinQueryInfos(JoinType.Left, m.Id == mr.MessageId)).Where((m, mr) => m.Type == 1 && m.DeleteMark == null && mr.UserId == client.UserId && mr.IsRead == 0).Select((m, mr) => new { mr.Id, mr.UserId, mr.IsRead, m.Type, m.DeleteMark }).CountAsync();
                    var unreadMessageCount = await _sqlSugarClient.Queryable<MessageEntity, MessageReceiveEntity>((m, mr) => new JoinQueryInfos(JoinType.Left, m.Id == mr.MessageId)).Where((m, mr) => m.Type == 2 && m.DeleteMark == null && mr.UserId == client.UserId && mr.IsRead == 0).Select((m, mr) => new { mr.Id, mr.UserId, mr.IsRead, m.Type, m.DeleteMark }).CountAsync();
                    var unreadSystemMessageCount = await _sqlSugarClient.Queryable<MessageEntity, MessageReceiveEntity>((m, mr) => new JoinQueryInfos(JoinType.Left, m.Id == mr.MessageId)).Where((m, mr) => m.Type == 3 && m.DeleteMark == null && mr.UserId == client.UserId && mr.IsRead == 0).Select((m, mr) => new { mr.Id, mr.UserId, mr.IsRead, m.Type, m.DeleteMark }).CountAsync();
                    var unreadScheduleCount = await _sqlSugarClient.Queryable<MessageEntity, MessageReceiveEntity>((m, mr) => new JoinQueryInfos(JoinType.Left, m.Id == mr.MessageId)).Where((m, mr) => m.Type == 4 && m.DeleteMark == null && mr.UserId == client.UserId && mr.IsRead == 0).Select((m, mr) => new { mr.Id, mr.UserId, mr.IsRead, m.Type, m.DeleteMark }).CountAsync();
                    var unreadTotalCount = await _sqlSugarClient.Queryable<MessageEntity, MessageReceiveEntity>((m, mr) => new JoinQueryInfos(JoinType.Left, m.Id == mr.MessageId)).Where((m, mr) => m.DeleteMark == null && mr.UserId == client.UserId && mr.IsRead == 0).Select((m, mr) => new { mr.Id, mr.UserId, mr.IsRead, m.Type, m.DeleteMark }).CountAsync();
                    var messageDefault = await _sqlSugarClient.Queryable<MessageEntity, MessageReceiveEntity>((m, mr) => new JoinQueryInfos(JoinType.Left, m.Id == mr.MessageId)).Where((m, mr) => m.DeleteMark == null && mr.UserId == client.UserId).OrderBy((m, mr) => m.LastModifyTime, OrderByType.Desc).FirstAsync();
                    var messageDefaultText = messageDefault == null ? string.Empty : messageDefault.Title;
                    var messageDefaultTime = messageDefault == null ? DateTime.Now : messageDefault.CreatorTime;
                    await SendMessageAsync(client.ConnectionId, new { method = MessageSendType.initMessage.ToString(), onlineUserList, unreadNums, unreadNoticeCount, unreadMessageCount, messageDefaultText, messageDefaultTime, unreadSystemMessageCount, unreadScheduleCount, unreadTotalCount }.ToJsonString());

                    await SendMessageToTenantAsync(client.TenantId, new { method = MessageSendType.online.ToString(), userId = client.UserId }.ToJsonString(), client.ConnectionId);
                }

                break;

            // 发送消息
            case MothodType.SendMessage:
                {
                    string toUserId = message.toUserId;
                    MessageReceiveType messageType = message.messageType;
                    object messageContent = message.messageContent;
                    string fileName = string.Empty;

                    var toUserEntity = await _sqlSugarClient.Queryable<UserEntity>().SingleAsync(it => it.Id == toUserId);

                    // 将发送消息对象信息补全
                    var toAccount = toUserEntity.Account;
                    var toHeadIcon = toUserEntity.HeadIcon;
                    var toRealName = toUserEntity.RealName;

                    var entity = new IMContentEntity();
                    var toMessage = new object();
                    switch (messageType)
                    {
                        case MessageReceiveType.text:
                            entity = CreateIMContent(client.UserId, toUserId, messageContent.ToString(), messageType.ToString());
                            break;
                        case MessageReceiveType.image:
                            {
                                var directoryPath = FileVariable.IMContentFilePath;
                                if (!Directory.Exists(directoryPath))
                                    Directory.CreateDirectory(directoryPath);
                                var imageInput = messageContent.ToObject<MessagetImageInput>();
                                fileName = fileName = imageInput.name;

                                toMessage = new { path = "/api/file/Image/IM/" + fileName, width = imageInput.width, height = imageInput.height };

                                entity = CreateIMContent(client.UserId, toUserId, toMessage.ToJsonString(), messageType.ToString());
                            }

                            break;
                        case MessageReceiveType.voice:
                            var voiceInput = messageContent.ToObject<MessageVoiceInput>();
                            toMessage = new { path = "/api/file/Image/IM/" + voiceInput.name, length = voiceInput.length };
                            entity = CreateIMContent(client.UserId, toUserId, toMessage.ToJsonString(), messageType.ToString());
                            break;
                    }

                    // 写入到会话表中
                    if (await _sqlSugarClient.Queryable<ImReplyEntity>().AnyAsync(it => it.UserId == client.UserId && it.ReceiveUserId == toUserId))
                    {
                        var imReplyEntity = await _sqlSugarClient.Queryable<ImReplyEntity>().SingleAsync(it => it.UserId == client.UserId && it.ReceiveUserId == toUserId);
                        imReplyEntity.ReceiveTime = entity.SendTime;
                        await _sqlSugarClient.Updateable(imReplyEntity).ExecuteCommandAsync();
                    }
                    else
                    {
                        var imReplyEntity = new ImReplyEntity()
                        {
                            Id = SnowflakeIdHelper.NextId(),
                            UserId = client.UserId,
                            ReceiveUserId = toUserId,
                            ReceiveTime = entity.SendTime
                        };
                        await _sqlSugarClient.Insertable(imReplyEntity).ExecuteCommandAsync();
                    }

                    await _sqlSugarClient.Insertable(entity).ExecuteCommandAsync();

                    switch (messageType)
                    {
                        case MessageReceiveType.text:
                            await SendMessageAsync(client.ConnectionId, new { method = MessageSendType.sendMessage.ToString(), client.UserId, account = client.Account, headIcon = client.HeadIcon, realName = client.UserName, toAccount, toHeadIcon, messageType = messageType.ToString(), toUserId, toRealName, toMessage = messageContent, dateTime = DateTime.Now, latestDate = DateTime.Now }.ToJsonString());
                            break;
                        case MessageReceiveType.image:
                            var imageInput = messageContent.ToObject<MessagetImageInput>();
                            toMessage = new { path = "/api/file/Image/IM/" + fileName, width = imageInput.width, height = imageInput.height };
                            await SendMessageAsync(client.ConnectionId, new { method = MessageSendType.sendMessage.ToString(), client.UserId, account = client.Account, headIcon = client.HeadIcon, realName = client.UserName, toAccount, toHeadIcon, messageType = messageType.ToString(), toUserId, toMessage, dateTime = DateTime.Now, latestDate = DateTime.Now }.ToJsonString());
                            break;
                        case MessageReceiveType.voice:
                            var voiceInput = messageContent.ToObject<MessageVoiceInput>();
                            toMessage = new { path = "/api/file/Image/IM/" + voiceInput.name, length = voiceInput.length };
                            await SendMessageAsync(client.ConnectionId, new { method = MessageSendType.sendMessage.ToString(), client.UserId, account = client.Account, headIcon = client.HeadIcon, realName = client.UserName, toAccount, toHeadIcon, messageType = messageType.ToString(), toUserId, toMessage, dateTime = DateTime.Now }.ToJsonString());
                            break;
                    }

                    if (WebSocketConnectionManager.GetSocketClientToUserCount(string.Format("{0}-{1}", client.TenantId, toUserId)) > 0)
                    {
                        switch (messageType)
                        {
                            case MessageReceiveType.text:
                                await SendMessageToUserAsync(string.Format("{0}-{1}", client.TenantId, toUserId), new { method = MessageSendType.receiveMessage.ToString(), messageType = messageType.ToString(), formUserId = client.UserId, formMessage = messageContent, dateTime = DateTime.Now, latestDate = DateTime.Now, headIcon = client.HeadIcon, realName = client.UserName, account = client.Account }.ToJsonString());
                                break;
                            case MessageReceiveType.image:
                                var imageInput = messageContent.ToObject<MessagetImageInput>();
                                var formMessage = new { path = "/api/file/Image/IM/" + fileName, width = imageInput.width, height = imageInput.height };
                                await SendMessageToUserAsync(string.Format("{0}-{1}", client.TenantId, toUserId), new { method = MessageSendType.receiveMessage.ToString(), messageType = messageType.ToString(), formUserId = client.UserId, formMessage, dateTime = DateTime.Now, latestDate = DateTime.Now, headIcon = client.HeadIcon, realName = client.UserName, account = client.Account }.ToJsonString());
                                break;
                            case MessageReceiveType.voice:
                                var voiceInput = messageContent.ToObject<MessageVoiceInput>();
                                toMessage = new { path = "/api/file/Image/IM/" + voiceInput.name, length = voiceInput.length };
                                await SendMessageToUserAsync(string.Format("{0}-{1}", client.TenantId, toUserId), new { method = MessageSendType.receiveMessage.ToString(), messageType = messageType.ToString(), formUserId = client.UserId, formMessage = toMessage, dateTime = DateTime.Now, latestDate = DateTime.Now, headIcon = client.HeadIcon, realName = client.UserName, account = client.Account }.ToJsonString());
                                break;
                        }
                    }
                    await GeTuiMessage(toUserId, client);
                }

                break;
            case MothodType.UpdateReadMessage:
                var fromUserId = message.formUserId;
                await _sqlSugarClient.Updateable<IMContentEntity>()
                    .SetColumns(x => new IMContentEntity()
                    {
                        State = 1,
                        ReceiveTime = DateTime.Now
                    }).Where(x => x.State == 0 && x.SendUserId == fromUserId && x.ReceiveUserId == client.UserId).ExecuteCommandAsync();
                break;
            case MothodType.MessageList:
                var sendUserId = message.toUserId; // 发送者
                var receiveUserId = message.formUserId; // 接收者

                var data = await _sqlSugarClient.Queryable<IMContentEntity>().WhereIF(!string.IsNullOrEmpty(message.keyword), it => it.Content.Contains(message.keyword))
                        .Where(i => (i.SendUserId == message.toUserId && i.ReceiveUserId == message.formUserId) || ((i.SendUserId == message.formUserId && i.ReceiveUserId == message.toUserId) && (SqlFunc.IsNullOrEmpty(i.SendDeleteMark) || i.SendDeleteMark != client.UserId))).OrderBy(it => it.SendTime, message.sord == "asc" ? OrderByType.Asc : OrderByType.Desc)
                        .Select(it => new IMContentListOutput
                        {
                            id = it.Id,
                            sendUserId = it.SendUserId,
                            sendTime = it.SendTime,
                            receiveUserId = it.ReceiveUserId,
                            receiveTime = it.ReceiveTime,
                            content = it.Content,
                            contentType = it.ContentType,
                            state = it.State
                        }).ToPagedListAsync(message.currentPage, message.pageSize);

                await SendMessageAsync(client.ConnectionId, new { method = MessageSendType.messageList.ToString(), list = data.list.OrderBy(x => x.sendTime), pagination = data.pagination }.ToJsonString());
                break;
            case MothodType.HeartCheck: break;
        }
    }

    /// <summary>
    /// 获取在线用户列表.
    /// </summary>
    /// <param name="tenantId">租户ID.</param>
    /// <returns></returns>
    public async Task<List<UserOnlineModel>> GetOnlineUserList(string tenantId)
    {
        string cacheKey = string.Format("{0}:{1}", CommonConst.CACHEKEYONLINEUSER, tenantId);
        return await _cacheManager.GetAsync<List<UserOnlineModel>>(cacheKey);
    }

    /// <summary>
    /// 保存在线用户列表.
    /// </summary>
    /// <param name="tenantId">租户ID.</param>
    /// <param name="onlineList">在线用户列表.</param>
    /// <returns></returns>
    public async Task<bool> SetOnlineUserList(string tenantId, List<UserOnlineModel> onlineList)
    {
        return await _cacheManager.SetAsync(string.Format("{0}:{1}", CommonConst.CACHEKEYONLINEUSER, tenantId), onlineList);
    }

    /// <summary>
    /// 创建IM内容.
    /// </summary>
    /// <returns></returns>
    private IMContentEntity CreateIMContent(string sendUserId, string receiveUserId, string message, string messageType)
    {
        return new IMContentEntity()
        {
            Id = SnowflakeIdHelper.NextId(),
            SendUserId = sendUserId,
            SendTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
            ReceiveUserId = receiveUserId,
            State = 0,
            Content = message,
            ContentType = messageType
        };
    }

    private async Task GeTuiMessage(string toUserIds, WebSocketClient client)
    {
        var getuiUrl = "{0}?clientId={1}&title={2}&content={3}1&text={4}&create=true";
        if (toUserIds.Any())
        {
            var clientIdList = await _sqlSugarClient.Queryable<UserDeviceEntity>().Where(x => toUserIds == x.UserId && x.DeleteMark == null).Select(x => x.ClientId).ToListAsync();
            if (clientIdList.Any())
            {
                var clientId = string.Join(",", clientIdList);
                var textDic = new Dictionary<string, string>();
                textDic.Add("type", "3");
                textDic.Add("name", client.UserName + "/" + client.Account);
                textDic.Add("formUserId", client.UserId);
                textDic.Add("headIcon", "/api/File/Image/userAvatar/" + client.HeadIcon);
                getuiUrl = string.Format(getuiUrl, _messageOptions.AppPushUrl, clientId, client.UserName + "/" + client.Account, "您有一条新消息", textDic.ToJsonString());
                await getuiUrl.GetAsStringAsync();
            }
        }
    }

    /// <summary>
    /// 获取全局租户缓存.
    /// </summary>
    /// <returns></returns>
    private async Task<GlobalTenantCacheModel> GetGlobalTenantCache(string tenantId)
    {
        string cacheKey = string.Format("{0}", CommonConst.GLOBALTENANT);
        return (await _cacheManager.GetAsync<List<GlobalTenantCacheModel>>(cacheKey)).Find(it => it.TenantId.Equals(tenantId));
    }
}