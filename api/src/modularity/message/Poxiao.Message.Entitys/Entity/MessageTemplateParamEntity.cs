using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Message.Entitys.Entity;

/// <summary>
/// 消息模板参数
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_MESSAGE_TEMPLATE_PARAM")]
public class MessageTemplateParamEntity : CLDEntityBase
{
    /// <summary>
    /// 参数名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FIELDNAME")]
    public string? FieldName { get; set; }

    /// <summary>
    /// 模板id.
    /// </summary>
    [SugarColumn(ColumnName = "F_TEMPLATEID")]
    public string? TemplateId { get; set; }

    /// <summary>
    /// 参数.
    /// </summary>
    [SugarColumn(ColumnName = "F_FIELD")]
    public string? Field { get; set; }

    /// <summary>
    /// 租户id.
    /// </summary>
    [SugarColumn(ColumnName = "F_TENANTID")]
    public string? TenantId { get; set; }
}
