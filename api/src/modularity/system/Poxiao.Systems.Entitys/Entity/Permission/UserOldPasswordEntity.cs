using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Systems.Entitys.Entity.Permission;

/// <summary>
/// 用户旧密码记录表.
/// </summary>
[SugarTable("BASE_USER_OLD_PASSWORD")]
public class UserOldPasswordEntity : OEntityBase<string>
{
    /// <summary>
    /// 用户ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_USERID")]
    public string UserId { get; set; }

    /// <summary>
    /// 用户ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Account")]
    public string Account { get; set; }

    /// <summary>
    /// 账户.
    /// </summary>
    [SugarColumn(ColumnName = "F_OldPassword")]
    public string OldPassword { get; set; }

    /// <summary>
    /// 秘钥.
    /// </summary>
    [SugarColumn(ColumnName = "F_Secretkey")]
    public string Secretkey { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_CreatorTime")]
    public DateTime CreatorTime { get; set; }

    /// <summary>
    /// 租户ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_TenantId")]
    public string TenantId { get; set; }
}
