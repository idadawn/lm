namespace Poxiao.Infrastructure.Contracts;

/// <summary>
/// 实体类基类.
/// </summary>
[SuppressSniffer]
public abstract class OEntityBase<TKey> : ITenantFilter, IOEntity<TKey>
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// 获取或设置 编号.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", IsPrimaryKey = true)]
    public TKey Id { get; set; }

    /// <summary>
    /// 获取或设置 租户id.
    /// </summary>
    [SugarColumn(ColumnName = "F_TenantId", ColumnDescription = "租户id", IsNullable = true)]
    public string TenantId { get; set; }
}
