using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Lab.Entity;

/// <summary>
/// 产品规格版本快照.
/// </summary>
[SugarTable("LAB_PRODUCT_SPEC_VERSION")]
[Tenant(ClaimConst.TENANTID)]
public class ProductSpecVersionEntity : CLDEntityBase
{
    /// <summary>
    /// 产品规格ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_PRODUCT_SPEC_ID", IsNullable = false, Length = 50)]
    public string ProductSpecId { get; set; }

    /// <summary>
    /// 版本号（从1开始递增）.
    /// </summary>
    [SugarColumn(ColumnName = "F_VERSION", IsNullable = false)]
    public int Version { get; set; }

    /// <summary>
    /// 版本名称（如：v1.0, v2.0）.
    /// </summary>
    [SugarColumn(ColumnName = "F_VERSION_NAME", Length = 100, IsNullable = true)]
    public string VersionName { get; set; }

    /// <summary>
    /// 版本说明（记录变更原因）.
    /// </summary>
    [SugarColumn(ColumnName = "F_VERSION_DESCRIPTION", Length = 500, IsNullable = true)]
    public string VersionDescription { get; set; }

    /// <summary>
    /// 是否为当前版本（1=是，0=否）.
    /// </summary>
    [SugarColumn(ColumnName = "F_IS_CURRENT", IsNullable = false)]
    public int IsCurrent { get; set; } = 0;

    /// <summary>
    /// 重写基类的 EnabledMark 属性，忽略数据库映射（表中无此字段）.
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public override int? EnabledMark { get; set; }

    /// <summary>
    /// 重写创建时间，映射到正确的数据库列名.
    /// </summary>
    [SugarColumn(ColumnName = "F_CREATOR_TIME", ColumnDescription = "创建时间", IsNullable = true)]
    public override DateTime? CreatorTime { get; set; }

    /// <summary>
    /// 重写创建用户ID，映射到正确的数据库列名.
    /// </summary>
    [SugarColumn(
        ColumnName = "F_CREATOR_USER_ID",
        ColumnDescription = "创建用户",
        IsNullable = true
    )]
    public override string CreatorUserId { get; set; }

    /// <summary>
    /// 重写修改时间，映射到正确的数据库列名.
    /// </summary>
    [SugarColumn(
        ColumnName = "F_LAST_MODIFY_TIME",
        ColumnDescription = "修改时间",
        IsNullable = true
    )]
    public override DateTime? LastModifyTime { get; set; }

    /// <summary>
    /// 重写修改用户ID，映射到正确的数据库列名.
    /// </summary>
    [SugarColumn(
        ColumnName = "F_LAST_MODIFY_USER_ID",
        ColumnDescription = "修改用户",
        IsNullable = true
    )]
    public override string LastModifyUserId { get; set; }

    /// <summary>
    /// 重写删除标记，映射到正确的数据库列名.
    /// </summary>
    [SugarColumn(ColumnName = "F_DELETE_MARK", ColumnDescription = "删除标志", IsNullable = true)]
    public override int? DeleteMark { get; set; }

    /// <summary>
    /// 重写删除时间，映射到正确的数据库列名.
    /// </summary>
    [SugarColumn(ColumnName = "F_DELETE_TIME", ColumnDescription = "删除时间", IsNullable = true)]
    public override DateTime? DeleteTime { get; set; }

    /// <summary>
    /// 重写删除用户ID，映射到正确的数据库列名.
    /// </summary>
    [SugarColumn(
        ColumnName = "F_DELETE_USER_ID",
        ColumnDescription = "删除用户",
        IsNullable = true
    )]
    public override string DeleteUserId { get; set; }

    /// <summary>
    /// 重写租户ID，映射到正确的数据库列名.
    /// </summary>
    [SugarColumn(ColumnName = "F_TENANTID", ColumnDescription = "租户ID", IsNullable = true)]
    public new string TenantId { get; set; }
}
