using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Message.Entitys.Entity;

/// <summary>
/// 消息连接
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_MESSAGE_SHORT_LINK")]
public class MessageShortLinkEntity : CLDEntityBase
{
    /// <summary>
    /// 短链接.
    /// </summary>
    [SugarColumn(ColumnName = "F_SHORTLINK")]
    public string? ShortLink { get; set; }

    /// <summary>
    /// PC端链接.
    /// </summary>
    [SugarColumn(ColumnName = "F_REALPCLINK")]
    public string? RealPcLink { get; set; }

    /// <summary>
    /// App端链接.
    /// </summary>
    [SugarColumn(ColumnName = "F_REALAPPLINK")]
    public string? RealAppLink { get; set; }

    /// <summary>
    /// 内容.
    /// </summary>
    [SugarColumn(ColumnName = "F_BODYTEXT")]
    public string? BodyText { get; set; }

    /// <summary>
    /// 是否点击后失效.
    /// </summary>
    [SugarColumn(ColumnName = "F_ISUSED")]
    public int? IsUsed { get; set; }

    /// <summary>
    /// 点击次数.
    /// </summary>
    [SugarColumn(ColumnName = "F_CLICKNUM")]
    public int? ClickNum { get; set; }

    /// <summary>
    /// 失效次数.
    /// </summary>
    [SugarColumn(ColumnName = "F_UNABLENUM")]
    public int? UnableNum { get; set; }

    /// <summary>
    /// 失效时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_UNABLETIME")]
    public DateTime? UnableTime { get; set; }

    /// <summary>
    /// 用户id.
    /// </summary>
    [SugarColumn(ColumnName = "F_USERID")]
    public string? UserId { get; set; }

    /// <summary>
    /// 租户id.
    /// </summary>
    [SugarColumn(ColumnName = "F_TENANTID")]
    public string? TenantId { get; set; }
}
