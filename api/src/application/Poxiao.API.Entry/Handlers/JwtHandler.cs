using Poxiao.Authorization;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Enums;
using Poxiao.DataEncryption;
using Microsoft.AspNetCore.Authorization;

namespace Poxiao.API.Entry.Handlers;

/// <summary>
/// jwt处理程序.
/// </summary>
public class JwtHandler : AppAuthorizeHandler
{
    /// <summary>
    /// 重写 Handler 添加自动刷新.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task HandleAsync(AuthorizationHandlerContext context)
    {
        try
        {
            Console.WriteLine("[JwtHandler.HandleAsync] Start");
            var httpContext = context.GetCurrentHttpContext();
            Console.WriteLine($"[JwtHandler.HandleAsync] Path: {httpContext?.Request.Path}");

            // 检查 httpContext.User 而不是 context.User
            var user = httpContext?.User;
            Console.WriteLine($"[JwtHandler.HandleAsync] httpContext.User.Identity.IsAuthenticated: {user?.Identity?.IsAuthenticated}");
            Console.WriteLine($"[JwtHandler.HandleAsync] httpContext.User.Identity.Name: {user?.Identity?.Name}");
            Console.WriteLine($"[JwtHandler.HandleAsync] context.User.Identity.IsAuthenticated: {context.User?.Identity?.IsAuthenticated}");
            Console.WriteLine($"[JwtHandler.HandleAsync] context.User.Identity.Name: {context.User?.Identity?.Name}");

            // 如果 httpContext.User 已认证但 context.User 未认证，同步它们
            if (user?.Identity?.IsAuthenticated == true && context.User?.Identity?.IsAuthenticated != true)
            {
                Console.WriteLine("[JwtHandler.HandleAsync] Syncing httpContext.User to context.User");
                context = new AuthorizationHandlerContext(
                    context.Requirements,
                    user,
                    context.Resource
                );
            }

            // 简化逻辑：直接调用 AuthorizeHandleAsync，让框架决定是否授权
            Console.WriteLine("[JwtHandler.HandleAsync] Calling AuthorizeHandleAsync without custom checks");
            await AuthorizeHandleAsync(context);
            Console.WriteLine("[JwtHandler.HandleAsync] AuthorizeHandleAsync completed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[JwtHandler.HandleAsync] EXCEPTION: {ex.Message}");
            Console.WriteLine($"[JwtHandler.HandleAsync] STACK TRACE: {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// 授权判断逻辑，授权通过返回 true，否则返回 false.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public override async Task<bool> PipelineAsync(AuthorizationHandlerContext context, DefaultHttpContext httpContext)
    {
        Console.WriteLine("[JwtHandler.PipelineAsync] Start");

        // 日志：调试 JWT 认证
        var token = httpContext.Request.Headers["Authorization"].ToString();
        Console.WriteLine($"[JWT] Request Path: {httpContext.Request.Path}");
        Console.WriteLine($"[JWT] Authorization Header: {token}");
        Console.WriteLine($"[JWT] User Claims: {string.Join(", ", context.User.Claims.Select(c => $"{c.Type}={c.Value}"))}");

        // 此处已经自动验证 Jwt Token的有效性了，无需手动验证
        var checkResult = await CheckAuthorzieAsync(httpContext);
        Console.WriteLine($"[JwtHandler.PipelineAsync] CheckAuthorzieAsync Result: {checkResult}");
        return checkResult;
    }

    /// <summary>
    /// 检查权限.
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    private static async Task<bool> CheckAuthorzieAsync(DefaultHttpContext httpContext)
    {
        Console.WriteLine("[JwtHandler.CheckAuthorzieAsync] Start");

        // 管理员跳过判断
        var adminClaim = App.User.FindFirst(ClaimConst.CLAINMADMINISTRATOR)?.Value;
        Console.WriteLine($"[JwtHandler.CheckAuthorzieAsync] Administrator Claim: {adminClaim}");
        if (adminClaim == ((int)AccountType.Administrator).ToString())
        {
            Console.WriteLine("[JwtHandler.CheckAuthorzieAsync] User is Administrator - Authorized");
            return true;
        }

        // 路由名称
        var routeName = httpContext.Request.Path.Value[1..].Replace("/", ":");
        if (httpContext.Request.Path.StartsWithSegments("/api"))
            routeName = httpContext.Request.Path.Value[5..].Replace("/", ":");
        Console.WriteLine($"[JwtHandler.CheckAuthorzieAsync] Route Name: {routeName}");

        // 默认路由(获取登录用户信息)
        var defalutRoute = new List<string>()
        {
            "oauth:CurrentUser"
        };

        if (defalutRoute.Contains(routeName))
        {
            Console.WriteLine("[JwtHandler.CheckAuthorzieAsync] Route is in default route list - Authorized");
            return true;
        }

        // 获取用户权限集合（按钮或API接口）
        //var permissionList = await App.GetService<ISysMenuService>().GetLoginPermissionList(userManager.UserId);

        // 检查授权
        //return permissionList.Contains(routeName);
        Console.WriteLine("[JwtHandler.CheckAuthorzieAsync] Default authorization - Authorized");
        return true;
    }
}
