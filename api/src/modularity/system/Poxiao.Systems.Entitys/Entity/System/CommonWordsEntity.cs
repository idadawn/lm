using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Systems.Entitys.Entity.System;

/// <summary>
/// 常用语
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_COMMONWORDS")]
public class CommonWordsEntity : CLDEntityBase
{
    /// <summary>
    /// 应用id.
    /// </summary>
    [SugarColumn(ColumnName = "F_SYSTEMIDS")]
    public string SystemIds { get; set; }

    /// <summary>
    /// 应用名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_SYSTEMNAMES")]
    public string SystemNames { get; set; }

    /// <summary>
    /// 常用语.
    /// </summary>
    [SugarColumn(ColumnName = "F_COMMONWORDSTEXT")]
    public string CommonWordsText { get; set; }

    /// <summary>
    /// 常用语类型(0:系统,1:个人).
    /// </summary>
    [SugarColumn(ColumnName = "F_COMMONWORDSTYPE")]
    public int CommonWordsType { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long SortCode { get; set; }
}
