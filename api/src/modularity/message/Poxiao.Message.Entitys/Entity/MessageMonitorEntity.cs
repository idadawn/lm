using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Message.Entitys.Entity;

/// <summary>
/// 消息监控
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_MESSAGE_MONITOR")]
public class MessageMonitorEntity : CLDEntityBase
{
    /// <summary>
    /// 账号id.
    /// </summary>
    [SugarColumn(ColumnName = "F_ACCOUNTID")]
    public string? AccountId { get; set; }

    /// <summary>
    /// 账号名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_ACCOUNTNAME")]
    public string? AccountName { get; set; }

    /// <summary>
    /// 账号编码.
    /// </summary>
    [SugarColumn(ColumnName = "F_ACCOUNTCODE")]
    public string? AccountCode { get; set; }

    /// <summary>
    /// 消息类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_MESSAGETYPE")]
    public string? MessageType { get; set; }

    /// <summary>
    /// 消息来源.
    /// </summary>
    [SugarColumn(ColumnName = "F_MESSAGESOURCE")]
    public string? MessageSource { get; set; }

    /// <summary>
    /// 发送时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_SENDTIME")]
    public DateTime? SendTime { get; set; }

    /// <summary>
    /// 消息模板id.
    /// </summary>
    [SugarColumn(ColumnName = "F_MESSAGETEMPLATEID")]
    public string? MessageTemplateId { get; set; }

    /// <summary>
    /// 标题.
    /// </summary>
    [SugarColumn(ColumnName = "F_TITLE")]
    public string? Title { get; set; }

    /// <summary>
    /// 接收人.
    /// </summary>
    [SugarColumn(ColumnName = "F_RECEIVEUSER")]
    public string? ReceiveUser { get; set; }

    /// <summary>
    /// 内容.
    /// </summary>
    [SugarColumn(ColumnName = "F_CONTENT")]
    public string? Content { get; set; }

    /// <summary>
    /// 租户id.
    /// </summary>
    [SugarColumn(ColumnName = "F_TENANTID")]
    public string? TenantId { get; set; }
}
