using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Net;
using Poxiao.Infrastructure.Security;
using Poxiao.EventBus;
using Poxiao.EventHandler;
using Poxiao.Logging.Attributes;
using Poxiao.Systems.Entitys.System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using System.Security.Claims;
using SqlSugar;

namespace Poxiao.Infrastructure.Core.Filter;

/// <summary>
/// 请求日志拦截.
/// </summary>
public class RequestActionFilter : IAsyncActionFilter
{
    /// <summary>
    /// 事件总线.
    /// </summary>
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// 构造函数.
    /// </summary>
    public RequestActionFilter(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// 请求日记写入.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userContext = App.User;
        var httpContext = context.HttpContext;
        var httpRequest = httpContext.Request;
        UserAgent userAgent = new UserAgent(httpContext);

        Stopwatch sw = new Stopwatch();
        sw.Start();
        var actionContext = await next();
        sw.Stop();

        // 判断是否请求成功（没有异常就是请求成功）
        var isRequestSucceed = actionContext.Exception == null;
        var headers = httpRequest.Headers;
        if (!context.ActionDescriptor.EndpointMetadata.Any(m => m.GetType() == typeof(IgnoreLogAttribute)))
        {
            var userId = userContext?.FindFirstValue(ClaimConst.CLAINMUSERID);
            var userName = userContext?.FindFirstValue(ClaimConst.CLAINMREALNAME);
            var tenantId = userContext?.FindFirstValue(ClaimConst.TENANTID);

            await _eventPublisher.PublishAsync(new LogEventSource("Log:CreateReLog", tenantId, new SysLogEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                UserId = userId,
                UserName = userName,
                Category = 5,
                IPAddress = NetHelper.Ip,
                RequestURL = httpRequest.Path,
                RequestDuration = (int)sw.ElapsedMilliseconds,
                RequestMethod = httpRequest.Method,
                PlatForm = string.Format("{0}-{1}", userAgent.OS.ToString(), userAgent.RawValue),
                CreatorTime = DateTime.Now
            }));

            if (context.ActionDescriptor.EndpointMetadata.Any(m => m.GetType() == typeof(OperateLogAttribute)))
            {
                // 操作参数
                var args = context.ActionArguments.ToJsonString();
                var result = (actionContext.Result as JsonResult)?.Value;
                var module = context.ActionDescriptor.EndpointMetadata.Where(x => x.GetType() == typeof(OperateLogAttribute)).ToList().FirstOrDefault() as OperateLogAttribute;

                await _eventPublisher.PublishAsync(new LogEventSource("Log:CreateOpLog", tenantId, new SysLogEntity
                {
                    Id = SnowflakeIdHelper.NextId(),
                    UserId = userId,
                    UserName = userName,
                    Category = 3,
                    IPAddress = NetHelper.Ip,
                    RequestURL = httpRequest.Path,
                    RequestDuration = (int)sw.ElapsedMilliseconds,
                    RequestMethod = module.Action,
                    PlatForm = string.Format("{0}-{1}", userAgent.OS.ToString(), userAgent.RawValue),
                    CreatorTime = DateTime.Now,
                    ModuleName = module.ModuleName,
                    Json = string.Format("{0}应用【{1}】【{2}】", module.Action, args, result?.ToJsonString())
                }));
            }
        }
    }
}