using Microsoft.AspNetCore.Authorization;
using Poxiao.Infrastructure.Configuration;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.Systems.Entitys.System;
using Microsoft.AspNetCore.Mvc;
using Poxiao.Infrastructure.Security;
using SqlSugar;
using System.Text;

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
        var version = "1.0.0";
        return await Task.FromResult(new { env, dbOptions, version });
    }
    #endregion
}