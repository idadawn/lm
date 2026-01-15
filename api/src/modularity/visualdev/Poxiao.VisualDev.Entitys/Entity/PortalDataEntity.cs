using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.VisualDev.Entitys.Entity;

/// <summary>
/// 门户子表.
/// </summary>
[SugarTable("BASE_PORTAL_DATA")]
public class PortalDataEntity : CLEntityBase
{
    /// <summary>
    /// 门户id.
    /// </summary>
    [SugarColumn(ColumnName = "F_PortalId")]
    public string PortalId { get; set; }

    /// <summary>
    /// 系统id.
    /// </summary>
    [SugarColumn(ColumnName = "F_System_Id")]
    public string SystemId { get; set; }

    /// <summary>
    /// 表单配置JSON.
    /// </summary>
    [SugarColumn(ColumnName = "F_FORMDATA")]
    public string? FormData { get; set; }

    /// <summary>
    /// web:网页端 app:手机端.
    /// </summary>
    [SugarColumn(ColumnName = "F_Platform")]
    public string Platform { get; set; }

    /// <summary>
    /// 类型（model：模型、release：发布）.
    /// </summary>
    [SugarColumn(ColumnName = "F_Type")]
    public string Type { get; set; }

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
    public string? DeleteUserId { get; set; }
}
