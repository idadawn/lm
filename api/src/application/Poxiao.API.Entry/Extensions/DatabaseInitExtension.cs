using Poxiao.DatabaseAccessor;
using Poxiao.Lab.Entity.Entity;
using SqlSugar;

namespace Poxiao.API.Entry.Extensions;

/// <summary>
/// 数据库初始化扩展.
/// </summary>
public static class DatabaseInitExtension
{
    /// <summary>
    /// 初始化 Lab 模块数据库表.
    /// </summary>
    public static void InitializeLabDatabase(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<
            ISqlSugarRepository<ProductSpecEntity>
        >();

        // 初始化所有 Lab 模块的表
        repository
            .AsSugarClient()
            .CodeFirst.InitTables(
                //typeof(ProductSpecEntity),o
                //typeof(AppearanceFeatureEntity),
                //typeof(AppearanceFeatureCategoryEntity),
                //typeof(AppearanceFeatureCorrectionEntity),
                //typeof(AppearanceFeatureLevelEntity),
                //typeof(RawDataEntity),
                //typeof(RawDataImportLogEntity),
                //typeof(ExcelImportTemplateEntity),
                //typeof(UnitCategoryEntity),
                //typeof(UnitDefinitionEntity)
            );

        // 初始化磁性能单位
        SeedMagneticUnits(repository);

        // 初始化数量单位
        SeedQuantityUnits(repository);
    }

    /// <summary>
    /// 初始化数量单位.
    /// </summary>
    private static void SeedQuantityUnits(ISqlSugarRepository<ProductSpecEntity> repository)
    {
        var db = repository.AsSugarClient();

        // 1. 确保 "数量" 维度存在
        var categoryCode = "QUANTITY";
        var category = db.Queryable<UnitCategoryEntity>().First(u => u.Code == categoryCode);
        if (category == null)
        {
            category = new UnitCategoryEntity
            {
                Name = "数量",
                Code = categoryCode,
                Description = "包含计数单位",
                SortCode = 200,
            };
            category.Creator();
            db.Insertable(category).ExecuteCommand();
        }

        // 2. 确保单位 "个" 存在
        if (
            !db.Queryable<UnitDefinitionEntity>()
                .Any(u => u.CategoryId == category.Id && u.Name == "个")
        )
        {
            var unit = new UnitDefinitionEntity
            {
                CategoryId = category.Id,
                Name = "个",
                Symbol = "个",
                IsBase = 1,
                ScaleToBase = 1,
                Precision = 0,
                SortCode = 1,
            };
            unit.Creator();
            db.Insertable(unit).ExecuteCommand();
        }
    }

    /// <summary>
    /// 初始化磁性能单位.
    /// </summary>
    private static void SeedMagneticUnits(ISqlSugarRepository<ProductSpecEntity> repository)
    {
        var db = repository.AsSugarClient();

        // 1. 确保 "磁性能" 维度存在
        var categoryCode = "MAGNETIC";
        var category = db.Queryable<UnitCategoryEntity>().First(u => u.Code == categoryCode);
        if (category == null)
        {
            category = new UnitCategoryEntity
            {
                Name = "磁性能",
                Code = categoryCode,
                Description = "包含激磁功率、铁损、磁场强度等单位",
                SortCode = 100,
            };
            category.Creator();
            db.Insertable(category).ExecuteCommand();
        }

        // 2. 确保单位存在
        var units = new List<UnitDefinitionEntity>();

        // Ss激磁功率(VA/kg) - 设为基准单位
        if (
            !db.Queryable<UnitDefinitionEntity>()
                .Any(u => u.CategoryId == category.Id && u.Name == "Ss激磁功率")
        )
        {
            var unit = new UnitDefinitionEntity
            {
                CategoryId = category.Id,
                Name = "Ss激磁功率",
                Symbol = "VA/kg",
                IsBase = 1,
                ScaleToBase = 1,
                Precision = 2,
                SortCode = 1,
            };
            unit.Creator();
            units.Add(unit);
        }

        // Ps铁损(W/kg)
        if (
            !db.Queryable<UnitDefinitionEntity>()
                .Any(u => u.CategoryId == category.Id && u.Name == "Ps铁损")
        )
        {
            var unit = new UnitDefinitionEntity
            {
                CategoryId = category.Id,
                Name = "Ps铁损",
                Symbol = "W/kg",
                IsBase = 0,
                ScaleToBase = 1, // 假设 1 W/kg = 1 VA/kg (仅作示例，实际需确认换算关系，这里暂设为1)
                Precision = 2,
                SortCode = 2,
            };
            unit.Creator();
            units.Add(unit);
        }

        // Hc(A/m)
        if (
            !db.Queryable<UnitDefinitionEntity>()
                .Any(u => u.CategoryId == category.Id && u.Name == "Hc")
        )
        {
            var unit = new UnitDefinitionEntity
            {
                CategoryId = category.Id,
                Name = "Hc",
                Symbol = "A/m",
                IsBase = 0,
                ScaleToBase = 1, // 这里的换算比例可能不适用，因为量纲不同，但作为独立单位存在
                Precision = 2,
                SortCode = 3,
            };
            unit.Creator();
            units.Add(unit);
        }

        if (units.Count > 0)
        {
            db.Insertable(units).ExecuteCommand();
        }
    }
}
