using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Message.Entitys.Entity;

/// <summary>
/// 微信公众号用户
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_MESSAGE_WECHAT_USER")]
public class MessageWechatUserEntity : CLDEntityBase
{
    /// <summary>
    /// 公众号id.
    /// </summary>
    [SugarColumn(ColumnName = "F_GZHID")]
    public string? GzhId { get; set; }

    /// <summary>
    /// 用户id.
    /// </summary>
    [SugarColumn(ColumnName = "F_USERID")]
    public string? UserId { get; set; }

    /// <summary>
    /// 公众号用户id.
    /// </summary>
    [SugarColumn(ColumnName = "F_OPENID")]
    public string? OpenId { get; set; }

    /// <summary>
    /// 是否关注.
    /// </summary>
    [SugarColumn(ColumnName = "F_CLOSEMARK")]
    public int? CloseMark { get; set; }

    /// <summary>
    /// 租户id.
    /// </summary>
    [SugarColumn(ColumnName = "F_TENANTID")]
    public string? TenantId { get; set; }
}
