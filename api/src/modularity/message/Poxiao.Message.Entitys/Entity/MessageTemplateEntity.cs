using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Message.Entitys.Entity;

/// <summary>
/// 消息模板配置
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_MESSAGE_TEMPLATE_CONFIG")]
public class MessageTemplateEntity : CLDEntityBase
{
    /// <summary>
    /// 名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FULLNAME")]
    public string? FullName { get; set; }

    /// <summary>
    /// 编码.
    /// </summary>
    [SugarColumn(ColumnName = "F_ENCODE")]
    public string? EnCode { get; set; }

    /// <summary>
    /// 模板类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_TEMPLATETYPE")]
    public string? TemplateType { get; set; }

    /// <summary>
    /// 消息来源.
    /// </summary>
    [SugarColumn(ColumnName = "F_MESSAGESOURCE")]
    public string? MessageSource { get; set; }

    /// <summary>
    /// 消息类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_MESSAGETYPE")]
    public string? MessageType { get; set; }

    /// <summary>
    /// 标题.
    /// </summary>
    [SugarColumn(ColumnName = "F_TITLE")]
    public string? Title { get; set; }

    /// <summary>
    /// 内容.
    /// </summary>
    [SugarColumn(ColumnName = "F_CONTENT")]
    public string? Content { get; set; }

    /// <summary>
    /// 模板编号.
    /// </summary>
    [SugarColumn(ColumnName = "F_TEMPLATECODE")]
    public string? TemplateCode { get; set; }

    /// <summary>
    /// 跳转方式.
    /// </summary>
    [SugarColumn(ColumnName = "F_WxSkip")]
    public string? WxSkip { get; set; }

    /// <summary>
    /// 小程序id.
    /// </summary>
    [SugarColumn(ColumnName = "F_XcxAppId")]
    public string? XcxAppId { get; set; }

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
