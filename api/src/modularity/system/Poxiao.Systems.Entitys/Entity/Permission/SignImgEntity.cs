using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Systems.Entitys.Entity.Permission;

/// <summary>
/// 用户签名类.
/// </summary>
[SugarTable("BASE_SIGNIMG")]
public class SignImgEntity : CLEntityBase
{
    /// <summary>
    /// 签名.
    /// </summary>
    [SugarColumn(ColumnName = "F_SIGNIMG")]
    public string SignImg { get; set; }

    /// <summary>
    /// 是否默认(0:否，1：是).
    /// </summary>
    [SugarColumn(ColumnName = "F_ISDEFAULT")]
    public int? IsDefault { get; set; }

    /// <summary>
    /// 获取或设置 删除标志.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteMark", ColumnDescription = "删除标志")]
    public int? DeleteMark { get; set; }

    /// <summary>
    /// 获取或设置 删除时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteTime", ColumnDescription = "删除时间")]
    public DateTime? DeleteTime { get; set; }

    /// <summary>
    /// 获取或设置 删除用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteUserId", ColumnDescription = "删除用户")]
    public string DeleteUserId { get; set; }
}
