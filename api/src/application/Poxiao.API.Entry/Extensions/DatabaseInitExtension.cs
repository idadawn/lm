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
                typeof(ProductSpecEntity),
                typeof(AppearanceFeatureEntity),
                typeof(AppearanceFeatureCategoryEntity),
                typeof(AppearanceFeatureCorrectionEntity),
                typeof(AppearanceFeatureLevelEntity),
                typeof(RawDataEntity),
                typeof(RawDataImportLogEntity)
            );
    }
}
