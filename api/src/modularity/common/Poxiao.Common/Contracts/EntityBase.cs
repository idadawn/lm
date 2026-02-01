namespace Poxiao.Infrastructure.Contracts;

/// <summary>
/// 实体类基类.
/// </summary>
[SuppressSniffer]
public abstract class EntityBase<TKey> : ITenantFilter, IOEntity<TKey>
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// 获取或设置 编号.
    /// </summary>
    [SugarColumn(ColumnName = "id", IsPrimaryKey = true, ColumnDescription = "主键")]
    public TKey Id { get; set; }

    /// <summary>
    /// 获取或设置 租户id.
    /// </summary>
    [SugarColumn(ColumnName = "tenant_id", ColumnDescription = "租户id")]
    public string? TenantId { get; set; }
}