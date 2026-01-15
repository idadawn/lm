using System.Text;
using System.Xml;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Security;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.Extras.Thirdparty.WeChat;
using Poxiao.Extras.Thirdparty.WeChat.Internal;
using Poxiao.FriendlyException;
using Poxiao.Logging.Attributes;
using Poxiao.Message.Entitys.Entity;
using Poxiao.Systems.Entitys.Permission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Senparc.Weixin.MP;
using SqlSugar;

namespace Poxiao.Message.Service;

/// <summary>
/// 公众号.
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "Message", Name = "WechatOpen", Order = 240)]
[Route("api/message/[controller]")]
public class WechatOpenService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<MessageWechatUserEntity> _repository;

    public WechatOpenService(ISqlSugarRepository<MessageWechatUserEntity> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 验证.
    /// </summary>
    /// <param name="input">实体对象.</param>
    /// <returns></returns>
    [HttpGet("token")]
    [AllowAnonymous]
    [IgnoreLog]
    [NonUnify]
    public dynamic CheckToken([FromQuery] string signature, [FromQuery] string timestamp, [FromQuery] string nonce, [FromQuery] string echostr)
    {
        var token = "WEIXINPoxiao";
        var hashCode = CheckSignature.GetSignature(timestamp, nonce, token);
        if (hashCode == signature)
        {
            Console.WriteLine("验证成功");
            return echostr;
        }
        else
        {
            Console.WriteLine("验证失败");
            return string.Empty;
        }
    }

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">实体对象.</param>
    /// <returns></returns>
    [HttpPost("token")]
    [AllowAnonymous]
    [IgnoreLog]
    [NonUnify]
    public async Task<dynamic> Create([FromQuery] string signature, [FromQuery] string timestamp, [FromQuery] string nonce, [FromQuery] string openid)
    {
        var input = await GetWechatMPEvent();
        var weChatMp = new WeChatMPUtil();
        if (weChatMp.CheckToken(signature, timestamp, nonce))
        {
            var messageAccountEntity = await _repository.AsSugarClient().Queryable<MessageAccountEntity>().FirstAsync(x => x.AppKey == input.ToUserName && x.DeleteMark == null);
            var wechatUser = new WeChatMPUtil(messageAccountEntity.AppId, messageAccountEntity.AppSecret).GetWeChatUserInfo(openid);
            if (wechatUser.IsNotEmptyOrNull() && wechatUser.unionid.IsNotEmptyOrNull())
            {
                var socialsUser = await _repository.AsSugarClient().Queryable<SocialsUsersEntity>().FirstAsync(x => x.SocialId == wechatUser.unionid && x.SocialType == "WECHAT_OPEN" && x.DeleteMark == null);
                var entity = await _repository.GetFirstAsync(x => x.GzhId == input.ToUserName && x.UserId == socialsUser.UserId && x.OpenId == openid);
                if (entity.IsNullOrEmpty())
                {
                    entity = new MessageWechatUserEntity
                    {
                        GzhId = input.ToUserName,
                        UserId = socialsUser.UserId,
                        OpenId = openid,
                        CloseMark = 1
                    };
                    await _repository.AsInsertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
                }
                else
                {
                    entity.CloseMark = input.Event.Equals("subscribe") ? 1 : 0;
                    await _repository.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandHasChangeAsync();
                }
            }
            return string.Empty;
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 获取xml参数.
    /// </summary>
    /// <returns></returns>
    private async Task<WechatMPEvent> GetWechatMPEvent()
    {
        var request = App.HttpContext.Request;
        var buffer = new byte[Convert.ToInt32(request.ContentLength)];
        await request.Body.ReadAsync(buffer, 0, buffer.Length);
        var body = Encoding.UTF8.GetString(buffer);
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(body);
        var input = new WechatMPEvent();
        input.ToUserName = doc.DocumentElement.SelectSingleNode("ToUserName").InnerText.Trim();
        input.FromUserName = doc.DocumentElement.SelectSingleNode("FromUserName").InnerText.Trim();
        input.Event= doc.DocumentElement.SelectSingleNode("Event").InnerText.Trim();
        return input;
    }
}
