namespace Poxiao.Kpi.Core.Enums;

/// <summary>
/// 数据类别
/// </summary>
[JsonConverter(typeof(EnumUseNameConverter<DataModelType>))]
public enum DataModelType
{
    /// <summary>
    /// 数据库表.
    /// </summary>
    [Description("数据库.")]
    Db = 0,

    /// <summary>
    /// 建模.
    /// </summary>
    [Description("建模.")]
    Model = 1,
}

/// <summary>
/// 存储类别
/// </summary>
[JsonConverter(typeof(EnumUseNameConverter<SchemaStorageType>))]
public enum SchemaStorageType
{
    /// <summary>
    /// 表.
    /// </summary>
    [Description("表.")]
    Table = 0,

    /// <summary>
    /// 视图.
    /// </summary>
    [Description("视图.")]
    View = 1,

    /// <summary>
    /// 实时数据.
    /// </summary>
    [Description("实时数据.")]
    RealTime = 2,
}

/// <summary>
/// 数据类别
/// </summary>
[JsonConverter(typeof(EnumUseNameConverter<MetricDataType>))]
public enum MetricDataType
{
    /// <summary>
    /// 静态数据.
    /// </summary>
    [Description("静态数据.")]
    Static = 0,

    /// <summary>
    /// 实时数据.
    /// </summary>
    [Description("实时数据.")]
    RealTime = 1,
}