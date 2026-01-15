using System.Net.NetworkInformation;
using System.Text;
using Poxiao.DataEncryption;
using Poxiao.JsonSerialization;
using Poxiao.RemoteRequest.Extensions;
using Microsoft.AspNetCore.Http;

namespace Poxiao.Infrastructure.Security;

/// <summary>
/// 网络操作
/// 版 本：V3.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao.
/// </summary>
[SuppressSniffer]
public static class NetHelper
{
    #region Ip(获取Ip)

    /// <summary>
    /// 获取Ip.
    /// </summary>
    public static string Ip
    {
        get
        {
            string result = string.Empty;
            if (App.HttpContext != null)
                result = GetWebClientIp();
            result = result.Equals("::1") ? "127.0.0.1" : result;
            result = result.Replace(":", string.Empty).Replace("ffff", string.Empty);
            return result;
        }
    }

    /// <summary>
    /// 请求Url.
    /// </summary>
    public static string Url
    {
        get
        {
            return new StringBuilder().Append(App.HttpContext?.Request?.Scheme).Append("://")
                .Append(App.HttpContext?.Request?.Host).Append(App.HttpContext?.Request?.PathBase)
                .Append(App.HttpContext?.Request?.Path).Append(App.HttpContext?.Request?.QueryString).ToString();
        }
    }

    /// <summary>
    /// 通过用户信息和租户id获取token.
    /// </summary>
    /// <param name="userId">用户id.</param>
    /// <param name="account">用户账号.</param>
    /// <param name="realName">用户名.</param>
    /// <param name="isAdministrator">是否管理员.</param>
    /// <param name="tenantId">租户id.</param>
    /// <param name="isPost">是否用户post请求.</param>
    /// <returns></returns>
    public static string GetToken(string userId, string account, string realName, int isAdministrator, string tenantId, bool isPost = true)
    {
        var token = JWTEncryption.Encrypt(
              new Dictionary<string, object>
              {
                    { ClaimConst.CLAINMUSERID, userId },
                    { ClaimConst.CLAINMACCOUNT, account },
                    { ClaimConst.CLAINMREALNAME, realName },
                    { ClaimConst.CLAINMADMINISTRATOR, isAdministrator },
                    { ClaimConst.TENANTID, tenantId },
              }, 900);
        return isPost ? string.Format("Bearer {0}", token) : string.Format("Bearer%20{0}", token);
    }

    /// <summary>
    /// 得到客户端IP地址.
    /// </summary>
    /// <returns></returns>
    private static string GetWebClientIp()
    {
        HttpContext httpContext = App.HttpContext;
        string ip = httpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(ip))
        {
            ip = App.HttpContext.Connection.RemoteIpAddress.ToString();
        }

        return ip;
    }

    #endregion

    #region 获取mac地址

    /// <summary>
    /// 返回描述本地计算机上的网络接口的对象(网络接口也称为网络适配器).
    /// </summary>
    /// <returns></returns>
    public static NetworkInterface[] NetCardInfo()
    {
        return NetworkInterface.GetAllNetworkInterfaces();
    }

    /// <summary>
    /// 通过NetworkInterface读取网卡Mac.
    /// </summary>
    /// <returns></returns>
    public static List<string> GetMacByNetworkInterface()
    {
        List<string> macs = new List<string>();
        NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
        foreach (NetworkInterface ni in interfaces)
        {
            macs.Add(ni.GetPhysicalAddress().ToString());
        }

        return macs;
    }

    #endregion

    #region Ip城市(获取Ip城市)

    /// <summary>
    /// 获取IP地址信息.
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    public static async Task<string> GetLocation(string ip)
    {
        string res = string.Empty;
        try
        {
            string url = string.Format("https://sp0.baidu.com/8aQDcjqpAAV3otqbppnN2DJv/api.php?query={0}&resource_id=6006&ie=utf8&oe=gbk&format=json", ip);
            string result = await url.GetAsStringAsync();
            dataone? resJson = result.ToObject<obj>().data.FirstOrDefault();
            string data = resJson?.location;
            res = data != null ? data.Split(' ')[0] : "本地局域网";
        }
        catch
        {
            res = string.Empty;
        }

        return res;
    }

    /// <summary>
    /// 百度接口.
    /// </summary>
    public class obj
    {
        public List<dataone>? data { get; set; }
    }

    public class dataone
    {
        public string? location { get; set; }
    }

    #endregion
}