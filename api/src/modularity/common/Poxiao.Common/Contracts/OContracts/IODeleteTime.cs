namespace Poxiao.Infrastructure.Contracts;

/// <summary>
/// 定义逻辑删除功能.
/// </summary>
public interface IODeleteTime
{
    /// <summary>
    /// 获取或设置 数据逻辑删除时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteTime", ColumnDescription = "删除时间")]
    DateTime? DeleteTime { get; set; }
}