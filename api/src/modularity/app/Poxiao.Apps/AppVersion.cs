using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.Infrastructure.Configuration;
using Poxiao.Infrastructure.Options;
using Poxiao.Infrastructure.Security;
using Poxiao.Systems.Entitys.System;
using SqlSugar;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Poxiao.Apps;

/// <summary>
/// App版本信息
/// 版 本：V3.3
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2022-04-07.
/// </summary>
[ApiDescriptionSettings(Tag = "App", Name = "Version", Order = 806)]
[Route("api/App/[controller]")]
public class AppVersion : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<SysConfigEntity> _repository; // 系统设置

    /// <summary>
    /// 原始数据库.
    /// </summary>
    private readonly SqlSugarScope _db;

    /// <summary>
    /// 代理蒲公英 app/check 用的 HttpClient（静态复用，避免端口耗尽）.
    /// </summary>
    private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

    /// <summary>
    /// 构造.
    /// </summary>
    /// <param name="sysConfigRepository"></param>
    /// <param name="context"></param>
    public AppVersion(
        ISqlSugarRepository<SysConfigEntity> repository,
        ISqlSugarClient context)
    {
        _repository = repository;
        _db = (SqlSugarScope)context;
    }

    #region Get

    /// <summary>
    /// 版本信息.
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetInfo()
    {
        SysConfigEntity? data = new SysConfigEntity();

        if (KeyVariable.MultiTenancy)
        {
            data = await _db.Queryable<SysConfigEntity>().Where(x => x.Category.Equals("SysConfig") && x.Key == "sysVersion").FirstAsync();
        }
        else
        {
            data = await _repository.AsQueryable().Where(x => x.Category.Equals("SysConfig") && x.Key == "sysVersion").FirstAsync();
        }

        return new { sysVersion = data.Value };
    }

    /// <summary>
    /// 获取详细信息.
    /// </summary>
    /// <returns></returns>
    [HttpGet("/api/ver/info")]
    [AllowAnonymous]
    public async Task<dynamic> GetDetailInfoAsync()
    {
        // 获取使用的环境变量
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        // 获取配置文件的数据库信息
        var dbOptions = App.GetOptions<ConnectionStringsOptions>();
        var version = "1.1.0";
        return await Task.FromResult(new { env, dbOptions, version });
    }

    /// <summary>
    /// App 更新检测（移动端）。后端代理蒲公英 app/check，避免在客户端内置账号级 Key。
    /// 路由固定 /api/app/version，对应移动端 utils/update.js 的 UPDATE_SOURCE='backend'。
    /// 蒲公英 Key 从配置 Pgyer:ApiKey / Pgyer:AppKey（或环境变量 Pgyer__ApiKey / Pgyer__AppKey）读取，不入库、不下发。
    /// </summary>
    /// <param name="platform">客户端平台（android/ios），可空.</param>
    /// <param name="version">客户端 versionName，可空.</param>
    /// <param name="versionCode">客户端 versionCode，用于与最新版本比较.</param>
    /// <returns>{ hasUpdate, latestVersion, latestVersionCode, downloadUrl, forceUpdate, updateLog }.</returns>
    [HttpGet("/api/app/version")]
    [AllowAnonymous]
    public async Task<dynamic> CheckAppUpdate(string? platform = null, string? version = null, int versionCode = 0)
    {
        var pgyer = App.GetConfig<PgyerOptions>("Pgyer", true);
        if (pgyer == null || string.IsNullOrWhiteSpace(pgyer.ApiKey) || string.IsNullOrWhiteSpace(pgyer.AppKey))
        {
            // 未配置服务端蒲公英 Key 时，按"无更新"返回，不阻断 App 启动
            return new { hasUpdate = false };
        }

        try
        {
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["_api_key"] = pgyer.ApiKey,
                ["appKey"] = pgyer.AppKey,
            });
            using var resp = await _http.PostAsync("https://www.pgyer.com/apiv2/app/check", content);
            var body = await resp.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            if (!root.TryGetProperty("code", out var codeEl) || codeEl.GetInt32() != 0
                || !root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
            {
                return new { hasUpdate = false };
            }

            var latestVersion = GetJsonStr(data, "buildVersion");
            var latestCode = ParseIntSafe(GetJsonStr(data, "buildVersionNo"));
            var downloadUrl = GetJsonStr(data, "downloadURL");
            var updateLog = GetJsonStr(data, "buildUpdateDescription");

            var hasUpdate = (latestCode > 0 && versionCode > 0)
                ? latestCode > versionCode
                : string.CompareOrdinal(latestVersion, version ?? string.Empty) > 0;

            if (!hasUpdate)
            {
                return new { hasUpdate = false };
            }

            return new
            {
                hasUpdate = true,
                latestVersion,
                latestVersionCode = latestCode,
                downloadUrl,
                forceUpdate = false,
                updateLog,
            };
        }
        catch
        {
            // 网络/解析异常按"无更新"处理，避免影响 App 启动
            return new { hasUpdate = false };
        }
    }

    private static string GetJsonStr(JsonElement obj, string name)
        => obj.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.String ? el.GetString() ?? string.Empty : string.Empty;

    private static int ParseIntSafe(string s)
        => int.TryParse(s, out var n) ? n : 0;
    #endregion
}