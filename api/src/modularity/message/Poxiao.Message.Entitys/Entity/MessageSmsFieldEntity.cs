using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Message.Entitys.Entity;

/// <summary>
/// 短信变量
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_MESSAGE_SMS_FIELD")]
public class MessageSmsFieldEntity : CLDEntityBase
{
    /// <summary>
    /// 参数id.
    /// </summary>
    [SugarColumn(ColumnName = "F_FIELDID")]
    public string? FieldId { get; set; }

    /// <summary>
    /// 短信变量.
    /// </summary>
    [SugarColumn(ColumnName = "F_SMSFIELD")]
    public string? SmsField { get; set; }

    /// <summary>
    /// 模板id.
    /// </summary>
    [SugarColumn(ColumnName = "F_TEMPLATEID")]
    public string? TemplateId { get; set; }

    /// <summary>
    /// 参数.
    /// </summary>
    [SugarColumn(ColumnName = "F_Field")]
    public string? Field { get; set; }

    /// <summary>
    /// 租户id.
    /// </summary>
    [SugarColumn(ColumnName = "F_TENANTID")]
    public string? TenantId { get; set; }
}
