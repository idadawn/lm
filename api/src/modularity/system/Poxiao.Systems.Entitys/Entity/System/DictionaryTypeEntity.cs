using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Systems.Entitys.System;

/// <summary>
/// 字典分类
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_DICTIONARYTYPE")]
public class DictionaryTypeEntity : CLDEntityBase
{
    /// <summary>
    /// 上级.
    /// </summary>
    [SugarColumn(ColumnName = "F_PARENTID")]
    public string ParentId { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FULLNAME")]
    public string FullName { get; set; }

    /// <summary>
    /// 编码.
    /// </summary>
    [SugarColumn(ColumnName = "F_ENCODE")]
    public string EnCode { get; set; }

    /// <summary>
    /// 树形.
    /// </summary>
    [SugarColumn(ColumnName = "F_ISTREE")]
    public int? IsTree { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string Description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }
}