namespace Poxiao.Extras.CollectiveOAuth.Cache;

/// <summary>
/// 授权状态默认缓存.
/// </summary>
public class DefaultAuthStateCache : IAuthStateCache
{
    /// <summary>
    /// 默认缓存前缀.
    /// </summary>
    private static string defaultCachePrefix = "CollectiveOAuth_Status_";

    /// <summary>
    /// 保存缓存.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="value">值.</param>
    public void cache(string key, string value)
    {
        HttpRuntimeCache.Set($"{defaultCachePrefix}{key}", value);
    }

    /// <summary>
    /// 保存缓存.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="value">值.</param>
    /// <param name="timeout">过期时间戳.</param>
    public void cache(string key, string value, long timeout)
    {
        HttpRuntimeCache.Set($"{defaultCachePrefix}{key}", value, timeout);
    }

    /// <summary>
    /// 是否存在.
    /// </summary>
    /// <param name="key">键.</param>
    /// <returns>true or false.</returns>
    public bool containsKey(string key)
    {
        var cacheObj = HttpRuntimeCache.Get($"{defaultCachePrefix}{key}");
        if (cacheObj != null)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 取值缓存.
    /// </summary>
    /// <param name="key">键.</param>
    /// <returns></returns>
    public string get(string key)
    {
        var cacheObj = HttpRuntimeCache.Get($"{defaultCachePrefix}{key}");
        if (cacheObj != null)
        {
            return Convert.ToString(cacheObj);
        }
        else
        {
            return null;
        }
    }
}