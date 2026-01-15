using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Message.Entitys.Entity;

/// <summary>
/// 消息发送模板配置
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_MESSAGE_SEND_TEMPLATE")]
public class MessageSendTemplateEntity : CLDEntityBase
{
    /// <summary>
    /// 消息发送配置id.
    /// </summary>
    [SugarColumn(ColumnName = "F_SENDCONFIGID")]
    public string? SendConfigId { get; set; }

    /// <summary>
    /// 消息模板id.
    /// </summary>
    [SugarColumn(ColumnName = "F_TEMPLATEID")]
    public string? TemplateId { get; set; }

    /// <summary>
    /// 消息类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_MESSAGETYPE")]
    public string? MessageType { get; set; }

    /// <summary>
    /// 账号配置id.
    /// </summary>
    [SugarColumn(ColumnName = "F_ACCOUNTCONFIGID")]
    public string? AccountConfigId { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }

    /// <summary>
    /// 说明.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string? Description { get; set; }

    /// <summary>
    /// 租户id.
    /// </summary>
    [SugarColumn(ColumnName = "F_TENANTID")]
    public string? TenantId { get; set; }
}
