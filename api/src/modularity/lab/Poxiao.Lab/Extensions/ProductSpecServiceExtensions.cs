using System;
using System.Threading.Tasks;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Entity;
using Poxiao.Lab.Entity.Extensions;
using SqlSugar;

namespace Poxiao.Lab.Extensions;

/// <summary>
/// 产品规格服务扩展方法
/// </summary>
public static class ProductSpecServiceExtensions
{
    /// <summary>
    /// 确保产品规格具有默认的计算属性（已废弃，请使用属性表）
    /// </summary>
    /// <param name="repository">仓储</param>
    /// <param name="entity">产品规格实体</param>
    [Obsolete("扩展属性已迁移到 LAB_PRODUCT_SPEC_ATTRIBUTE 表，请使用 ProductSpecService 中的 EnsureCoreAttributes 方法")]
    public static void EnsureDefaultCalculationProperties(this ISqlSugarRepository<ProductSpecEntity> repository, ProductSpecEntity entity)
    {
        // 不再操作 PropertyJson，扩展属性已迁移到独立表
        // 核心属性现在由 ProductSpecService.EnsureCoreAttributes 方法处理
    }

    /// <summary>
    /// 批量更新产品规格的计算属性（已废弃，请使用属性表）
    /// </summary>
    /// <param name="repository">仓储</param>
    /// <param name="specs">产品规格列表</param>
    [Obsolete("扩展属性已迁移到 LAB_PRODUCT_SPEC_ATTRIBUTE 表，请使用属性表操作")]
    public static async Task BatchUpdateCalculationPropertiesAsync(this ISqlSugarRepository<ProductSpecEntity> repository, params ProductSpecEntity[] specs)
    {
        // 不再操作 PropertyJson，扩展属性已迁移到独立表
        await Task.CompletedTask;
    }

    /// <summary>
    /// 验证计算属性（已废弃，请使用属性表验证）
    /// </summary>
    /// <param name="entity">产品规格实体</param>
    /// <returns>验证结果</returns>
    [Obsolete("扩展属性已迁移到 LAB_PRODUCT_SPEC_ATTRIBUTE 表，请使用 ProductSpecService 中的 ValidateAttributes 方法")]
    public static ValidationResult ValidateCalculationProperties(this ProductSpecEntity entity)
    {
        // 不再验证 PropertyJson，扩展属性已迁移到独立表
        // 验证现在由 ProductSpecService.ValidateAttributes 方法处理
        return new ValidationResult();
    }

    /// <summary>
    /// 添加新的计算属性（已废弃，请使用属性表）
    /// </summary>
    /// <param name="entity">产品规格实体</param>
    /// <param name="propertyName">属性名称</param>
    /// <param name="propertyValue">属性值</param>
    /// <param name="propertyType">属性类型</param>
    [Obsolete("扩展属性已迁移到 LAB_PRODUCT_SPEC_ATTRIBUTE 表，请使用属性表操作")]
    public static void AddCalculationProperty(this ProductSpecEntity entity, string propertyName, object propertyValue, Type propertyType = null)
    {
        // 不再操作 PropertyJson，扩展属性已迁移到独立表
    }

    /// <summary>
    /// 获取计算属性的哈希值（已废弃，请使用属性表）
    /// </summary>
    /// <param name="entity">产品规格实体</param>
    /// <returns>哈希值</returns>
    [Obsolete("扩展属性已迁移到 LAB_PRODUCT_SPEC_ATTRIBUTE 表，请从属性表计算哈希值")]
    public static string GetCalculationPropertiesHash(this ProductSpecEntity entity)
    {
        // 返回默认哈希值，因为扩展属性已迁移到独立表
        return "0";
    }
}

/// <summary>
/// 验证结果
/// </summary>
public class ValidationResult
{
    private readonly List<string> _errors = new();

    public bool IsValid => _errors.Count == 0;
    public IReadOnlyList<string> Errors => _errors.AsReadOnly();

    public void AddError(string error)
    {
        _errors.Add(error);
    }

    public void AddErrors(IEnumerable<string> errors)
    {
        _errors.AddRange(errors);
    }

    public string GetErrorMessage()
    {
        return string.Join("; ", _errors);
    }
}