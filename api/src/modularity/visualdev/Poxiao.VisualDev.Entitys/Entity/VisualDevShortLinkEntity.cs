using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.VisualDev.Entitys;

/// <summary>
/// 可视化开发功能实体.
/// </summary>
[SugarTable("BASE_VISUALDEV_SHORT_LINK")]
public class VisualDevShortLinkEntity : CLDEntityBase
{
    /// <summary>
    /// 短链接.
    /// </summary>
    [SugarColumn(ColumnName = "F_SHORTLINK")]
    public string ShortLink { get; set; }

    /// <summary>
    /// 外链填单开关.
    /// </summary>
    [SugarColumn(ColumnName = "F_FormUse")]
    public int FormUse { get; set; }

    /// <summary>
    /// 外链填单.
    /// </summary>
    [SugarColumn(ColumnName = "F_FormLink")]
    public string FormLink { get; set; }

    /// <summary>
    /// 外链密码开关(1：开 , 0：关).
    /// </summary>
    [SugarColumn(ColumnName = "F_FormPassUse")]
    public int FormPassUse { get; set; }

    /// <summary>
    /// 外链填单密码.
    /// </summary>
    [SugarColumn(ColumnName = "F_FormPassword")]
    public string FormPassword { get; set; }

    /// <summary>
    /// 公开查询开关.
    /// </summary>
    [SugarColumn(ColumnName = "F_ColumnUse")]
    public int ColumnUse { get; set; }

    /// <summary>
    /// 公开查询.
    /// </summary>
    [SugarColumn(ColumnName = "F_ColumnLink")]
    public string ColumnLink { get; set; }

    /// <summary>
    /// 查询密码开关.
    /// </summary>
    [SugarColumn(ColumnName = "F_ColumnPassUse")]
    public int ColumnPassUse { get; set; }

    /// <summary>
    /// 公开查询密码.
    /// </summary>
    [SugarColumn(ColumnName = "F_ColumnPassword")]
    public string ColumnPassword { get; set; }

    /// <summary>
    /// 查询条件.
    /// </summary>
    [SugarColumn(ColumnName = "F_ColumnCondition")]
    public string ColumnCondition { get; set; }

    /// <summary>
    /// 显示内容.
    /// </summary>
    [SugarColumn(ColumnName = "F_ColumnText")]
    public string ColumnText { get; set; }

    /// <summary>
    /// PC端链接.
    /// </summary>
    [SugarColumn(ColumnName = "F_RealPcLink")]
    public string RealPcLink { get; set; }

    /// <summary>
    /// App端链接.
    /// </summary>
    [SugarColumn(ColumnName = "F_RealAppLink")]
    public string RealAppLink { get; set; }

    /// <summary>
    /// 用户id.
    /// </summary>
    [SugarColumn(ColumnName = "F_UserId")]
    public string UserId { get; set; }

    /// <summary>
    /// 租户id.
    /// </summary>
    [SugarColumn(ColumnName = "F_TenantId")]
    public string TenantId { get; set; }
}