using Microsoft.AspNetCore.Mvc;
using Poxiao.Extras.CollectiveOAuth.Models;
using Poxiao.Extras.CollectiveOAuth.Request;
using Poxiao.Systems.Entitys.Dto.Socials;
using Poxiao.Systems.Entitys.Model.Permission.SocialsUser;

namespace Poxiao.Systems.Interfaces.Permission;

/// <summary>
/// 业务契约：第三方登录.
/// </summary>
public interface ISocialsUserService
{
    AuthCallbackNew SetAuthCallback(string code, string state);

    IAuthRequest GetAuthRequest(string authSource, string userId, bool isLogin, string ticket, string tenantId);

    List<SocialsUserListOutput> GetLoginList(string ticket);

    Task<string> Binding([FromQuery] SocialsUserInputModel model);

    Task<dynamic> GetSocialsUserInfo([FromQuery] SocialsUserInputModel model);

    Task<SocialsUserInfo> GetUserInfo(string source, string uuid, string socialName);

    string GetSocialUuid(AuthResponse res);
}