using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Systems.Entitys.System;

/// <summary>
/// 数据权限连接管理
/// 版 本：V3.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2017.09.20.
/// </summary>
[SugarTable("BASE_MODULEDATAAUTHORIZELINK")]
[Tenant(ClaimConst.TENANTID)]
public class ModuleDataAuthorizeLinkEntity : OEntityBase<string>
{
    /// <summary>
    /// 数据源连接主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_LINKID")]
    public string LinkId { get; set; }

    /// <summary>
    /// 表名.
    /// </summary>
    [SugarColumn(ColumnName = "F_LINKTABLES")]
    public string LinkTables { get; set; }

    /// <summary>
    /// 权限类型(1:列表权限，2：数据权限，3：表单权限).
    /// </summary>
    [SugarColumn(ColumnName = "F_TYPE")]
    public string Type { get; set; }

    /// <summary>
    /// 菜单主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_MODULEID")]
    public string ModuleId { get; set; }
}