namespace Poxiao.Kpi.Core.Entitys;

/// <summary>
/// 创建数据表
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2024-01-05.
/// </summary>
[SugarTable("metric_table_collection", TableDescription = "保存通过外部创建的数据表的表名")]
public class MetricDataIETableCollectionEntity
{
    /// <summary>
    /// 数据表名.
    /// </summary>
    [SugarColumn(ColumnName = "table_name", ColumnDescription = "数据表名")]
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置 编号.
    /// </summary>
    [SugarColumn(ColumnName = "id", IsPrimaryKey = true, ColumnDescription = "主键")]
    public string Id { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    [SugarColumn(ColumnName = "created_time", ColumnDescription = "创建时间")]
    public DateTime? CreatedTime { get; set; }

    /// <summary>
    /// 创建.
    /// </summary>
    public virtual void Create()
    {
        this.CreatedTime = DateTime.Now;
        this.Id = this.Id == null ? SnowflakeIdHelper.NextId() : this.Id;
    }
}