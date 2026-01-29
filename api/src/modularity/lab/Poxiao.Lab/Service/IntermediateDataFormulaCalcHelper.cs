using System.Globalization;
using System.Reflection;
using Poxiao.Lab.Entity;

namespace Poxiao.Lab.Service;

internal static class IntermediateDataFormulaCalcHelper
{
    public static Dictionary<string, object> ExtractContextDataFromEntity(IntermediateDataEntity entity)
    {
        var contextData = new Dictionary<string, object>();

        var entityType = typeof(IntermediateDataEntity);
        var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            if (
                prop.Name == "Id"
                || prop.Name == "RawDataId"
                || prop.Name == "CreatorUserId"
                || prop.Name == "CreatorTime"
                || prop.Name == "LastModifyUserId"
                || prop.Name == "LastModifyTime"
                || prop.Name == "DeleteMark"
                || prop.Name == "DeleteUserId"
                || prop.Name == "DeleteTime"
                || prop.Name == "TenantId"
                || prop.Name == "AppearanceFeatureIdsList"
            )
            {
                continue;
            }

            var propType = prop.PropertyType;
            var underlyingType = Nullable.GetUnderlyingType(propType) ?? propType;

            if (
                underlyingType == typeof(decimal)
                || underlyingType == typeof(int)
                || underlyingType == typeof(long)
                || underlyingType == typeof(double)
                || underlyingType == typeof(float)
                || underlyingType == typeof(short)
                || underlyingType == typeof(byte)
            )
            {
                var value = prop.GetValue(entity);
                if (value != null)
                {
                    try
                    {
                        contextData[prop.Name] = Convert.ToDecimal(value);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        if (!contextData.ContainsKey("Length") && contextData.ContainsKey("ProductLength"))
        {
            contextData["Length"] = contextData["ProductLength"];
        }

        if (!contextData.ContainsKey("Layers") && contextData.ContainsKey("ProductLayers"))
        {
            contextData["Layers"] = contextData["ProductLayers"];
        }

        if (!contextData.ContainsKey("Density"))
        {
            if (contextData.ContainsKey("Density"))
            {
                contextData["Density"] = contextData["Density"];
            }
            else if (contextData.ContainsKey("ProductDensity"))
            {
                contextData["Density"] = contextData["ProductDensity"];
            }
        }

        for (var i = 1; i <= 22; i++)
        {
            var propName = $"Detection{i}";
            var prop = entityType.GetProperty(propName);
            if (prop != null)
            {
                var value = prop.GetValue(entity);
                if (value != null)
                {
                    try
                    {
                        contextData[propName] = Convert.ToDecimal(value);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        for (var i = 1; i <= 22; i++)
        {
            var propName = $"Thickness{i}";
            var prop = entityType.GetProperty(propName);
            if (prop != null)
            {
                var value = prop.GetValue(entity);
                if (value != null)
                {
                    try
                    {
                        contextData[propName] = Convert.ToDecimal(value);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        for (var i = 1; i <= 22; i++)
        {
            var propName = $"LaminationDist{i}";
            var prop = entityType.GetProperty(propName);
            if (prop != null)
            {
                var value = prop.GetValue(entity);
                if (value != null)
                {
                    try
                    {
                        contextData[propName] = Convert.ToDecimal(value);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        if (entity.DetectionColumns.HasValue && entity.DetectionColumns.Value > 0)
        {
            contextData["DetectionColumns"] = entity.DetectionColumns.Value;
        }

        return contextData;
    }

    public static void SetFormulaValueToEntity(
        IntermediateDataEntity entity,
        string columnName,
        decimal? value
    )
    {
        if (string.IsNullOrWhiteSpace(columnName))
        {
            return;
        }

        var property = typeof(IntermediateDataEntity).GetProperty(columnName);
        if (property == null || !property.CanWrite)
        {
            return;
        }

        try
        {
            var targetType = property.PropertyType;
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var underlyingType = Nullable.GetUnderlyingType(targetType);
                if (underlyingType == typeof(decimal))
                {
                    property.SetValue(entity, value);
                    return;
                }
            }
            else if (targetType == typeof(decimal))
            {
                property.SetValue(entity, value ?? 0m);
                return;
            }
            else if (targetType == typeof(decimal?))
            {
                property.SetValue(entity, value);
                return;
            }
        }
        catch
        {
            // ignored
        }
    }

    public static decimal? ParseDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        if (decimal.TryParse(value, out parsed))
        {
            return parsed;
        }

        return null;
    }

    public static bool IsRangeFormula(string formula)
    {
        if (string.IsNullOrWhiteSpace(formula))
        {
            return false;
        }

        return formula.IndexOf("RANGE(", StringComparison.OrdinalIgnoreCase) >= 0
            || formula.IndexOf("DIFF_FIRST_LAST", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}