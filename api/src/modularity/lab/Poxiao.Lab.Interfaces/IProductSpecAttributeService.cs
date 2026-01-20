using Poxiao.Lab.Entity;

namespace Poxiao.Lab.Interfaces;

/// <summary>
/// 产品规格扩展属性服务接口.
/// </summary>
public interface IProductSpecAttributeService
{
    /// <summary>
    /// 获取产品规格的所有扩展属性.
    /// </summary>
    /// <param name="productSpecId">产品规格ID.</param>
    /// <returns>扩展属性列表.</returns>
    Task<List<ProductSpecAttributeEntity>> GetAttributesByProductSpecId(string productSpecId);

    /// <summary>
    /// 保存产品规格的扩展属性（批量）.
    /// </summary>
    /// <param name="productSpecId">产品规格ID.</param>
    /// <param name="attributes">扩展属性列表.</param>
    /// <returns></returns>
    Task SaveAttributes(string productSpecId, List<ProductSpecAttributeEntity> attributes);

    /// <summary>
    /// 删除产品规格的所有扩展属性.
    /// </summary>
    /// <param name="productSpecId">产品规格ID.</param>
    /// <returns></returns>
    Task DeleteAttributesByProductSpecId(string productSpecId);
}
