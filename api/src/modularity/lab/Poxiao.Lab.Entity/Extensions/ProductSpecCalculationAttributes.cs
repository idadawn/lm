using System;
using System.Collections.Generic;
using System.Linq;

namespace Poxiao.Lab.Entity.Extensions;

/// <summary>
/// 产品规格计算属性管理器（已废弃，请使用属性表）
/// 用于管理所有用于后续计算的属性
/// </summary>
[Obsolete("扩展属性已迁移到 LAB_PRODUCT_SPEC_ATTRIBUTE 表，请使用属性表查询")]
public class ProductSpecCalculationAttributes
{
    private readonly ProductSpecEntity _productSpec;
    private readonly Dictionary<string, object> _attributes;

    [Obsolete("扩展属性已迁移到 LAB_PRODUCT_SPEC_ATTRIBUTE 表")]
    public ProductSpecCalculationAttributes(ProductSpecEntity productSpec)
    {
        _productSpec = productSpec ?? throw new ArgumentNullException(nameof(productSpec));
        // 不再从 PropertyJson 读取，扩展属性已迁移到独立表
        _attributes = new Dictionary<string, object>();
    }

    /// <summary>
    /// 长度（米）- 默认4，从扩展属性读取
    /// </summary>
    public decimal Length
    {
        get => GetExtendedValue<decimal?>("length") ?? GetDefaultValue<decimal>("length", 4m);
        set => SetExtendedValue("length", value);
    }

    /// <summary>
    /// 层数 - 默认20，从扩展属性读取
    /// </summary>
    public int Layers
    {
        get => GetExtendedValue<int?>("layers") ?? GetDefaultValue<int>("layers", 20);
        set => SetExtendedValue("layers", value);
    }

    /// <summary>
    /// 密度 - 默认7.25，从扩展属性读取
    /// </summary>
    public decimal Density
    {
        get => GetExtendedValue<decimal?>("density") ?? GetDefaultValue<decimal>("density", 7.25m);
        set => SetExtendedValue("density", value);
    }

    /// <summary>
    /// 宽度（米）- 扩展属性
    /// </summary>
    public decimal? Width
    {
        get => GetExtendedValue<decimal?>(nameof(Width));
        set => SetExtendedValue(nameof(Width), value);
    }

    /// <summary>
    /// 厚度（毫米）- 扩展属性
    /// </summary>
    public decimal? Thickness
    {
        get => GetExtendedValue<decimal?>(nameof(Thickness));
        set => SetExtendedValue(nameof(Thickness), value);
    }

    /// <summary>
    /// 重量（kg）- 扩展属性
    /// </summary>
    public decimal? Weight
    {
        get => GetExtendedValue<decimal?>(nameof(Weight));
        set => SetExtendedValue(nameof(Weight), value);
    }

    /// <summary>
    /// 抗拉强度（MPa）- 扩展属性
    /// </summary>
    public decimal? TensileStrength
    {
        get => GetExtendedValue<decimal?>(nameof(TensileStrength));
        set => SetExtendedValue(nameof(TensileStrength), value);
    }

    /// <summary>
    /// 屈服强度（MPa）- 扩展属性
    /// </summary>
    public decimal? YieldStrength
    {
        get => GetExtendedValue<decimal?>(nameof(YieldStrength));
        set => SetExtendedValue(nameof(YieldStrength), value);
    }

    /// <summary>
    /// 延伸率（%）- 扩展属性
    /// </summary>
    public decimal? Elongation
    {
        get => GetExtendedValue<decimal?>(nameof(Elongation));
        set => SetExtendedValue(nameof(Elongation), value);
    }

    /// <summary>
    /// 获取所有计算属性的字典
    /// </summary>
    public Dictionary<string, object> GetAllAttributes()
    {
        var result = new Dictionary<string, object>
        {
            ["length"] = Length,
            ["layers"] = Layers,
            ["density"] = Density
        };

        // 添加所有扩展属性
        foreach (var attr in _attributes)
        {
            result[attr.Key] = attr.Value;
        }

        return result;
    }

    /// <summary>
    /// 添加新的计算属性
    /// </summary>
    public void AddAttribute<T>(string name, T value)
    {
        SetExtendedValue(name, value);
    }

    /// <summary>
    /// 移除计算属性
    /// </summary>
    public bool RemoveAttribute(string name)
    {
        if (_attributes.ContainsKey(name))
        {
            _attributes.Remove(name);
            _productSpec.SetExtendedProperties(_attributes);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 保存所有更改到实体（已废弃，请使用属性表）
    /// </summary>
    [Obsolete("扩展属性已迁移到 LAB_PRODUCT_SPEC_ATTRIBUTE 表，请使用属性表保存")]
    public void SaveChanges()
    {
        // 不再操作 PropertyJson，扩展属性已迁移到独立表
    }

    private T GetDefaultValue<T>(string propertyName, T defaultValue)
    {
        return defaultValue;
    }

    private T GetExtendedValue<T>(string propertyName)
    {
        return _productSpec.GetExtendedProperty(propertyName, default(T));
    }

    private void SetExtendedValue<T>(string propertyName, T value)
    {
        _productSpec.SetExtendedProperty(propertyName, value);
        _attributes[propertyName] = value;
    }
}

/// <summary>
/// 产品规格计算属性扩展方法（已废弃，请使用属性表）
/// </summary>
public static class ProductSpecCalculationExtensions
{
    /// <summary>
    /// 获取计算属性管理器（已废弃，请使用属性表）
    /// </summary>
    [Obsolete("扩展属性已迁移到 LAB_PRODUCT_SPEC_ATTRIBUTE 表，请从属性表读取")]
    public static ProductSpecCalculationAttributes GetCalculationAttributesManager(this ProductSpecEntity productSpec)
    {
        return new ProductSpecCalculationAttributes(productSpec);
    }

    /// <summary>
    /// 计算产品总重量（基于长度、层数、密度和宽度、厚度）
    /// </summary>
    public static decimal? CalculateTotalWeight(this ProductSpecEntity productSpec)
    {
        var attr = productSpec.GetCalculationAttributesManager();

        if (attr.Width.HasValue && attr.Thickness.HasValue)
        {
            // 体积 = 长度 * 宽度 * 厚度 * 层数
            // 重量 = 体积 * 密度
            // 注意：厚度需要转换为米
            var volume = attr.Length * attr.Width.Value * (attr.Thickness.Value / 1000) * attr.Layers;
            return volume * attr.Density;
        }

        return null;
    }

    /// <summary>
    /// 计算单位重量（每米重量）
    /// </summary>
    public static decimal? CalculateWeightPerMeter(this ProductSpecEntity productSpec)
    {
        var totalWeight = productSpec.CalculateTotalWeight();
        if (totalWeight.HasValue)
        {
            var attr = productSpec.GetCalculationAttributesManager();
            return totalWeight.Value / attr.Length;
        }
        return null;
    }
}