using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Systems.Entitys.Permission;

/// <summary>
/// 用户第三方登录 .
/// </summary>
[SugarTable("Base_TenantSocials")]
public class TenantSocialsEntity : OCEntityBase
{
    /// <summary>
    /// 用户id.
    /// </summary>
    [SugarColumn(ColumnName = "F_UserId")]
    public string UserId { get; set; }

    /// <summary>
    /// 用户账号.
    /// </summary>
    [SugarColumn(ColumnName = "F_Account")]
    public string Account { get; set; }

    /// <summary>
    /// 用户账号名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_AccountName")]
    public string AccountName { get; set; }

    /// <summary>
    /// 第三方类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_SocialType")]
    public string SocialType { get; set; }

    /// <summary>
    /// 第三方账号id.
    /// </summary>
    [SugarColumn(ColumnName = "F_SocialId")]
    public string SocialId { get; set; }

    /// <summary>
    /// 第三方账号.
    /// </summary>
    [SugarColumn(ColumnName = "F_SocialName")]
    public string SocialName { get; set; }

    /// <summary>
    /// 租户id.
    /// </summary>
    [SugarColumn(ColumnName = "F_TenantId")]
    public string TenantId { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "F_Description")]
    public string Description { get; set; }

    /// <summary>
    /// 获取或设置 删除标志.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteMark", ColumnDescription = "删除标志")]
    public virtual int? DeleteMark { get; set; }

    /// <summary>
    /// 获取或设置 删除时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteTime", ColumnDescription = "删除时间")]
    public virtual DateTime? DeleteTime { get; set; }

    /// <summary>
    /// 获取或设置 删除用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteUserId", ColumnDescription = "删除用户")]
    public virtual string DeleteUserId { get; set; }

}