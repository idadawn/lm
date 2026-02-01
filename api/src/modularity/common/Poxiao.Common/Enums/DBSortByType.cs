using Poxiao.Infrastructure.Models;

namespace Poxiao.Infrastructure.Enums;

/// <summary>
/// 数据库排序类别.
/// </summary>
[JsonConverter(typeof(EnumUseNameConverter<DBSortByType>))]
public enum DBSortByType
{
    /// <summary>
    /// 升序.
    /// </summary>
    [Description("升序")]
    ASC = 0,

    /// <summary>
    /// 降序.
    /// </summary>
    [Description("降序")]
    DESC = 1
}

/// <summary>
/// 数据库聚合类别.
/// </summary>
[JsonConverter(typeof(EnumUseNameConverter<DBAggType>))]
public enum DBAggType
{
    /// <summary>
    /// 求和.
    /// </summary>s
    [Description("求和")]
    SUM = 0,

    /// <summary>
    /// 最大值.
    /// </summary>s
    [Description("最大值")]
    MAX = 1,

    /// <summary>
    /// 最小值.
    /// </summary>s
    [Description("最小值")]
    MIN = 2,

    /// <summary>
    /// 平均值.
    /// </summary>s
    [Description("平均值")]
    AVG = 3,

    /// <summary>
    /// 计数.
    /// </summary>s
    [Description("计数")]
    COUNT = 4,

    /// <summary>
    /// 不重复计数.
    /// </summary>s
    [Description("不重复计数")]
    COUNTDISTINCT = 5,

    /// <summary>
    /// 无.
    /// </summary>s
    [Description("无")]
    None = 6
}