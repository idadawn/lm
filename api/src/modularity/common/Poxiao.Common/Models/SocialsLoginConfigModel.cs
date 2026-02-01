using NPOI.SS.Formula.Functions;

namespace Poxiao.Infrastructure.Models;

/// <summary>
/// 通用登录配置-第三方登录配置.
/// </summary>
public class SocialsLoginConfigModel
{
    /// <summary>
    /// 是否跳转.
    /// </summary>
    public bool Redirect = false;

    /// <summary>
    /// 跳转URL地址.
    /// </summary>
    public string Url;

    /// <summary>
    /// 跳转URL地址.
    /// </summary>
    public string RedirectRrl;

    /// <summary>
    /// 跳转登录轮询票据参数名称.
    /// </summary>
    public string TicketParams;

    /// <summary>
    /// 第三方登录列表.
    /// </summary>
    public List<object> SocialsList;

}

/// <summary>
/// 轮询登录模型.
/// </summary>
public class SocialsLoginTicketModel
{
    /// <summary>
    /// 状态.
    /// </summary>
    public int Status = 2;

    /// <summary>
    /// 额外的值, 登录Token、第三方登录的ID.
    /// </summary>
    public string Value;

    /// <summary>
    /// 前端主题.
    /// </summary>
    public string Theme;

    /// <summary>
    /// 票据有效期, 时间戳.
    /// </summary>
    public long TicketTimeout;
}

public enum SocialsLoginTicketStatus
{
    /// <summary>
    /// 登录成功.
    /// </summary>
    Success = 1,

    /// <summary>
    /// 未登录.
    /// </summary>
    UnLogin = 2,

    /// <summary>
    /// 登录失败.
    /// </summary>
    ErrLogin = 3,

    /// <summary>
    /// 未绑定.
    /// </summary>
    UnBind = 4,

    /// <summary>
    /// 失效.
    /// </summary>
    Invalid = 5,

    /// <summary>
    /// 多租户.
    /// </summary>
    Multitenancy = 6,
}