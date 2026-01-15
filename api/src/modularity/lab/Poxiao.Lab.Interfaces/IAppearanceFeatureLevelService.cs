using Poxiao.Lab.Entity.Dto.AppearanceFeatureLevel;

namespace Poxiao.Lab.Interfaces;

/// <summary>
/// 外观特性等级服务接口.
/// </summary>
public interface IAppearanceFeatureLevelService
{
    /// <summary>
    /// 获取列表.
    /// </summary>
    Task<List<AppearanceFeatureLevelListOutput>> GetList(AppearanceFeatureLevelListQuery input);

    /// <summary>
    /// 获取详情.
    /// </summary>
    Task<AppearanceFeatureLevelInfoOutput> GetInfo(string id);

    /// <summary>
    /// 创建.
    /// </summary>
    Task Create(AppearanceFeatureLevelCrInput input);

    /// <summary>
    /// 更新.
    /// </summary>
    Task Update(string id, AppearanceFeatureLevelUpInput input);

    /// <summary>
    /// 删除.
    /// </summary>
    Task Delete(string id);

    /// <summary>
    /// 获取所有启用的等级（用于下拉选择）.
    /// </summary>
    Task<List<AppearanceFeatureLevelListOutput>> GetEnabledLevels();
}
