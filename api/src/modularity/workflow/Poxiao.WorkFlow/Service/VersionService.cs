using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Poxiao.DynamicApiController;
using Poxiao.DependencyInjection;
using System.IO;
using System.Text.RegularExpressions;

namespace Poxiao.WorkFlow;

/// <summary>
/// 版本服务实现
/// </summary>
[ApiDescriptionSettings(Tag = "WorkFlow", Name = "Version", Order = 1)]
[Route("api/workflow/v1/[controller]")]
public class VersionService : IDynamicApiController, ITransient
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public VersionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 获取版本信息
    /// </summary>
    public Task<dynamic> GetVersionAsync()
    {
        var apiVersion = GetApiVersion();
        var webVersion = GetWebVersion();
        var isCompatible = apiVersion == webVersion;

        return Task.FromResult<dynamic>(new
        {
            apiVersion,
            webVersion,
            isCompatible,
            message = isCompatible ? null : "检测到新版本，请刷新页面获取最新版本。"
        });
    }

    private string GetApiVersion()
    {
        try
        {
            var appVersionFile = Path.Combine(AppContext.BaseDirectory, "Version.txt");
            if (File.Exists(appVersionFile))
            {
                return File.ReadAllText(appVersionFile).Trim();
            }
        }
        catch { }
        return "1.0.0";
    }

    private string GetWebVersion()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var webVersion = httpContext.Request.Headers["X-Web-Version"].FirstOrDefault();
            if (!string.IsNullOrEmpty(webVersion))
                return webVersion;
        }
        return "1.0.1";
    }
}
