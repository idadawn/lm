namespace Poxiao.Infrastructure.Contracts;

/// <summary>
/// 实体类基类.
/// </summary>
public interface IOEntity<TKey>
{
    /// <summary>
    /// 获取或设置 实体唯一标识，主键.
    /// </summary>
    TKey Id { get; set; }
}