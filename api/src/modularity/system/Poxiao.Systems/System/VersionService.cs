using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.Systems.Interfaces;
using System.IO;
using System.Text.RegularExpressions;

namespace Poxiao.Systems;

/// <summary>
/// 版本服务实现
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "Version", Order = 1)]
[Route("api/system/v1/[controller]")]
public class VersionService : IDynamicApiController, ITransient, IVersionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// 构造函数
    /// </summary>
    public VersionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 获取当前版本信息
    /// </summary>
    public Task<VersionOutput> GetVersionAsync()
    {
        var apiVersion = GetApiVersion();
        var webVersion = GetWebVersion();
        var isCompatible = apiVersion == webVersion;

        var output = new VersionOutput
        {
            ApiVersion = apiVersion,
            WebVersion = webVersion,
            IsCompatible = isCompatible,
            Message = isCompatible ? null : "检测到新版本，请刷新页面获取最新版本。"
        };

        return Task.FromResult(output);
    }

    /// <summary>
    /// 获取更新日志
    /// </summary>
    public Task<List<ChangelogItem>> GetChangelogAsync()
    {
        var changelog = ParseChangelog();
        return Task.FromResult(changelog);
    }

    private string GetApiVersion()
    {
        try
        {
            // 尝试从应用目录读取
            var appVersionFile = Path.Combine(AppContext.BaseDirectory, "Version.txt");
            if (File.Exists(appVersionFile))
            {
                return File.ReadAllText(appVersionFile).Trim();
            }
        }
        catch
        {
            // 忽略读取错误
        }

        return "1.0.0";
    }

    private string GetWebVersion()
    {
        // 从 HTTP 头获取 Web 版本
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var webVersion = httpContext.Request.Headers["X-Web-Version"].FirstOrDefault();
            if (!string.IsNullOrEmpty(webVersion))
            {
                return webVersion;
            }
        }

        // 从配置或环境变量获取
        return Poxiao.App.Configuration["WebVersion"] ?? "1.0.1";
    }

    private List<ChangelogItem> ParseChangelog()
    {
        var items = new List<ChangelogItem>();

        try
        {
            var changelogFile = Path.Combine(AppContext.BaseDirectory, "CHANGELOG.md");
            if (!File.Exists(changelogFile))
            {
                changelogFile = Path.Combine(AppContext.BaseDirectory, "..", "..", "CHANGELOG.md");
            }

            if (File.Exists(changelogFile))
            {
                var lines = File.ReadAllLines(changelogFile);
                ChangelogItem? currentItem = null;

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // 检查是否是版本行，如 ## [1.0.2] - 2026-03-18
                    if (line.StartsWith("## ["))
                    {
                        var match = Regex.Match(line, @"## \[([\d.]+)\] - (\d{4}-\d{2}-\d{2})");
                        if (match.Success)
                        {
                            currentItem = new ChangelogItem
                            {
                                Version = match.Groups[1].Value,
                                Date = match.Groups[2].Value
                            };
                            items.Add(currentItem);
                        }
                    }
                    else if (currentItem != null)
                    {
                        if (line.StartsWith("- "))
                        {
                            var content = line.Substring(2).Trim();
                            if (line.Contains("### Added"))
                            {
                                currentItem.Added.Add(content);
                            }
                            else if (line.Contains("### Fixed"))
                            {
                                currentItem.Fixed.Add(content);
                            }
                            else if (line.Contains("### Improved"))
                            {
                                currentItem.Improved.Add(content);
                            }
                            else if (currentItem.Added.Count == 0 && currentItem.Fixed.Count == 0 && currentItem.Improved.Count == 0)
                            {
                                // 默认添加到 Added
                                currentItem.Added.Add(content);
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // 忽略解析错误
        }

        return items;
    }
}
