using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Message.Entitys.Entity;

/// <summary>
/// 消息账号配置
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_MESSAGE_ACCOUNT_CONFIG")]
public class MessageAccountEntity : CLDEntityBase
{
    /// <summary>
    /// 配置类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_TYPE")]
    public string? Type { get; set; }

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
    /// 发件人昵称.
    /// </summary>
    [SugarColumn(ColumnName = "F_ADDRESSORNAME")]
    public string? AddressorName { get; set; }

    /// <summary>
    /// SMTP服务器.
    /// </summary>
    [SugarColumn(ColumnName = "F_SMTPSERVER")]
    public string? SmtpServer { get; set; }

    /// <summary>
    /// SMTP端口.
    /// </summary>
    [SugarColumn(ColumnName = "F_SMTPPORT")]
    public int? SmtpPort { get; set; }

    /// <summary>
    /// SSL安全链接.
    /// </summary>
    [SugarColumn(ColumnName = "F_SSLLINK")]
    public string? SslLink { get; set; }

    /// <summary>
    /// SMTP用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_SMTPUSER")]
    public string? SmtpUser { get; set; }

    /// <summary>
    /// SMTP密码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SMTPPASSWORD")]
    public string? SmtpPassword { get; set; }

    /// <summary>
    /// 渠道.
    /// </summary>
    [SugarColumn(ColumnName = "F_CHANNEL")]
    public string? Channel { get; set; }

    /// <summary>
    /// 短信签名.
    /// </summary>
    [SugarColumn(ColumnName = "F_SMSSIGNATURE")]
    public string? SmsSignature { get; set; }

    /// <summary>
    /// 应用ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_APPID")]
    public string? AppId { get; set; }

    /// <summary>
    /// 应用Secret.
    /// </summary>
    [SugarColumn(ColumnName = "F_APPSECRET")]
    public string? AppSecret { get; set; }

    /// <summary>
    /// EndPoint（阿里云）.
    /// </summary>
    [SugarColumn(ColumnName = "F_ENDPOINT")]
    public string? EndPoint { get; set; }

    /// <summary>
    /// SDK AppID（腾讯云）.
    /// </summary>
    [SugarColumn(ColumnName = "F_SDKAPPID")]
    public string? SdkAppId { get; set; }

    /// <summary>
    /// AppKey（腾讯云）.
    /// </summary>
    [SugarColumn(ColumnName = "F_APPKEY")]
    public string? AppKey { get; set; }

    /// <summary>
    /// 地域域名（腾讯云）.
    /// </summary>
    [SugarColumn(ColumnName = "F_ZONENAME")]
    public string? ZoneName { get; set; }

    /// <summary>
    /// 地域参数（腾讯云）.
    /// </summary>
    [SugarColumn(ColumnName = "F_ZONEPARAM")]
    public string? ZoneParam { get; set; }

    /// <summary>
    /// 企业id.
    /// </summary>
    [SugarColumn(ColumnName = "F_ENTERPRISEID")]
    public string? EnterpriseId { get; set; }

    /// <summary>
    /// AgentID.
    /// </summary>
    [SugarColumn(ColumnName = "F_AGENTID")]
    public string? AgentId { get; set; }

    /// <summary>
    /// WebHook类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_WEBHOOKTYPE")]
    public string? WebhookType { get; set; }

    /// <summary>
    /// WebHook地址.
    /// </summary>
    [SugarColumn(ColumnName = "F_WEBHOOKADDRESS")]
    public string? WebhookAddress { get; set; }

    /// <summary>
    /// 认证类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_APPROVETYPE")]
    public string? ApproveType { get; set; }

    /// <summary>
    /// Bearer令牌.
    /// </summary>
    [SugarColumn(ColumnName = "F_BEARER")]
    public string? Bearer { get; set; }

    /// <summary>
    /// 用户名（基本认证）.
    /// </summary>
    [SugarColumn(ColumnName = "F_USERNAME")]
    public string? UserName { get; set; }

    /// <summary>
    /// 密码（基本认证）.
    /// </summary>
    [SugarColumn(ColumnName = "F_PASSWORD")]
    public string? Password { get; set; }

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
