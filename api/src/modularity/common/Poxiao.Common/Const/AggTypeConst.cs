namespace Poxiao.Infrastructure.Const;

/// <summary>
/// 聚合类型常量
/// </summary>
[SuppressSniffer]
public class AggTypeConst
{
    /// <summary>
    /// 值类型.
    /// </summary>
    public static readonly List<string> DbIntTypes = new() { "int", "bigint", "float", "double", "decimal" };

    /// <summary>
    /// 值筛选方式.
    /// </summary>
    public static readonly List<string> DbByValueFilterModel = new() { "int", "bigint", "float", "double", "decimal" };

    /// <summary>
    /// 日期范围筛选方式.
    /// </summary>
    public static readonly List<string> DbByDateRangeFilterModel = new() { "datetime", "date" };

    /// <summary>
    /// 给定默认值.
    /// </summary>
    public static readonly string DefaultValue = "0";

    /// <summary>
    /// 其他.
    /// </summary>
    public static readonly string Other = "other";
}

