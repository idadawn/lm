namespace Poxiao.Infrastructure.Contracts;

/// <summary>
/// 定义创建时间.
/// </summary>
public interface ICreatorTime
{
    /// <summary>
    /// 获取或设置 创建时间.
    /// </summary>
    [SugarColumn(ColumnName = "created_time", ColumnDescription = "创建时间")]
    DateTime? CreatedTime { get; set; }
}