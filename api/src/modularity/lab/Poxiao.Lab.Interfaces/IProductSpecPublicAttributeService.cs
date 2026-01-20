using Poxiao.Lab.Entity;

namespace Poxiao.Lab.Interfaces;

/// <summary>
/// 产品规格公共属性服务接口.
/// </summary>
public interface IProductSpecPublicAttributeService
{
    /// <summary>
    /// 获取所有公共属性.
    /// </summary>
    /// <returns>公共属性列表.</returns>
    Task<List<ProductSpecPublicAttributeEntity>> GetPublicAttributes();

    /// <summary>
    /// 创建公共属性.
    /// </summary>
    /// <param name="entity">公共属性实体.</param>
    /// <returns></returns>
    Task Create(ProductSpecPublicAttributeEntity entity);

    /// <summary>
    /// 更新公共属性.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <param name="entity">公共属性实体.</param>
    /// <returns></returns>
    Task Update(string id, ProductSpecPublicAttributeEntity entity);

    /// <summary>
    /// 删除公共属性.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    Task Delete(string id);

    /// <summary>
    /// 将公共属性应用到所有现有产品（如果属性名称不存在则添加）.
    /// </summary>
    /// <param name="publicAttributeId">公共属性ID.</param>
    /// <returns></returns>
    Task ApplyToAllProducts(string publicAttributeId);
}
