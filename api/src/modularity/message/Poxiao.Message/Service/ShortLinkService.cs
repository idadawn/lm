using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Dtos.OAuth;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Net;
using Poxiao.Infrastructure.Options;
using Poxiao.Infrastructure.Security;
using Poxiao.DataEncryption;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.Extras.DatabaseAccessor.SqlSugar.Models;
using Poxiao.FriendlyException;
using Poxiao.Logging.Attributes;
using Poxiao.Message.Entitys.Entity;
using Poxiao.Message.Interfaces.Message;
using Poxiao.RemoteRequest.Extensions;
using Poxiao.Systems.Entitys.Permission;
using Poxiao.Systems.Interfaces.System;
using Poxiao.UnifyResult;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace Poxiao.Message.Service;

/// <summary>
/// 公众号.
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "Message", Name = "ShortLink", Order = 240)]
[Route("api/message/[controller]")]
public class ShortLinkService : IShortLinkService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<MessageShortLinkEntity> _repository;
    private readonly ISysConfigService _sysConfigService;
    private readonly ConnectionStringsOptions _connectionStrings;
    private readonly TenantOptions _tenant;
    private readonly MessageOptions _messageOptions = App.GetConfig<MessageOptions>("Message", true);
    private SqlSugarScope _sqlSugarClient;

    public ShortLinkService(
        ISqlSugarRepository<MessageShortLinkEntity> repository,
        ISysConfigService sysConfigService,
        IOptions<ConnectionStringsOptions> connectionOptions,
        IOptions<TenantOptions> tenantOptions,
        ISqlSugarClient sqlSugarClient
        )
    {
        _repository = repository;
        _sysConfigService = sysConfigService;
        _connectionStrings = connectionOptions.Value;
        _tenant = tenantOptions.Value;
        _sqlSugarClient = (SqlSugarScope)sqlSugarClient;
    }

    /// <summary>
    /// 新建.
    /// </summary>
    /// <returns></returns>
    [HttpGet("{shortLink}")]
    [HttpGet("{shortLink}/{tenantId}")]
    [AllowAnonymous]
    [IgnoreLog]
    public async Task GetInfo(string shortLink, string tenantId)
    {
        var defaultConnection = _connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default");
        if (defaultConnection == null)
            throw Oops.Oh("Default connection not found");
        string configId = defaultConnection.ConfigId?.ToString() ?? "default";
        ConnectionConfigOptions options = PoxiaoTenantExtensions.GetLinkToOrdinary(configId, defaultConnection.DBName);
        UserAgent userAgent = new UserAgent(App.HttpContext);
        if (tenantId.IsNotEmptyOrNull())
        {
            var interFace = string.Format("{0}{1}", _tenant.MultiTenancyDBInterFace, tenantId);
            var response = await interFace.GetAsStringAsync();
            var result = response.ToObject<RESTfulResult<TenantInterFaceOutput>>();
            if (result.code != 200)
            {
                throw Oops.Oh(result.msg);
            }
            else if (result.data.dotnet == null && result.data.linkList == null)
            {
                throw Oops.Oh(ErrorCode.D1025);
            }
            else
            {
                if (result.data.linkList == null || result.data.linkList?.Count == 0)
                {
                    options = PoxiaoTenantExtensions.GetLinkToOrdinary(tenantId, result.data.dotnet);
                }
                else if (result.data.dotnet == null)
                {
                    options = PoxiaoTenantExtensions.GetLinkToCustom(tenantId, result.data.linkList);
                }
            }
            if (!"default".Equals(tenantId) && _tenant.MultiTenancyType.Equals("COLUMN"))
            {
                _sqlSugarClient.QueryFilter.AddTableFilter<ITenantFilter>(it => it.TenantId == result.data.dotnet);
            }
            else
            {
                _sqlSugarClient.AddConnection(PoxiaoTenantExtensions.GetConfig(options));
                _sqlSugarClient.ChangeDatabase(tenantId);
            }
        }

        var entity = await _sqlSugarClient.Queryable<MessageShortLinkEntity>().SingleAsync(x => x.ShortLink == shortLink && x.DeleteMark == null);
        if (entity == null) throw Oops.Oh(ErrorCode.D7009);
        // 验证失效以及修改点击次数
        if (entity.IsUsed == 1)
        {
            if (entity.UnableTime < DateTime.Now || entity.ClickNum == entity.UnableNum) throw Oops.Oh(ErrorCode.D7010);
            ++entity.ClickNum;
            await _repository.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandHasChangeAsync();
        }
        string accessToken = CreateToken(entity.UserId);
        // 验证请求端
        var urlLink = userAgent.IsMobileDevice ? entity.RealAppLink : entity.RealPcLink;
        urlLink = string.Format("{0}&token={1}", urlLink, accessToken);
        App.HttpContext.Response.Redirect(urlLink);
    }

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="userId">用户ID.</param>
    /// <param name="bodyText"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<MessageShortLinkEntity> Create(string userId, string bodyText)
    {
        var sysconfig = await _sysConfigService.GetInfo();
        var entity = new MessageShortLinkEntity();
        entity.ShortLink = RandomExtensions.NextLetterAndNumberString(new Random(), 6);
        entity.UserId = userId;
        if (sysconfig.isClick == 1)
        {
            entity.IsUsed = 1;
            entity.ClickNum = 0;
            entity.UnableNum = sysconfig.unClickNum;
            entity.UnableTime = DateTime.Now.AddHours(sysconfig.linkTime);
        }
        entity.BodyText = bodyText;
        entity.RealAppLink = string.Format("{0}/pages/workFlow/flowBefore/index?config={1}", _messageOptions.DoMainApp, bodyText.ToBase64String());
        entity.RealPcLink = string.Format("{0}/workFlowDetail?config={1}", _messageOptions.DoMainPc, bodyText.ToBase64String());
        return await _repository.AsInsertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Create()).ExecuteReturnEntityAsync();
    }

    public string CreateToken(string userId)
    {
        var defaultConnection = _connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default");
        if (defaultConnection == null)
            throw Oops.Oh("Default connection not found");
        ConnectionConfigOptions options = PoxiaoTenantExtensions.GetLinkToOrdinary(defaultConnection.ConfigId?.ToString() ?? "default", defaultConnection.DBName);
        var userEntity = _sqlSugarClient.Queryable<UserEntity>().Single(u => u.Id == userId && u.DeleteMark == null);
        var token = NetHelper.GetToken(userEntity.Id, userEntity.Account, userEntity.RealName, userEntity.IsAdministrator, options.ConfigId, false);
        return token;
    }
}
