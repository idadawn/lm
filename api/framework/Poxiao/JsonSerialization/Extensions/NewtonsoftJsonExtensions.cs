using Poxiao.JsonSerialization;

namespace Newtonsoft.Json;

/// <summary>
/// Newtonsoft.Json 拓展
/// </summary>
[SuppressSniffer]
public static class NewtonsoftJsonExtensions
{
    /// <summary>
    /// 添加 long/long? 类型序列化处理
    /// </summary>
    /// <remarks></remarks>
    public static IList<JsonConverter> AddLongTypeConverters(this IList<JsonConverter> converters)
    {
        converters.Add(new NewtonsoftJsonLongToStringJsonConverter());
        converters.Add(new NewtonsoftJsonNullableLongToStringJsonConverter());

        return converters;
    }

    /// <summary>
    /// 添加 Clay 类型序列化处理
    /// </summary>
    /// <remarks></remarks>
    public static IList<JsonConverter> AddClayConverters(this IList<JsonConverter> converters)
    {
        converters.Add(new NewtonsoftJsonClayJsonConverter());

        return converters;
    }

    /// <summary>
    /// 添加 DateOnly/DateOnly? 类型序列化处理
    /// </summary>
    /// <param name="converters"></param>
    /// <returns></returns>
    public static IList<JsonConverter> AddDateOnlyConverters(this IList<JsonConverter> converters)
    {
        converters.Add(new NewtonsoftJsonDateOnlyJsonConverter());
        converters.Add(new NewtonsoftJsonNullableDateOnlyJsonConverter());

        return converters;
    }

    /// <summary>
    /// 添加 TimeOnly/TimeOnly? 类型序列化处理
    /// </summary>
    /// <param name="converters"></param>
    /// <returns></returns>
    public static IList<JsonConverter> AddTimeOnlyConverters(this IList<JsonConverter> converters)
    {
        converters.Add(new NewtonsoftJsonTimeOnlyJsonConverter());
        converters.Add(new NewtonsoftJsonNullableTimeOnlyJsonConverter());

        return converters;
    }
}
