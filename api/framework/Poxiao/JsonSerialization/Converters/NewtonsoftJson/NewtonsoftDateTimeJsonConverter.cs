using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Poxiao.JsonSerialization;

/// <summary>
/// DateTime 类型序列化
/// </summary>
[SuppressSniffer]
public class NewtonsoftDateTimeJsonConverter : DateTimeConverterBase
{
    internal static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// 读.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="existingValue">The existing property value of the JSON that is being converted.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var nullable = IsNullable(objectType);

        if (reader.TokenType == JsonToken.Null || string.IsNullOrEmpty(reader.Value.ToString()))
        {
            if (!nullable)
                throw new JsonSerializationException(string.Format(CultureInfo.InvariantCulture, "Cannot convert null value to {0}.", objectType));

            return null;
        }

        long result;
        if (reader.TokenType == JsonToken.Integer)
            result = (long)reader.Value;
        else if (reader.TokenType == JsonToken.Date)
            return reader.Value;
        else
        {
            if (reader.TokenType != JsonToken.String)
                throw new Exception("Unexpected token parsing date. Expected Integer or String, got.");
            if (!long.TryParse((string)reader.Value, out result))
                throw new Exception("Cannot convert invalid value to.");
        }

        var d = DateTimeOffset.FromUnixTimeMilliseconds(result).ToLocalTime().DateTime;

        var t = nullable ? Nullable.GetUnderlyingType(objectType) : objectType;
        if (t == typeof(DateTimeOffset))
            return new DateTimeOffset(d, TimeSpan.Zero);

        return d;
    }

    /// <summary>
    /// 写入.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        long num;
        if (value is DateTime)
        {
            num = (long)((DateTime)value).ToUniversalTime().Subtract(UnixEpoch).TotalMilliseconds;
        }
        else
        {
            if (!(value is DateTimeOffset))
            {
                throw new JsonSerializationException("Expected date object value.");
            }

            num = (long)((DateTimeOffset)value).ToUniversalTime().Subtract(UnixEpoch).TotalMilliseconds;
        }

        writer.WriteValue(num);
    }

    /// <summary>
    /// 类型是否为空.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    private static bool IsNullable(Type t)
    {
        if (t.IsValueType)
        {
            return IsNullableType(t);
        }

        return true;
    }

    private static bool IsNullableType(Type t)
    {
        if (t.IsGenericType)
        {
            return t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        return false;
    }
}