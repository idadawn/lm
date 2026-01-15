using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Message.Entitys.Entity;

/// <summary>
/// 消息发送记录配置
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_MESSAGE_SEND_RECORD")]
public class MessageSendRecordEntity : CLDEntityBase
{
    /// <summary>
    /// 发送配置id.
    /// </summary>
    [SugarColumn(ColumnName = "F_SENDCONFIGID")]
    public string? SendConfigId { get; set; }

    /// <summary>
    /// 消息来源.
    /// </summary>
    [SugarColumn(ColumnName = "F_MESSAGESOURCE")]
    public string? MessageSource { get; set; }

    /// <summary>
    /// 被引用id.
    /// </summary>
    [SugarColumn(ColumnName = "F_USEDID")]
    public string? UsedId { get; set; }

    /// <summary>
    /// 租户id.
    /// </summary>
    [SugarColumn(ColumnName = "F_TENANTID")]
    public string? TenantId { get; set; }
}
