using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Apps.Entitys;

/// <summary>
/// App常用数据
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_APPDATA")]
public class AppDataEntity : OCDEntityBase
{
    /// <summary>
    /// 对象类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_OBJECTTYPE")]
    public string ObjectType { get; set; }

    /// <summary>
    /// 对象主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_OBJECTID")]
    public string ObjectId { get; set; }

    /// <summary>
    /// 对象json.
    /// </summary>
    [SugarColumn(ColumnName = "F_OBJECTDATA")]
    public string ObjectData { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string Description { get; set; }
}