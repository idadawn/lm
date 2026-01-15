using System;
using System.Collections.Generic;
using System.Linq;
using Poxiao.Infrastructure.Security;
using Poxiao.Lab.Entity.Entity;

namespace Poxiao.Lab.Entity.Extensions;

/// <summary>
/// 产品规格扩展方法
/// </summary>
public static class ProductSpecExtensions
{
    // 注意：以下方法已废弃，扩展属性现在存储在 LAB_PRODUCT_SPEC_ATTRIBUTE 表中
    // 保留这些方法仅用于向后兼容，但不会实际使用 PropertyJson 字段
    
    /// <summary>
    /// 获取产品规格的扩展属性（已废弃，请使用属性表）
    /// </summary>
    /// <param name="entity">产品规格实体</param>
    /// <returns>扩展属性字典</returns>
    [Obsolete("扩展属性已迁移到 LAB_PRODUCT_SPEC_ATTRIBUTE 表，请使用属性表查询")]
    public static Dictionary<string, object> GetExtendedProperties(this ProductSpecEntity entity)
    {
        // 返回空字典，因为扩展属性已迁移到独立表
        return new Dictionary<string, object>();
    }

    /// <summary>
    /// 设置产品规格的扩展属性（已废弃，请使用属性表）
    /// </summary>
    /// <param name="entity">产品规格实体</param>
    /// <param name="properties">扩展属性字典</param>
    [Obsolete("扩展属性已迁移到 LAB_PRODUCT_SPEC_ATTRIBUTE 表，请使用属性表保存")]
    public static void SetExtendedProperties(this ProductSpecEntity entity, Dictionary<string, object> properties)
    {
        // 不再操作 PropertyJson，扩展属性已迁移到独立表
    }

    /// <summary>
    /// 获取扩展属性值（已废弃，请使用属性表）
    /// </summary>
    /// <param name="entity">产品规格实体</param>
    /// <param name="key">属性键</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>属性值</returns>
    [Obsolete("扩展属性已迁移到 LAB_PRODUCT_SPEC_ATTRIBUTE 表，请使用属性表查询")]
    public static T GetExtendedProperty<T>(this ProductSpecEntity entity, string key, T defaultValue = default)
    {
        // 返回默认值，因为扩展属性已迁移到独立表
        return defaultValue;
    }

    /// <summary>
    /// 设置扩展属性值（已废弃，请使用属性表）
    /// </summary>
    /// <param name="entity">产品规格实体</param>
    /// <param name="key">属性键</param>
    /// <param name="value">属性值</param>
    [Obsolete("扩展属性已迁移到 LAB_PRODUCT_SPEC_ATTRIBUTE 表，请使用属性表保存")]
    public static void SetExtendedProperty<T>(this ProductSpecEntity entity, string key, T value)
    {
        // 不再操作 PropertyJson，扩展属性已迁移到独立表
    }

    /// <summary>
    /// 从属性列表获取计算属性（用于后续计算）
    /// </summary>
    /// <param name="entity">产品规格实体</param>
    /// <param name="attributes">属性列表</param>
    /// <returns>计算属性对象</returns>
    public static CalculationProperties GetCalculationPropertiesFromAttributes(
        this ProductSpecEntity entity,
        List<ProductSpecAttributeEntity> attributes)
    {
        var result = new CalculationProperties
        {
            Length = 4m,
            Layers = 20,
            Density = 7.25m,
            AdditionalProperties = new Dictionary<string, object>()
        };

        if (attributes != null)
        {
            foreach (var attr in attributes)
            {
                if (!string.IsNullOrEmpty(attr.AttributeValue))
                {
                    object value = attr.AttributeValue;
                    if (attr.ValueType == "number")
                    {
                        if (decimal.TryParse(attr.AttributeValue, out var decimalValue))
                            value = decimalValue;
                        else if (int.TryParse(attr.AttributeValue, out var intValue))
                            value = intValue;
                    }

                    if (attr.AttributeKey == "length")
                        result.Length = (decimal)value;
                    else if (attr.AttributeKey == "layers")
                        result.Layers = (int)value;
                    else if (attr.AttributeKey == "density")
                        result.Density = (decimal)value;
                    else
                        result.AdditionalProperties[attr.AttributeKey] = value;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 获取计算属性（已废弃，请使用 GetCalculationPropertiesFromAttributes）
    /// </summary>
    /// <param name="entity">产品规格实体</param>
    /// <returns>计算属性对象</returns>
    [Obsolete("请使用 GetCalculationPropertiesFromAttributes 方法，从属性表读取")]
    public static CalculationProperties GetCalculationProperties(this ProductSpecEntity entity)
    {
        // 返回默认值，因为扩展属性已迁移到独立表
        return new CalculationProperties
        {
            Length = 4m,
            Layers = 20,
            Density = 7.25m,
            AdditionalProperties = new Dictionary<string, object>()
        };
    }

    /// <summary>
    /// 保存计算属性到扩展属性（已废弃，请使用属性表）
    /// </summary>
    /// <param name="entity">产品规格实体</param>
    /// <param name="calcProps">计算属性</param>
    [Obsolete("扩展属性已迁移到 LAB_PRODUCT_SPEC_ATTRIBUTE 表，请使用属性表保存")]
    public static void SaveCalculationProperties(this ProductSpecEntity entity, CalculationProperties calcProps)
    {
        // 不再操作 PropertyJson，扩展属性已迁移到独立表
    }
}

/// <summary>
/// 计算属性类，用于后续计算
/// </summary>
public class CalculationProperties
{
    /// <summary>
    /// 长度（米）
    /// </summary>
    public decimal Length { get; set; } = 4m;

    /// <summary>
    /// 层数
    /// </summary>
    public int Layers { get; set; } = 20;

    /// <summary>
    /// 密度
    /// </summary>
    public decimal Density { get; set; } = 7.25m;

    /// <summary>
    /// 额外属性字典，用于扩展
    /// </summary>
    public Dictionary<string, object> AdditionalProperties { get; set; } = new();
}