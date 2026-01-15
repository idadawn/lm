using Poxiao.ConfigurableOptions;

namespace Poxiao.Infrastructure.Cache;

/// <summary>
/// 缓存配置.
/// </summary>
public class CacheOptions : IConfigurableOptions
{
    /// <summary>
    /// 缓存类型.
    /// </summary>
    public CacheType CacheType { get; set; }

    /// <summary>
    /// Redis配置.
    /// </summary>
    public string RedisConnectionString { get; set; }

    /// <summary>
    /// 服务器地址.
    /// </summary>
    public string ip { get; set; }

    /// <summary>
    /// 端口.
    /// </summary>
    public int port { get; set; }

    /// <summary>
    /// 密码.
    /// </summary>
    public string password { get; set; }
}

public enum CacheType
{
    /// <summary>
    /// 内存缓存.
    /// </summary>
    MemoryCache,

    /// <summary>
    /// Redis缓存.
    /// </summary>
    RedisCache
}