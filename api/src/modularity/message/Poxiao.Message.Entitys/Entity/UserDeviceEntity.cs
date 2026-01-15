using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Message.Entitys.Entity;

/// <summary>
/// 个推用户表.
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_USER_DEVICE")]
public class UserDeviceEntity : CLDEntityBase
{
    /// <summary>
    /// 设备id.
    /// </summary>
    [SugarColumn(ColumnName = "F_CLIENTID")]
    public string? ClientId { get; set; }

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
