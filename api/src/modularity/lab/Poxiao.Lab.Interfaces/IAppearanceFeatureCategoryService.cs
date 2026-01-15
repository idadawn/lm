using Poxiao.Lab.Entity.Dto.AppearanceFeature;

namespace Poxiao.Lab.Interfaces;

/// <summary>
/// 外观特性大类服务接口.
/// </summary>
public interface IAppearanceFeatureCategoryService
{
    /// <summary>
    /// 获取列表.
    /// </summary>
    Task<List<AppearanceFeatureCategoryListOutput>> GetList(AppearanceFeatureCategoryListQuery input);

    /// <summary>
    /// 获取详情.
    /// </summary>
    Task<AppearanceFeatureCategoryInfoOutput> GetInfo(string id);

    /// <summary>
    /// 创建.
    /// </summary>
    Task Create(AppearanceFeatureCategoryCrInput input);

    /// <summary>
    /// 更新.
    /// </summary>
    Task Update(string id, AppearanceFeatureCategoryUpInput input);

    /// <summary>
    /// 删除.
    /// </summary>
    Task Delete(string id);

    /// <summary>
    /// 获取所有大类（用于下拉选择）.
    /// </summary>
    Task<List<AppearanceFeatureCategoryListOutput>> GetAllCategories();
}
