/// <summary>
/// 枚举序列化使用枚举名称
/// </summary>
[AttributeUsage(AttributeTargets.Enum)]
public class JsonUseEnumNameAttribute : Attribute
{
}

/// <summary>
/// 枚举序列化使用枚举名称.
/// </summary>
/// <typeparam name="TEnum">枚举.</typeparam>
public class EnumUseNameConverter<TEnum> : StringEnumConverter where TEnum : struct
{
    private static readonly Dictionary<string, TEnum> enumMembers = new Dictionary<string, TEnum>();

    static EnumUseNameConverter()
    {
        if (typeof(TEnum).GetCustomAttribute<JsonUseEnumNameAttribute>() != null)
        {
            var fields = typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var field in fields)
            {
                var attr = field.GetCustomAttribute<EnumMemberAttribute>();
                if (attr != null)
                {
                    enumMembers[attr.Value] = (TEnum)field.GetValue(null);
                }
            }
        }
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType != JsonToken.String) return base.ReadJson(reader, objectType, existingValue, serializer);

        string value = reader.Value.ToString();

        if (enumMembers.TryGetValue(value, out var result))
        {
            return result;
        }

        if (Enum.TryParse<TEnum>(value, out result))
        {
            return result;
        }

        // 返回默认值
        return default(TEnum);
    }
}