namespace Poxiao.Extras.CollectiveOAuth.Utils;

/// <summary>
/// 网址构造器.
/// </summary>
public class UrlBuilder
{
    private Dictionary<string, object> _paramDic = new Dictionary<string, object>();
    private string _baseUrl;

    /// <summary>
    /// 初始化一个<see cref="UrlBuilder"/>类型的新实例.
    /// </summary>
    private UrlBuilder()
    {

    }

    /// <summary>
    /// 从基本路径.
    /// </summary>
    /// <param name="baseUrl">基础路径.</param>
    /// <returns>new {@code UrlBuilder}</returns>
    public static UrlBuilder fromBaseUrl(string baseUrl)
    {
        UrlBuilder builder = new UrlBuilder();
        builder._baseUrl = baseUrl;
        return builder;
    }

    /// <summary>
    /// 添加参数.
    /// </summary>
    /// <param name="key">参数名称.</param>
    /// <param name="value">参数值.</param>
    /// <returns>this UrlBuilder.</returns>
    public UrlBuilder queryParam(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new Exception("参数名不能为空");
        }

        string valueAsString = value != null ? Convert.ToString(value) : null;
        this._paramDic.Add(key, valueAsString);
        return this;
    }

    /// <summary>
    /// 构造url.
    /// </summary>
    /// <returns>url.</returns>
    public string build()
    {
        return this.build(false);
    }

    /// <summary>
    /// 构造url.
    /// </summary>
    /// <param name="encode">转码.</param>
    /// <returns>url.</returns>
    public string build(bool encode)
    {
        if (this._paramDic.Count == 0 || this._paramDic == null)
        {
            return this._baseUrl;
        }

        string baseUrl = this.appendIfNotContain(this._baseUrl, "?", "&");
        string paramString = GlobalAuthUtil.parseMapToString(this._paramDic);
        return baseUrl + paramString;
    }

    /// <summary>
    /// 如果给定字符串{@code str}中不包含{@code appendStr}，则在{@code str}后追加{@code appendStr}:
    /// 如果已包含{@code appendStr}，则在{@code str}后追加{@code otherwise}.
    /// </summary>
    /// <param name="str">给定的字符串.</param>
    /// <param name="appendStr">需要追加的内容.</param>
    /// <param name="otherwise">当{@code appendStr}不满足时追加到{@code str}后的内容.</param>
    /// <returns>追加后的字符串.</returns>
    public string appendIfNotContain(string str, string appendStr, string otherwise)
    {
        if (string.IsNullOrWhiteSpace(str) || string.IsNullOrWhiteSpace(appendStr))
        {
            return str;
        }

        if (str.Contains(appendStr))
        {
            return str + otherwise;
        }

        return str + appendStr;
    }
}