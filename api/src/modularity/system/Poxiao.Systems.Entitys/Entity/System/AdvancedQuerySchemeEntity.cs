using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Systems.Entitys.System;

/// <summary>
/// 高级查询方案
/// 版 本：V3.4
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2022-06-06.
/// </summary>
[SugarTable("BASE_ADVANCEDQUERYSCHEME")]
public class AdvancedQuerySchemeEntity : OEntityBase<string>
{
    /// <summary>
    /// 方案名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FULLNAME")]
    public string FullName { get; set; }

    /// <summary>
    /// 匹配逻辑.
    /// </summary>
    [SugarColumn(ColumnName = "F_MATCHLOGIC")]
    public string MatchLogic { get; set; }

    /// <summary>
    /// 条件规则Json.
    /// </summary>
    [SugarColumn(ColumnName = "F_CONDITIONJSON")]
    public string ConditionJson { get; set; }

    /// <summary>
    /// 菜单主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_MODULEID")]
    public string ModuleId { get; set; }

    /// <summary>
    /// 获取或设置 创建时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_CREATORTIME")]
    public DateTime? CreatorTime { get; set; }

    /// <summary>
    /// 获取或设置 所属用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_CREATORUSERID")]
    public string CreatorUserId { get; set; }

    /// <summary>
    /// 获取或设置 删除标志.
    /// </summary>
    [SugarColumn(ColumnName = "F_DELETEMARK")]
    public int? DeleteMark { get; set; }

    /// <summary>
    /// 获取或设置 删除时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_DELETETIME")]
    public DateTime? DeleteTime { get; set; }
}