using Poxiao.Lab.Entity;

namespace Poxiao.Lab.Interfaces;

/// <summary>
/// 公共维度服务接口.
/// </summary>
public interface IPublicDimensionService
{
    /// <summary>
    /// 获取所有公共维度.
    /// </summary>
    /// <returns>公共维度列表.</returns>
    Task<List<PublicDimensionEntity>> GetPublicDimensions();

    /// <summary>
    /// 根据ID获取公共维度.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns>公共维度实体.</returns>
    Task<PublicDimensionEntity> GetById(string id);

    /// <summary>
    /// 创建公共维度.
    /// </summary>
    /// <param name="entity">公共维度实体.</param>
    /// <returns></returns>
    Task Create(PublicDimensionEntity entity);

    /// <summary>
    /// 更新公共维度.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <param name="entity">公共维度实体.</param>
    /// <returns></returns>
    Task Update(string id, PublicDimensionEntity entity);

    /// <summary>
    /// 删除公共维度.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    Task Delete(string id);

    /// <summary>
    /// 获取当前版本号.
    /// </summary>
    /// <param name="dimensionId">公共维度ID.</param>
    /// <returns>当前版本号.</returns>
    Task<int> GetCurrentVersionAsync(string dimensionId);

    /// <summary>
    /// 创建新版本（当维度信息修改时调用）.
    /// </summary>
    /// <param name="dimensionId">公共维度ID.</param>
    /// <param name="versionDescription">版本说明.</param>
    /// <returns>新版本号.</returns>
    Task<int> CreateNewVersionAsync(string dimensionId, string versionDescription = null);

    /// <summary>
    /// 获取维度版本列表.
    /// </summary>
    /// <param name="dimensionId">公共维度ID.</param>
    /// <returns>版本列表.</returns>
    Task<List<PublicDimensionVersionEntity>> GetVersionListAsync(string dimensionId);
}
