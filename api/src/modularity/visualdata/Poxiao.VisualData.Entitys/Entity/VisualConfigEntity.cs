using Poxiao.Infrastructure.Security;
using Poxiao.Extras.DatabaseAccessor.SqlSugar.Models;
using SqlSugar;
using Yitter.IdGenerator;

namespace Poxiao.VisualData.Entity;

/// <summary>
/// 可视化配置表.
/// </summary>
[SugarTable("BLADE_VISUAL_CONFIG")]
public class VisualConfigEntity : ITenantFilter
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "ID", ColumnDescription = "主键", IsPrimaryKey = true)]
    public string Id { get; set; }

    /// <summary>
    /// 可视化表主键.
    /// </summary>
    [SugarColumn(ColumnName = "VISUAL_ID", ColumnDescription = "可视化表主键")]
    public string VisualId { get; set; }

    /// <summary>
    /// 配置json.
    /// </summary>
    [SugarColumn(ColumnName = "DETAIL", ColumnDescription = "配置json")]
    public string Detail { get; set; }

    /// <summary>
    /// 组件json.
    /// </summary>
    [SugarColumn(ColumnName = "COMPONENT", ColumnDescription = "组件json")]
    public string Component { get; set; }

    /// <summary>
    /// 租户id.
    /// </summary>
    [SugarColumn(ColumnName = "F_TenantId", ColumnDescription = "租户id")]
    public string TenantId { get; set; }

    /// <summary>
    /// 创建.
    /// </summary>
    public virtual void Create()
    {
        this.Id = SnowflakeIdHelper.NextId();
    }
}
