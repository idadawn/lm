using Poxiao.Lab.Entity.Dto.ProductSpec;
using Poxiao.Lab.Entity.Entity;

namespace Poxiao.Lab.Interfaces;

/// <summary>
/// 产品规格服务接口.
/// </summary>
public interface IProductSpecService
{
    /// <summary>
    /// 获取列表.
    /// </summary>
    /// <param name="input">查询参数.</param>
    /// <returns></returns>
    Task<List<ProductSpecListOutput>> GetList(ProductSpecListQuery input);

    /// <summary>
    /// 获取详情.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    Task<ProductSpecInfoOutput> GetInfo(string id);

    /// <summary>
    /// 创建.
    /// </summary>
    /// <param name="input">创建参数.</param>
    /// <returns></returns>
    Task Create(ProductSpecCrInput input);

    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <param name="input">更新参数.</param>
    /// <returns></returns>
    Task Update(string id, ProductSpecUpInput input);

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    Task Delete(string id);
}
