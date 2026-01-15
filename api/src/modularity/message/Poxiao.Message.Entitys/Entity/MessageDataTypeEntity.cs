using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Message.Entitys.Entity;

/// <summary>
/// 消息字典配置
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_MESSAGE_DATA_TYPE")]
public class MessageDataTypeEntity : CLDEntityBase
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
    /// 租户id.
    /// </summary>
    [SugarColumn(ColumnName = "F_TENANTID")]
    public string? TenantId { get; set; }
}
