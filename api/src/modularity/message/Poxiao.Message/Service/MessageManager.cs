using Poxiao.DependencyInjection;
using Poxiao.Extras.Thirdparty.DingDing;
using Poxiao.Extras.Thirdparty.Email;
using Poxiao.Extras.Thirdparty.Sms;
using Poxiao.Extras.Thirdparty.WeChat;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Core.Handlers;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Dtos.Message;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Options;
using Poxiao.Infrastructure.Security;
using Poxiao.Message.Entitys;
using Poxiao.Message.Entitys.Entity;
using Poxiao.Message.Interfaces;
using Poxiao.Message.Interfaces.Message;
using Poxiao.RemoteRequest.Extensions;
using Poxiao.Systems.Entitys.System;
using Poxiao.Systems.Interfaces.Permission;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using SqlSugar;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using MessageTemplateEntity = Poxiao.Message.Entitys.Entity.MessageTemplateEntity;

namespace Poxiao.Message.Service;

/// <summary>
/// 消息中心处理类.
/// </summary>
public class MessageManager : IMessageManager, ITransient
{
    private readonly ISqlSugarRepository<MessageEntity> _repository;
    private readonly IUsersService _usersService;
    private readonly IMHandler _imHandler;
    private readonly IUserManager _userManager;
    private readonly IShortLinkService _shortLinkService;
    private readonly MessageOptions _messageOptions = App.GetConfig<MessageOptions>("Message", true);

    public MessageManager(
        ISqlSugarRepository<MessageEntity> repository,
        IUsersService usersService,
        IMHandler imHandler,
        IShortLinkService shortLinkService,
        IUserManager userManager)
    {
        _repository = repository;
        _usersService = usersService;
        _shortLinkService = shortLinkService;
        _imHandler = imHandler;
        _userManager = userManager;
    }

    #region Public

    /// <summary>
    /// 默认消息发送.
    /// </summary>
    /// <param name="toUserIds"></param>
    /// <param name="messageEntity"></param>
    /// <param name="receiveEntityList"></param>
    /// <returns></returns>
    public async Task SendDefaultMsg(List<string> toUserIds, MessageEntity messageEntity, List<MessageReceiveEntity> receiveEntityList)
    {
        await WebSocketSend(toUserIds, messageEntity, receiveEntityList);

        #region 消息监控
        var messageMonitorEntity = new MessageMonitorEntity();
        messageMonitorEntity.MessageType = "1";
        messageMonitorEntity.MessageSource = messageEntity.Type.ToString();
        messageMonitorEntity.SendTime = DateTime.Now;
        messageMonitorEntity.MessageTemplateId = string.Empty;
        messageMonitorEntity.Title = messageEntity.Title;
        messageMonitorEntity.Content = string.Empty;
        messageMonitorEntity.ReceiveUser = toUserIds.ToJsonString();
        await _repository.AsSugarClient().Insertable(messageMonitorEntity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        #endregion
    }

    /// <summary>
    /// 自定义消息发送.
    /// </summary>
    /// <param name="messageSendModel"></param>
    /// <param name="bodyDic">跳转页面参数,参数格式 key:用户id，value:跳转json.</param>
    /// <returns></returns>
    public async Task<string> SendDefinedMsg(MessageSendModel messageSendModel, Dictionary<string, object> bodyDic)
    {
        var errorList = new List<string>();
        var messageTemplateEntity = await _repository.AsSugarClient().Queryable<MessageTemplateEntity>().FirstAsync(x => x.Id == messageSendModel.templateId && x.DeleteMark == null);
        var messageAccountEntity = await _repository.AsSugarClient().Queryable<MessageAccountEntity>().FirstAsync(x => x.Id == messageSendModel.accountConfigId && x.DeleteMark == null);
        var paramsDic = messageSendModel.paramJson.ToDictionary(x => x.field, y => y.value); //参数
        var title = messageTemplateEntity.Title;
        var content = messageTemplateEntity.Content;
        if (messageTemplateEntity.MessageType == "6") messageSendModel.toUser = new List<string> { _userManager.UserId };
        foreach (var item in messageSendModel.toUser)
        {
            var userId = item.Replace("-delegate", string.Empty);
            var userName = await _usersService.GetUserName(userId);
            try
            {
                if (bodyDic.IsNotEmptyOrNull() && bodyDic.ContainsKey(item))
                {
                    var shortLinkEntity = await _shortLinkService.Create(userId, bodyDic[item].ToJsonString());
                    paramsDic["@FlowLink"] = string.Format("{0}/dev/api/message/ShortLink/{1}", _messageOptions.DoMainPc, shortLinkEntity.ShortLink);
                }
                messageTemplateEntity.Title = MessageTemplateManage(title, paramsDic);
                messageTemplateEntity.Content = MessageTemplateManage(content, paramsDic);
                switch (messageTemplateEntity.MessageType)
                {
                    case "1": //站内信
                        var messageEntity = new MessageEntity();
                        var messageReceiveList = new List<MessageReceiveEntity>();
                        if (paramsDic.ContainsKey("@MsgId") && messageTemplateEntity.MessageSource == "1")
                        {
                            messageEntity = _repository.GetById(paramsDic["@MsgId"]);
                            messageEntity.DefaultTitle = messageTemplateEntity.Title;
                        }
                        else
                        {
                            messageEntity = GetMessageEntity(messageTemplateEntity.EnCode, paramsDic, messageTemplateEntity.MessageSource.ParseToInt());
                        }
                        messageReceiveList = GetMessageReceiveList(new List<string>() { userId }, messageEntity, bodyDic);
                        await WebSocketSend(new List<string>() { userId }, messageEntity, messageReceiveList);
                        break;
                    case "2": //邮件
                        EmailSend(new List<string>() { userId }, messageTemplateEntity, messageAccountEntity);
                        break;
                    case "3": //短信
                        SmsSend(new List<string>() { userId }, messageTemplateEntity, messageAccountEntity, paramsDic);
                        break;
                    case "4": //钉钉
                        var dingIds = _repository.AsSugarClient().Queryable<SynThirdInfoEntity>()
                            .Where(x => x.ThirdType == 2 && x.DataType == 3 && x.SysObjId == userId && !SqlFunc.IsNullOrEmpty(x.ThirdObjId))
                        .Select(x => x.ThirdObjId).ToList();
                        if (dingIds.Count > 0)
                        {
                            var dingMsg = new { msgtype = "text", text = new { content = messageTemplateEntity.Content } }.ToJsonString();
                            DingWorkMessageParameter dingWorkMsgModel = new DingWorkMessageParameter()
                            {
                                toUsers = string.Join(",", dingIds),
                                agentId = messageAccountEntity.AgentId,
                                msg = dingMsg
                            };
                            new DingUtil(messageAccountEntity.AppId, messageAccountEntity.AppSecret).SendWorkMsg(dingWorkMsgModel);
                        }
                        else
                        {
                            throw Oops.Oh(ErrorCode.D7015);
                        }
                        break;
                    case "5": //企业微信
                        var qyIds = _repository.AsSugarClient().Queryable<SynThirdInfoEntity>()
                            .Where(x => x.ThirdType == 1 && x.DataType == 3 && x.SysObjId == userId && !SqlFunc.IsNullOrEmpty(x.ThirdObjId))
                        .Select(x => x.ThirdObjId).ToList();
                        var weChat = new WeChatUtil(messageAccountEntity.EnterpriseId, messageAccountEntity.AppSecret);
                        if (qyIds.Count > 0)
                        {
                            await weChat.SendText(messageAccountEntity.AppId, messageTemplateEntity.Content, string.Join(",", qyIds));
                        }
                        else
                        {
                            throw Oops.Oh(ErrorCode.D7015);
                        }
                        break;
                    case "6": //WebHook
                        await WebHookSend(messageTemplateEntity, messageAccountEntity);
                        break;
                    case "7": //微信公众号
                        var body = bodyDic.ContainsKey(item) ? bodyDic[item].ToJsonString() : string.Empty;
                        WeChatMpSend(userId, messageTemplateEntity, messageAccountEntity, paramsDic, body);
                        break;
                }
            }
            catch (Exception ex)
            {
                errorList.Add(string.Format("用户{0}【{1}】", userName, ex.Message));
            }
        }

        #region 消息监控
        var messageMonitorEntity = new MessageMonitorEntity();
        messageMonitorEntity.MessageType = messageTemplateEntity.MessageType;
        messageMonitorEntity.MessageSource = messageTemplateEntity.MessageSource;
        messageMonitorEntity.SendTime = DateTime.Now;
        messageMonitorEntity.MessageTemplateId = messageTemplateEntity.Id;
        messageMonitorEntity.Title = messageTemplateEntity.Title;
        messageMonitorEntity.Content = messageTemplateEntity.Content;
        messageMonitorEntity.ReceiveUser = messageSendModel.toUser.ToJsonString();
        await _repository.AsSugarClient().Insertable(messageMonitorEntity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        #endregion
        return errorList.Any() ? string.Join(",", errorList) : string.Empty;
    }

    /// <summary>
    /// 获取自定义消息发送配置.
    /// </summary>
    /// <param name="sendConfigId"></param>
    /// <returns></returns>
    public async Task<List<MessageSendModel>> GetMessageSendModels(string sendConfigId)
    {
        var list = await _repository.AsSugarClient().Queryable<MessageSendTemplateEntity, MessageTemplateEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.TemplateId == b.Id))
            .Where((a, b) => a.SendConfigId == sendConfigId && a.DeleteMark == null && b.DeleteMark == null)
            .Select((a, b) => new MessageSendModel
            {
                accountConfigId = a.AccountConfigId,
                id = a.Id,
                messageType = SqlFunc.Subqueryable<MessageDataTypeEntity>().Where(u => u.Type == "1" && u.EnCode == a.MessageType).Select(u => u.FullName),
                msgTemplateName = b.FullName,
                sendConfigId = a.SendConfigId,
                templateId = a.TemplateId,
            }).ToListAsync();
        foreach (var item in list)
        {
            // 是否存在参数.
            var flag = await _repository.AsSugarClient().Queryable<MessageSmsFieldEntity>().AnyAsync(x => x.TemplateId == item.templateId && x.DeleteMark == null);
            if (flag)
            {
                item.paramJson = await _repository.AsSugarClient().Queryable<MessageTemplateParamEntity, MessageTemplateEntity, MessageSmsFieldEntity>((a, b, c) => new JoinQueryInfos(JoinType.Left, a.TemplateId == b.Id, JoinType.Left, a.TemplateId == c.TemplateId))
                .Where((a, b, c) => a.TemplateId == item.templateId && a.DeleteMark == null && b.DeleteMark == null && a.Field == c.Field && a.Field != "@flowLink")
                .Select((a, b) => new MessageSendParam
                {
                    field = a.Field,
                    fieldName = a.FieldName,
                    id = a.Id,
                    templateCode = b.TemplateCode,
                    templateId = a.TemplateId,
                    templateName = b.FullName,
                    templateType = b.TemplateType
                }).ToListAsync();
            }
            else
            {
                item.paramJson = await _repository.AsSugarClient().Queryable<MessageTemplateParamEntity, MessageTemplateEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.TemplateId == b.Id))
                .Where((a, b) => a.TemplateId == item.templateId && a.DeleteMark == null && b.DeleteMark == null && a.Field != "@flowLink")
                .Where((a, b) => b.Title.Contains(a.Field) || b.Content.Contains(a.Field))
                .Select((a, b) => new MessageSendParam
                {
                    field = a.Field,
                    fieldName = a.FieldName,
                    id = a.Id,
                    templateCode = b.TemplateCode,
                    templateId = a.TemplateId,
                    templateName = b.FullName,
                    templateType = b.TemplateType
                }).ToListAsync();
            }
        }
        return list;
    }

    /// <summary>
    /// 获取消息实例.
    /// </summary>
    /// <param name="enCode">消息编码.</param>
    /// <param name="paramDic">标题或内容替换参数.</param>
    /// <param name="type">消息类型 1-公告 2-流程 3-系统 4-日程.</param>
    /// <param name="flowType">流程类型 1-审批 2-委托.</param>
    /// <returns></returns>
    public MessageEntity GetMessageEntity(string enCode, Dictionary<string, string> paramDic, int type, int flowType = 1)
    {
        var messageEntity = new MessageEntity();
        messageEntity.Id = SnowflakeIdHelper.NextId();
        messageEntity.Type = type;
        messageEntity.LastModifyTime = DateTime.Now;
        messageEntity.LastModifyUserId = _userManager.UserId;
        messageEntity.FlowType = flowType;

        var msgTemplateEntity = _repository.AsSugarClient().Queryable<MessageTemplateEntity>().First(x => x.EnCode == enCode && x.DeleteMark == null);

        if (msgTemplateEntity.IsNotEmptyOrNull() && flowType == 1)
        {
            messageEntity.Title = MessageTemplateManage(msgTemplateEntity.Title, paramDic);
            messageEntity.BodyText = MessageTemplateManage(msgTemplateEntity.Content, paramDic);
            if (type == 3)
            {
                messageEntity.BodyText = messageEntity.Title;
            }
        }
        else
        {
            messageEntity.Title = string.Format("{0}已{1}您的{2}流程!", _userManager.User.RealName, paramDic["delegateType"], paramDic["flowName"]);
        }
        return messageEntity;
    }

    /// <summary>
    /// 获取消息接收数据.
    /// </summary>
    /// <param name="toUserIds">发送人员.</param>
    /// <param name="messageEntity">消息实例.</param>
    /// <param name="bodyDic">跳转页面参数,参数格式 key:用户id，value:跳转json.</param>
    /// <returns></returns>
    public List<MessageReceiveEntity> GetMessageReceiveList(List<string> toUserIds, MessageEntity messageEntity, Dictionary<string, object> bodyDic = null)
    {
        List<MessageReceiveEntity> receiveEntityList = new List<MessageReceiveEntity>();
        if (messageEntity.Type == 1 || messageEntity.Type == 3)
        {
            receiveEntityList = toUserIds
                .Select(x => new MessageReceiveEntity()
                {
                    Id = SnowflakeIdHelper.NextId(),
                    MessageId = messageEntity.Id,
                    UserId = x,
                    IsRead = 0,
                    BodyText = messageEntity.BodyText,
                }).ToList();
        }
        else
        {
            receiveEntityList = toUserIds
                .Select(x => new MessageReceiveEntity()
                {
                    Id = SnowflakeIdHelper.NextId(),
                    MessageId = messageEntity.Id,
                    UserId = x.Replace("-delegate", string.Empty),
                    IsRead = 0,
                    BodyText = bodyDic.IsNotEmptyOrNull() && bodyDic.ContainsKey(x) ? bodyDic[x].ToJsonString() : null,
                }).ToList();
        }

        return receiveEntityList;
    }

    /// <summary>
    /// 强制下线.
    /// </summary>
    /// <param name="connectionId"></param>
    public async Task ForcedOffline(string connectionId)
    {
        await _imHandler.SendMessageAsync(connectionId, new { method = "logout", msg = "此账号已在其他地方登录" }.ToJsonString());
    }
    #endregion

    #region Private

    /// <summary>
    /// 保存数据.
    /// </summary>
    /// <param name="entity">实体对象.</param>
    /// <param name="receiveEntityList">收件用户.</param>
    private int SaveMessage(MessageEntity entity, List<MessageReceiveEntity> receiveEntityList)
    {
        try
        {
            _repository.AsSugarClient().Insertable(receiveEntityList).ExecuteCommand();
            if (entity.Type == 1)
            {
                entity.EnabledMark = 1;
                if (_repository.IsAny(x => x.Id == entity.Id))
                {
                    entity.LastModifyTime = DateTime.Now;
                    entity.LastModifyUserId = _userManager.UserId;
                }
                return _repository.AsSugarClient().Storageable(entity).ExecuteCommand();
            }
            else
            {
                return _repository.AsInsertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Create()).ExecuteCommand();
            }
        }
        catch (Exception)
        {
            return 0;
        }
    }

    /// <summary>
    /// 消息模板参数替换.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="paramDic"></param>
    /// <param name="isGzh"></param>
    /// <returns></returns>
    private string MessageTemplateManage(string text, Dictionary<string, string> paramDic, bool isGzh = false)
    {
        if (text.IsNotEmptyOrNull())
        {
            // 系统参数
            //var sysParams = new List<string> { "@Title", "@CreatorUserName", "@Content", "@Remark", "@FlowLink", "@StartDate", "@StartTime", "@EndDate", "@EndTime" };
            foreach (var item in paramDic.Keys)
            {
                if (isGzh)
                {
                    text = text.Replace(item, paramDic[item]);
                }
                else
                {
                    text = text.Replace("{" + item + "}", paramDic[item]);
                }
            }
        }
        return text;
    }

    /// <summary>
    /// 邮箱.
    /// </summary>
    /// <param name="userList"></param>
    /// <param name="messageTemplateEntity"></param>
    /// <param name="messageAccountEntity"></param>
    private void EmailSend(List<string> userList, MessageTemplateEntity messageTemplateEntity, MessageAccountEntity messageAccountEntity)
    {
        var emailList = new List<string>();
        foreach (var item in userList)
        {
            var user = _usersService.GetInfoByUserId(item);
            if (user.IsNotEmptyOrNull() && user.Email.IsNotEmptyOrNull())
            {
                emailList.Add(user.Email);
            }
        }
        var mailModel = new MailInfo();
        mailModel.To = string.Join(",", emailList);
        mailModel.Subject = messageTemplateEntity.Title;
        mailModel.BodyText = HttpUtility.HtmlDecode(messageTemplateEntity.Content);
        MailUtil.Send(
            new MailParameterInfo
            {
                AccountName = messageAccountEntity.AddressorName,
                Account = messageAccountEntity.SmtpUser,
                Password = messageAccountEntity.SmtpPassword,
                SMTPHost = messageAccountEntity.SmtpServer,
                SMTPPort = messageAccountEntity.SmtpPort.ParseToInt(),
                Ssl = messageAccountEntity.SslLink.Equals("1")
            }, mailModel);
    }

    /// <summary>
    /// 短信.
    /// </summary>
    /// <param name="userList"></param>
    /// <param name="messageTemplateEntity"></param>
    /// <param name="messageAccountEntity"></param>
    /// <param name="smsParams"></param>
    private void SmsSend(List<string> userList, MessageTemplateEntity messageTemplateEntity, MessageAccountEntity messageAccountEntity, Dictionary<string, string> smsParams)
    {
        var phoneList = new List<string>(); //电话号码
        foreach (var item in userList)
        {
            var user = _usersService.GetInfoByUserId(item);
            if (user.IsNotEmptyOrNull() && user.MobilePhone.IsNotEmptyOrNull())
            {
                phoneList.Add("+86" + user.MobilePhone);
            }
        }
        var smsModel = new SmsParameterInfo()
        {
            keyId = messageAccountEntity.AppId,
            keySecret = messageAccountEntity.AppSecret,
            region = messageAccountEntity.ZoneParam,
            domain = messageAccountEntity.Channel.Equals("1") ? messageAccountEntity.EndPoint : messageAccountEntity.ZoneName,
            templateId = messageTemplateEntity.TemplateCode,
            signName = messageAccountEntity.SmsSignature,
            appId = messageAccountEntity.SdkAppId
        };
        var smsFieldList = _repository.AsSugarClient().Queryable<MessageSmsFieldEntity>().Where(x => x.TemplateId == messageTemplateEntity.Id).ToDictionary(x => x.SmsField, y => y.Field);
        foreach (var item in smsFieldList.Keys)
        {
            if (smsParams.Keys.Contains(smsFieldList[item].ToString()))
            {
                smsFieldList[item] = smsParams[smsFieldList[item].ToString()];
            }
        }
        if (messageAccountEntity.Channel.Equals("1"))
        {
            messageTemplateEntity.Content = SmsUtil.GetTemplateByAli(smsModel);
            smsModel.mobileAli = string.Join(",", phoneList);
            smsModel.templateParamAli = smsFieldList.ToJsonString();
            foreach (var item in smsFieldList.Keys)
            {
                messageTemplateEntity.Content = messageTemplateEntity.Content.Replace("${" + item + "}", smsFieldList[item].ToString());
            }
            SmsUtil.SendSmsByAli(smsModel);
        }
        else
        {
            messageTemplateEntity.Content = SmsUtil.GetTemplateByTencent(smsModel);
            smsModel.mobileTx = phoneList.ToArray();
            List<string> mList = new List<string>();
            var fields = messageTemplateEntity.Content.Substring3();
            foreach (string item in fields)
            {
                if (smsFieldList.ContainsKey(item))
                    mList.Add(smsFieldList[item].ToString());
            }
            smsModel.templateParamTx = mList.ToArray();
            foreach (var item in smsFieldList.Keys)
            {
                messageTemplateEntity.Content = messageTemplateEntity.Content.Replace("{" + item + "}", smsFieldList[item].ToString());
            }
            SmsUtil.SendSmsByTencent(smsModel);
        }
    }

    /// <summary>
    /// webhook.
    /// </summary>
    /// <param name="messageTemplateEntity"></param>
    /// <param name="messageAccountEntity"></param>
    /// <returns></returns>
    private async Task WebHookSend(MessageTemplateEntity messageTemplateEntity, MessageAccountEntity messageAccountEntity)
    {
        // 钉钉
        if (messageAccountEntity.WebhookType == "1")
        {
            // 认证
            if (messageAccountEntity.ApproveType == "2") SignWebhook(messageAccountEntity);
            new DingUtil().SendGroupMsg(messageAccountEntity.WebhookAddress, messageTemplateEntity.Content);
        }
        // 企业微信
        if (messageAccountEntity.WebhookType == "2")
        {
            var bodyDic = new Dictionary<string, object>();
            bodyDic.Add("msgtype", "text");
            bodyDic.Add("text", new { content = messageTemplateEntity.Content });
            await messageAccountEntity.WebhookAddress.SetBody(bodyDic).PostAsStringAsync();
        }
    }

    /// <summary>
    /// webhook签名.
    /// </summary>
    /// <param name="messageAccountEntity"></param>
    private void SignWebhook(MessageAccountEntity messageAccountEntity)
    {
        //  webhook加签密钥
        var secret = messageAccountEntity.Bearer;

        //  获取时间戳
        var timestamp = DateTime.Now.ParseToUnixTime();

        var signature = string.Empty;
        using (var hmac = new HMACSHA256(secret.ToBase64String().ToBytes()))
        {
            byte[] hashmessage = hmac.ComputeHash(timestamp.ToString().ToBytes(Encoding.UTF8));
            signature = hashmessage.ToHexString();
        }

        messageAccountEntity.WebhookAddress = string.Format("{0}&timestamp={1}&signature={2}", messageAccountEntity.WebhookAddress, timestamp, signature);
    }

    /// <summary>
    /// 公众号.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="messageTemplateEntity"></param>
    /// <param name="messageAccountEntity"></param>
    /// <param name="paramDic"></param>
    /// <param name="bodyDic"></param>
    private void WeChatMpSend(string userId, MessageTemplateEntity messageTemplateEntity, MessageAccountEntity messageAccountEntity, Dictionary<string, string> paramDic, string bodyDic)
    {
        var weChatMP = new WeChatMPUtil(messageAccountEntity.AppId, messageAccountEntity.AppSecret);
        var wechatUser = _repository.AsSugarClient().Queryable<MessageWechatUserEntity>().Single(x => userId == x.UserId);
        if (wechatUser == null) throw Oops.Oh(ErrorCode.D7016);
        var openId = wechatUser.IsNotEmptyOrNull() ? wechatUser.OpenId : string.Empty;
        var mpFieldList = _repository.AsSugarClient().Queryable<MessageSmsFieldEntity>().Where(x => x.TemplateId == messageTemplateEntity.Id).ToList();
        var mpTempDic = new Dictionary<string, object>();
        foreach (var item in mpFieldList)
        {
            if (paramDic.Keys.Contains(item.Field))
            {
                mpTempDic[item.SmsField] = new { value = paramDic[item.Field] };
            }
        }
        var url = paramDic.ContainsKey("@flowLink") ? paramDic["@flowLink"] : string.Empty;
        // 跳转小程序
        if (messageTemplateEntity.WxSkip == "1")
        {
            var config = bodyDic.ToBase64String();
            var token = _shortLinkService.CreateToken(userId);
            var miniProgram = new TemplateModel_MiniProgram
            {
                appid = messageTemplateEntity.XcxAppId,
                pagepath = "/pages/workFlow/flowBefore/index?config=" + config + "&token=" + token
            };
            weChatMP.SendTemplateMessage(openId, messageTemplateEntity.TemplateCode, url, mpTempDic, miniProgram);
        }
        else
        {
            weChatMP.SendTemplateMessage(openId, messageTemplateEntity.TemplateCode, url, mpTempDic);
        }
    }

    /// <summary>
    /// 站内信.
    /// </summary>
    /// <param name="toUserIds"></param>
    /// <param name="messageEntity"></param>
    /// <param name="receiveEntityList"></param>
    /// <returns></returns>
    private async Task WebSocketSend(List<string> toUserIds, MessageEntity messageEntity, List<MessageReceiveEntity> receiveEntityList)
    {
        SaveMessage(messageEntity, receiveEntityList);
        if (toUserIds.Any())
        {
            foreach (var item in toUserIds)
            {
                var userId = item.Replace("-delegate", string.Empty);
                // 消息推送 - 指定用户
                await _imHandler.SendMessageToUserAsync(string.Format("{0}-{1}", _userManager.TenantId, userId), new { method = "messagePush", messageType = 2, userId = _userManager.UserId, toUserId = toUserIds, title = messageEntity.Title, unreadNoticeCount = 1, id = messageEntity.Id }.ToJsonString());
            }
        }
        else
        {
            await _imHandler.SendMessageToTenantAsync(_userManager.TenantId, new { method = "messagePush", messageType = 1, userId = _userManager.UserId, toUserId = toUserIds, title = messageEntity.Title, unreadNoticeCount = 1, id = messageEntity.Id }.ToJsonString());
        }
    }
    #endregion
}
