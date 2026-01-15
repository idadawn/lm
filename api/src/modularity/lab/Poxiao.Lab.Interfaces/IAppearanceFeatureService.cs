using Poxiao.Lab.Entity.Dto.AppearanceFeature;
using Poxiao.Lab.Entity.Entity;

namespace Poxiao.Lab.Interfaces;

/// <summary>
/// 外观特征服务接口.
/// </summary>
public interface IAppearanceFeatureService
{
    /// <summary>
    /// 获取列表.
    /// </summary>
    /// <param name="input">查询参数.</param>
    /// <returns></returns>
    Task<List<AppearanceFeatureListOutput>> GetList(AppearanceFeatureListQuery input);

    /// <summary>
    /// 获取详情.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    Task<AppearanceFeatureInfoOutput> GetInfo(string id);

    /// <summary>
    /// 创建.
    /// </summary>
    /// <param name="input">创建参数.</param>
    /// <returns></returns>
    Task Create(AppearanceFeatureCrInput input);

    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <param name="input">更新参数.</param>
    /// <returns></returns>
    Task Update(string id, AppearanceFeatureUpInput input);

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    Task Delete(string id);

    /// <summary>
    /// 保存人工修正记录.
    /// </summary>
    /// <param name="input">修正记录输入.</param>
    /// <returns></returns>
    Task SaveCorrection(AppearanceFeatureCorrectionInput input);

    /// <summary>
    /// 批量匹配.
    /// </summary>
    /// <param name="input">批量匹配输入.</param>
    /// <returns></returns>
    Task<List<MatchItemOutput>> BatchMatch(BatchMatchInput input);
}
