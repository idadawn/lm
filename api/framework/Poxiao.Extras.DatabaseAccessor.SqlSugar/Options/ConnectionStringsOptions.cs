using Microsoft.Extensions.Configuration;
using Poxiao.ConfigurableOptions;

namespace SqlSugar;

/// <summary>
/// 数据库配置.
/// </summary>
public sealed class ConnectionStringsOptions : IConfigurableOptions<ConnectionStringsOptions>
{
    /// <summary>
    /// 数据库集合
    /// </summary>
    public List<DbConnectionConfig> ConnectionConfigs { get; set; }

    public void PostConfigure(ConnectionStringsOptions options, IConfiguration configuration)
    {
        foreach (var dbConfig in options.ConnectionConfigs)
        {
            if (string.IsNullOrWhiteSpace(dbConfig.ConfigId?.ToString()))
                dbConfig.ConfigId = "default";
        }
    }
}

/// <summary>
/// 数据库连接配置.
/// </summary>
public sealed class DbConnectionConfig : ConnectionConfig
{
    /// <summary>
    /// 数据库名称.
    /// </summary>
    public string DBName { get; set; }

    /// <summary>
    /// 数据库地址.
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// 数据库端口号.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 账号.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 密码.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// 默认数据库连接字符串.
    /// </summary>
    public string DefaultConnection { get; set; }
}