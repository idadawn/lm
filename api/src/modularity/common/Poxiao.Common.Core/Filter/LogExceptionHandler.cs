using Microsoft.AspNetCore.Mvc.Filters;
using Poxiao.DataEncryption;
using Poxiao.DependencyInjection;
using Poxiao.EventBus;
using Poxiao.EventHandler;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Net;
using Poxiao.Infrastructure.Security;
using Poxiao.Logging.Attributes;
using Poxiao.Systems.Entitys.System;
using SqlSugar;
using System.Security.Claims;

namespace Poxiao.Infrastructure.Core.Filter;

/// <summary>
/// 全局异常处理.
/// </summary>
public class LogExceptionHandler : IGlobalExceptionHandler, ISingleton
{
    private readonly IEventPublisher _eventPublisher;

    public LogExceptionHandler(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// 异步写入异常日记.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task OnExceptionAsync(ExceptionContext context)
    {
        var userContext = App.User;
        var httpContext = context.HttpContext;
        var httpRequest = httpContext?.Request;
        var headers = httpRequest?.Headers;
        UserAgent userAgent = new UserAgent(httpContext);

        if (!context.ActionDescriptor.EndpointMetadata.Any(m => m.GetType() == typeof(IgnoreLogAttribute)))
        {
            string userId = userContext?.FindFirstValue(ClaimConst.CLAINMUSERID);
            string userName = userContext?.FindFirstValue(ClaimConst.CLAINMREALNAME);
            string tenantId = userContext?.FindFirstValue(ClaimConst.TENANTID);

            await _eventPublisher.PublishAsync(new LogEventSource("Log:CreateExLog", tenantId, new SysLogEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                UserId = userId,
                UserName = userName,
                Category = 4,
                IPAddress = NetHelper.Ip,
                RequestURL = httpRequest.Path,
                RequestMethod = httpRequest.Method,
                Json = context.Exception.Message + "\n" + context.Exception.StackTrace + "\n" + context.Exception.TargetSite.GetParameters().ToString(),
                PlatForm = string.Format("{0}-{1}", userAgent.OS.ToString(), userAgent.RawValue),
                CreatorTime = DateTime.Now
            }));
        }
    }
}